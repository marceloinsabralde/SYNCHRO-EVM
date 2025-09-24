// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;
using Kumara.WebApi.Types;

namespace Kumara.WebApi.Controllers.Requests;

public class ActivityUpdateRequest : TrackPropertySet
{
    private DateWithOptionalTime? _actualStart;
    private DateWithOptionalTime? _actualFinish;
    private decimal _percentComplete;

    public DateWithOptionalTime? ActualStart
    {
        get => _actualStart;
        set => SetProperty(ref _actualStart, value);
    }

    public DateWithOptionalTime? ActualFinish
    {
        get => _actualFinish;
        set => SetProperty(ref _actualFinish, value);
    }

    public decimal PercentComplete
    {
        get => _percentComplete;
        set => SetProperty(ref _percentComplete, value);
    }
}
