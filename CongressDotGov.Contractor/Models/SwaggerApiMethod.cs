using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CongressDotGov.Contractor.Models
{
    public record SwaggerApiMethod(string Method, string OperationId, string ParentNamespace, string Summary, List<SwaggerApiParameter> Parameters);
    public record SwaggerApiParameter(string name, string @in, string description, string required, string type, string defaultValue);
    public record SwaggerApiParameterDefault(string name, string value);
}
