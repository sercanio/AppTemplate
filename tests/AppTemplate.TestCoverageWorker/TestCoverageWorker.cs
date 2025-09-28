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

    var solutionPath = FindSolutionPath();
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

  private async Task<string?> RunTestsWithCoverageAsync(string solutionPath, string outputDir, CancellationToken cancellationToken)
  {
    var coverageFile = Path.Combine(outputDir, "coverage.cobertura.xml");
    var arguments = $"test \"{solutionPath}\" --no-build " +
                   $"--collect:\"XPlat Code Coverage\" " +
                   $"--results-directory \"{outputDir}\" " +
                   $"--logger \"trx;LogFileName=TestResults.trx\" " +
                   $"-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura " +
                   $"DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude=\"[*.Tests]*,[*]*Program,[*]*Startup\"";

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

    // Find the generated coverage file
    var coverageFiles = Directory.GetFiles(outputDir, "coverage.cobertura.xml", SearchOption.AllDirectories);
    if (coverageFiles.Length > 0)
    {
      var latestCoverageFile = coverageFiles.OrderByDescending(File.GetCreationTime).First();
      var destinationFile = Path.Combine(outputDir, "coverage.cobertura.xml");

      if (latestCoverageFile != destinationFile)
      {
        File.Copy(latestCoverageFile, destinationFile, true);
      }

      return destinationFile;
    }

    _logger.LogError("Coverage file not found");
    return null;
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
    // Simple XML parsing for coverage statistics
    var lineRate = ExtractAttributeValue(xmlContent, "coverage", "line-rate");
    var branchRate = ExtractAttributeValue(xmlContent, "coverage", "branch-rate");
    var linesValid = ExtractAttributeValue(xmlContent, "coverage", "lines-valid");
    var linesCovered = ExtractAttributeValue(xmlContent, "coverage", "lines-covered");
    var branchesValid = ExtractAttributeValue(xmlContent, "coverage", "branches-valid");
    var branchesCovered = ExtractAttributeValue(xmlContent, "coverage", "branches-covered");

    return new CoverageStatistics
    {
      GeneratedAt = DateTime.UtcNow,
      LinesCoverage = decimal.TryParse(lineRate, out var lr) ? Math.Round(lr * 100, 2) : 0,
      BranchesCoverage = decimal.TryParse(branchRate, out var br) ? Math.Round(br * 100, 2) : 0,
      TotalLines = int.TryParse(linesValid, out var lv) ? lv : 0,
      CoveredLines = int.TryParse(linesCovered, out var lc) ? lc : 0,
      TotalBranches = int.TryParse(branchesValid, out var bv) ? bv : 0,
      CoveredBranches = int.TryParse(branchesCovered, out var bc) ? bc : 0
    };
  }

  private static string ExtractAttributeValue(string xml, string elementName, string attributeName)
  {
    var elementStart = xml.IndexOf($"<{elementName}", StringComparison.OrdinalIgnoreCase);
    if (elementStart == -1) return "0";

    var elementEnd = xml.IndexOf(">", elementStart);
    if (elementEnd == -1) return "0";

    var elementContent = xml.Substring(elementStart, elementEnd - elementStart);
    var attributeStart = elementContent.IndexOf($"{attributeName}=\"", StringComparison.OrdinalIgnoreCase);
    if (attributeStart == -1) return "0";

    attributeStart += $"{attributeName}=\"".Length;
    var attributeEnd = elementContent.IndexOf("\"", attributeStart);
    if (attributeEnd == -1) return "0";

    return elementContent.Substring(attributeStart, attributeEnd - attributeStart);
  }

  private string? FindSolutionPath()
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