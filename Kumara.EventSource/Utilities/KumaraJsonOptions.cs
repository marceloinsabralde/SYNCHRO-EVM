// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kumara.EventSource.Utilities;

/// <summary>
/// Provides default JSON serializer options to ensure consistent serialization across the application.
/// </summary>
public static class KumaraJsonOptions
{
    /// <summary>
    /// Gets the default JSON serializer options with camelCase property naming.
    /// </summary>
    public static JsonSerializerOptions DefaultOptions =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };

    /// <summary>
    /// Gets the default JSON serializer options with camelCase property naming and indented output.
    /// </summary>
    public static JsonSerializerOptions DefaultIndentedOptions =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
        };
}
