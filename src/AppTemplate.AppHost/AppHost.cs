using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("apptemplate-db")
    .WithPgAdmin()
    .AddDatabase("AppTemplateDb");

var redis = builder.AddRedis("apptemplate-redis")
    .WaitFor(db)
    .WithRedisInsight();

var seq = builder.AddSeq("apptemplate-seq", port: 5341)
    .WaitFor(db)
    .WaitFor(redis);

builder.AddProject<Projects.AppTemplate_Web>("apptemplate-web")
    .WithReference(db)
    .WithReference(redis)
    .WithReference(seq)
    .WaitFor(db)
    .WaitFor(redis)
    .WaitFor(seq);

builder.Build().Run();