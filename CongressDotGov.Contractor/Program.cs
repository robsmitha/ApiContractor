using CongressDotGov.Contractor.Generators;

var bin = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

await new FromSwaggerJsonRequestGenerator().RunAsync(bin);

//await new FromSampleJsonResponseGenerator().RunAsync(bin);

//await new FromSwaggerJsonResponseGenerator().RunAsync(bin, apiKey: args[0], targetNamespace: args[1]);
