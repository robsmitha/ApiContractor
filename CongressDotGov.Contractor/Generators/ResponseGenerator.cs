using CongressDotGov.Contractor.Customization;
using Humanizer;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace CongressDotGov.Contractor.Generators
{
    public class ResponseGenerator
    {
        public async Task RunAsync(string bin, string targetNamespace)
        {
            foreach (var directoryPath in Directory.GetDirectories(Path.Combine(bin, "___generated___", "sample-json")))
            {
                var directory = Path.GetFileName(directoryPath);
                if (targetNamespace != "*" && !directory.Equals(targetNamespace, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var parentNamespace = PascalCasePropertyNameGenerator.ConvertToPascalCase(directory).Pluralize();
                var generatedFilePath = Path.Combine(bin, "___generated___", "responses", parentNamespace);
                if (!Directory.Exists(generatedFilePath))
                {
                    Directory.CreateDirectory(generatedFilePath);
                }

                foreach (string file in Directory.GetFiles(directoryPath, "*.json"))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var content = await File.ReadAllTextAsync(file);

                    var uniqueNamespace = PascalCasePropertyNameGenerator.ConvertToPascalCase(name);

                    var jsonSchema = JsonSchema.FromSampleJson(content);
                    var generator = new CSharpGenerator(jsonSchema, new CSharpGeneratorSettings
                    {
                        Namespace = $"CapitolSharp.Congress.{parentNamespace}.{uniqueNamespace}",
                        RequiredPropertiesMustBeDefined = true,
                        GenerateOptionalPropertiesAsNullable = true,
                        PropertyNameGenerator = new PascalCasePropertyNameGenerator()
                    });
                    var responseName = $"{uniqueNamespace}Response";
                    var cSharpCode = generator.GenerateFile(responseName);

                    await File.WriteAllTextAsync(Path.Combine(generatedFilePath, $"{responseName}.g.cs"), cSharpCode);
                }
            }
        }
    }
}
