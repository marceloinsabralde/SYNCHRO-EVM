// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Database;
using Kumara.TestCommon.Extensions;
using Kumara.WebApi.Models;
using Kumara.WebApi.Types;

namespace Kumara.WebApi.Tests.Models;

public class SettingTests : DatabaseTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanRoundTripSupportedValueTypes(object testValue)
    {
        _dbContext.Settings.Add(
            new()
            {
                ITwinId = Guid.CreateVersion7(),
                Key = SettingKey.ActualsHaveTime,
                Value = testValue,
            }
        );
        _dbContext.SaveChanges();

        var setting = _dbContext.Settings.First();
        setting.Value.ShouldBe(testValue);
    }

    [Fact]
    public void FailsValidationSavingUnsupportedValueType()
    {
        _dbContext.Settings.Add(
            new()
            {
                ITwinId = Guid.CreateVersion7(),
                Key = SettingKey.ActualsHaveTime,
                Value = 42,
            }
        );

        var ex = Should.Throw<ValidationException>(() =>
        {
            _dbContext.SaveChanges();
        });
        ex.Message.ShouldBe("Number types are not supported");
    }

    [Fact]
    public void ErrorsReadingUnsupportedValueType()
    {
        _dbContext.Settings.Add(
            new()
            {
                ITwinId = Guid.CreateVersion7(),
                Key = SettingKey.ActualsHaveTime,
                Value = 42,
            }
        );

        _dbContext.SaveChangesWithoutValidation();
        var setting = _dbContext.Settings.First();

        var ex = Should.Throw<InvalidOperationException>(() =>
        {
            var _ = setting.Value;
        });
        ex.Message.ShouldBe("Number types are not supported");
    }
}
