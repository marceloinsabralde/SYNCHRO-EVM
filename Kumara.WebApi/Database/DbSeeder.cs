// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Models;

namespace Kumara.Database;

public static class DbSeeder
{
    public static void SeedDevelopmentData(ApplicationDbContext dbContext)
    {
        List<Guid> iTwinIds =
        [
            new Guid("10000000-0000-0000-0000-000000000000"),
            new Guid("20000000-0000-0000-0000-000000000000"),
        ];

        if (!dbContext.Companies.Any())
        {
            dbContext.Companies.Add(new Company { Name = "Big Lifters" });
            dbContext.SaveChanges();
        }

        if (!dbContext.ControlAccounts.Any())
        {
            ControlAccount[] controlAccounts =
            [
                new ControlAccount
                {
                    ITwinId = iTwinIds[0],
                    ReferenceCode = "CIV001",
                    Name = "Clear and Grub",
                },
                new ControlAccount
                {
                    ITwinId = iTwinIds[0],
                    ReferenceCode = "CIV002",
                    Name = "Excavation & Backfill",
                    PlannedStart = new DateOnly(2025, 3, 23),
                    ActualStart = new DateOnly(2025, 3, 25),
                },
                new ControlAccount
                {
                    ITwinId = iTwinIds[1],
                    ReferenceCode = "OTH001",
                    Name = "Staff",
                },
            ];
            dbContext.ControlAccounts.AddRange(controlAccounts);
            dbContext.SaveChanges();
        }

        if (!dbContext.Activities.Any())
        {
            Activity[] activities =
            [
                new Activity
                {
                    ITwinId = iTwinIds[0],
                    ControlAccount = dbContext.ControlAccounts.First(controlAccount =>
                        controlAccount.ITwinId == iTwinIds[0]
                        && controlAccount.ReferenceCode == "CIV001"
                    ),
                    ReferenceCode = "CIV001-A1",
                    Name = "Activity 1",
                    PlannedStart = new DateOnly(2023, 1, 1),
                    PlannedFinish = new DateOnly(2024, 12, 1),
                    ActualStart = new DateOnly(2023, 1, 1),
                    ActualFinish = new DateOnly(2025, 1, 1),
                },
                new Activity
                {
                    ITwinId = iTwinIds[0],
                    ControlAccount = dbContext.ControlAccounts.First(controlAccount =>
                        controlAccount.ITwinId == iTwinIds[0]
                        && controlAccount.ReferenceCode == "CIV001"
                    ),
                    ReferenceCode = "CIV001-A2",
                    Name = "Activity 2",
                },
                new Activity
                {
                    ITwinId = iTwinIds[0],
                    ControlAccount = dbContext.ControlAccounts.First(controlAccount =>
                        controlAccount.ITwinId == iTwinIds[0]
                        && controlAccount.ReferenceCode == "CIV002"
                    ),
                    ReferenceCode = "CIV002-A1",
                    Name = "Activity 1",
                },
                new Activity
                {
                    ITwinId = iTwinIds[1],
                    ControlAccount = dbContext.ControlAccounts.First(controlAccount =>
                        controlAccount.ITwinId == iTwinIds[1]
                        && controlAccount.ReferenceCode == "OTH001"
                    ),
                    ReferenceCode = "OTH001-A1",
                    Name = "Activity 1",
                },
            ];
            dbContext.Activities.AddRange(activities);
            dbContext.SaveChanges();
        }

        if (!dbContext.UnitsOfMeasure.Any())
        {
            UnitOfMeasure[] unitsOfMeasure =
            [
                new UnitOfMeasure
                {
                    ITwinId = iTwinIds[0],
                    Name = "Hours",
                    Symbol = "h",
                },
                new UnitOfMeasure
                {
                    ITwinId = iTwinIds[0],
                    Name = "Meters",
                    Symbol = "m",
                },
                new UnitOfMeasure
                {
                    ITwinId = iTwinIds[0],
                    Name = "Tonnes",
                    Symbol = "t",
                },
                new UnitOfMeasure
                {
                    ITwinId = iTwinIds[0],
                    Name = "Litres",
                    Symbol = "L",
                },
                new UnitOfMeasure
                {
                    ITwinId = iTwinIds[1],
                    Name = "Hours",
                    Symbol = "h",
                },
                new UnitOfMeasure
                {
                    ITwinId = iTwinIds[1],
                    Name = "Kilograms",
                    Symbol = "kg",
                },
            ];
            dbContext.UnitsOfMeasure.AddRange(unitsOfMeasure);
            dbContext.SaveChanges();
        }

        if (!dbContext.Materials.Any())
        {
            Material[] materials =
            [
                new Material
                {
                    Name = "Aggregate 10mm",
                    ITwinId = iTwinIds[0],
                    ResourceRoleId = Guid.CreateVersion7(),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[0] && uom.Name == "Tonnes"
                    ),
                },
                new Material
                {
                    Name = "Coarse Sand",
                    ITwinId = iTwinIds[1],
                    ResourceRoleId = Guid.CreateVersion7(),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[1] && uom.Name == "Kilograms"
                    ),
                },
            ];
            dbContext.Materials.AddRange(materials);
            dbContext.SaveChanges();
        }

        if (!dbContext.MaterialActivityAllocations.Any())
        {
            MaterialActivityAllocation[] materialActivityAllocations =
            [
                new MaterialActivityAllocation
                {
                    ITwinId = iTwinIds[0],
                    Material = dbContext.Materials.First(material =>
                        material.ITwinId == iTwinIds[0] && material.Name == "Aggregate 10mm"
                    ),
                    Activity = dbContext.Activities.First(activity =>
                        activity.ITwinId == iTwinIds[0] && activity.ReferenceCode == "CIV001-A1"
                    ),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[0] && uom.Name == "Tonnes"
                    ),
                    QuantityAtComplete = 418.02m,
                },
                new MaterialActivityAllocation
                {
                    ITwinId = iTwinIds[0],
                    Material = dbContext.Materials.First(material =>
                        material.ITwinId == iTwinIds[0] && material.Name == "Aggregate 10mm"
                    ),
                    Activity = dbContext.Activities.First(activity =>
                        activity.ITwinId == iTwinIds[0] && activity.ReferenceCode == "CIV001-A2"
                    ),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[0] && uom.Name == "Tonnes"
                    ),
                    QuantityAtComplete = 823.92m,
                },
                new MaterialActivityAllocation
                {
                    ITwinId = iTwinIds[1],
                    Material = dbContext.Materials.First(material =>
                        material.ITwinId == iTwinIds[1] && material.Name == "Coarse Sand"
                    ),
                    Activity = dbContext.Activities.First(activity =>
                        activity.ITwinId == iTwinIds[1] && activity.ReferenceCode == "OTH001-A1"
                    ),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[1] && uom.Name == "Kilograms"
                    ),
                    QuantityAtComplete = 3300m,
                },
            ];
            dbContext.MaterialActivityAllocations.AddRange(materialActivityAllocations);
            dbContext.SaveChanges();
        }

        if (!dbContext.ProgressEntries.Any())
        {
            ProgressEntry[] progressEntries =
            [
                new ProgressEntry
                {
                    Activity = dbContext.Activities.First(activity =>
                        activity.ITwinId == iTwinIds[0] && activity.ReferenceCode == "CIV001-A1"
                    ),
                    ITwinId = iTwinIds[0],
                    Material = dbContext.Materials.First(material =>
                        material.ITwinId == iTwinIds[0] && material.Name == "Aggregate 10mm"
                    ),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[0] && uom.Name == "Tonnes"
                    ),
                    ProgressDate = new(2025, 03, 01),
                    QuantityDelta = 1m,
                },
                new ProgressEntry
                {
                    Activity = dbContext.Activities.First(activity =>
                        activity.ITwinId == iTwinIds[0] && activity.ReferenceCode == "CIV001-A1"
                    ),
                    ITwinId = iTwinIds[0],
                    Material = dbContext.Materials.First(material =>
                        material.ITwinId == iTwinIds[0] && material.Name == "Aggregate 10mm"
                    ),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[0] && uom.Name == "Tonnes"
                    ),
                    ProgressDate = new(2025, 03, 02),
                    QuantityDelta = 2m,
                },
                new ProgressEntry
                {
                    Activity = dbContext.Activities.First(activity =>
                        activity.ITwinId == iTwinIds[0] && activity.ReferenceCode == "CIV001-A1"
                    ),
                    ITwinId = iTwinIds[0],
                    Material = dbContext.Materials.First(material =>
                        material.ITwinId == iTwinIds[0] && material.Name == "Aggregate 10mm"
                    ),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[0] && uom.Name == "Tonnes"
                    ),
                    ProgressDate = new(2025, 03, 03),
                    QuantityDelta = 3m,
                },
                new ProgressEntry
                {
                    Activity = dbContext.Activities.First(activity =>
                        activity.ITwinId == iTwinIds[0] && activity.ReferenceCode == "CIV001-A2"
                    ),
                    ITwinId = iTwinIds[0],
                    Material = dbContext.Materials.First(material =>
                        material.ITwinId == iTwinIds[0] && material.Name == "Aggregate 10mm"
                    ),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[0] && uom.Name == "Tonnes"
                    ),
                    ProgressDate = new(2025, 03, 04),
                    QuantityDelta = 4m,
                },
                new ProgressEntry
                {
                    Activity = dbContext.Activities.First(activity =>
                        activity.ITwinId == iTwinIds[1] && activity.ReferenceCode == "OTH001-A1"
                    ),
                    ITwinId = iTwinIds[1],
                    Material = dbContext.Materials.First(material =>
                        material.ITwinId == iTwinIds[1] && material.Name == "Coarse Sand"
                    ),
                    QuantityUnitOfMeasure = dbContext.UnitsOfMeasure.First(uom =>
                        uom.ITwinId == iTwinIds[1] && uom.Name == "Kilograms"
                    ),
                    ProgressDate = new(2025, 03, 05),
                    QuantityDelta = 5m,
                },
            ];
            dbContext.ProgressEntries.AddRange(progressEntries);
            dbContext.SaveChanges();
        }
    }
}
