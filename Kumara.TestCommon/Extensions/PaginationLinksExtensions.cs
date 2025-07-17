// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Controllers.Responses;

namespace Kumara.TestCommon.Extensions;

public static class PaginationLinksExtensions
{
    public const string BASE_URL = "http://localhost";

    public static void ShouldHaveLinks(this PaginationLinks links, string self, string? next = null)
    {
        links.Self.Href.ShouldBe(BASE_URL.Concat(self));

        if (next is not null)
        {
            links.Next.ShouldNotBeNull();
            links.Next.Href.ShouldBe(BASE_URL.Concat(next));
        }
        else
        {
            links.Next.ShouldBeNull();
        }
    }
}
