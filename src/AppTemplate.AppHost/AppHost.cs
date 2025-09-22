using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("apptemplate-db")
    .WithPgAdmin()
    .WithDataVolume(isReadOnly: false)
    .AddDatabase("AppTemplateDb");

var redis = builder.AddRedis("apptemplate-redis")
    .WaitFor(db)
    .WithRedisInsight();

var seq = builder.AddSeq("apptemplate-seq")
    .WaitFor(db)
    .WaitFor(redis);

// Add the migration service
var migrations = builder.AddProject<Projects.AppTemplate_MigrationService>("migration-worker")
    .WithReference(db)
    .WaitFor(db);

// Update the web project to wait for migrations
builder.AddProject<Projects.AppTemplate_Web>("apptemplate-web")
    .WithReference(db)
    .WithReference(redis)
    .WithReference(seq)
    .WaitFor(migrations)
    .WaitForCompletion(migrations);

builder.Build().Run();