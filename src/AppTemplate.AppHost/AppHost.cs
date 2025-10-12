using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

//var db = builder.AddPostgres("apptemplate-db")
//    .WithPgAdmin()
//    .WithDataVolume(isReadOnly: false)
//    .WithLifetime(ContainerLifetime.Persistent)
//    .AddDatabase("AppTemplateDb");

//var redis = builder.AddRedis("apptemplate-redis")
//    .WaitFor(db)
//    .WithRedisInsight()
//    .WithLifetime(ContainerLifetime.Persistent);

//var seq = builder.AddSeq("apptemplate-seq")
//    .WaitFor(db)
//    .WaitFor(redis)
//    .WithLifetime(ContainerLifetime.Persistent);

// Add the migration service
var migrations = builder.AddProject<Projects.AppTemplate_MigrationService>("migration-worker");

// Add the test coverage worker
builder.AddProject<Projects.AppTemplate_TestCoverageWorker>("test-coverage-worker")
  .WithUrl("https://localhost:7294/test-coverage");

// Update the web project to wait for migrations
builder.AddProject<Projects.AppTemplate_Web>("apptemplate-web")
    .WaitFor(migrations)
    .WaitForCompletion(migrations);

builder.Build().Run();