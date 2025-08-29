// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Options;

public class OpenIdConnectOptions
{
    public required string ApiName { get; set; }
    public required string ApiSecret { get; set; }
    public required string Authority { get; set; }
    public required string GeneralScope { get; set; }
    public required string DelegationClientId { get; set; }
    public required string DelegationClientSecret { get; set; }
    public required string[] ValidIssuers { get; set; }
}
