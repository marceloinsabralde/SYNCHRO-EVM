// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
namespace Kumara.Models;

public class Company
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public string? Description { get; set; }
}
