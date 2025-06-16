// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text;
using System.Text.Json;

namespace Kumara.Common.Utilities;

public class ContinuationToken
{
    public Guid Id { get; set; }

    public Dictionary<string, string> QueryParameters { get; set; } = new();

    public string ToBase64String()
    {
        string jsonString = JsonSerializer.Serialize(this, JsonSerializerOptions.Default);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
    }

    public static ContinuationToken? Parse(string input)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(input);
            string json = Encoding.UTF8.GetString(bytes);

            ContinuationToken? result = JsonSerializer.Deserialize<ContinuationToken>(
                json,
                JsonSerializerOptions.Default
            );

            return result;
        }
        catch
        {
            return null;
        }
    }
}
