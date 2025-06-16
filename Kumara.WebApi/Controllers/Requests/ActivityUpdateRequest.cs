// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;
using Kumara.WebApi.Types;

namespace Kumara.WebApi.Controllers.Requests;

public class ActivityUpdateRequest : TrackPropertySet
{
    private DateWithOptionalTime? _actualStart;
    private DateWithOptionalTime? _actualFinish;

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
}
