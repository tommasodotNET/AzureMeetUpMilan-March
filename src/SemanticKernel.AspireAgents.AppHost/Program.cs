var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.AddConnectionString("openAiConnectionName");

builder.AddProject<Projects.SemanticKernel_AspireAgents_GroupChatAPI>("group-chat-api")
    .WithReference(openai);

builder.Build().Run();
