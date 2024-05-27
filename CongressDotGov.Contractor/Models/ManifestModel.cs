using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CongressDotGov.Contractor.Models
{

    public class ManifestModel
    {
        [JsonProperty("cSharpRequestModel")]
        public CSharpRequestModel CSharpRequestModel { get; set; }

        [JsonProperty("cSharpResponseDto")]
        public CSharpResponseDto CSharpResponseDto { get; set; }

        [JsonProperty("typeScriptResponseDto")]
        public TypeScriptResponseDto TypeScriptResponseDto { get; set; }
    }

    public class CSharpRequestModel
    {
        [JsonProperty("rootNamespace")]
        public string RootNamespace { get; set; }

        [JsonProperty("usingNamespaces")]
        public List<string> UsingNamespaces { get; set; }

        [JsonProperty("parameterTypes")]
        public List<ParameterType> ParameterTypes { get; set; }

        [JsonProperty("apiEndpointClass")]
        public string ApiEndpointClass { get; set; }

        [JsonProperty("classSummary")]
        public string ClassSummary { get; set; }

        [JsonProperty("requestBaseClass")]
        public List<RequestBaseClass> RequestBaseClass { get; set; }
    }

    public class CSharpResponseDto
    {
        [JsonProperty("rootNamespace")]
        public string RootNamespace { get; set; }
    }

    public class ParameterType
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class RequestBaseClass
    {
        [JsonProperty("parameters")]
        public List<string> Parameters { get; set; }

        [JsonProperty("class")]
        public string Class { get; set; }
    }

    public class TypeScriptResponseDto
    {
        [JsonProperty("ignoreEslintRules")]
        public List<string> IgnoreEslintRules { get; set; }
    }


}
