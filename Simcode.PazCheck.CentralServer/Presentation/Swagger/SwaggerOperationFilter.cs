using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace Simcode.PazCheck.CentralServer.Presentation
{
    /// <summary>
    ///     For correct generaing OpenAPI doc for JSON API.
    /// </summary>
    public class SwaggerOperationFilter : IOperationFilter
    {   
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody?.Content is not null)
            {
                if (operation.RequestBody.Content.TryGetValue("application/*+json", out OpenApiMediaType? openApiMediaType))
                {
                    operation.RequestBody.Content.Clear();
                    operation.RequestBody.Content.Add("application/vnd.api+json", openApiMediaType);
                }
            }
        }
    }
}