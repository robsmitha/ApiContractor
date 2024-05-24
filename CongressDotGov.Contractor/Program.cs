using CongressDotGov.Contractor.Generators;

var bin = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
var apiKey = Environment.GetEnvironmentVariable("ApiKey");
var targetNamespace = Environment.GetEnvironmentVariable("TargetNamespace");

Console.WriteLine("Starting Api Contractor [TargetNamespace: {targetNamespace}].", targetNamespace);

if (Environment.GetEnvironmentVariable("GenerateRequestModels") == "true")
{
    Console.WriteLine("Generating Api request models [TargetNamespace: {targetNamespace}].", targetNamespace);
    await new FromSwaggerJsonRequestGenerator().RunAsync(bin, targetNamespace);
}

if (Environment.GetEnvironmentVariable("GatherSampleJson") == "true")
{
    Console.WriteLine("Gathering sample json [TargetNamespace: {targetNamespace}].", targetNamespace);
    await new FromSwaggerJsonSampleJsonGenerator().RunAsync(bin, apiKey, targetNamespace);
}

if (Environment.GetEnvironmentVariable("GenerateResponseDtos") == "true")
{
    Console.WriteLine("Generating Api response DTOs from sample json [TargetNamespace: {targetNamespace}]", targetNamespace);
    await new FromSampleJsonResponseGenerator().RunAsync(bin, targetNamespace);
}

Console.WriteLine("Done");
