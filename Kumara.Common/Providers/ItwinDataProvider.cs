// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Bentley.ConnectCoreLibs.Providers.Abstractions;
using Bentley.ConnectCoreLibs.Providers.Abstractions.ConnectedContextModels;
using Bentley.ConnectCoreLibs.Providers.Abstractions.Interfaces;

namespace Kumara.Common.Providers;

public class ItwinDataProvider(
    IAuthorizationContext authContext,
    IITwinProvider itwinProvider,
    IRBACiTwinProvider rbacProvider
)
{
    public async Task<ITwin> GetCurrentItwin()
    {
        var response = await itwinProvider.GetAsync(authContext.ItwinGuid);
        return response.iTwin;
    }

    public async Task<IEnumerable<string>> GetCurrentRbacPermissions()
    {
        var response = await rbacProvider.GetiTwinPermissionsByUserIdAsync(
            authContext.ItwinGuid,
            authContext.UserGuid
        );
        return response.Permissions;
    }
}
