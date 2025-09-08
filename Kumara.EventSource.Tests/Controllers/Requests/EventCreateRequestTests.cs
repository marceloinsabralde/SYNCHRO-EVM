// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using System.Text.Json.Nodes;
using Kumara.EventSource.Controllers.Requests;
using Kumara.TestCommon.Helpers;

namespace Kumara.EventSource.Tests.Controllers.Requests;

public class EventCreateRequestTests
{
    private static JsonObject GetValidActivityCreatedV1JsonObject() =>
        new()
        {
            ["id"] = Guid.CreateVersion7(),
            ["name"] = "Test Activity",
            ["referenceCode"] = "ACT001",
        };

    private static JsonObject GetValidControlAccountCreatedV1JsonObject() =>
        new() { ["id"] = Guid.CreateVersion7(), ["name"] = "Test Control Account" };

    private static string GetDataFieldErrorMessage(string eventTypeName, string? extraErrors = null)
    {
        List<string> messageParts =
        [
            $"The Data field does not conform to the \"{eventTypeName}\" Event Type.",
        ];

        if (extraErrors is not null)
            messageParts.Add(extraErrors);

        return string.Join(" ", messageParts);
    }

    private static EventCreateRequest GetValidEventCreateRequestObject(
        string type,
        JsonObject dataJsonObject
    ) =>
        new()
        {
            ITwinId = Guid.CreateVersion7(),
            AccountId = Guid.CreateVersion7(),
            EventType = type,
            Data = JsonDocument.Parse(dataJsonObject.ToJsonString()),
        };

    [Fact]
    public void PassesValidation()
    {
        var requestObject = GetValidEventCreateRequestObject(
            type: "activity.created.v1",
            dataJsonObject: GetValidActivityCreatedV1JsonObject()
        );

        var results = ModelHelpers.ValidateModel(requestObject);
        results.ShouldBeEmpty();
    }

    [Fact]
    public void DataWithDifferentFieldNameCase_PassesValidation()
    {
        var dataJsonObject = GetValidActivityCreatedV1JsonObject();
        dataJsonObject.Remove("referenceCode");
        dataJsonObject.Add("ReferenceCode", "Different Case");

        var requestObject = GetValidEventCreateRequestObject(
            type: "activity.created.v1",
            dataJsonObject: dataJsonObject
        );

        var results = ModelHelpers.ValidateModel(requestObject);
        results.ShouldBeEmpty();
    }

    [Fact]
    public void DataAndTypeMismatch_FailsValidation()
    {
        var requestObject = GetValidEventCreateRequestObject(
            type: "activity.created.v1",
            dataJsonObject: GetValidControlAccountCreatedV1JsonObject()
        );

        var results = ModelHelpers.ValidateModel(requestObject);
        results.ShouldBe([GetDataFieldErrorMessage(requestObject.EventType)]);
    }

    [Fact]
    public void DataIsEmptyJson_FailsValidation()
    {
        var requestObject = GetValidEventCreateRequestObject(
            type: "activity.created.v1",
            dataJsonObject: new JsonObject()
        );

        var results = ModelHelpers.ValidateModel(requestObject);
        results.ShouldBe([GetDataFieldErrorMessage(requestObject.EventType)]);
    }

    [Fact]
    public void DataWithMissingRequiredField_FailsValidation()
    {
        var dataJsonObject = GetValidActivityCreatedV1JsonObject();
        dataJsonObject.Remove("name");

        var requestObject = GetValidEventCreateRequestObject(
            type: "activity.created.v1",
            dataJsonObject: dataJsonObject
        );

        var results = ModelHelpers.ValidateModel(requestObject);
        results.ShouldBe([GetDataFieldErrorMessage(requestObject.EventType)]);
    }

    [Fact]
    public void DataWithUnknownField_FailsValidation()
    {
        var dataJsonObject = GetValidActivityCreatedV1JsonObject();
        dataJsonObject.Add("unknown", "unknown");

        var requestObject = GetValidEventCreateRequestObject(
            type: "activity.created.v1",
            dataJsonObject: dataJsonObject
        );

        var results = ModelHelpers.ValidateModel(requestObject);
        results.ShouldBe([GetDataFieldErrorMessage(requestObject.EventType)]);
    }

    [Fact]
    public void DataWithFailingDataAnnotations_FailsValidation()
    {
        var dataJsonObject = GetValidActivityCreatedV1JsonObject();
        dataJsonObject["name"] = "";
        dataJsonObject["referenceCode"] = "";

        var requestObject = GetValidEventCreateRequestObject(
            type: "activity.created.v1",
            dataJsonObject: dataJsonObject
        );

        var results = ModelHelpers.ValidateModel(requestObject);
        var extraErrors = "The Name field is required. The ReferenceCode field is required.";
        results.ShouldBe([GetDataFieldErrorMessage(requestObject.EventType, extraErrors)]);
    }

    [Fact]
    public void DataWithUnknownType_SkipsValidatingData_FailsValidation()
    {
        var requestObject = GetValidEventCreateRequestObject(
            type: "unknown.type.v1",
            dataJsonObject: GetValidActivityCreatedV1JsonObject()
        );

        var results = ModelHelpers.ValidateModel(requestObject);
        results.ShouldBe([$"\"{requestObject.EventType}\" is not a valid Event Type."]);
    }
}
