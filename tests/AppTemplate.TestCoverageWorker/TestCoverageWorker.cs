using System.Diagnostics;
using System.Text.Json;

namespace AppTemplate.TestCoverageWorker;

public class TestCoverageWorker : BackgroundService
{
  private readonly ILogger<TestCoverageWorker> _logger;
  private readonly TestCoverageOptions _options;
  private readonly IHostApplicationLifetime _hostApplicationLifetime;

  // Layer configurations
  private readonly Dictionary<string, LayerConfig> _layers = new()
  {
    ["Domain"] = new LayerConfig
    {
      Name = "Domain",
      IncludePattern = "[AppTemplate.Domain]*",
      Description = "Business logic and domain models"
    },
    ["Application"] = new LayerConfig
    {
      Name = "Application",
      IncludePattern = "[AppTemplate.Application]*",
      Description = "Use cases and application services"
    },
    ["Infrastructure"] = new LayerConfig
    {
      Name = "Infrastructure",
      IncludePattern = "[AppTemplate.Infrastructure]*",
      Description = "Data access and external services"
    },
    ["Web"] = new LayerConfig
    {
      Name = "Web",
      IncludePattern = "[AppTemplate.Web]*",
      Description = "API controllers and middleware"
    }
  };

  public TestCoverageWorker(
      ILogger<TestCoverageWorker> logger,
      IConfiguration configuration,
      IHostApplicationLifetime hostApplicationLifetime)
  {
    _logger = logger;
    _hostApplicationLifetime = hostApplicationLifetime;
    _options = new TestCoverageOptions();
    configuration.GetSection("TestCoverage").Bind(_options);

    if (string.IsNullOrWhiteSpace(_options.OutputDirectory))
    {
      _options.OutputDirectory = "coverage-reports";
      _logger.LogInformation("Using default output directory: {dir}", _options.OutputDirectory);
    }
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Test Coverage Worker started at: {time}", DateTimeOffset.Now);

    try
    {
      await GenerateTestCoverageReportAsync(stoppingToken);
      _logger.LogInformation("Test Coverage Worker completed successfully at: {time}", DateTimeOffset.Now);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Test Coverage Worker was cancelled.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error occurred while generating test coverage report");
    }
    finally
    {
      _logger.LogInformation("Test Coverage Worker finishing and stopping application at: {time}", DateTimeOffset.Now);
    }
  }

  private async Task GenerateTestCoverageReportAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting test coverage report generation...");

    var solutionPath = await FindSolutionPathAsync();
    if (string.IsNullOrEmpty(solutionPath))
    {
      _logger.LogError("Could not find solution file");
      return;
    }

    _logger.LogInformation("Found solution file: {solutionPath}", solutionPath);

    if (!File.Exists(solutionPath))
    {
      _logger.LogError("Solution file does not exist: {solutionPath}", solutionPath);
      return;
    }

    var testsDir = Path.Combine(Path.GetDirectoryName(solutionPath)!);
    var outputDir = Path.Combine(testsDir, _options.OutputDirectory);

    await CleanupCoverageReportsDirectoryAsync(outputDir);
    Directory.CreateDirectory(outputDir);

    _logger.LogInformation("Output directory: {outputDir}", outputDir);

    // Generate layer-by-layer coverage reports FIRST
    var layerResults = new Dictionary<string, CoverageStatistics>();

    foreach (var layer in _layers.Values)
    {
      _logger.LogInformation("=== Generating Coverage Report for {layerName} Layer ===", layer.Name);

      var layerCoverageFile = await GenerateLayerCoverageAsync(
          solutionPath,
          outputDir,
          layer,
          cancellationToken);

      if (!string.IsNullOrEmpty(layerCoverageFile))
      {
        var layerOutputDir = Path.Combine(outputDir, layer.Name.ToLowerInvariant());
        await GenerateHtmlReportAsync(layerCoverageFile, layerOutputDir, cancellationToken);

        var xmlContent = await File.ReadAllTextAsync(layerCoverageFile, cancellationToken);
        var stats = ParseCoverageStatistics(xmlContent);
        layerResults[layer.Name] = stats;
      }
    }

