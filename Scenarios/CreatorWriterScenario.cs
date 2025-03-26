using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Liquid;
using Microsoft.SemanticKernel.Prompty;

namespace SemanticKernel.Agents.Scenarios
{
    public class CreatorWriterScenario
    {
        private AgentGroupChat chat;
        private ContentSafety contentSafety;

        public async Task InitializeScenarioAsync(bool useAzureOpenAI)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();


            //AzureEventSourceListener listener = new AzureEventSourceListener(
            //    (args, text) => Console.WriteLine(text),
            //    level: System.Diagnostics.Tracing.EventLevel.Verbose);
         
        }

        public async Task ExecuteScenario(string prompt)
        {
            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, prompt));
            await foreach (var content in chat.InvokeAsync())
            {
                Console.WriteLine();
                Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
                Console.WriteLine();
            }

            Console.WriteLine($"# IS COMPLETE: {chat.IsComplete}");
        }
    }
}
