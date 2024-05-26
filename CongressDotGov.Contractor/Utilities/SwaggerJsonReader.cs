using CongressDotGov.Contractor.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CongressDotGov.Contractor.Utilities
{
    public static class SwaggerJsonReader
    {
        public static async Task<Dictionary<string, List<SwaggerApiMethod>>> ReadAsync(string bin)
        {
            var parameterLookup = await GetParameterLookup(bin);

            var apiMethods = new Dictionary<string, List<SwaggerApiMethod>>();

            var swaggerJson = await File.ReadAllTextAsync(Path.Combine(bin, "swagger.json"));
            var swaggerObj = JObject.Parse(swaggerJson);
            var paths = (JObject)swaggerObj["paths"];

            foreach (var path in paths)
            {
                apiMethods.Add(path.Key, []);

                var pathDetails = (JObject)path.Value;
                foreach (var method in pathDetails)
                {
                    var methodDetails = (JObject)method.Value;
                    var operationId = methodDetails["operationId"].ToString();

                    parameterLookup.TryGetValue(operationId, out var overrideParameters);
                    if (overrideParameters?.Any(o => o.name == "operationId") == true)
                    {
                        operationId = overrideParameters?.First(o => o.name == "operationId").value;
                    }

                    var summary = methodDetails["summary"].ToString();
                    var parameters = GetApiParameters(methodDetails, parameterLookup["_defaults"], overrideParameters);

                    apiMethods[path.Key].Add(new SwaggerApiMethod(method.Key, operationId, GetParentNamespace(path.Key), summary, parameters));
                }
            }

            return apiMethods;
        }

        static async Task<Dictionary<string, List<SwaggerApiParameterDefault>>> GetParameterLookup(string bin)
        {
            var swaggerParametersJson = await File.ReadAllTextAsync(Path.Combine(bin, "swagger-parameters.json"));

            var obj = JObject.Parse(swaggerParametersJson);

            var parameterLookup = obj.ToObject<Dictionary<string, Dictionary<string, string>>>()
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(kv => new SwaggerApiParameterDefault(kv.Key, kv.Value)).ToList()
                );

            return parameterLookup;
        }

        static List<SwaggerApiParameter> GetApiParameters(JObject methodDetails,
            List<SwaggerApiParameterDefault> defaultParameters, List<SwaggerApiParameterDefault> overrideParameters)
        {
            var parameterList = new List<SwaggerApiParameter>();

            var parameters = (JArray)methodDetails["parameters"];
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    parameterList.Add(new(parameter["name"].ToString(), parameter["in"].ToString(),
                        parameter["description"].ToString(), parameter["required"].ToString(),
                        parameter["type"].ToString(), GetDefaultValue(parameter["name"].ToString(), defaultParameters, overrideParameters)));
                }
            }
            return parameterList;
        }

        static string GetDefaultValue(string name, List<SwaggerApiParameterDefault> defaultParameters, List<SwaggerApiParameterDefault> overrideParameters)
        {
            var overrideParameter = overrideParameters?.FirstOrDefault(f => f.name == name);

            if (!string.IsNullOrEmpty(overrideParameter?.value))
            {
                return overrideParameter.value;
            }

            var defaultParameter = defaultParameters.Find(f => f.name == name);
            return defaultParameter?.value ?? "MISSING:" + name;
        }

        static string GetParentNamespace(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.");
            }

            // Remove the leading '/' if it exists
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            // Find the first '/' after the initial chunk
            int firstSlashIndex = path.IndexOf('/');

            if (firstSlashIndex == -1)
            {
                // If there are no more slashes, the entire string is the first chunk
                return path;
            }
            else
            {
                // Extract the first chunk
                return path.Substring(0, firstSlashIndex);
            }
        }
    }
}
