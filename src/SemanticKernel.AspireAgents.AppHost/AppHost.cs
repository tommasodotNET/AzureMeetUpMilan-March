var builder = DistributedApplication.CreateBuilder(args);

var existingOpenAIName = builder.AddParameter("existingOpenAIName");
var existingOpenAIResourceGroup = builder.AddParameter("existingOpenAIResourceGroup");

var azureOpenAI = builder.AddAzureOpenAI("azureOpenAI");
        
// If you want to use an existing Azure OpenAI resource, uncomment the following line
azureOpenAI.AsExisting(existingOpenAIName, existingOpenAIResourceGroup);

builder.AddProject<Projects.SemanticKernel_AspireAgents_GroupChatAPI>("group-chat-api")
    .WithReference(azureOpenAI)
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
