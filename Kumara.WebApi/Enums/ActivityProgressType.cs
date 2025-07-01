// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json.Serialization;

namespace Kumara.WebApi.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActivityProgressType
{
    /// <summary>
    /// Percent complete is set manually.
    /// </summary>
    Manual,

    /// <summary>
    /// Percent complete is calculated via Progress Quantity: Quantity To Date /  Quantity At Complete
    /// </summary>
    Physical,
}
