using System.Diagnostics;
using System.Text.Json;
using Spectre.Console;

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
      Description = "Business logic and domain models",
      Color = Color.Green
    },
    ["Application"] = new LayerConfig
    {
      Name = "Application",
      IncludePattern = "[AppTemplate.Application]*",
      Description = "Use cases and application services",
      Color = Color.Blue
    },
    ["Infrastructure"] = new LayerConfig
    {
      Name = "Infrastructure",
      IncludePattern = "[AppTemplate.Infrastructure]*",
      Description = "Data access and external services",
      Color = Color.Orange1
    },
    ["Web"] = new LayerConfig
    {
      Name = "Web",
      IncludePattern = "[AppTemplate.Presentation]*",
      Description = "API controllers and middleware",
      Color = Color.Purple
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
    try
    {
      ShowWelcomeBanner();
      await GenerateTestCoverageReportAsync(stoppingToken);
      ShowCompletionMessage();
    }
    catch (OperationCanceledException)
    {
      AnsiConsole.MarkupLine("[yellow]⚠️ Test Coverage Worker was cancelled.[/]");
    }
    catch (Exception ex)
    {
      AnsiConsole.WriteException(ex);
      _logger.LogError(ex, "Error occurred while generating test coverage report");
    }
  }

  private void ShowWelcomeBanner()
  {
    var rule = new Rule("[bold blue]🧪 Test Coverage Report Generator[/]")
    {
      Justification = Justify.Left
    };
    AnsiConsole.Write(rule);
    AnsiConsole.WriteLine();

    var panel = new Panel(new Markup(
        "[bold]AppTemplate Coverage Analysis[/]\n" +
        "[dim]Generating comprehensive test coverage reports for all layers...[/]"))
    {
      Border = BoxBorder.Rounded,
      BorderStyle = new Style(Color.Blue),
      Padding = new Padding(2, 1)
    };
    AnsiConsole.Write(panel);
    AnsiConsole.WriteLine();
  }

  private async Task GenerateTestCoverageReportAsync(CancellationToken cancellationToken)
  {
    var solutionPath = await FindSolutionPathAsync();
    if (string.IsNullOrEmpty(solutionPath))
    {
      AnsiConsole.MarkupLine("[red]❌ Could not find solution file[/]");
      return;
    }

    AnsiConsole.MarkupLine($"[green]✅ Found solution:[/] [dim]{solutionPath}[/]");

    if (!File.Exists(solutionPath))
    {
      AnsiConsole.MarkupLine("[red]❌ Solution file does not exist[/]");
      return;
    }

    var testsDir = Path.Combine(Path.GetDirectoryName(solutionPath)!);
    var outputDir = Path.Combine(testsDir, _options.OutputDirectory);

    await CleanupCoverageReportsDirectoryAsync(outputDir);
    Directory.CreateDirectory(outputDir);

    AnsiConsole.MarkupLine($"[blue]📁 Output directory:[/] [dim]{outputDir}[/]");
    AnsiConsole.WriteLine();

    // Generate layer-by-layer coverage reports
    var layerResults = new Dictionary<string, CoverageStatistics>();

    await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
          var overallTask = ctx.AddTask("[green]Overall Progress[/]", maxValue: _layers.Count + 2);

          foreach (var layer in _layers.Values)
          {
            var layerTask = ctx.AddTask($"[{layer.Color.ToMarkup()}]{layer.Name} Layer[/]");

            AnsiConsole.MarkupLine($"\n[bold {layer.Color.ToMarkup()}]🔍 Processing {layer.Name} Layer[/]");
            AnsiConsole.MarkupLine($"[dim]{layer.Description}[/]");

            var layerCoverageFile = await GenerateLayerCoverageAsync(
                solutionPath,
                outputDir,
                layer,
                cancellationToken,
                layerTask);

            if (!string.IsNullOrEmpty(layerCoverageFile))
            {
              var layerOutputDir = Path.Combine(outputDir, layer.Name.ToLowerInvariant());
              await GenerateHtmlReportAsync(layerCoverageFile, layerOutputDir, cancellationToken);

              var xmlContent = await File.ReadAllTextAsync(layerCoverageFile, cancellationToken);
              var stats = ParseCoverageStatistics(xmlContent);
              layerResults[layer.Name] = stats;

              AnsiConsole.MarkupLine($"[green]✅ {layer.Name} completed[/] - [bold]{stats.LinesCoverage:F1}%[/] line coverage");
            }
            else
            {
              AnsiConsole.MarkupLine($"[red]❌ {layer.Name} failed[/]");
            }

            layerTask.Value = 100;
            overallTask.Increment(1);
          }

          // Generate overall coverage report
          AnsiConsole.MarkupLine("\n[bold blue]🔗 Generating Overall Coverage Report[/]");
          var overallTask2 = ctx.AddTask("[blue]Overall Coverage[/]");

          var overallCoverageFile = await GenerateOverallCoverageAsync(solutionPath, outputDir, cancellationToken);

          if (!string.IsNullOrEmpty(overallCoverageFile))
          {
            await GenerateHtmlReportAsync(overallCoverageFile, Path.Combine(outputDir, "overall"), cancellationToken);
          }

          overallTask2.Value = 100;
          overallTask.Increment(1);

          // Generate summary report
          AnsiConsole.MarkupLine("\n[bold yellow]📊 Generating Summary Report[/]");
          var summaryTask = ctx.AddTask("[yellow]Summary Report[/]");

          await GenerateSummaryReportAsync(outputDir, overallCoverageFile, layerResults, cancellationToken);

          summaryTask.Value = 100;
          overallTask.Increment(1);
        });

    // Display results
    await DisplayCoverageResults(outputDir, layerResults);
  }

  private async Task DisplayCoverageResults(string outputDir, Dictionary<string, CoverageStatistics> layerResults)
  {
    AnsiConsole.WriteLine();

    // Create coverage summary table
    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Blue)
        .AddColumn(new TableColumn("[bold]Layer[/]").Centered())
        .AddColumn(new TableColumn("[bold]Line Coverage[/]").Centered())
        .AddColumn(new TableColumn("[bold]Branch Coverage[/]").Centered())
        .AddColumn(new TableColumn("[bold]Lines[/]").Centered())
        .AddColumn(new TableColumn("[bold]Branches[/]").Centered())
        .AddColumn(new TableColumn("[bold]Status[/]").Centered());

    foreach (var (layerName, stats) in layerResults.OrderByDescending(x => x.Value.LinesCoverage))
    {
      var layer = _layers[layerName];
      var statusIcon = GetCoverageStatusIcon(stats.LinesCoverage);
      var statusColor = GetCoverageStatusColor(stats.LinesCoverage);

      table.AddRow(
          $"[{layer.Color.ToMarkup()}]{layerName}[/]",
          $"[{statusColor}]{stats.LinesCoverage:F1}%[/]",
          $"[{statusColor}]{stats.BranchesCoverage:F1}%[/]",
          $"[dim]{stats.CoveredLines}/{stats.TotalLines}[/]",
          $"[dim]{stats.CoveredBranches}/{stats.TotalBranches}[/]",
          $"[{statusColor}]{statusIcon}[/]"
      );
    }

    var panel = new Panel(table)
    {
      Header = new PanelHeader("[bold blue]📊 Coverage Summary[/]"),
      Border = BoxBorder.Double,
      BorderStyle = new Style(Color.Blue)
    };

    AnsiConsole.Write(panel);

    // Display file access information
    ShowAccessInformation(outputDir);
  }

  private string GetCoverageStatusIcon(decimal coverage)
  {
    return coverage switch
    {
      >= 80 => "🟢",
      >= 60 => "🟡",
      >= 40 => "🟠",
      _ => "🔴"
    };
  }

  private string GetCoverageStatusColor(decimal coverage)
  {
    return coverage switch
    {
      >= 80 => "green",
      >= 60 => "yellow",
      >= 40 => "orange1",
      _ => "red"
    };
  }

  private void ShowAccessInformation(string outputDir)
  {
    AnsiConsole.WriteLine();

    // Create clickable file URLs
    var overallHtmlPath = Path.Combine(outputDir, "overall", "html", "index.html");
    var overallFileUrl = ConvertToFileUrl(overallHtmlPath);

    var layerUrls = _layers.Values.Select(layer =>
    {
      var layerPath = Path.Combine(outputDir, layer.Name.ToLowerInvariant(), "html", "index.html");
      var fileUrl = ConvertToFileUrl(layerPath);
      return new { Layer = layer, Path = layerPath, Url = fileUrl };
    }).ToList();

    var accessInfo = new Panel(
        new Markup(
            "[bold green]REPORT FILES GENERATED[/]\n\n" +
            "[blue]Overall Report:[/]\n" +
            $"[link={overallFileUrl}][dim]{overallHtmlPath}[/][/]\n\n" +
            "[blue]Layer Reports:[/]\n" +
            string.Join("\n", layerUrls.Select(item =>
                $"[link={item.Url}][dim]{item.Layer.Name}: {item.Path}[/][/]"))
        ))
    {
      Header = new PanelHeader("[bold green]ACCESS INFORMATION[/]"),
      Border = BoxBorder.Rounded,
      BorderStyle = new Style(Color.Green)
    };

    AnsiConsole.Write(accessInfo);
    AnsiConsole.WriteLine();
  }

  private string ConvertToFileUrl(string filePath)
  {
    // Convert file path to file:// URL format
    if (string.IsNullOrEmpty(filePath))
      return string.Empty;

    try
    {
      var uri = new Uri(filePath);
      return uri.ToString();
    }
    catch
    {
      // Fallback: manually construct file URL
      var normalizedPath = Path.GetFullPath(filePath).Replace('\\', '/');

      if (OperatingSystem.IsWindows())
      {
        // Windows: file:///C:/path/to/file
        return $"file:///{normalizedPath}";
      }
      else
      {
        // Unix-like: file:///path/to/file
        return $"file://{normalizedPath}";
      }
    }
  }

  private void ShowCompletionMessage()
  {
    AnsiConsole.WriteLine();

    var successRule = new Rule("[bold green]COVERAGE REPORT GENERATION COMPLETE[/]")
    {
      Justification = Justify.Center,
      Style = Style.Parse("green")
    };

    AnsiConsole.Write(successRule);
    AnsiConsole.WriteLine();
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

  private async Task<string?> GenerateLayerCoverageAsync(
      string solutionPath,
      string outputDir,
      LayerConfig layer,
      CancellationToken cancellationToken,
      ProgressTask? progressTask = null)
  {
    var layerDir = Path.Combine(outputDir, layer.Name.ToLowerInvariant());
    Directory.CreateDirectory(layerDir);

    var excludePatterns = GetStandardExcludePatterns();
    var includePatterns = layer.IncludePattern;

    progressTask?.StartTask();

    var coverageFile = await RunTestsWithCoverageAsync(
        solutionPath,
        layerDir,
        includePatterns,
        excludePatterns,
        cancellationToken,
        progressTask);

    progressTask?.StopTask();
    return coverageFile;
  }

  private async Task<string?> RunTestsWithCoverageAsync(
      string solutionPath,
      string outputDir,
      string includePatterns,
      string excludePatterns,
      CancellationToken cancellationToken,
      ProgressTask? progressTask = null)
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

    var dotnetPath = GetDotNetExecutablePath();

    var processInfo = new ProcessStartInfo
    {
      FileName = dotnetPath,
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

        // Update progress based on test output
        if (progressTask != null && e.Data.Contains("Test run"))
        {
          progressTask.Value = Math.Min(progressTask.Value + 10, 90);
        }
      }
    };

    process.ErrorDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
      {
        errors.Add(e.Data);
      }
    };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    await process.WaitForExitAsync(cancellationToken);

    if (process.ExitCode != 0)
    {
      AnsiConsole.MarkupLine($"[yellow]⚠️ Test execution completed with exit code {process.ExitCode}[/]");
    }

    await Task.Delay(2000, cancellationToken);
    return await FindAndMergeCoverageFilesAsync(outputDir);
  }

  private static string GetDotNetExecutablePath()
  {
    // Try to get the dotnet path from DOTNET_ROOT environment variable first
    var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
    if (!string.IsNullOrEmpty(dotnetRoot))
    {
      var dotnetPath = Path.Combine(dotnetRoot, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
      if (File.Exists(dotnetPath))
      {
        return dotnetPath;
      }
    }

    // Fallback: Use Process.GetCurrentProcess() to find the dotnet host
    var currentProcess = Environment.ProcessPath;
    if (!string.IsNullOrEmpty(currentProcess))
    {
      var processDir = Path.GetDirectoryName(currentProcess);
      if (!string.IsNullOrEmpty(processDir))
      {
        var dotnetPath = Path.Combine(processDir, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
        if (File.Exists(dotnetPath))
        {
          return dotnetPath;
        }
      }
    }

    // Last resort: Check common installation paths
    if (OperatingSystem.IsWindows())
    {
      var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      var dotnetPath = Path.Combine(programFiles, "dotnet", "dotnet.exe");
      if (File.Exists(dotnetPath))
      {
        return dotnetPath;
      }
    }
    else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
    {
      var commonPaths = new[] { "/usr/local/share/dotnet/dotnet", "/usr/share/dotnet/dotnet", "/usr/bin/dotnet" };
      foreach (var path in commonPaths)
      {
        if (File.Exists(path))
        {
          return path;
        }
      }
    }

    // If all else fails, fall back to "dotnet" and rely on PATH (with a warning)
    AnsiConsole.MarkupLine("[yellow]⚠️ Could not locate dotnet executable, falling back to PATH resolution[/]");
    return "dotnet";
  }

  private async Task<string?> GenerateOverallCoverageAsync(
      string solutionPath,
      string outputDir,
      CancellationToken cancellationToken)
  {
    var overallDir = Path.Combine(outputDir, "overall");
    Directory.CreateDirectory(overallDir);

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
    }
    catch (Exception ex)
    {
      AnsiConsole.WriteException(ex);
    }
  }

  private async Task CleanupCoverageReportsDirectoryAsync(string outputDir)
  {
    try
    {
      if (Directory.Exists(outputDir))
      {
        AnsiConsole.MarkupLine("[yellow]🧹 Cleaning up existing coverage reports...[/]");
        await Task.Delay(500);

        var maxRetries = 3;
        var retryDelay = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
          try
          {
            Directory.Delete(outputDir, true);
            AnsiConsole.MarkupLine("[green]✅ Cleanup completed[/]");
            break;
          }
          catch (IOException) when (attempt < maxRetries)
          {
            AnsiConsole.MarkupLine($"[yellow]⚠️ Cleanup attempt {attempt}/{maxRetries} failed, retrying...[/]");
            await Task.Delay(retryDelay);
            retryDelay *= 2;
          }
          catch (UnauthorizedAccessException) when (attempt < maxRetries)
          {
            AnsiConsole.MarkupLine($"[yellow]⚠️ Access denied on attempt {attempt}/{maxRetries}, retrying...[/]");
            await Task.Delay(retryDelay);
            retryDelay *= 2;
          }
        }
      }
    }
    catch (Exception ex)
    {
      AnsiConsole.WriteException(ex);
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

        if (coverageFiles.Length == 0)
        {
          if (retry == maxRetries - 1)
          {
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
      catch (Exception) when (retry < maxRetries - 1)
      {
        await Task.Delay(retryDelay, CancellationToken.None);
      }
    }

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
        return;
      }
      catch (IOException) when (retry < maxRetries - 1)
      {
        await Task.Delay(retryDelay, CancellationToken.None);
      }
    }

    throw new IOException($"Failed to copy file from {sourceFile} to {destinationFile} after {maxRetries} attempts");
  }

  private async Task MergeCoverageFilesAsync(string[] coverageFiles, string outputFile)
  {
    try
    {
      var largestFile = coverageFiles
          .Select(f => new { File = f, Size = new FileInfo(f).Length })
          .OrderByDescending(x => x.Size)
          .First();

      await CopyFileWithRetryAsync(largestFile.File, outputFile);
    }
    catch (Exception)
    {
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
      if (coverageFiles.Length == 1)
      {
        await CopyFileWithRetryAsync(coverageFiles[0], outputFile);
        return;
      }

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

        if (coverage == null)
        {
          continue;
        }

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

      await using var writer = new StreamWriter(outputFile);
      await writer.WriteAsync(combinedDoc.ToString());
    }
    catch (Exception)
    {
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
      process.Start();
      await process.WaitForExitAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      AnsiConsole.WriteException(ex);
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
    public required Color Color { get; init; }
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
