using System.Text;
using AppTemplate.TestCoverageWorker;

// Set console encoding to UTF-8 to support Unicode characters
Console.OutputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(args);

// Add the test coverage worker service
builder.Services.AddHostedService<TestCoverageWorker>();

// Add configuration for test coverage settings
builder.Services.Configure<TestCoverageOptions>(
    builder.Configuration.GetSection("TestCoverage"));

var host = builder.Build();
host.Run();
