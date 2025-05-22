var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.AddConnectionString("azureOpenAI");

builder.AddProject<Projects.SemanticKernel_AspireAgents_GroupChatAPI>("group-chat-api")
    .WithReference(openai);

builder.Build().Run();
