// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bentley.ConnectCoreLibs.User;

namespace Kumara.TestCommon.Utilities;

public static class StubTokenGenerator
{
    // Dynamic Generic version of `RandomPerson() with { FirstName = "Jim" }`
    static T RecordWith<T>(this T original, object? overrides)
        where T : class
    {
        if (overrides is null)
            return original;

        var clone = original; // shallow copy
        foreach (var prop in overrides.GetType().GetProperties())
        {
            var targetProp = typeof(T).GetProperty(prop.Name);
            if (targetProp != null && targetProp.CanWrite)
            {
                targetProp.SetValue(clone, prop.GetValue(overrides));
            }
        }
        return clone;
    }

    record Person(
        Guid UserId,
        string Email,
        string FirstName,
        string LastName,
        Guid OrganizationId,
        string[] Roles
    )
    {
        public IEnumerable<Claim> AsClaims()
        {
            return new[]
            {
                new Claim(OIDCUserDefaults.UserIdClaimKeyDefault, this.UserId.ToString()),
                new Claim(OIDCUserDefaults.EmailClaimKey, this.Email),
                new Claim(OIDCUserDefaults.FirstNameClaimKey, this.FirstName),
                new Claim(OIDCUserDefaults.LastNameClaimKey, this.LastName),
                new Claim(
                    OIDCUserDefaults.UltimateReferenceIdClaimKey,
                    this.OrganizationId.ToString()
                ),
            }.Concat(this.Roles.Select(r => new Claim(OIDCUserDefaults.RoleClaimKey, r)));
        }
    };

    static Person RandomPerson()
    {
        var person = new Bogus.Person();

        return new StubTokenGenerator.Person(
            UserId: Guid.NewGuid(),
            OrganizationId: Guid.NewGuid(),
            Email: person.Email,
            FirstName: person.FirstName,
            LastName: person.LastName,
            Roles: []
        );
    }

    const int DefaultTokenExpiryHours = 1;

    static string GetToken(Person person)
    {
        return new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(
                claims: person.AsClaims(),
                expires: DateTime.UtcNow.AddHours(DefaultTokenExpiryHours)
            )
        );
    }

    public static string GenerateToken(object? overrides = null)
    {
        var person = RecordWith(RandomPerson(), overrides);
        return GetToken(person);
    }
}
