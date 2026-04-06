using k8s.Models;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.LanManager_Api>("lanmanager-api");

var frontend = builder.AddJavaScriptApp("frontend", "../../frontend", "dev")
    .WithReference(api)
    .WithHttpEndpoint(port: 5173, env: "VITE_PORT")
    .WithExternalHttpEndpoints();

//Add tunnel
var publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
    .WithAnonymousAccess()
    .WithReference(api.GetEndpoint("https"));

builder.Build().Run();
