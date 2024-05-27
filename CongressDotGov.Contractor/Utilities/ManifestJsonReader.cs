using CongressDotGov.Contractor.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CongressDotGov.Contractor.Utilities
{
    public static class ManifestJsonReader
    {
        public static async Task<ManifestModel> ReadAsync(string bin)
        {
            var manifestJson = await File.ReadAllTextAsync(Path.Combine(bin, "manifest.json"));
            return JsonConvert.DeserializeObject<ManifestModel>(manifestJson);
        }
    }
}
