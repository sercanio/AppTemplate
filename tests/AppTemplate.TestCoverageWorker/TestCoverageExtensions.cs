using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;

namespace AppTemplate.TestCoverageWorker;

public static class TestCoverageExtensions
{
  public static IEndpointRouteBuilder MapTestCoverageEndpoints(this IEndpointRouteBuilder endpoints)
  {
    var group = endpoints.MapGroup("/test-coverage");

    group.MapGet("/", async (HttpContext context) =>
    {
      var coverageIndexPath = GetCoverageIndexPath();

      if (!File.Exists(coverageIndexPath))
      {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync(
          """
            Test coverage report not found. 
            Run the test coverage worker first 
            or if it is already running wait a 
            few minutes for the report to be generated.
          """);
          
        return;
      }

      // Redirect to the actual HTML file
      context.Response.Redirect("/test-coverage/html/index.html");
    });

    group.MapGet("/{*path}", async (HttpContext context, string path) =>
    {
      var coverageReportsPath = GetCoverageReportsPath();
      var requestedFile = Path.Combine(coverageReportsPath, path.Replace('/', Path.DirectorySeparatorChar));

      // Security check: ensure the file is within the coverage reports directory
      var fullCoverageReportsPath = Path.GetFullPath(coverageReportsPath);
      var fullRequestedPath = Path.GetFullPath(requestedFile);

      if (!fullRequestedPath.StartsWith(fullCoverageReportsPath))
      {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Access denied.");
        return;
      }

      if (!File.Exists(requestedFile))
      {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync($"File not found: {path}");
        return;
      }

      // Set appropriate content type
      var contentTypeProvider = new FileExtensionContentTypeProvider();
      if (!contentTypeProvider.TryGetContentType(requestedFile, out var contentType))
      {
        contentType = "application/octet-stream";
      }

      context.Response.ContentType = contentType;

      // Add cache headers for static files
      context.Response.Headers.CacheControl = "public, max-age=300"; // 5 minutes cache

      await context.Response.SendFileAsync(requestedFile);
    });

    return endpoints;
  }

  private static string GetCoverageReportsPath()
  {
    // Start from the current directory (web app directory)
    var currentDir = Directory.GetCurrentDirectory();
    
    // Look for solution file by going up the directory tree
    var solutionDir = FindSolutionDirectory(currentDir);
    
    if (solutionDir != null)
    {
      // Coverage reports are in solution_root/tests/coverage-reports
      var testsDir = Path.Combine(solutionDir, "tests", "coverage-reports");
      if (Directory.Exists(testsDir))
      {
        return testsDir;
      }
      
      // Fallback: try solution_root/coverage-reports
      var rootCoverageDir = Path.Combine(solutionDir, "coverage-reports");
      if (Directory.Exists(rootCoverageDir))
      {
        return rootCoverageDir;
      }
    }

    // Fallback: try to find coverage-reports directory by searching up the tree
    var searchDir = currentDir;
    while (searchDir != null)
    {
      // Look for tests/coverage-reports first
      var testsCandidate = Path.Combine(searchDir, "tests", "coverage-reports");
      if (Directory.Exists(testsCandidate))
      {
        return testsCandidate;
      }
      
      // Then look for coverage-reports directly
      var candidatePath = Path.Combine(searchDir, "coverage-reports");
      if (Directory.Exists(candidatePath))
      {
        return candidatePath;
      }
      
      var parentDir = Directory.GetParent(searchDir);
      searchDir = parentDir?.FullName;
    }

    // Last fallback to current directory
    return Path.Combine(currentDir, "coverage-reports");
  }

  private static string? FindSolutionDirectory(string startDirectory)
  {
    var currentDir = startDirectory;
    
    while (currentDir != null)
    {
      var solutionFiles = Directory.GetFiles(currentDir, "*.sln");
      if (solutionFiles.Length > 0)
      {
        return currentDir;
      }

      var parentDir = Directory.GetParent(currentDir);
      currentDir = parentDir?.FullName;
    }

    return null;
  }

  private static string GetCoverageIndexPath()
  {
    var coverageReportsPath = GetCoverageReportsPath();
    return Path.Combine(coverageReportsPath, "html", "index.html");
  }
}