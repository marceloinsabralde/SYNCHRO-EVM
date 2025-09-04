// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
namespace Kumara.Common.Providers;

public interface IAuthorizationContext
{
    string? UserId { get; }
    string? Email { get; }
    string? ItwinId { get; }

    bool HasRole(string role);

    System.Guid UserGuid
    {
        get => Guid.TryParse(UserId, out var g) ? g : throw new ArgumentNullException("UserId");
    }
    System.Guid ItwinGuid
    {
        get => Guid.TryParse(ItwinId, out var g) ? g : throw new ArgumentNullException("ItwinId");
    }
}
