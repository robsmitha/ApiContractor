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
            var apiMethods = new Dictionary<string, List<SwaggerApiMethod>>();

            var swaggerJson = await File.ReadAllTextAsync(Path.Combine(bin, "congressdotgov-swagger.json"));
            var swaggerParametersJson = await File.ReadAllTextAsync(Path.Combine(bin, "congressdotgov-swagger-overrides.json"));

            var swaggerParametersObj = JObject.Parse(swaggerParametersJson);
            var overrideLookup = swaggerParametersObj.ToObject<Dictionary<string, Dictionary<string, string>>>()
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(kv => new SwaggerApiParameterDefault(kv.Key, kv.Value)).ToList()
                );


            var swaggerObj = JObject.Parse(swaggerJson);
            var paths = (JObject)swaggerObj["paths"];
            foreach (var path in paths)
            {
                apiMethods.Add(path.Key, []);

                var pathDetails = (JObject)path.Value;
                foreach (var method in pathDetails)
                {
                    var parameterList = new List<SwaggerApiParameter>();

                    var methodDetails = (JObject)method.Value;
                    var operationId = methodDetails["operationId"].ToString();
                    var summary = methodDetails["summary"].ToString();
                    overrideLookup.TryGetValue(operationId, out var overrideOperation);
                    if (overrideOperation?.Any(o => o.name == "operationId") == true)
                    {
                        operationId = overrideOperation?.First(o => o.name == "operationId").value;
                    }

                    var parameters = (JArray)methodDetails["parameters"];
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            parameterList.Add(new(parameter["name"].ToString(), parameter["in"].ToString(),
                                parameter["description"].ToString(), parameter["required"].ToString(),
                                parameter["type"].ToString(), GetDefaultValue(parameter["name"].ToString(), overrideOperation)));
                        }
                    }

                    apiMethods[path.Key].Add(new SwaggerApiMethod(method.Key, operationId, GetParentNamespace(path.Key), summary, parameterList));
                }
            }
            return apiMethods;
        }
        static string GetDefaultValue(string name, List<SwaggerApiParameterDefault> defaultParameters)
        {
            var defaultParameter = defaultParameters?.FirstOrDefault(f => f.name == name);

            if (!string.IsNullOrEmpty(defaultParameter?.value))
            {
                return defaultParameter.value;
            }

            if (name == "congress")
            {
                return "117";
            }

            if (name == "billType")
            {
                return "hr";
            }

            if (name == "billNumber")
            {
                return "3076";
            }

            if (name == "bioguideId")
            {
                return "H001038";
            }

            if (name == "stateCode")
            {
                return "MI";
            }

            if (name == "district")
            {
                return "10";
            }

            if (name == "chamber")
            {
                return "house";
            }

            if (name == "committeeCode")
            {
                return "hspw00";
            }

            if (name == "reportType")
            {
                return "hrpt";
            }

            if (name == "reportNumber")
            {
                return "617";
            }

            if (name == "jacketNumber")
            {
                return "48144";
            }

            if (name == "eventId")
            {
                return "115538";
            }

            if (name == "volumeNumber")
            {
                return "166";
            }

            if (name == "issueNumber")
            {
                return "153";
            }

            if (name == "year")
            {
                return "1990";
            }

            if (name == "month")
            {
                return "4";
            }

            if (name == "day")
            {
                return "18";
            }

            if (name == "communicationType")
            {
                return "ec";
            }

            if (name == "communicationNumber")
            {
                return "3324";
            }

            if (name == "requirementNumber")
            {
                return "8070";
            }

            if (name == "nominationNumber")
            {
                return "2467";
            }

            if (name == "ordinal")
            {
                return "1";
            }

            if (name == "treatyNumber")
            {
                return "13";
            }

            if (name == "treatySuffix")
            {
                return "A";
            }

            return "MISSING:" + name;
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
