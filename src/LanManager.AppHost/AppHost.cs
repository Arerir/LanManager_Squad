var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.LanManager_Api>("lanmanager-api");

// React frontend (Vite dev server — Trinity will configure this)
// Uncomment when frontend is scaffolded:
// var frontend = builder.AddNpmApp("frontend", "../frontend")
//     .WithReference(api)
//     .WithHttpEndpoint(port: 5173, env: "PORT");

builder.Build().Run();
