using CongressDotGov.Contractor.Customization;
using Humanizer;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.CodeGeneration.TypeScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CongressDotGov.Contractor.Generators
{
    public static class TypeScriptResponseGenerator
    {
        public static async Task RunAsync(string bin, string targetNamespace, bool useGeneratedSampleJson = true)
        {
            foreach (var directoryPath in Directory.GetDirectories(useGeneratedSampleJson ? Path.Combine(bin, "___generated___", "sample-json") : Path.Combine(bin, "sample-json")))
            {
                var directory = Path.GetFileName(directoryPath);
                if (targetNamespace != "*" && !directory.Equals(targetNamespace, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var parentNamespace = PascalCasePropertyNameGenerator.ConvertToPascalCase(directory).Pluralize();
                var generatedFilePath = Path.Combine(bin, "___generated___", "typescript", "responses", parentNamespace);
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
                    var generator = new TypeScriptGenerator(jsonSchema);
                    var responseName = $"{uniqueNamespace}Response";
                    var cSharpCode = generator.GenerateFile(responseName);

                    await File.WriteAllTextAsync(Path.Combine(generatedFilePath, $"{responseName}.g.ts"), cSharpCode);
                }
            }

        }
    }
}
