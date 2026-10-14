using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Market.Extension
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.MethodInfo;
            var attributes = method.GetCustomAttributes(true)
                .Concat(method.DeclaringType.GetCustomAttributes(true))
                .ToArray();

            if (attributes.OfType<AllowAnonymousAttribute>().Any() || !attributes.OfType<AuthorizeAttribute>().Any())
                return;

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "MarketCookie"
                    }
                }] = System.Array.Empty<string>()
            });
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Authentication required." });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Insufficient permissions." });
        }
    }
}