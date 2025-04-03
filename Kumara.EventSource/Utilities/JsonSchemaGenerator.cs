// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using CaseConverter;
using Newtonsoft.Json;
using NJsonSchema;

namespace Kumara.EventSource.Utilities;

public static class JsonSchemaGenerator
{
    public static void GenerateJsonSchemas()
    {
        string assemblyPath = Assembly.GetExecutingAssembly().Location;
        string generatedFilePath = Path.Combine("Models", "Events", "Schemas", ".generated");

        if (
            File.Exists(generatedFilePath)
            && File.GetLastWriteTime(assemblyPath) <= File.GetLastWriteTime(generatedFilePath)
        )
        {
            return;
        }

        IEnumerable<Type> eventTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { Namespace: "Kumara.EventSource.Models.Events", IsClass: true });

        foreach (Type type in eventTypes)
        {
            string eventTypeNameInKebabCase = type.Name.ToKebabCase();
            string eventTypeNameInDottedCase = eventTypeNameInKebabCase.Replace("-", ".");
            string eventTypeNameInTitleCase = type.Name.InsertCharacterBeforeUpperCase();
            JsonSchema schema = JsonSchema.FromType(type);
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

            File.WriteAllText(schemaPath, schema.ToJson(Formatting.Indented));
        }

        File.WriteAllText(generatedFilePath, string.Empty);
    }
}
