namespace AppTemplate.TestCoverageWorker;

public class TestCoverageOptions
{
  public double IntervalHours { get; set; } = 24; // Run daily by default
  public string OutputDirectory { get; set; } = "coverage-reports";
  public bool RunOnce { get; set; } = false; // Set to true for single execution
  public string[]? ExcludePatterns { get; set; }
  public bool GenerateHtmlReport { get; set; } = true;
  public bool GenerateJsonSummary { get; set; } = true;
}