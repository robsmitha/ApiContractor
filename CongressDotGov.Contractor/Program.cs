using CongressDotGov.Contractor.Generators;

var bin = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
var apiKey = Environment.GetEnvironmentVariable("ApiKey");
var targetNamespace = Environment.GetEnvironmentVariable("TargetNamespace");

_ = bool.TryParse(Environment.GetEnvironmentVariable("UseGeneratedJson"), out var useGeneratedJson);
Console.WriteLine($"Starting Api Contractor [TargetNamespace: {targetNamespace}].");

if (bool.TryParse(Environment.GetEnvironmentVariable("CSharpRequestGenerator"), out var cSharpRequestGenerator) && cSharpRequestGenerator)
{
    Console.WriteLine($"Generating C# request models [TargetNamespace: {targetNamespace}].");
    await CSharpRequestGenerator.RunAsync(bin, targetNamespace);
}

if (bool.TryParse(Environment.GetEnvironmentVariable("SampleJsonGenerator"), out var sampleJsonGenerator) && sampleJsonGenerator)
{
    Console.WriteLine($"Gathering sample json [TargetNamespace: {targetNamespace}].");
    await SampleJsonGenerator.RunAsync(bin, apiKey, targetNamespace);
}

if (bool.TryParse(Environment.GetEnvironmentVariable("CSharpResponseGenerator"), out var cSharpResponseGenerator) && cSharpResponseGenerator)
{
    Console.WriteLine($"Generating C# response DTOs from sample json [TargetNamespace: {targetNamespace}, UseGeneratedJson: {useGeneratedJson}]");
    await CSharpResponseGenerator.RunAsync(bin, targetNamespace, useGeneratedJson);
}

if (bool.TryParse(Environment.GetEnvironmentVariable("CSharpUnitTestGenerator"), out var cSharpUnitTestGenerator) && cSharpUnitTestGenerator)
{
    Console.WriteLine($"Generating C# request tests [TargetNamespace: {targetNamespace}].");
    await CSharpUnitTestGenerator.RunAsync(bin);
}

if (bool.TryParse(Environment.GetEnvironmentVariable("TypeScriptResponseGenerator"), out var typeScriptResponseGenerator) && typeScriptResponseGenerator)
{
    Console.WriteLine($"Generating TypeScript response dtos from sample json [TargetNamespace: {targetNamespace}, UseGeneratedJson: {useGeneratedJson}]");
    await TypeScriptResponseGenerator.RunAsync(bin, targetNamespace, useGeneratedJson);
}


Console.WriteLine("Done");
