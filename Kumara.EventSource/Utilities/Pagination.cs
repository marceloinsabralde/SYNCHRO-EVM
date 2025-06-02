// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text;
using System.Text.Json;

namespace Kumara.EventSource.Utilities;

public static class Pagination
{
    private static readonly JsonSerializerOptions SerializerOptions =
        KumaraJsonOptions.DefaultOptions;

    public static string CreateContinuationToken(
        Guid id,
        Dictionary<string, string>? queryParameters = null
    )
    {
        ContinuationToken tokenData = new()
        {
            Id = id,
            QueryParameters = queryParameters ?? new Dictionary<string, string>(),
        };

        string json = JsonSerializer.Serialize(tokenData, SerializerOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    public static ContinuationToken? ParseContinuationToken(string token)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(token);
            string json = Encoding.UTF8.GetString(bytes);

            ContinuationToken? result = JsonSerializer.Deserialize<ContinuationToken>(
                json,
                SerializerOptions
            );

            if (result != null && result.QueryParameters == null)
            {
                result.QueryParameters = new Dictionary<string, string>();
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    public class ContinuationToken
    {
        public Guid Id { get; set; }

        public Dictionary<string, string> QueryParameters { get; set; } = new();
    }
}
