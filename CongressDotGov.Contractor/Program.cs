using CongressDotGov.Contractor.Generators;

var bin = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
var apiKey = Environment.GetEnvironmentVariable("ApiKey");
var targetNamespace = Environment.GetEnvironmentVariable("TargetNamespace");

_ = bool.TryParse(Environment.GetEnvironmentVariable("UseGeneratedJson"), out var useGeneratedJson);
Console.WriteLine($"Starting Api Contractor [TargetNamespace: {targetNamespace}].");

if (Environment.GetEnvironmentVariable("GenerateRequestModels") == "true")
{
    Console.WriteLine($"Generating C# request models [TargetNamespace: {targetNamespace}].");
    await CSharpRequestGenerator.RunAsync(bin, targetNamespace);
}

if (Environment.GetEnvironmentVariable("GatherSampleJson") == "true")
{
    Console.WriteLine($"Gathering sample json [TargetNamespace: {targetNamespace}].");
    await SampleJsonGenerator.RunAsync(bin, apiKey, targetNamespace);
}

if (Environment.GetEnvironmentVariable("GenerateResponseDtos") == "true")
{
    Console.WriteLine($"Generating C# response DTOs from sample json [TargetNamespace: {targetNamespace}, UseGeneratedJson: {useGeneratedJson}]");
    await CSharpResponseGenerator.RunAsync(bin, targetNamespace, useGeneratedJson);
}

if (Environment.GetEnvironmentVariable("GenerateTests") == "true")
{
    Console.WriteLine($"Generating C# request tests [TargetNamespace: {targetNamespace}].");
    await CSharpUnitTestGenerator.RunAsync(bin);
}

if (Environment.GetEnvironmentVariable("GenerateTypeScriptResponseDtos") == "true")
{
    Console.WriteLine($"Generating TypeScript response dtos from sample json [TargetNamespace: {targetNamespace}, UseGeneratedJson: {useGeneratedJson}]");
    await TypeScriptResponseGenerator.RunAsync(bin, targetNamespace, useGeneratedJson);
}


Console.WriteLine("Done");
