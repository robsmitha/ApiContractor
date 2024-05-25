using CongressDotGov.Contractor.Customization;
using CongressDotGov.Contractor.Utilities;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CongressDotGov.Contractor.Generators
{
    public class SampleJsonGenerator
    {
        public async Task RunAsync(string bin, string apiKey, string targetNamespace)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("ApiKey must be provided to generate response from swagger json.");
                return;
            }

            var apiMethods = await SwaggerJsonReader.ReadAsync(bin);

            var generatedFilePath = Path.Combine(bin, "___generated___", "sample-json");
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
                if (targetNamespace != "*" && !method.ParentNamespace.Equals(targetNamespace))
                {
                    continue;
                }

                var writeFilePath = Path.Combine(generatedFilePath, method.ParentNamespace);
                if (!Directory.Exists(writeFilePath))
                {
                    Directory.CreateDirectory(writeFilePath);
                }

                var endpoint = Regex.Replace(template, @"\{[^}]+\}",
                    m => method.Parameters.FirstOrDefault(p => p.name == m.Value.Replace("{", "").Replace("}", ""))?.defaultValue ?? $"MISSING{m.Value.Replace("{", "").Replace("}", "")}");

                if (!endpoint.Contains("MISSING:"))
                {
                    try
                    {
                        var json = await httpClient.GetStringAsync("https://api.congress.gov/v3" + endpoint + "?format=json");
                        var responseNamespace = method.OperationId;
                        var parsedJson = JsonConvert.DeserializeObject(json);
                        await File.WriteAllTextAsync(Path.Combine(writeFilePath, $"{responseNamespace}.json"),
                            JsonConvert.SerializeObject(parsedJson, Formatting.Indented));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error calling api [{endpoint}, {e.Message}]");
                    }
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
