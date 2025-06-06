﻿# Azure AI Agents

## Initialize scenario
```csharp
AIProjectClient client = new AIProjectClient("swedencentral.api.azureml.ms;bc06be43-f36c-449a-b690-4fea320f3e73;ai-agents;ai-agents-project",
new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ExcludeVisualStudioCredential = true,
    ExcludeEnvironmentCredential = true,
    ExcludeManagedIdentityCredential = true
}));
var agentsClient = client.GetAgentsClient();
var agent = await agentsClient.GetAgentAsync("asst_XlORPWTtq4FSyMItEw2lbT2a");

string researcherAgentName = "ResearcherAgent";

AzureAIAgent researcherAgent = new(agent, agentsClient)
{
    Name = researcherAgentName,
    Kernel = KernelCreator.CreateKernel(useAzureOpenAI)
};
```

# Prompty

## Initialize scenario
```csharp
string writerAgentName = "WriterAgent";
string promptyTemplate = await File.ReadAllTextAsync("Prompts/writerAgent.prompty");
var promptTemplate = KernelFunctionPrompty.ToPromptTemplateConfig(promptyTemplate);

ChatCompletionAgent writerAgent = new ChatCompletionAgent(promptTemplate, new LiquidPromptTemplateFactory())
{
    Name = writerAgentName,
    Kernel = KernelCreator.CreateKernel(useAzureOpenAI)
};
```

# Reviewer agent

## Initialize scenario
```csharp
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
    Kernel = KernelCreator.CreateKernelWithReasoningModel(useAzureOpenAI)
};
```

# Group chat setup

```csharp
KernelFunction selectionFunction =
    AgentGroupChat.CreatePromptFunctionForStrategy(
        $$$"""
        Determine which participant takes the next turn in a conversation based on the the most recent participant.
        State only the name of the participant to take the next turn.
        No participant should take more than one turn in a row.

        Choose only from these participants:
        - {{{researcherAgentName}}}
        - {{{writerAgentName}}}
        - {{{reviewerAgentName}}}

        Always follow these rules when selecting the next participant:
        - The user will share a topic to research
        - After the user, it's {{{researcherAgentName}}}'s turn to research the given topic.
        - After {{{researcherAgentName}}}, it is {{{writerAgentName}}}'s turn to write the text.
        - After {{{writerAgentName}}}, it is {{{reviewerAgentName}}}'s turn to review the text.

        History:
        {{$history}}
        """,
        safeParameterNames: "history");

            // Define the selection strategy
            KernelFunctionSelectionStrategy selectionStrategy =
                new(selectionFunction, KernelCreator.CreateKernel(useAzureOpenAI))
                {
                    // Always start with the writer agent.
                    InitialAgent = researcherAgent,
                    // Parse the function response.
                    ResultParser = (result) => result.GetValue<string>() ?? researcherAgentName,
                    // The prompt variable name for the history argument.
                    HistoryVariableName = "history",
                    // Save tokens by not including the entire history in the prompt
                    HistoryReducer = new ChatHistoryTruncationReducer(3),
                };

            KernelFunction terminationFunction =
    AgentGroupChat.CreatePromptFunctionForStrategy(
        $$$"""
        Determine if the reviewer has approved.  If so, respond with a single word: yes

        History:
        {{$history}}
        """,
        safeParameterNames: "history");

            // Define the termination strategy
            KernelFunctionTerminationStrategy terminationStrategy =
              new(terminationFunction, KernelCreator.CreateKernel(useAzureOpenAI))
              {
                  // Only the reviewer may give approval.
                  Agents = [reviewerAgent],
                  // Parse the function response.
                  ResultParser = (result) =>
                    result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                  // The prompt variable name for the history argument.
                  HistoryVariableName = "history",
                  // Save tokens by not including the entire history in the prompt
                  HistoryReducer = new ChatHistoryTruncationReducer(1),
                  // Limit total number of turns no matter what
                  MaximumIterations = 10,
              };

            chat = new(researcherAgent, writerAgent, reviewerAgent)
            {
                ExecutionSettings = new()
                {
                    SelectionStrategy = selectionStrategy,
                    TerminationStrategy = terminationStrategy
                }
            };
```

# Azure AI Content Safety

## Initialize scenario
```csharp
string apiKey = configuration["AzureAIContentSafety:ApiKey"];
string endpoint = configuration["AzureAIContentSafety:Endpoint"];

contentSafety = new ContentSafety(endpoint, apiKey);
```

## Execute scenario
```csharp
// Set the media type and blocklists
MediaType mediaType = MediaType.Text;
string[] blocklists = { "banned_tools" };

// Detect content safety
DetectionResult detectionResult = await contentSafety.Detect(mediaType, content.Content, blocklists);

// Set the reject thresholds for each category
Dictionary<Category, int> rejectThresholds = new Dictionary<Category, int> 
{
    { Category.Hate, 4 }, { Category.SelfHarm, 4 }, { Category.Sexual, 4 }, { Category.Violence, 4 }
};

// Make a decision based on the detection result and reject thresholds
Decision decisionResult = contentSafety.MakeDecision(detectionResult, rejectThresholds);

if (decisionResult.SuggestedAction == Action.Reject)
{
    Console.WriteLine($"The content has been rejected");
}
else
{
    Console.WriteLine();
    Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
    Console.WriteLine();
}
```