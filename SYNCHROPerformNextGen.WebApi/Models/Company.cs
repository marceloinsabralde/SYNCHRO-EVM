// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
namespace SYNCHROPerformNextGen.Models;

public class Company
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public string? Description { get; set; }
}
