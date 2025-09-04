// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Bentley.CONNECT.Licensing.SaaS.Client.Entitlement;
using Bentley.ConnectCoreLibs.Providers.Abstractions;
using Bentley.ConnectCoreLibs.Providers.Abstractions.ConnectedContextModels;
using Bentley.ConnectCoreLibs.Providers.Abstractions.Interfaces;

namespace Kumara.Common.Providers;

public class ItwinDataProvider(
    IAuthorizationContext authContext,
    IITwinProvider itwinProvider,
    IRBACiTwinProvider rbacProvider,
    IEntitlementWorkflow entitlementWorkflow
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

    public async Task<LicenseStatus> GetCurrentEntitlements()
    {
        const int gprId = 3579; // GPRID for EVM

        var entitlementContext = EntitlementContextFactory.Current.CreateContext(
            authContext.UserGuid,
            Environment.MachineName,
            authContext.ItwinGuid,
            [gprId]
        );

        var entitlementResult = await entitlementWorkflow.GetLicenseStatusAsync(entitlementContext);

        return entitlementResult.LicenseStatus;
    }
}
