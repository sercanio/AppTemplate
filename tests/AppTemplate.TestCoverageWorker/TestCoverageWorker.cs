using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace AppTemplate.TestCoverageWorker;

public class TestCoverageWorker : BackgroundService
{
  private readonly ILogger<TestCoverageWorker> _logger;
  private readonly TestCoverageOptions _options;
  private readonly IHostApplicationLifetime _hostApplicationLifetime;

  public TestCoverageWorker(
      ILogger<TestCoverageWorker> logger,
      IConfiguration configuration,
      IHostApplicationLifetime hostApplicationLifetime)
  {
    _logger = logger;
    _hostApplicationLifetime = hostApplicationLifetime;
    _options = new TestCoverageOptions();
    configuration.GetSection("TestCoverage").Bind(_options);
    
    // Validate configuration
    if (_options.IntervalHours <= 0)
    {
        _logger.LogWarning("Invalid IntervalHours value: {hours}. Using default: 24", _options.IntervalHours);
        _options.IntervalHours = 24;
    }
    
    if (string.IsNullOrWhiteSpace(_options.OutputDirectory))
    {
        _options.OutputDirectory = "coverage-reports";
        _logger.LogInformation("Using default output directory: {dir}", _options.OutputDirectory);
    }
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Test Coverage Worker started at: {time}", DateTimeOffset.Now);

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        await GenerateTestCoverageReportAsync(stoppingToken);

        if (_options.RunOnce)
        {
          _logger.LogInformation("Running in single execution mode. Stopping worker.");
          _hostApplicationLifetime.StopApplication();
          break;
        }

        await Task.Delay(TimeSpan.FromHours(_options.IntervalHours), stoppingToken);
      }
      catch (OperationCanceledException)
      {
        _logger.LogInformation("Test Coverage Worker cancelled.");
        break;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while generating test coverage report");

        // Wait before retrying
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
      }
    }

    _logger.LogInformation("Test Coverage Worker stopped at: {time}", DateTimeOffset.Now);
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

    var outputDir = Path.Combine(Path.GetDirectoryName(solutionPath)!, _options.OutputDirectory);
    
    // Clean up the entire coverage reports directory before starting
    await CleanupCoverageReportsDirectoryAsync(outputDir);
    
    Directory.CreateDirectory(outputDir);

    _logger.LogInformation("Output directory: {outputDir}", outputDir);

    // Step 1: Run tests with coverage collection
    _logger.LogInformation("Running tests with coverage collection...");
    var coverageFile = await RunTestsWithCoverageAsync(solutionPath, outputDir, cancellationToken);

    if (string.IsNullOrEmpty(coverageFile))
    {
      _logger.LogError("Failed to generate coverage data");
      return;
    }

    // Step 2: Generate HTML report
    _logger.LogInformation("Generating HTML coverage report...");
    await GenerateHtmlReportAsync(coverageFile, outputDir, cancellationToken);

    // Step 3: Generate summary statistics
    _logger.LogInformation("Generating coverage statistics...");
    await GenerateCoverageStatisticsAsync(coverageFile, outputDir, cancellationToken);
    _logger.LogInformation("Test coverage report generation completed. Output directory: {outputDir}", outputDir);

    // Step 4: Log the path to the HTML report index file
    var indexPath = Path.Combine(outputDir, "html", "index.html");
    if (File.Exists(indexPath))
    {
      var fileUrl = $"file:///{indexPath.Replace("\\", "/")}";
      _logger.LogInformation("Open the HTML report in your browser: {fileUrl}", fileUrl);
    }
    else
    {
      _logger.LogWarning("HTML report index file not found at expected location: {indexPath}", indexPath);
    }
  }

  private async Task CleanupCoverageReportsDirectoryAsync(string outputDir)
{
    try
    {
        if (Directory.Exists(outputDir))
        {
            _logger.LogInformation("Cleaning up existing coverage reports directory: {outputDir}", outputDir);
            
            // Give a small delay to ensure no processes are using the files
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
                    retryDelay *= 2; // Exponential backoff
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
        else
        {
            _logger.LogInformation("Coverage reports directory does not exist, no cleanup needed");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to cleanup coverage reports directory: {outputDir}. Continuing with generation...", outputDir);
        // Don't throw - we can still try to generate reports even if cleanup failed
    }
}

  private async Task<string?> RunTestsWithCoverageAsync(string solutionPath, string outputDir, CancellationToken cancellationToken)
  {
    // Clean up any existing coverage files first
    await CleanupOldCoverageFilesAsync(outputDir);

    var excludePatterns = string.Join(",", new[]
    {
        "[*Tests]*,[*Tests.*]*,[*.Tests]*,[*.Tests.*]*",
        "[AppTemplate.Web]AppTemplate.Web.Program",
        "[AppTemplate.Web]AppTemplate.Web.Startup", 
        "[*]*.Migrations.*",
        "[System.*]*,[Microsoft.*]*,[testhost]*"
    });

    var arguments = $"test \"{solutionPath}\" --configuration Release " +
                   $"--collect:\"XPlat Code Coverage\" " +
                   $"--results-directory \"{outputDir}\" " +
                   $"--logger \"trx;LogFileName=TestResults.trx\" " +
                   $"-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura " +
                   $"DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude=\"{excludePatterns}\" " +
                   $"DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.IncludeTestAssembly=false";

    _logger.LogInformation("Executing command: dotnet {arguments}", arguments);

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
        _logger.LogWarning("Test error: {data}", e.Data);
      }
    };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    await process.WaitForExitAsync(cancellationToken);

    if (process.ExitCode != 0)
    {
      _logger.LogError("Test execution failed with exit code {exitCode}", process.ExitCode);
      
      // Log all captured output for debugging
      if (output.Count > 0)
      {
        _logger.LogError("Standard Output:");
        foreach (var line in output)
        {
          _logger.LogError("  {line}", line);
        }
      }
      
      if (errors.Count > 0)
      {
        _logger.LogError("Standard Error:");
        foreach (var line in errors)
        {
          _logger.LogError("  {line}", line);
        }
      }
      
      // Also log the working directory and command for troubleshooting
      _logger.LogError("Working Directory: {workingDir}", Directory.GetCurrentDirectory());
      _logger.LogError("Solution Path: {solutionPath}", solutionPath);
      _logger.LogError("Output Directory: {outputDir}", outputDir);
      _logger.LogError("Full command: dotnet {arguments}", arguments);
      
      return null;
    }

    // Wait a bit for files to be released
    await Task.Delay(2000, cancellationToken);

    return await FindAndMergeCoverageFilesAsync(outputDir);
}

