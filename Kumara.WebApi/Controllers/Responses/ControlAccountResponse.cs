// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Models;
using NodaTime;

namespace Kumara.WebApi.Controllers.Responses;

public class ControlAccountResponse
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }

    public Guid TaskId { get; set; }

    public required string ReferenceCode { get; set; }

    public required string Name { get; set; }

    public DateOnly? ActualStart { get; set; }

    public DateOnly? ActualFinish { get; set; }

    public DateOnly? PlannedStart { get; set; }

    public DateOnly? PlannedFinish { get; set; }

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
            ActualStart = controlAccount.ActualStart,
            ActualFinish = controlAccount.ActualFinish,
            PlannedStart = controlAccount.PlannedStart,
            PlannedFinish = controlAccount.PlannedFinish,
            CreatedAt = controlAccount.CreatedAt,
            UpdatedAt = controlAccount.UpdatedAt,
        };
    }
}
