// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.IdentityModel.Tokens.Jwt;
using Kumara.TestCommon.Utilities;

namespace Kumara.Common.Tests.Utilities;

static class TestExtensions
{
    public static string GetClaim(this JwtSecurityToken token, string key)
    {
        return token.Claims.First(c => c.Type == key).Value;
    }

    public static IEnumerable<string> GetClaims(this JwtSecurityToken token, string key)
    {
        return token.Claims.Where(c => c.Type == key).Select(c => c.Value);
    }
}

public class StubTokenGeneratorTests
{
    [Fact]
    public void TokenHasBentleyClaims()
    {
        string token = StubTokenGenerator.GenerateToken();
        var jwt = ParseToken(token);
        Assert.NotNull(jwt.Subject);
        Assert.NotNull(jwt.GetClaim("given_name"));
        Assert.NotNull(jwt.GetClaim("family_name"));
        Assert.NotNull(jwt.GetClaim("email"));
        Assert.NotNull(jwt.GetClaim("org"));
    }

    [Fact]
    public void TokenCanSpecifySubject()
    {
        var expectedUserId = Guid.NewGuid();
        string token = StubTokenGenerator.GenerateToken(new { UserId = expectedUserId });
        var jwt = ParseToken(token);

        Assert.Equal(expectedUserId.ToString(), jwt.Subject);
    }

    [Fact]
    public void TokenCanSpecifyClaims()
    {
        var expectedOrganizationId = Guid.NewGuid();
        IEnumerable<string> expectedRoles = new[] { "Member", "CONNECTServicesAdmin" };
        string token = StubTokenGenerator.GenerateToken(
            new
            {
                OrganizationId = expectedOrganizationId,
                FirstName = "Dwight",
                LastName = "Schrute",
                Email = "dwight@scranton.dunder-mifflin.com",
                Roles = expectedRoles,
            }
        );
        var jwt = ParseToken(token);

        Assert.Equal("Dwight", jwt.GetClaim("given_name"));
        Assert.Equal("Schrute", jwt.GetClaim("family_name"));
        Assert.Equal("dwight@scranton.dunder-mifflin.com", jwt.GetClaim("email"));
        Assert.Equal(expectedOrganizationId.ToString(), jwt.GetClaim("org"));
        Assert.Equal(expectedRoles, jwt.GetClaims("role"));
    }

    private JwtSecurityToken ParseToken(string jwt)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(jwt);
    }
}
