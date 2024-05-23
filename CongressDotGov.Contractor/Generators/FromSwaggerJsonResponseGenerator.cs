using CongressDotGov.Contractor.Customization;
using CongressDotGov.Contractor.Utilities;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System.Text.RegularExpressions;

namespace CongressDotGov.Contractor.Generators
{
    public class FromSwaggerJsonResponseGenerator
    {
        public async Task RunAsync(string bin, string apiKey, string targetNamespace)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("ApiKey must be provided to generate response from swagger json.");
                return;
            }

            var apiMethods = await SwaggerJsonReader.ReadAsync(bin);

            var generatedFilePath = Path.Combine(bin, "___generated___");
            if (!Directory.Exists(generatedFilePath))
            {
                Directory.CreateDirectory(generatedFilePath);
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var incompleteApiMethods = new List<(string, string)>();
            foreach (var template in apiMethods.Keys)
            {
                // only get methods in this swagger file
                var method = apiMethods[template].First();
                if (!method.ParentNamespace.Equals(targetNamespace))
                {
                    continue;
                }

                var uniqueNamespace = $"{PascalCasePropertyNameGenerator.ConvertToPascalCase(method.OperationId)}Response";
                var writeFilePath = Path.Combine(generatedFilePath, method.ParentNamespace);
                if (!Directory.Exists(writeFilePath))
                {
                    Directory.CreateDirectory(writeFilePath);
                }

                var endpoint = Regex.Replace(template, @"\{[^}]+\}",
                    m => method.Parameters.FirstOrDefault(p => p.name == m.Value.Replace("{", "").Replace("}", ""))?.defaultValue ?? $"MISSING{m.Value.Replace("{", "").Replace("}", "")}");

                if (!endpoint.Contains("MISSING:"))
                {
                    var content = await httpClient.GetStringAsync("https://api.congress.gov/v3" + endpoint + "?format=json");

                    var jsonSchema = JsonSchema.FromSampleJson(content);
                    var generator = new CSharpGenerator(jsonSchema, new CSharpGeneratorSettings
                    {
                        Namespace = $"CapitolSharp.Congress.{PascalCasePropertyNameGenerator.ConvertToPascalCase(method.OperationId)}.{uniqueNamespace}",
                        RequiredPropertiesMustBeDefined = true,
                        GenerateOptionalPropertiesAsNullable = true,
                        PropertyNameGenerator = new PascalCasePropertyNameGenerator()
                    });
                    var cSharpCode = generator.GenerateFile(uniqueNamespace);

                    await File.WriteAllTextAsync(writeFilePath, cSharpCode);
                }
                else
                {
                    incompleteApiMethods.Add((method.OperationId, template));
                }
            }

            foreach (var incompleteApiMethod in incompleteApiMethods)
            {
                Console.WriteLine("Api Method is missing parameters [{method}, {path}]", incompleteApiMethod.Item1, incompleteApiMethod.Item2);
            }
        }
    }
}
