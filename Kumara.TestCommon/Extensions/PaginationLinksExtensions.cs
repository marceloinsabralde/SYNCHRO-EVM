// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Controllers.Responses;

namespace Kumara.TestCommon.Extensions;

public static class PaginationLinksExtensions
{
    public const string BASE_URL = "http://localhost";

    public static void ShouldHaveLinks(
        this PaginationLinks links,
        string self,
        bool shouldHaveNext = false
    )
    {
        if (self.Contains(BASE_URL))
            links.Self.Href.ShouldBe(self);
        else
            links.Self.Href.ShouldBe(BASE_URL.Concat(self));

        if (shouldHaveNext)
        {
            links.Next.ShouldNotBeNull();
            links.Next.Href.ShouldContain("$continuationToken");
        }
        else
        {
            links.Next.ShouldBeNull();
        }
    }
}
