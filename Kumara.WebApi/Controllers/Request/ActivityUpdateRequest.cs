// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Utilities;

namespace Kumara.WebApi.Controllers.Requests;

public class ActivityUpdateRequest : TrackPropertySet
{
    private DateTimeOffset? _actualStart;
    private DateTimeOffset? _actualFinish;

    public DateTimeOffset? ActualStart
    {
        get => _actualStart;
        set => SetProperty(ref _actualStart, value);
    }

    public DateTimeOffset? ActualFinish
    {
        get => _actualFinish;
        set => SetProperty(ref _actualFinish, value);
    }
}
