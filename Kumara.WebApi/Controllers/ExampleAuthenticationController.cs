// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel;
using Kumara.Common.Controllers.Responses;
using Kumara.Common.Providers;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v0/authorization")]
[ApiController]
[Produces("application/json")]
public class AuthenticationExampleController(IAuthorizationContext authContext) : ControllerBase
{
    public record AuthenticationExampleResponse(
        string? Subject,
        string? iTwinId,
        string? Email,
        bool IsEmployee,
        bool IsConnectServicesAdmin,
        string EntitlementStatus,
        string iTwinMembershipStatus,
        string RBACStatus
    );

    [HttpGet("{iTwinId}")]
    [EndpointName("GetAuthorizationStatusForiTwin")]
    public ActionResult<ShowResponse<AuthenticationExampleResponse>> Get(
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
                    iTwinMembershipStatus: "TODO: Unknown",
                    RBACStatus: "TODO: Unknown"
                )
            )
        );
    }
}
