using CongressDotGov.Contractor.Generators;

var bin = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
var apiKey = Environment.GetEnvironmentVariable("ApiKey");
var targetNamespace = Environment.GetEnvironmentVariable("TargetNamespace");

Console.WriteLine($"Starting Api Contractor [TargetNamespace: {targetNamespace}].");

if (Environment.GetEnvironmentVariable("GenerateRequestModels") == "true")
{
    Console.WriteLine($"Generating Api request models [TargetNamespace: {targetNamespace}].");
    await RequestGenerator.RunAsync(bin, targetNamespace);
}

if (Environment.GetEnvironmentVariable("GatherSampleJson") == "true")
{
    Console.WriteLine($"Gathering sample json [TargetNamespace: {targetNamespace}].");
    await SampleJsonGenerator.RunAsync(bin, apiKey, targetNamespace);
}

if (Environment.GetEnvironmentVariable("GenerateResponseDtos") == "true")
{
    _ = bool.TryParse(Environment.GetEnvironmentVariable("UseGeneratedJson"), out var useGeneratedJson);
    Console.WriteLine($"Generating Api response DTOs from sample json [TargetNamespace: {targetNamespace}, UseGeneratedJson: {useGeneratedJson}]");
    await ResponseGenerator.RunAsync(bin, targetNamespace, useGeneratedJson);
}

if (Environment.GetEnvironmentVariable("GenerateTests") == "true")
{
    Console.WriteLine($"Generating Api request tests [TargetNamespace: {targetNamespace}].");
    await UnitTestGenerator.RunAsync(bin);
}

Console.WriteLine("Done");
