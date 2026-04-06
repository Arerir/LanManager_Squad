var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.LanManager_Api>("lanmanager-api");

var frontend = builder.AddNpmApp("frontend", "../../frontend", "dev")
    .WithReference(api)
    .WithHttpEndpoint(port: 5173, env: "VITE_PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
