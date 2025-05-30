var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.AddConnectionString("azureOpenAI");

builder.AddProject<Projects.SemanticKernel_AspireAgents_GroupChatAPI>("group-chat-api")
    .WithReference(openai)
    .WithHttpCommand("/group-chat", "Invoke Group Chat",
    commandOptions: new()
    {
        Method = HttpMethod.Get,
        Description = "Invoke the group chat orchestration with a prompt",
        PrepareRequest = (context) =>
        {
            context.Request.RequestUri = new Uri(context.Request.RequestUri, $"/group-chat?prompt=Semantic Kernel");
            return Task.CompletedTask;
        },
        IconName = "ChatRegular"
    });

builder.Build().Run();
