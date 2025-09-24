// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Kumara.Common.Utilities;

[SwaggerSchemaFilter(typeof(ContinuationTokenSchemaPatcher))]
[OpenApiSchemaTransformer(typeof(ContinuationTokenSchemaPatcher))]
public class ContinuationToken : IParsable<ContinuationToken>
{
    public Guid Id { get; set; }

    public Dictionary<string, string> QueryParameters { get; set; } = new();

    public override string ToString()
    {
        return ToBase64String();
    }

    public string ToBase64String()
    {
        string jsonString = JsonSerializer.Serialize(this, JsonSerializerOptions.Default);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
    }

    public static ContinuationToken Parse(string s, IFormatProvider? provider)
    {
        byte[] bytes = Convert.FromBase64String(s);
        string json = Encoding.UTF8.GetString(bytes);

        ContinuationToken? result = JsonSerializer.Deserialize<ContinuationToken>(
            json,
            JsonSerializerOptions.Default
        );

        if (result is null)
            throw new ArgumentException($"Unable to parse string: {s}");

        return result;
    }

    public static ContinuationToken Parse(string s) => Parse(s, null);

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out ContinuationToken result
    )
    {
        result = null;
        if (string.IsNullOrEmpty(s))
            return false;

        try
        {
            result = Parse(s, provider);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        [MaybeNullWhen(false)] out ContinuationToken result
    ) => TryParse(s, null, out result);
}

public class ContinuationTokenSchemaPatcher : SchemaPatcher
{
    protected override void Patch(OpenApiSchema schema, Type type)
    {
        var nullable = schema.Nullable;
        Clear(schema);
        schema.Nullable = nullable;

        schema.Type = "string";
    }
}

[SwaggerSchemaFilter(typeof(ContinuationTokenSchemaPatcher))]
[OpenApiSchemaTransformer(typeof(ContinuationTokenSchemaPatcher))]
public class ContinuationToken<T>(T value) : IParsable<ContinuationToken<T>>
{
    public T Value { get; set; } = value;

    public override string ToString()
    {
        return ToBase64String();
    }

    public string ToBase64String()
    {
        string jsonString = JsonSerializer.Serialize(Value, JsonSerializerOptions.Default);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
    }

    public static ContinuationToken<T> Parse(string s, IFormatProvider? provider)
    {
        byte[] bytes = Convert.FromBase64String(s);
        string json = Encoding.UTF8.GetString(bytes);

        T? result = JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Default);

        if (result is null)
            throw new ArgumentException($"Unable to parse string: {s}");

        return new ContinuationToken<T>(result);
    }

    public static ContinuationToken<T> Parse(string s) => Parse(s, null);

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out ContinuationToken<T> result
    )
    {
        result = null;
        if (string.IsNullOrEmpty(s))
            return false;

        try
        {
            result = Parse(s, provider);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
