// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel;
using Bentley.CONNECT.Licensing.SaaS.Client.Entitlement;
using Bentley.ConnectCoreLibs.Providers.Abstractions.ConnectedContextModels;
using Kumara.Common.Controllers.Responses;
using Kumara.Common.Providers;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v0/authorization")]
[ApiController]
[Produces("application/json")]
public class AuthenticationExampleController(
    IAuthorizationContext authContext,
    ItwinDataProvider itwinData
) : ControllerBase
{
    public record AuthenticationExampleResponse(
        string? Subject,
        string? iTwinId,
        string? Email,
        bool IsEmployee,
        bool IsConnectServicesAdmin,
        string EntitlementStatus,
        LicenseStatus iTwinLicenseStatus,
        string iTwinMembershipStatus,
        ITwin itwin,
        string RBACStatus,
        IEnumerable<string> rbacPermissions
    );

    [HttpGet("{iTwinId}")]
    [EndpointName("GetAuthorizationStatusForiTwin")]
    public async Task<ActionResult<ShowResponse<AuthenticationExampleResponse>>> Get(
        [Description("The iTwin project ID.")] Guid iTwinId
    )
    {
        return Ok(
            ShowResponse.For(
                new AuthenticationExampleResponse(
                    Subject: authContext.UserId,
                    iTwinId: iTwinId.ToString(),
                    Email: authContext.Email,
                    IsEmployee: authContext.HasRole("BENTLEY_EMPLOYEE"),
                    IsConnectServicesAdmin: authContext.HasRole("Project Manager"),
                    EntitlementStatus: "TODO: Unknown",
                    iTwinLicenseStatus: await itwinData.GetCurrentEntitlements(),
                    iTwinMembershipStatus: "TODO: Unknown",
                    itwin: await itwinData.GetCurrentItwin(),
                    RBACStatus: "TODO: Unknown",
                    rbacPermissions: await itwinData.GetCurrentRbacPermissions()
                )
            )
        );
    }
}
