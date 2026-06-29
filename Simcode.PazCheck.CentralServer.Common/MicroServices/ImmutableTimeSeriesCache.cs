using System;
using System.Collections.Generic;

namespace Simcode.PazCheck.CentralServer.Common.MicroServices;

public readonly record struct Segment(int Offset, int Length)
{
    public Range ToRange() => new Range(Offset, Offset + Length);
}

public readonly record struct MissingRange(long FromTime_UnixTimeMilliseconds, long ToTime_UnixTimeMilliseconds)
{
    public bool IsValid => FromTime_UnixTimeMilliseconds < ToTime_UnixTimeMilliseconds;
}

public readonly record struct CacheQueryResult(
    Segment CachedDatapointsSegment,    
    MissingRange? LeftMissingRange,
    MissingRange? RightMissingRange)
{
    public bool IsEmpty => CachedDatapointsSegment.Length == 0 && LeftMissingRange is null && RightMissingRange is null;
}

/// <summary>
///     Lower time bound is always equals to first time point
/// </summary>
public sealed class ImmutableTimeSeriesCache
{
    public DateTime CreateTimeUtc { get; }

    /// <summary>
    ///     _datapoints.Count > 0, data point is { value, time }
    /// </summary>
    public List<List<decimal>> Datapoints { get; }

    public long CachedFromTimeInclusive_UnixTimeMilliseconds => (long)Datapoints[0][1];

    public long CachedToTimeInclusive_UnixTimeMilliseconds => (long)Datapoints[^1][1];

    /// <summary>
    ///     Preconditions: datapoints.Count > 0, data point is { value, time }
    /// </summary>
    /// <param name="datapoints"></param>    
    public ImmutableTimeSeriesCache(List<List<decimal>> datapoints)
    {
        CreateTimeUtc = DateTime.UtcNow;
        Datapoints = datapoints;        
    }

    /// <summary>
    ///     Preconditions: fromTime_UnixTimeMilliseconds строго меньше toTime_UnixTimeMilliseconds
    ///     Возвращает значения включительно.
    /// </summary>
    /// <param name="fromTime_UnixTimeMilliseconds"></param>
    /// <param name="toTime_UnixTimeMilliseconds"></param>
    /// <returns></returns>
    public CacheQueryResult Query(
        long fromTime_UnixTimeMilliseconds,
        long toTime_UnixTimeMilliseconds)
    {
        if (fromTime_UnixTimeMilliseconds >= toTime_UnixTimeMilliseconds)
            throw new InvalidOperationException();

        var cachedFromTimeInclusive_UnixTimeMilliseconds = CachedFromTimeInclusive_UnixTimeMilliseconds;
        var cachedToTimeInclusive_UnixTimeMilliseconds = CachedToTimeInclusive_UnixTimeMilliseconds;

        MissingRange? leftMissing = null;
        MissingRange? rightMissing = null;

        if (fromTime_UnixTimeMilliseconds < cachedFromTimeInclusive_UnixTimeMilliseconds)
        {
            if (toTime_UnixTimeMilliseconds < cachedFromTimeInclusive_UnixTimeMilliseconds)
                return default;
            leftMissing = new MissingRange(
                    fromTime_UnixTimeMilliseconds,
                    cachedFromTimeInclusive_UnixTimeMilliseconds - 1);            
        }

        if (toTime_UnixTimeMilliseconds > cachedToTimeInclusive_UnixTimeMilliseconds)
        {
            if (fromTime_UnixTimeMilliseconds > cachedToTimeInclusive_UnixTimeMilliseconds)
                return default;            
            rightMissing = new MissingRange(
                    cachedToTimeInclusive_UnixTimeMilliseconds,
                    toTime_UnixTimeMilliseconds);
        }

        int start;
        if (fromTime_UnixTimeMilliseconds <= cachedFromTimeInclusive_UnixTimeMilliseconds)
        {
            start = 0;
        }
        else
        {
            start = GetBound(fromTime_UnixTimeMilliseconds);
            if ((long)Datapoints[start][1] > fromTime_UnixTimeMilliseconds)
            {
                start -= 1;
                if (start < 0)
                    start = 0;
            }
        }

        int endExclusive;
        if (toTime_UnixTimeMilliseconds >= cachedToTimeInclusive_UnixTimeMilliseconds)
        {
            endExclusive = Datapoints.Count;
        }
        else
        {
            endExclusive = GetBound(toTime_UnixTimeMilliseconds);
            if (endExclusive < Datapoints.Count && (long)Datapoints[endExclusive][1] <= toTime_UnixTimeMilliseconds)
            {
                endExclusive += 1;
            }
        }

        return new CacheQueryResult(new Segment(
            start,
            endExclusive - start
            ), leftMissing, rightMissing);
    }

    /// <summary>
    ///     Возвращает позицию, первую, которая больше либо равна.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private int GetBound(decimal time)
    {
        int lo = 0;
        int hi = Datapoints.Count;

        while (lo < hi)
        {
            int mid = lo + ((hi - lo) >> 1);
            if (Datapoints[mid][1] < time)
                lo = mid + 1;
            else
                hi = mid;
        }

        return lo;
    }
}


//private static void ValidateStrictAscending(ReadOnlySpan<List<decimal>> points)
//{
//    for (int i = 1; i < points.Length; i++)
//    {
//        if (points[i - 1][1] >= points[i][1])
//        {
//            throw new ArgumentException(
//                "Points must be strictly ordered by UnixTimeMilliseconds without duplicates.");
//        }
//    }
//}