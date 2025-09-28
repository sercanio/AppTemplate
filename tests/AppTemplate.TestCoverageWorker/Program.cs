using AppTemplate.TestCoverageWorker;

var builder = Host.CreateApplicationBuilder(args);

// Add Aspire service defaults (logging, metrics, health checks, service discovery)
builder.AddServiceDefaults();

// Add the test coverage worker service
builder.Services.AddHostedService<TestCoverageWorker>();

// Add configuration for test coverage settings
builder.Services.Configure<TestCoverageOptions>(
    builder.Configuration.GetSection("TestCoverage"));

var host = builder.Build();
host.Run();