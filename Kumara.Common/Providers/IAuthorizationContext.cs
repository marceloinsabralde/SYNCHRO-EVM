// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
namespace Kumara.Common.Providers;

public interface IAuthorizationContext
{
    string? UserId { get; }
    string? Email { get; }
    bool HasRole(string role);
}
