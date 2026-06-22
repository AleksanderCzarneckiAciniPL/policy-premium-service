using System.Text.Json.Nodes;
using PolicyPremium.Api.Validation;

namespace PolicyPremium.Api.Extensions;

/// <summary>
/// Registers the OpenAPI document for the Policy Premium API, including the metadata shown in
/// Swagger UI and a transformer that advertises <see cref="EnumNameAttribute"/> values as schema
/// enums.
/// </summary>
internal static class OpenApiServiceCollectionExtensions
{
    public static IServiceCollection AddPolicyPremiumOpenApi(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            // Document-level metadata shown at the top of Swagger UI.
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Policy Premium API";
                document.Info.Version = "v1";
                document.Info.Description =
                    "Calculates and stores insurance policy premium quotes.\n\n" +
                    "Premium = max(100, sumInsured * 0.005 * coverageMultiplier * regionMultiplier * claimsMultiplier), " +
                    "rounded to two decimal places. Quotes are held in memory only and are lost on restart.";
                return Task.CompletedTask;
            });

            // [Range], [Required] etc. flow into the schema automatically, but [EnumName] does not.
            // Project its permitted names onto the schema's `enum` so Swagger advertises them as a list.
            options.AddSchemaTransformer((schema, context, _) =>
            {
                var enumName = context.JsonPropertyInfo?.AttributeProvider?
                    .GetCustomAttributes(typeof(EnumNameAttribute), inherit: false)
                    .Cast<EnumNameAttribute>()
                    .FirstOrDefault();

                if (enumName is not null)
                {
                    schema.Enum = enumName.Names
                        .Select(name => JsonValue.Create(name) as JsonNode)
                        .ToList()!;
                }

                return Task.CompletedTask;
            });
        });
}
