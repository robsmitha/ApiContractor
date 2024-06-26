﻿using CongressDotGov.Contractor.Customization;
using CongressDotGov.Contractor.Utilities;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CongressDotGov.Contractor.Generators
{
    public static class CSharpUnitTestGenerator
    {
        public static async Task RunAsync(string bin)
        {
            var apiMethods = await SwaggerJsonReader.ReadAsync(bin);
            var inlineData = new List<string>();
            var namespaces = new HashSet<string>();
            foreach (var template in apiMethods.Keys)
            {
                // only get methods in this swagger file
                var method = apiMethods[template].First();

                var parentNamespace = PascalCasePropertyNameGenerator.ConvertToPascalCase(method.ParentNamespace).Pluralize();
                namespaces.Add($"using CapitolSharp.Congress.{parentNamespace};");


                var requestName = $"{PascalCasePropertyNameGenerator.ConvertToPascalCase(method.OperationId)}Request";
                var resourceName = $"{method.ParentNamespace}/{method.OperationId}.json";
                inlineData.Add($"[InlineData(typeof({requestName}), \"{resourceName}\")]");
            }

            var sb = new StringBuilder();
            sb.AppendLine("//----------------------");
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     Generated using ApiContractor");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine("//----------------------");
            sb.AppendLine("using Moq;");
            sb.AppendLine("using CapitolSharp.Congress.Tests.Fixtures;");
            foreach (var @namespace in namespaces)
            {
                sb.AppendLine(@namespace);
            }
            sb.AppendLine();

            sb.AppendLine($"namespace CapitolSharp.Congress.Tests");
            sb.AppendLine("{");
            sb.AppendLine("\t[Collection(\"Congress collection\")]");
            sb.AppendLine("\tpublic class ApiClientTests(CapitolSharpFixture fixture) : IAsyncLifetime");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tpublic Task InitializeAsync() => Task.CompletedTask;");
            sb.AppendLine();

            sb.AppendLine("\t\t[Theory]");
            foreach(var inlineDataCase in inlineData)
            {
                sb.AppendLine($"\t\t{inlineDataCase}");
            }
            sb.AppendLine("\t\tpublic async Task ApiClient_SendRequest(Type requestType, string resourceName)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tdynamic request = Activator.CreateInstance(requestType);");
            sb.AppendLine("\t\t\tawait fixture.MockHttpResponseMessage(request, resourceName);");
            sb.AppendLine("\t\t\tvar response = await fixture.CapitolSharpCongress!.SendAsync(request);");
            sb.AppendLine("\t\t\tAssert.NotNull(response);");
            sb.AppendLine("\t\t}");
            sb.AppendLine();

            sb.AppendLine("\t\tpublic Task DisposeAsync()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tfixture.MockHttpHandler.Reset();");
            sb.AppendLine("\t\t\treturn Task.CompletedTask;");
            sb.AppendLine("\t\t}");

            sb.AppendLine("\t}");

            sb.AppendLine("}");


            var generatedFilePath = Path.Combine(bin, "___generated___", "tests");
            if (!Directory.Exists(generatedFilePath))
            {
                Directory.CreateDirectory(generatedFilePath);
            }
            var writeFilePath = Path.Combine(generatedFilePath, "ApiTests.cs");
            var cSharpCode = sb.ToString();
            await File.WriteAllTextAsync(writeFilePath, cSharpCode);
        }
    }
}
