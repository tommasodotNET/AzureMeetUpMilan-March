# Azure MeetUp Milan - Semantic Kernel Agents Demo

This repository contains a demonstration project showcased at the Azure MeetUp Milan event (March 2025). It illustrates how to leverage Microsoft's Semantic Kernel and Azure AI services to build intelligent agent-based scenarios.

## Overview

The demo focuses on the following key components:

- **Semantic Kernel**: A powerful SDK from Microsoft designed to simplify the integration of AI models into applications.
- **Azure AI Services**: Utilizing Azure OpenAI and Azure Content Safety to enhance agent capabilities and ensure safe interactions.
- **Agent Group Chat**: Demonstrates how multiple AI agents can collaborate and interact within a chat-based scenario.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Subscription](https://azure.microsoft.com/free/)
- [Azure OpenAI Service](https://azure.microsoft.com/services/cognitive-services/openai-service/)
- [Azure Content Safety](https://azure.microsoft.com/services/ai-services/content-safety/)

### Installation

1. Clone the repository:

```bash
git clone https://github.com/yourusername/AzureMeetUpMilan-March.git
cd AzureMeetUpMilan-March
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Configure Azure credentials:

Create an `appsettings.json` file or use User Secrets to store your Azure credentials securely.

Example `appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "<Your Azure OpenAI Endpoint>",
    "ApiKey": "<Your Azure OpenAI API Key>"
  },
  "ContentSafety": {
    "Endpoint": "<Your Azure Content Safety Endpoint>",
    "ApiKey": "<Your Azure Content Safety API Key>"
  }
}
```

Alternatively, use User Secrets:

```bash
dotnet user-secrets set "AzureOpenAI:Endpoint" "<Your Azure OpenAI Endpoint>"
dotnet user-secrets set "AzureOpenAI:ApiKey" "<Your Azure OpenAI API Key>"
dotnet user-secrets set "ContentSafety:Endpoint" "<Your Azure Content Safety Endpoint>"
dotnet user-secrets set "ContentSafety:ApiKey" "<Your Azure Content Safety API Key>"
```

### Running the Demo

Run the application using the following command:

```bash
dotnet run
```

## Scenario: Creator-Writer Agent Interaction

The `CreatorWriterScenario` demonstrates an interaction between multiple AI agents within a group chat context. Agents collaborate to generate content based on user prompts, showcasing the capabilities of Semantic Kernel and Azure AI services.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or suggestions.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Resources

- [Semantic Kernel Documentation](https://aka.ms/semantic-kernel)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Azure Content Safety Documentation](https://learn.microsoft.com/azure/ai-services/content-safety/)