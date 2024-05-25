using CongressDotGov.Contractor.Generators;

var bin = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
var apiKey = Environment.GetEnvironmentVariable("ApiKey");
var targetNamespace = Environment.GetEnvironmentVariable("TargetNamespace");

Console.WriteLine($"Starting Api Contractor [TargetNamespace: {targetNamespace}].");

if (Environment.GetEnvironmentVariable("GenerateRequestModels") == "true")
{
    Console.WriteLine($"Generating Api request models [TargetNamespace: {targetNamespace}].");
    await new RequestGenerator().RunAsync(bin, targetNamespace);
}

if (Environment.GetEnvironmentVariable("GatherSampleJson") == "true")
{
    Console.WriteLine($"Gathering sample json [TargetNamespace: {targetNamespace}].");
    await new SampleJsonGenerator().RunAsync(bin, apiKey, targetNamespace);
}

if (Environment.GetEnvironmentVariable("GenerateResponseDtos") == "true")
{
    Console.WriteLine($"Generating Api response DTOs from sample json [TargetNamespace: {targetNamespace}]");
    await new ResponseGenerator().RunAsync(bin, targetNamespace);
}

if (Environment.GetEnvironmentVariable("GenerateTests") == "true")
{
    Console.WriteLine($"Generating Api request tests [TargetNamespace: {targetNamespace}].");
    await new UnitTestGenerator().RunAsync(bin, targetNamespace);
}

Console.WriteLine("Done");
