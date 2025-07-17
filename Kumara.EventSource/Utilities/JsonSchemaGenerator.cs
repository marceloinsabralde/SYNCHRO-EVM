// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using CaseConverter;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.NewtonsoftJson.Generation;

namespace Kumara.EventSource.Utilities;

public static class JsonSchemaGenerator
{
    public static void GenerateJsonSchemas()
    {
        // Configure schema generation with camelCase naming
        NewtonsoftJsonSchemaGeneratorSettings settings = new()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            },
        };

        IEnumerable<Type> eventTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { Namespace: "Kumara.EventSource.Models.Events", IsClass: true });

        foreach (Type type in eventTypes)
        {
            string eventTypeNameInKebabCase = type.Name.ToKebabCase();
            string eventTypeNameInDottedCase = eventTypeNameInKebabCase.Replace("-", ".");
            string eventTypeNameInTitleCase = type.Name.InsertCharacterBeforeUpperCase();

            // Generate the schema with camelCase settings
            JsonSchema schema = JsonSchema.FromType(type, settings);

            // Create absolute path for schema files
            string schemaPath = Path.Combine(
                "Models",
                "Events",
                "Schemas",
                $"{eventTypeNameInDottedCase}.schema.json"
            );

            schema.Title = $"{eventTypeNameInTitleCase} Schema";
            schema.Description =
                $"Schema for the data payload of a {eventTypeNameInTitleCase}. "
                + $"Use `{eventTypeNameInDottedCase}` as the `type` attribute.";
            schema.Id =
                $"https://schemas.bentley.com/construction/events/{eventTypeNameInDottedCase}.schema.json";

            // Write the schema to file with custom JsonSerializerSettings
            JsonSerializerSettings serializerSettings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
            };

            // Create directory if it doesn't exist
            string? directory = Path.GetDirectoryName(schemaPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var existingJson = File.Exists(schemaPath) ? File.ReadAllText(schemaPath) : null;
            var newJson = JsonConvert.SerializeObject(schema, serializerSettings);

            if (existingJson != newJson)
            {
                File.WriteAllText(
                    schemaPath,
                    JsonConvert.SerializeObject(schema, serializerSettings)
                );
            }
        }
    }
}
