using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Liquid;
using Microsoft.SemanticKernel.Prompty;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.AddAzureOpenAIClient("azureOpenAI", configureSettings: settings =>
{
    settings.Credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions() { TenantId = "16b3c013-d300-468d-ac64-7eda0820b6d3" });
});
builder.Services.AddKernel().AddAzureOpenAIChatCompletion("gpt-4o");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/group-chat", async (Kernel kernel, string prompt) =>
{
    // OrchestrationMonitor monitor = new();

    // Use the clients as needed here
    var monitor = new OrchestrationMonitor();
    var scenario = await InitializeScenarioAsync(kernel, monitor);

    InProcessRuntime runtime = new InProcessRuntime();
    await runtime.StartAsync();

    var result = await scenario.InvokeAsync(prompt, runtime);

    string text = await result.GetValueAsync(TimeSpan.FromSeconds(90));

    await runtime.RunUntilIdleAsync();
    Console.WriteLine($"\n# RESULT: {text}");
    Console.WriteLine("\n\nORCHESTRATION HISTORY");

    foreach (ChatMessageContent message in monitor.History)
    {
        Console.WriteLine();
        Console.WriteLine($"# {message.Role} - {message.AuthorName ?? "*"}: '{message.Content}'");
        Console.WriteLine();
    }

    return Results.Ok();
})
.WithName("InvokeRemoteGroupChat");

app.MapDefaultEndpoints();

app.Run();

async Task<GroupChatOrchestration> InitializeScenarioAsync(Kernel kernel, OrchestrationMonitor monitor)
{
    PersistentAgentsClient agentsClient = AzureAIAgent.CreateAgentsClient("https://tstocchi-foundry.services.ai.azure.com/api/projects/tstocchi-foundry-project",
        new AzureCliCredential());
    PersistentAgent definition = await agentsClient.Administration.GetAgentAsync("asst_v4z53sEtLNEeM3t1eOWRgAhv");

    string researcherAgentName = "ResearcherAgent";

    AzureAIAgent researcherAgent = new(definition, agentsClient)
    {
        Name = researcherAgentName,
        Description = "Researcher agent",
        Kernel = kernel
    };

    string writerAgentName = "WriterAgent";
    string promptyTemplate = await File.ReadAllTextAsync("Prompts/writerAgent.prompty");
    var promptTemplate = KernelFunctionPrompty.ToPromptTemplateConfig(promptyTemplate);

    ChatCompletionAgent writerAgent = new ChatCompletionAgent(promptTemplate, new LiquidPromptTemplateFactory())
    {
        Name = writerAgentName,
        Description = "Writer agent",
        Kernel = kernel
    };

    string reviewerAgentName = "WriterFeedback";
    string reviewerAgentInstructions = """
            You are an expert agent specialized in reviewing and providing detailed feedback on texts. Your task is to carefully analyze the provided text and offer actionable suggestions to enhance its quality and effectiveness.

            When providing feedback, always clearly address the following points:

            1) Clarity and Readability:
                - Evaluate if the ideas and explanations are clearly presented and easily understandable.
                - Suggest ways to simplify complex or unclear sentences.

            2) Grammar and Style:
                - Identify and correct grammatical, spelling, or punctuation errors.
                - Suggest improvements in sentence structure and overall writing style to enhance readability.
                - Terminology and Accuracy:

            Ensure the text correctly uses terminology appropriate to the topic.

            Provide corrections or suggest better terminology choices if inaccuracies or inconsistencies are found.

            Present your feedback clearly, organizing your suggestions into the three points listed above. Include examples from the original text where relevant, and suggest revised formulations when providing corrections.

            Once you have provided the feedback, you must approve the work saying "I approve".
        """;

    ChatCompletionAgent reviewerAgent = new ChatCompletionAgent
    {
        Name = reviewerAgentName,
        Instructions = reviewerAgentInstructions,
        Description = "Reviewer agent",
        Kernel = kernel
    };

    GroupChatOrchestration orchestration = new GroupChatOrchestration(
        new RoundRobinGroupChatManager { MaximumInvocationCount = 5 },
        writerAgent, reviewerAgent)
    {
        ResponseCallback = monitor.ResponseCallback,
    };

    return orchestration;
}

public class OrchestrationMonitor
{
    public ChatHistory History { get; } = [];

    public ValueTask ResponseCallback(ChatMessageContent response)
    {
        this.History.Add(response);
        return ValueTask.CompletedTask;
    }
}