    // Generate overall coverage report by merging layer results
    _logger.LogInformation("=== Generating Overall Coverage Report ===");
    var overallCoverageFile = await GenerateOverallCoverageAsync(solutionPath, outputDir, cancellationToken);

    if (!string.IsNullOrEmpty(overallCoverageFile))
    {
      await GenerateHtmlReportAsync(overallCoverageFile, Path.Combine(outputDir, "overall"), cancellationToken);
    }

    // Generate summary report
    await GenerateSummaryReportAsync(outputDir, overallCoverageFile, layerResults, cancellationToken);

    // Log access information
    await LogWebAppAccessInfo(outputDir);
  }

  private async Task<string?> GenerateOverallCoverageAsync(
      string solutionPath,
      string outputDir,
      CancellationToken cancellationToken)
  {
    var overallDir = Path.Combine(outputDir, "overall");
    Directory.CreateDirectory(overallDir);

    // OPTION 1: Generate overall by merging layer coverage files
    // This will give accurate overall statistics
    var layerCoverageFiles = new List<string>();

    foreach (var layer in _layers.Values)
    {
      var layerCoverageFile = Path.Combine(
          outputDir,
          layer.Name.ToLowerInvariant(),
          "coverage.cobertura.xml");

      if (File.Exists(layerCoverageFile))
      {
        layerCoverageFiles.Add(layerCoverageFile);
      }
    }

    if (layerCoverageFiles.Count > 0)
    {
      var mergedFile = Path.Combine(overallDir, "coverage.cobertura.xml");
      await MergeMultipleCoverageFilesAsync(layerCoverageFiles.ToArray(), mergedFile);
      return mergedFile;
    }

    // OPTION 2: Fallback to running tests again (shouldn't happen)
    var excludePatterns = GetStandardExcludePatterns();
    var includePatterns = string.Join(",", _layers.Values.Select(l => l.IncludePattern));

    var coverageFile = await RunTestsWithCoverageAsync(
        solutionPath,
        overallDir,
        includePatterns,
        excludePatterns,
        cancellationToken);

    return coverageFile;
  }

  private async Task<string?> GenerateLayerCoverageAsync(
      string solutionPath,
      string outputDir,
      LayerConfig layer,
      CancellationToken cancellationToken)
  {
    var layerDir = Path.Combine(outputDir, layer.Name.ToLowerInvariant());
    Directory.CreateDirectory(layerDir);

    var excludePatterns = GetStandardExcludePatterns();
    var includePatterns = layer.IncludePattern;

    var coverageFile = await RunTestsWithCoverageAsync(
        solutionPath,
        layerDir,
        includePatterns,
        excludePatterns,
        cancellationToken);

    return coverageFile;
  }

  private string GetStandardExcludePatterns()
  {
    return string.Join(",", new[]
    {
        // Test assemblies
        "[*.Tests]*",
        "[*.Tests.*]*",
        "[*Tests]*",
        "[*Tests.*]*",
        "[testhost]*",

        // Program/Startup files
        "[*].Program",
        "[*].Startup",
        "[*]*Program*",
        "[*]*.AppHost",

        // Migrations
        "[*]*.Migrations.*",
        "[*]*Migration*",
        "[*]*Designer*",
        "[*]*DbContextModelSnapshot",

        // Configuration files
        "[*]*Configuration",
        "[*]*Options",
        "[*]*Settings",

        // DTOs and Models
        "[*]*Dto",
        "[*]*Request",
        "[*]*Response",
        "[*]*ViewModel",

        // Extensions and Attributes
        "[*]*Extensions",
        "[*]*Attribute",

        // System assemblies
        "[System.*]*",
        "[Microsoft.*]*",
        "[NETStandard.Library]*",

        // Auto-generated
        "[*]*Generated*",
        "[*]*.g.cs",
        "[*]*.designer.cs"
    });
  }

  private async Task<string?> RunTestsWithCoverageAsync(
      string solutionPath,
      string outputDir,
      string includePatterns,
      string excludePatterns,
      CancellationToken cancellationToken)
  {
    await CleanupOldCoverageFilesAsync(outputDir);

    var arguments = $"test \"{solutionPath}\" --configuration Release " +
                   $"--collect:\"XPlat Code Coverage\" " +
                   $"--results-directory \"{outputDir}\" " +
                   $"--logger \"trx;LogFileName=TestResults.trx\" " +
                   $"--verbosity quiet " +
                   $"-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura " +
                   $"DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include=\"{includePatterns}\" " +
                   $"DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude=\"{excludePatterns}\" " +
                   $"DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.IncludeTestAssembly=false " +
                   $"DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.UseSourceLink=false";

    _logger.LogDebug("Include patterns: {includePatterns}", includePatterns);
    _logger.LogDebug("Exclude patterns: {excludePatterns}", excludePatterns);

    var processInfo = new ProcessStartInfo
    {
      FileName = "dotnet",
      Arguments = arguments,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true
    };

    using var process = new Process { StartInfo = processInfo };

    var output = new List<string>();
    var errors = new List<string>();

    process.OutputDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
      {
        output.Add(e.Data);
        _logger.LogDebug("Test output: {data}", e.Data);
      }
    };

    process.ErrorDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
      {
        errors.Add(e.Data);
        _logger.LogDebug("Test error: {data}", e.Data);
      }
    };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    await process.WaitForExitAsync(cancellationToken);

    if (process.ExitCode != 0)
    {
      _logger.LogWarning("Test execution completed with exit code {exitCode}", process.ExitCode);
    }

    await Task.Delay(2000, cancellationToken);

    return await FindAndMergeCoverageFilesAsync(outputDir);
  }

  private async Task GenerateSummaryReportAsync(
      string outputDir,
      string? overallCoverageFile,
      Dictionary<string, CoverageStatistics> layerResults,
      CancellationToken cancellationToken)
  {
    try
    {
      var summary = new CoverageSummary
      {
        GeneratedAt = DateTime.UtcNow,
        Layers = layerResults
      };

      if (!string.IsNullOrEmpty(overallCoverageFile))
      {
        var xmlContent = await File.ReadAllTextAsync(overallCoverageFile, cancellationToken);
        summary.Overall = ParseCoverageStatistics(xmlContent);
      }

      var summaryFile = Path.Combine(outputDir, "coverage-summary.json");
      var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
      var json = JsonSerializer.Serialize(summary, jsonOptions);

      await File.WriteAllTextAsync(summaryFile, json, cancellationToken);

      // Log summary to console
      _logger.LogInformation("");
      _logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
      _logger.LogInformation("║           📊 TEST COVERAGE SUMMARY                             ║");
      _logger.LogInformation("╠════════════════════════════════════════════════════════════════╣");

      if (summary.Overall != null)
      {
        _logger.LogInformation("║  Overall Coverage:                                             ║");
        _logger.LogInformation("║    Lines:    {lines,5:F1}%  ({covered,6}/{total,6})                      ║",
            summary.Overall.LinesCoverage,
            summary.Overall.CoveredLines,
            summary.Overall.TotalLines);
        _logger.LogInformation("║    Branches: {branches,5:F1}%  ({covered,6}/{total,6})                      ║",
            summary.Overall.BranchesCoverage,
            summary.Overall.CoveredBranches,
            summary.Overall.TotalBranches);
        _logger.LogInformation("╠════════════════════════════════════════════════════════════════╣");
      }

      _logger.LogInformation("║  Layer-by-Layer Coverage:                                      ║");
      _logger.LogInformation("╟────────────────────────────────────────────────────────────────╢");

      foreach (var (layerName, stats) in layerResults.OrderByDescending(x => x.Value.LinesCoverage))
      {
        var emoji = GetCoverageEmoji(stats.LinesCoverage);
        _logger.LogInformation("║  {emoji} {layerName,-15}                                          ║", emoji, layerName);
        _logger.LogInformation("║    Lines:    {lines,5:F1}%  ({covered,6}/{total,6})                      ║",
            stats.LinesCoverage,
            stats.CoveredLines,
            stats.TotalLines);
        _logger.LogInformation("║    Branches: {branches,5:F1}%  ({covered,6}/{total,6})                      ║",
            stats.BranchesCoverage,
            stats.CoveredBranches,
            stats.TotalBranches);
        _logger.LogInformation("╟────────────────────────────────────────────────────────────────╢");
      }

      _logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");
      _logger.LogInformation("");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to generate summary report");
    }
  }

  private string GetCoverageEmoji(decimal coverage)
  {
    return coverage switch
    {
      >= 80 => "🟢",
      >= 60 => "🟡",
      >= 40 => "🟠",
      _ => "🔴"
    };
  }

  private async Task LogWebAppAccessInfo(string outputDir)
  {
    _logger.LogInformation("");
    _logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
    _logger.LogInformation("║           📁 COVERAGE REPORTS ACCESS                           ║");
    _logger.LogInformation("╠════════════════════════════════════════════════════════════════╣");
    _logger.LogInformation("║  Overall Report:                                               ║");
    _logger.LogInformation("║    📄 {0,-58} ║", Path.Combine(outputDir, "overall", "html", "index.html"));
    _logger.LogInformation("╟────────────────────────────────────────────────────────────────╢");
    _logger.LogInformation("║  Layer Reports:                                                ║");

    foreach (var layer in _layers.Values)
    {
      var layerPath = Path.Combine(outputDir, layer.Name.ToLowerInvariant(), "html", "index.html");
      if (File.Exists(layerPath))
      {
        _logger.LogInformation("║    📄 {0}: {1,-40} ║",
            layer.Name,
            Path.Combine(layer.Name.ToLowerInvariant(), "html", "index.html"));
      }
    }

    _logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");
    _logger.LogInformation("");
  }

  private async Task CleanupCoverageReportsDirectoryAsync(string outputDir)
  {
    try
    {
      if (Directory.Exists(outputDir))
      {
        _logger.LogInformation("Cleaning up existing coverage reports directory: {outputDir}", outputDir);
        await Task.Delay(500);

        var maxRetries = 3;
        var retryDelay = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
          try
          {
            Directory.Delete(outputDir, true);
            _logger.LogInformation("Successfully cleaned up coverage reports directory");
            break;
          }
          catch (IOException ex) when (attempt < maxRetries)
          {
            _logger.LogWarning(ex, "Attempt {attempt}/{maxRetries} to delete directory failed, retrying in {delay}ms",
                attempt, maxRetries, retryDelay);
            await Task.Delay(retryDelay);
            retryDelay *= 2;
          }
          catch (UnauthorizedAccessException ex) when (attempt < maxRetries)
          {
            _logger.LogWarning(ex, "Access denied on attempt {attempt}/{maxRetries}, retrying in {delay}ms",
                attempt, maxRetries, retryDelay);
            await Task.Delay(retryDelay);
            retryDelay *= 2;
          }
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to cleanup coverage reports directory: {outputDir}. Continuing with generation...", outputDir);
    }
  }

  private async Task CleanupOldCoverageFilesAsync(string outputDir)
  {
    try
    {
      if (Directory.Exists(outputDir))
      {
        var cutoffTime = DateTime.Now.AddDays(-1);

        var subdirs = Directory.GetDirectories(outputDir)
            .Where(d => Guid.TryParse(Path.GetFileName(d), out _))
            .Where(d => Directory.GetCreationTime(d) < cutoffTime);

        foreach (var dir in subdirs)
        {
          try
          {
            Directory.Delete(dir, true);
            _logger.LogDebug("Deleted old coverage directory: {dir}", dir);
          }
          catch (Exception ex)
          {
            _logger.LogWarning(ex, "Failed to delete old directory: {dir}", dir);
          }
        }

        var oldCoverageFiles = Directory.GetFiles(outputDir, "coverage*.xml")
            .Where(f => File.GetCreationTime(f) < cutoffTime);

        foreach (var file in oldCoverageFiles)
        {
          try
          {
            File.Delete(file);
            _logger.LogDebug("Deleted old coverage file: {file}", file);
          }
          catch (Exception ex)
          {
            _logger.LogWarning(ex, "Failed to delete old coverage file: {file}", file);
          }
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Failed to cleanup old coverage files");
    }
  }

  private async Task<string?> FindAndMergeCoverageFilesAsync(string outputDir)
  {
    var maxRetries = 5;
    var retryDelay = 1000;

    for (int retry = 0; retry < maxRetries; retry++)
    {
      try
      {
        var coverageFiles = Directory.GetFiles(outputDir, "coverage.cobertura.xml", SearchOption.AllDirectories)
            .Where(f => !f.Equals(Path.Combine(outputDir, "coverage.cobertura.xml"), StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("TestResults"))
            .ToArray();

        _logger.LogDebug("Found {count} coverage files on attempt {retry}", coverageFiles.Length, retry + 1);

        if (coverageFiles.Length == 0)
        {
          if (retry == maxRetries - 1)
          {
            _logger.LogError("No coverage files found after {retries} attempts", maxRetries);
            return null;
          }

          await Task.Delay(retryDelay, CancellationToken.None);
          continue;
        }

        var mergedFile = Path.Combine(outputDir, "coverage.cobertura.xml");

        if (coverageFiles.Length == 1)
        {
          await CopyFileWithRetryAsync(coverageFiles[0], mergedFile);
          return mergedFile;
        }

        await MergeCoverageFilesAsync(coverageFiles, mergedFile);
        return mergedFile;
      }
      catch (Exception ex) when (retry < maxRetries - 1)
      {
        _logger.LogWarning(ex, "Attempt {retry} failed, retrying in {delay}ms", retry + 1, retryDelay);
        await Task.Delay(retryDelay, CancellationToken.None);
      }
    }

    _logger.LogError("Failed to find and merge coverage files after {retries} attempts", maxRetries);
    return null;
  }

  private async Task CopyFileWithRetryAsync(string sourceFile, string destinationFile)
  {
    var maxRetries = 3;
    var retryDelay = 500;

    for (int retry = 0; retry < maxRetries; retry++)
    {
      try
      {
        if (File.Exists(destinationFile))
        {
          File.Delete(destinationFile);
        }

        File.Copy(sourceFile, destinationFile, true);
        _logger.LogDebug("Successfully copied coverage file from {source} to {dest}", sourceFile, destinationFile);
        return;
      }
      catch (IOException ex) when (retry < maxRetries - 1)
      {
        _logger.LogWarning(ex, "File copy attempt {retry} failed, retrying in {delay}ms", retry + 1, retryDelay);
        await Task.Delay(retryDelay, CancellationToken.None);
      }
    }

    throw new IOException($"Failed to copy file from {sourceFile} to {destinationFile} after {maxRetries} attempts");
  }

  private async Task MergeCoverageFilesAsync(string[] coverageFiles, string outputFile)
  {
    try
    {
      _logger.LogDebug("Merging {count} coverage files into {output}", coverageFiles.Length, outputFile);

      var largestFile = coverageFiles
          .Select(f => new { File = f, Size = new FileInfo(f).Length })
          .OrderByDescending(x => x.Size)
          .First();

      _logger.LogDebug("Using largest coverage file: {file} ({size} bytes)",
          largestFile.File, largestFile.Size);

      await CopyFileWithRetryAsync(largestFile.File, outputFile);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to merge coverage files");

      if (coverageFiles.Length > 0)
      {
        await CopyFileWithRetryAsync(coverageFiles[0], outputFile);
      }
    }
  }

  private async Task MergeMultipleCoverageFilesAsync(string[] coverageFiles, string outputFile)
  {
    try
    {
      _logger.LogInformation("Merging {count} layer coverage files for overall report", coverageFiles.Length);

      if (coverageFiles.Length == 1)
      {
        await CopyFileWithRetryAsync(coverageFiles[0], outputFile);
        return;
      }

      // Parse all coverage files and combine statistics
      var combinedDoc = new System.Xml.Linq.XDocument();
      var combinedCoverage = new System.Xml.Linq.XElement("coverage");

      int totalLinesValid = 0;
      int totalLinesCovered = 0;
      int totalBranchesValid = 0;
      int totalBranchesCovered = 0;

      var packagesElement = new System.Xml.Linq.XElement("packages");

      foreach (var file in coverageFiles)
      {
        var doc = System.Xml.Linq.XDocument.Load(file);
        var coverage = doc.Root;

        if (coverage == null) continue;

        // Accumulate totals
        totalLinesValid += int.Parse(coverage.Attribute("lines-valid")?.Value ?? "0");
        totalLinesCovered += int.Parse(coverage.Attribute("lines-covered")?.Value ?? "0");
        totalBranchesValid += int.Parse(coverage.Attribute("branches-valid")?.Value ?? "0");
        totalBranchesCovered += int.Parse(coverage.Attribute("branches-covered")?.Value ?? "0");

        // Add packages from this file
        var packages = coverage.Element("packages");
        if (packages != null)
        {
          foreach (var package in packages.Elements("package"))
          {
            packagesElement.Add(new System.Xml.Linq.XElement(package));
          }
        }
      }

      // Calculate rates
      decimal lineRate = totalLinesValid > 0 ? (decimal)totalLinesCovered / totalLinesValid : 0;
      decimal branchRate = totalBranchesValid > 0 ? (decimal)totalBranchesCovered / totalBranchesValid : 0;

      // Set combined attributes
      combinedCoverage.SetAttributeValue("line-rate", lineRate.ToString("0.####"));
      combinedCoverage.SetAttributeValue("branch-rate", branchRate.ToString("0.####"));
      combinedCoverage.SetAttributeValue("lines-covered", totalLinesCovered);
      combinedCoverage.SetAttributeValue("lines-valid", totalLinesValid);
      combinedCoverage.SetAttributeValue("branches-covered", totalBranchesCovered);
      combinedCoverage.SetAttributeValue("branches-valid", totalBranchesValid);
      combinedCoverage.SetAttributeValue("complexity", "0");
      combinedCoverage.SetAttributeValue("version", "1.9");
      combinedCoverage.SetAttributeValue("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

      combinedCoverage.Add(packagesElement);
      combinedDoc.Add(combinedCoverage);

      // Save merged file
      await using var writer = new StreamWriter(outputFile);
      await writer.WriteAsync(combinedDoc.ToString());

      _logger.LogInformation("Successfully merged coverage files:");
      _logger.LogInformation("  Lines: {covered}/{total} ({rate:P2})",
          totalLinesCovered, totalLinesValid, lineRate);
      _logger.LogInformation("  Branches: {covered}/{total} ({rate:P2})",
          totalBranchesCovered, totalBranchesValid, branchRate);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to merge multiple coverage files");

      // Fallback: use the largest file
      if (coverageFiles.Length > 0)
      {
        var largestFile = coverageFiles
            .Select(f => new { File = f, Size = new FileInfo(f).Length })
            .OrderByDescending(x => x.Size)
            .First();

        await CopyFileWithRetryAsync(largestFile.File, outputFile);
      }
    }
  }

  private async Task GenerateHtmlReportAsync(string coverageFile, string outputDir, CancellationToken cancellationToken)
  {
    try
    {
      var reportDir = Path.Combine(outputDir, "html");

      var arguments = $"-reports:\"{coverageFile}\" " +
                     $"-targetdir:\"{reportDir}\" " +
                     $"-reporttypes:Html " +
                     $"-title:\"AppTemplate Test Coverage Report\"";

      var processInfo = new ProcessStartInfo
      {
        FileName = "reportgenerator",
        Arguments = arguments,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };

      using var process = new Process { StartInfo = processInfo };

      process.OutputDataReceived += (sender, e) =>
      {
        if (!string.IsNullOrEmpty(e.Data))
          _logger.LogDebug("ReportGenerator output: {data}", e.Data);
      };

      process.ErrorDataReceived += (sender, e) =>
      {
        if (!string.IsNullOrEmpty(e.Data))
          _logger.LogDebug("ReportGenerator error: {data}", e.Data);
      };

      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      await process.WaitForExitAsync(cancellationToken);

      if (process.ExitCode == 0)
      {
        _logger.LogDebug("HTML report generated successfully at: {reportDir}", reportDir);
      }
      else
      {
        _logger.LogError("ReportGenerator failed with exit code {exitCode}", process.ExitCode);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to generate HTML report. Ensure 'reportgenerator' tool is installed globally");
    }
  }

  private CoverageStatistics ParseCoverageStatistics(string xmlContent)
  {
    try
    {
      var doc = System.Xml.Linq.XDocument.Parse(xmlContent);
      var coverage = doc.Root;

      if (coverage?.Name.LocalName != "coverage")
      {
        _logger.LogWarning("Invalid coverage XML format");
        return new CoverageStatistics { GeneratedAt = DateTime.UtcNow };
      }

      var lineRate = decimal.TryParse(coverage.Attribute("line-rate")?.Value, out var lr) ? lr : 0;
      var branchRate = decimal.TryParse(coverage.Attribute("branch-rate")?.Value, out var br) ? br : 0;
      var linesValid = int.TryParse(coverage.Attribute("lines-valid")?.Value, out var lv) ? lv : 0;
      var linesCovered = int.TryParse(coverage.Attribute("lines-covered")?.Value, out var lc) ? lc : 0;
      var branchesValid = int.TryParse(coverage.Attribute("branches-valid")?.Value, out var bv) ? bv : 0;
      var branchesCovered = int.TryParse(coverage.Attribute("branches-covered")?.Value, out var bc) ? bc : 0;

      var stats = new CoverageStatistics
      {
        GeneratedAt = DateTime.UtcNow,
        LinesCoverage = Math.Round(lineRate * 100, 2),
        BranchesCoverage = Math.Round(branchRate * 100, 2),
        TotalLines = linesValid,
        CoveredLines = linesCovered,
        TotalBranches = branchesValid,
        CoveredBranches = branchesCovered
      };

      return ValidateAndAdjustStatistics(stats);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to parse coverage XML");
      return new CoverageStatistics { GeneratedAt = DateTime.UtcNow };
    }
  }

  private async Task<string?> FindSolutionPathAsync()
  {
    var currentDir = Directory.GetCurrentDirectory();

    while (currentDir != null)
    {
      var solutionFiles = Directory.GetFiles(currentDir, "*.sln");
      if (solutionFiles.Length > 0)
      {
        return solutionFiles.First();
      }

      var parentDir = Directory.GetParent(currentDir);
      currentDir = parentDir?.FullName;
    }

    return null;
  }

  private CoverageStatistics ValidateAndAdjustStatistics(CoverageStatistics stats)
  {
    var adjustedStats = stats with
    {
      CoveredLines = Math.Min(stats.CoveredLines, stats.TotalLines),
      CoveredBranches = Math.Min(stats.CoveredBranches, stats.TotalBranches)
    };

    adjustedStats = adjustedStats with
    {
      LinesCoverage = adjustedStats.TotalLines > 0
            ? Math.Round((decimal)adjustedStats.CoveredLines / adjustedStats.TotalLines * 100, 2)
            : 0,
      BranchesCoverage = adjustedStats.TotalBranches > 0
            ? Math.Round((decimal)adjustedStats.CoveredBranches / adjustedStats.TotalBranches * 100, 2)
            : 0
    };

    return adjustedStats;
  }

  public record LayerConfig
  {
    public required string Name { get; init; }
    public required string IncludePattern { get; init; }
    public required string Description { get; init; }
  }

  public record CoverageStatistics
  {
    public DateTime GeneratedAt { get; init; }
    public decimal LinesCoverage { get; init; }
    public decimal BranchesCoverage { get; init; }
    public int TotalLines { get; init; }
    public int CoveredLines { get; init; }
    public int TotalBranches { get; init; }
    public int CoveredBranches { get; init; }
  }

  public record CoverageSummary
  {
    public DateTime GeneratedAt { get; init; }
    public CoverageStatistics? Overall { get; set; }
    public Dictionary<string, CoverageStatistics> Layers { get; init; } = new();
  }
}