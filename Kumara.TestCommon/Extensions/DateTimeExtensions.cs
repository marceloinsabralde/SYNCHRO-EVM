// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.TestCommon.Extensions;

public static class DateTimeExtensions
{
    public static DateTime SubtractDays(this DateTime dt, int days)
    {
        return dt.AddDays(days * -1);
    }

    public static DateTime SubtractMonths(this DateTime dt, int months)
    {
        return dt.AddMonths(months * -1);
    }

    public static DateTime SubtractYears(this DateTime dt, int years)
    {
        return dt.AddYears(years * -1);
    }

    public static DateOnly SubtractDays(this DateOnly d, int days)
    {
        return d.AddDays(days * -1);
    }

    public static DateOnly SubtractMonths(this DateOnly d, int months)
    {
        return d.AddMonths(months * -1);
    }

    public static DateOnly SubtractYears(this DateOnly d, int years)
    {
        return d.AddYears(years * -1);
    }
}