private async Task CleanupOldCoverageFilesAsync(string outputDir)
{
    try
    {
        if (Directory.Exists(outputDir))
        {
            // Only clean up very old files that might have been missed
            var cutoffTime = DateTime.Now.AddDays(-1);
            
            // Remove old GUID directories (older than 1 day)
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

            // Remove old coverage files (older than 1 day)
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
    var retryDelay = 1000; // 1 second

    for (int retry = 0; retry < maxRetries; retry++)
    {
        try
        {
            // Find ALL generated coverage files, excluding our target file
            var coverageFiles = Directory.GetFiles(outputDir, "coverage.cobertura.xml", SearchOption.AllDirectories)
                .Where(f => !f.Equals(Path.Combine(outputDir, "coverage.cobertura.xml"), StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Contains("TestResults"))
                .ToArray();

            _logger.LogInformation("Found {count} coverage files on attempt {retry}", coverageFiles.Length, retry + 1);

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
                // Single file - just copy it
                await CopyFileWithRetryAsync(coverageFiles[0], mergedFile);
                return mergedFile;
            }

            // Multiple files - use the largest one
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
            _logger.LogInformation("Successfully copied coverage file from {source} to {dest}", sourceFile, destinationFile);
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
        _logger.LogInformation("Merging {count} coverage files into {output}", coverageFiles.Length, outputFile);
        
        // Use the largest file (most comprehensive coverage)
        var largestFile = coverageFiles
            .Select(f => new { File = f, Size = new FileInfo(f).Length })
            .OrderByDescending(x => x.Size)
            .First();
            
        _logger.LogInformation("Using largest coverage file: {file} ({size} bytes)", 
            largestFile.File, largestFile.Size);
            
        await CopyFileWithRetryAsync(largestFile.File, outputFile);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to merge coverage files");
        
        // Fallback: just use the first file
        if (coverageFiles.Length > 0)
        {
            await CopyFileWithRetryAsync(coverageFiles[0], outputFile);
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
          _logger.LogWarning("ReportGenerator error: {data}", e.Data);
      };

      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      await process.WaitForExitAsync(cancellationToken);

      if (process.ExitCode == 0)
      {
        _logger.LogInformation("HTML report generated successfully at: {reportDir}", reportDir);
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

  private async Task GenerateCoverageStatisticsAsync(string coverageFile, string outputDir, CancellationToken cancellationToken)
  {
    try
    {
      var xmlContent = await File.ReadAllTextAsync(coverageFile, cancellationToken);
      var statistics = ParseCoverageStatistics(xmlContent);

      var statisticsFile = Path.Combine(outputDir, "coverage-statistics.json");
      var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
      var json = JsonSerializer.Serialize(statistics, jsonOptions);

      await File.WriteAllTextAsync(statisticsFile, json, cancellationToken);

      _logger.LogInformation("Coverage statistics: {statistics}", JsonSerializer.Serialize(statistics));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to generate coverage statistics");
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

        // Validate and adjust statistics
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

    // Look for .sln file in current directory and parent directories
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
    // Validate that covered lines/branches don't exceed totals
    var adjustedStats = stats with
    {
        CoveredLines = Math.Min(stats.CoveredLines, stats.TotalLines),
        CoveredBranches = Math.Min(stats.CoveredBranches, stats.TotalBranches)
    };

    // Recalculate percentages based on actual values
    adjustedStats = adjustedStats with
    {
        LinesCoverage = adjustedStats.TotalLines > 0 
            ? Math.Round((decimal)adjustedStats.CoveredLines / adjustedStats.TotalLines * 100, 2) 
            : 0,
        BranchesCoverage = adjustedStats.TotalBranches > 0 
            ? Math.Round((decimal)adjustedStats.CoveredBranches / adjustedStats.TotalBranches * 100, 2) 
            : 0
    };

    // Check if coverage seems unrealistically low
    if (adjustedStats.LinesCoverage < 10 && adjustedStats.TotalLines > 100)
    {
        _logger.LogWarning("Coverage seems unusually low ({coverage}%). Check exclude patterns.", adjustedStats.LinesCoverage);
        _logger.LogWarning("Total lines: {total}, Covered: {covered}", adjustedStats.TotalLines, adjustedStats.CoveredLines);
    }

    if (stats.CoveredLines != adjustedStats.CoveredLines || stats.CoveredBranches != adjustedStats.CoveredBranches)
    {
        _logger.LogWarning("Coverage data was adjusted due to inconsistencies");
    }

    return adjustedStats;
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
}}