// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Models;
using Kumara.WebApi.Types;
using NodaTime;

namespace Kumara.WebApi.Controllers.Responses;

public class ControlAccountResponse
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }

    public Guid TaskId { get; set; }

    public required string ReferenceCode { get; set; }

    public required string Name { get; set; }

    public decimal PercentComplete { get; set; }

    public DateWithOptionalTime? ActualStart { get; set; }

    public DateWithOptionalTime? ActualFinish { get; set; }

    public DateWithOptionalTime? PlannedStart { get; set; }

    public DateWithOptionalTime? PlannedFinish { get; set; }

    public Instant CreatedAt { get; set; }

    public Instant UpdatedAt { get; set; }

    public static ControlAccountResponse FromControlAccount(ControlAccount controlAccount)
    {
        return new ControlAccountResponse
        {
            Id = controlAccount.Id,
            ITwinId = controlAccount.ITwinId,
            TaskId = controlAccount.TaskId,
            ReferenceCode = controlAccount.ReferenceCode,
            Name = controlAccount.Name,
            PercentComplete = controlAccount.PercentComplete,
            ActualStart = controlAccount.ActualStart,
            ActualFinish = controlAccount.ActualFinish,
            PlannedStart = controlAccount.PlannedStart,
            PlannedFinish = controlAccount.PlannedFinish,
            CreatedAt = controlAccount.CreatedAt,
            UpdatedAt = controlAccount.UpdatedAt,
        };
    }
}
