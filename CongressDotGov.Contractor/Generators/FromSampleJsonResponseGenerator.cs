using CongressDotGov.Contractor.Customization;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace CongressDotGov.Contractor.Generators
{
    public class FromSampleJsonResponseGenerator
    {
        public async Task RunAsync(string bin)
        {
            foreach (var directoryPath in Directory.GetDirectories(Path.Combine(bin, "sample-json")))
            {
                var directory = Path.GetFileName(directoryPath);
                var generatedFilePath = Path.Combine(bin, "_generated", directory);
                if (!Directory.Exists(generatedFilePath))
                {
                    Directory.CreateDirectory(generatedFilePath);
                }

                var parentNamespace = PascalCasePropertyNameGenerator.UpperFirst(directory);

                foreach (string file in Directory.GetFiles(directoryPath, "*.json"))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var content = await File.ReadAllTextAsync(file);

                    var uniqueNamespace = $"{PascalCasePropertyNameGenerator.ConvertToPascalCase(name)}Response";

                    var jsonSchema = JsonSchema.FromSampleJson(content);
                    var generator = new CSharpGenerator(jsonSchema, new CSharpGeneratorSettings
                    {
                        Namespace = $"CapitolSharp.Congress.{parentNamespace}.{uniqueNamespace}",
                        RequiredPropertiesMustBeDefined = true,
                        GenerateOptionalPropertiesAsNullable = true,
                        PropertyNameGenerator = new PascalCasePropertyNameGenerator()
                    });
                    var cSharpCode = generator.GenerateFile(uniqueNamespace);

                    await File.WriteAllTextAsync(Path.Combine(generatedFilePath, $"{uniqueNamespace}.g.cs"), cSharpCode);
                }
            }
        }
    }
}
