using System;

namespace SemanticKernel.AspireAgents.GroupChatAPI;

public class ContentSafetySettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
}
