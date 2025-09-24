// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.RegularExpressions;
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
            Regex.Count(links.Next.Href, @"\$continuationToken").ShouldBe(1);
        }
        else
        {
            links.Next.ShouldBeNull();
        }
    }
}
