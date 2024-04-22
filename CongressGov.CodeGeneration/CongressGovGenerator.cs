using CongressGov.CodeGeneration.Customization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System;
using System.IO;
using System.Text;

namespace CongressGov.CodeGeneration
{
    [Generator]
    public class CongressGovGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            IncrementalValuesProvider<AdditionalText> jsonFiles = initContext.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".json"));

            IncrementalValuesProvider<(string name, string @namespace, string content)> namesAndContents = jsonFiles.Select((file, cancellationToken) 
                => (name: Path.GetFileNameWithoutExtension(file.Path),
                @namespace: new DirectoryInfo(file.Path.Replace("\\\\", "\\")).Parent.Name, 
                content: file.GetText(cancellationToken)!.ToString()));
            
            initContext.RegisterSourceOutput(namesAndContents, (spc, args) =>
            {
                try
                {
                    var jsonSchema = JsonSchema.FromSampleJson(args.content);

                    var generator = new CSharpGenerator(jsonSchema, new CSharpGeneratorSettings
                    {
                        Namespace = $"CapitolSharp.Congress.{args.@namespace}.{args.name}",
                        RequiredPropertiesMustBeDefined = true,
                        GenerateOptionalPropertiesAsNullable = true,
                        PropertyNameGenerator = new PascalCasePropertyNameGenerator()
                    });
                    var cSharpFile = generator.GenerateFile($"{args.name}Response");
                    spc.AddSource($"{args.name}Contract.g.cs", SourceText.From(cSharpFile, Encoding.UTF8));
                }
                catch (Exception e)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(e.Message);
                    sb.AppendLine(e.StackTrace);
                    spc.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "CAPITOL0001",
                                    $"Unexpected error generating {args.name} contract",
                                    sb.ToString(),
                                    nameof(CongressGovGenerator),
                                    DiagnosticSeverity.Error,
                                    isEnabledByDefault: true),
                                Location.None));
                }
            });
        }
    }
}
