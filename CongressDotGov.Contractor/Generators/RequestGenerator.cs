﻿using CongressDotGov.Contractor.Customization;
using CongressDotGov.Contractor.Models;
using CongressDotGov.Contractor.Utilities;
using Humanizer;
using NJsonSchema;
using System.IO;
using System.Text;

namespace CongressDotGov.Contractor.Generators
{
    public static class RequestGenerator
    {
        public static async Task RunAsync(string bin, string targetNamespace)
        {
            var apiMethods = await SwaggerJsonReader.ReadAsync(bin);

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
                sb.AppendLine("//     Generated using ApiContractor");
                sb.AppendLine("// </auto-generated>");
                sb.AppendLine("//----------------------");
                sb.AppendLine("using System;");
                sb.AppendLine("using CapitolSharp.Congress.Enums;");
                sb.AppendLine("using CapitolSharp.Congress.Models;");
                sb.AppendLine();
                sb.AppendLine($"namespace CapitolSharp.Congress.{parentNamespace}");
                sb.AppendLine("{");
                sb.AppendLine($"\t/// <summary>");
                sb.AppendLine($"\t/// {method.Summary}");
                sb.AppendLine($"\t/// </summary>");
                sb.AppendLine($"\t{RequestClass(method)}");
                sb.AppendLine("\t{");

                var pathParameters = method.Parameters.Where(p => p.@in == "path").ToList();
                foreach (var parameter in pathParameters)
                {
                    sb.AppendLine($"\t\t/// <summary>");
                    sb.AppendLine($"\t\t/// {parameter.description}");
                    sb.AppendLine($"\t\t/// </summary>");
                    sb.AppendLine($"\t\t{ApiParameter(parameter)}");
                    sb.AppendLine();
                }

                sb.AppendLine($"\t\t{CongressApiEndpoint(template, pathParameters)}");

                sb.AppendLine("\t}");
                sb.AppendLine("}");

                var requestNamespace = $"{PascalCasePropertyNameGenerator.ConvertToPascalCase(method.OperationId)}Request";
                var writeFilePath = Path.Combine(requestFilePath, $"{requestNamespace}.g.cs");
                var cSharpCode = sb.ToString();
                await File.WriteAllTextAsync(writeFilePath, cSharpCode);
            }
        }

        static string RequestClass(SwaggerApiMethod method)
        {
            var responseNamespace = PascalCasePropertyNameGenerator.ConvertToPascalCase(method.OperationId);
            var baseClass = $"JsonFormatApiRequest<{responseNamespace}.{responseNamespace}Response>";
            var queryParameters = method.Parameters.Where(p => p.@in == "query").ToList();
            if (queryParameters.Any(p => p.name == "sort"))
            {
                baseClass = $"SortableApiRequest<{responseNamespace}.{responseNamespace}Response>";
            }
            else if (queryParameters.Any(p => p.name == "fromDateTime" || p.name == "toDateTime"))
            {
                baseClass = $"DateRangeApiRequest<{responseNamespace}.{responseNamespace}Response>";
            }
            else if (queryParameters.Any(p => p.name == "offset" || p.name == "limit"))
            {
                baseClass = $"PagedApiRequest<{responseNamespace}.{responseNamespace}Response>";
            }
            return $"public class {PascalCasePropertyNameGenerator.ConvertToPascalCase(method.OperationId)}Request : {baseClass}";
        }


        static string ApiParameter(SwaggerApiParameter parameter)
        {
            string cSharpType;
            if (parameter.name == "billType")
            {
                cSharpType = "BillType";
            }
            else if (parameter.name == "chamber")
            {
                cSharpType = "Chamber";
            }
            else if (parameter.name == "amendmentType")
            {
                cSharpType = "AmendmentType";
            }
            else if (parameter.name == "communicationType")
            {
                cSharpType = "CommunicationType";
            }
            else if (parameter.name == "reportType")
            {
                cSharpType = "ReportType";
            }
            else if (parameter.name == "stateCode")
            {
                cSharpType = "StateCode";
            }
            else
            {
                cSharpType = parameter.type switch
                {
                    "integer" => "int",
                    _ => parameter.type,
                };
            }

            return $"public {cSharpType} {PascalCasePropertyNameGenerator.ConvertToPascalCase(parameter.name)} {{ get; set; }}";
        }


        static string CongressApiEndpoint(string template, List<SwaggerApiParameter> pathParams)
        {
            if (!pathParams.Any())
            {
                return $"public override CongressApiEndpoint Endpoint => new(\"{template}\");";
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

            return $"public override CongressApiEndpoint Endpoint => new(\"{replacedTemplate}\", {string.Join(", ", pathParameterOrdered)});";
        }
    }
}
