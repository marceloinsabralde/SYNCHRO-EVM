// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bentley.ConnectCoreLibs.Providers.Abstractions.Interfaces;

namespace Kumara.Common.Providers;

public interface IITwinPathProvider
{
    public Task<IList<Guid>> GetPathFromRootAsync(Guid iTwinId);
}

public class ITwinPathProvider(IITwinProvider iTwinProvider) : IITwinPathProvider
{
    public async Task<IList<Guid>> GetPathFromRootAsync(Guid iTwinId)
    {
        var path = new List<Guid>();
        var nextId = iTwinId;

        while (true)
        {
            if (path.Contains(nextId))
            {
                var pathString = string.Join(" -> ", path) + $" -> {nextId}";
                throw new InvalidOperationException(
                    $"Cycle detected in iTwin hierarchy: {string.Join(" -> ", path)} -> {nextId}"
                );
            }

            var response = await iTwinProvider.GetAsync(
                nextId,
                new() { Select = "id,parentId,iTwinAccountId" }
            );
            var iTwin = response.iTwin;

            if (response.Error is not null)
            {
                throw new InvalidOperationException(
                    $"Error retrieving iTwin {nextId}: {response.Error.Code}: {response.Error.Message}"
                );
            }

            if (iTwin is null || iTwin.Id is null)
            {
                throw new InvalidOperationException(
                    $"Error retrieving iTwin {nextId}: iTwin is null"
                );
            }

            path.Add(iTwin.Id.Value);

            if (iTwin.ParentId is null)
            {
                break;
            }

            if (iTwin.ParentId == iTwin.iTwinAccountId)
            {
                path.Add(iTwin.ParentId.Value);
                break;
            }

            nextId = iTwin.ParentId.Value;
        }

        path.Reverse();

        return path;
    }
}
