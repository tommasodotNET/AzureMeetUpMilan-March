using SemanticKernel.Agents.Scenarios;

CreatorWriterScenario creatorWriterScenario = new CreatorWriterScenario();
await creatorWriterScenario.InitializeScenarioAsync(false);

Console.WriteLine("Write your topic:");
string prompt = Console.ReadLine();
await creatorWriterScenario.ExecuteScenario(prompt);