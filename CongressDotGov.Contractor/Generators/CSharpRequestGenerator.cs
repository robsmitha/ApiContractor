﻿using CongressDotGov.Contractor.Customization;
using CongressDotGov.Contractor.Models;
using CongressDotGov.Contractor.Utilities;
using Humanizer;
using NJsonSchema;
using System.IO;
using System.Text;

namespace CongressDotGov.Contractor.Generators
{
    public static class CSharpRequestGenerator
    {
        public static async Task RunAsync(string bin, string targetNamespace)
        {
            var apiMethods = await SwaggerJsonReader.ReadAsync(bin);
            var manifest = await ManifestJsonReader.ReadAsync(bin);
            var cSharpRequestModel = manifest.CSharpRequestModel;
            
            var generatedFilePath = Path.Combine(bin, "___generated___", "requests");
            if (!Directory.Exists(generatedFilePath))
            {
                Directory.CreateDirectory(generatedFilePath);
            }

            foreach (var template in apiMethods.Keys)
            {
                // only get methods in this swagger file
                var method = apiMethods[template].First();
                if (targetNamespace != "*" && !method.ParentNamespace.Equals(targetNamespace))
                {
                    continue;
                }

                var parentNamespace = PascalCasePropertyNameGenerator.ConvertToPascalCase(method.ParentNamespace).Pluralize();
                var requestFilePath = Path.Combine(generatedFilePath, parentNamespace);
                if (!Directory.Exists(requestFilePath))
                {
                    Directory.CreateDirectory(requestFilePath);
                }

                var sb = new StringBuilder();
                sb.AppendLine("//----------------------");
                sb.AppendLine("// <auto-generated>");
                sb.AppendLine($"//     {cSharpRequestModel.ClassSummary}");
                sb.AppendLine("// </auto-generated>");
                sb.AppendLine("//----------------------");
                sb.AppendLine("using System;");
                foreach(var directive in cSharpRequestModel.UsingNamespaces)
                {
                    sb.AppendLine($"using {directive};");
                }
                sb.AppendLine();
                sb.AppendLine($"namespace {cSharpRequestModel.RootNamespace}.{parentNamespace}");
                sb.AppendLine("{");
                sb.AppendLine($"\t/// <summary>");
                sb.AppendLine($"\t/// {method.Summary}");
                sb.AppendLine($"\t/// </summary>");
                sb.AppendLine($"\t{RequestClass(method, cSharpRequestModel)}");
                sb.AppendLine("\t{");

                var pathParameters = method.Parameters.Where(p => p.@in == "path").ToList();
                foreach (var parameter in pathParameters)
                {
                    sb.AppendLine($"\t\t/// <summary>");
                    sb.AppendLine($"\t\t/// {parameter.description}");
                    sb.AppendLine($"\t\t/// </summary>");
                    sb.AppendLine($"\t\t{ApiParameter(parameter, cSharpRequestModel)}");
                    sb.AppendLine();
                }

                sb.AppendLine($"\t\t{ApiEndpoint(template, pathParameters, cSharpRequestModel)}");

                sb.AppendLine("\t}");
                sb.AppendLine("}");

                var requestNamespace = $"{PascalCasePropertyNameGenerator.ConvertToPascalCase(method.OperationId)}Request";
                var writeFilePath = Path.Combine(requestFilePath, $"{requestNamespace}.g.cs");
                var cSharpCode = sb.ToString();
                await File.WriteAllTextAsync(writeFilePath, cSharpCode);
            }
        }

        static string RequestClass(SwaggerApiMethod method, CSharpRequestModel cSharpRequestModel)
        {
            var responseNamespace = PascalCasePropertyNameGenerator.ConvertToPascalCase(method.OperationId);

            var defaultBaseClass = cSharpRequestModel.RequestBaseClass.First(c => c.Parameters?.Count == 0);

            var baseClass = $"{defaultBaseClass.Class}<{responseNamespace}.{responseNamespace}Response>";

            var queryParameters = method.Parameters.Where(p => p.@in == "query").ToList();
            foreach (var requestBase in cSharpRequestModel.RequestBaseClass)
            {
                if (requestBase.Parameters.Count != 0 && queryParameters.Any(p => requestBase.Parameters.Contains(p.name)))
                {
                    baseClass = $"{requestBase.Class}<{responseNamespace}.{responseNamespace}Response>";
                    break;
                }
            }
            return $"public class {PascalCasePropertyNameGenerator.ConvertToPascalCase(method.OperationId)}Request : {baseClass}";
        }


        static string ApiParameter(SwaggerApiParameter parameter, CSharpRequestModel cSharpRequestModel)
        {
            var param = cSharpRequestModel.ParameterTypes.FirstOrDefault(t => t.Name.Equals(parameter.name));
            var cSharpType = param?.Type;

            if (string.IsNullOrEmpty(cSharpType))
            {
                cSharpType = parameter.type switch
                {
                    "integer" => "int",
                    _ => parameter.type,
                };
            }

            return $"public {cSharpType} {PascalCasePropertyNameGenerator.ConvertToPascalCase(parameter.name)} {{ get; set; }}";
        }


        static string ApiEndpoint(string template, List<SwaggerApiParameter> pathParams, CSharpRequestModel cSharpRequestModel)
        {
            if (!pathParams.Any())
            {
                return $"public override {cSharpRequestModel.ApiEndpointClass} Endpoint => new(\"{template}\");";
            }
            // Check if the number of parameters matches the expected placeholders in the template
            int expectedParams = template.Split('{').Length - 1;
            if (pathParams.Count != expectedParams)
            {
                throw new ArgumentException("The number of parameters provided does not match the number of placeholders in the template.");
            }

            var sb = new StringBuilder();
            sb.Append(template);

            var pathParameterOrdered = new List<string>();
            for (int i = 0; i < pathParams.Count; i++)
            {
                sb.Replace(pathParams[i].name, i.ToString());
                pathParameterOrdered.Add(PascalCasePropertyNameGenerator.ConvertToPascalCase(pathParams[i].name));
            }

            var replacedTemplate = sb.ToString();

            return $"public override {cSharpRequestModel.ApiEndpointClass} Endpoint => new(\"{replacedTemplate}\", {string.Join(", ", pathParameterOrdered)});";
        }
    }
}
