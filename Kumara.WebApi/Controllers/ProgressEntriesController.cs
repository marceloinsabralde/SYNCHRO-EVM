// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Controllers.Requests;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/progress-entries")]
[ApiController]
public class ProgressEntriesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [EndpointName("CreateProgressEntry")]
    public IActionResult Create([FromBody] ProgressEntryCreateRequest progressEntryRequest)
    {
        Activity? activity = dbContext.Activities.FirstOrDefault(a =>
            a.ITwinId == progressEntryRequest.iTwinId && a.Id == progressEntryRequest.activityId
        );

        if (activity is null)
            ModelState.AddModelError(
                nameof(progressEntryRequest.activityId),
                "specified Activity could not be found."
            );

        Material? material = dbContext.Materials.FirstOrDefault(m =>
            m.ITwinId == progressEntryRequest.iTwinId && m.Id == progressEntryRequest.materialId
        );

        if (material is null)
            ModelState.AddModelError(
                nameof(progressEntryRequest.materialId),
                "specified Material could not be found."
            );

        UnitOfMeasure? quantityUom = dbContext.UnitsOfMeasure.FirstOrDefault(uom =>
            uom.ITwinId == progressEntryRequest.iTwinId
            && uom.Id == progressEntryRequest.quantityUnitOfMeasureId
        );

        if (quantityUom is null)
            ModelState.AddModelError(
                nameof(progressEntryRequest.quantityUnitOfMeasureId),
                "specified Quantity Unit of Measure could not be found."
            );

        if (ModelState.IsValid is not true)
            return BadRequest(new ValidationProblemDetails(ModelState));

        if (
            progressEntryRequest.id is not null
            && dbContext.ProgressEntries.Any(pe => pe.Id == progressEntryRequest.id)
        )
            return Conflict();

        var progressEntry = new ProgressEntry
        {
            ITwinId = progressEntryRequest.iTwinId,
            Activity = activity!,
            Material = material!,
            QuantityUnitOfMeasure = quantityUom!,
            QuantityDelta = progressEntryRequest.quantityDelta,
            ProgressDate = progressEntryRequest.progressDate,
        };

        if (progressEntryRequest.id is not null)
            progressEntry.Id = progressEntryRequest.id ?? throw new NullReferenceException();

        dbContext.ProgressEntries.Add(progressEntry);
        dbContext.SaveChanges();

        return Accepted(new CreatedResponse { Id = progressEntry.Id });
    }
}
