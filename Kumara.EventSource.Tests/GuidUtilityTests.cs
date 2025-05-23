// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Utilities;

namespace Kumara.Tests.EventSource;

[TestClass]
public class GuidUtilityTests
{
    [TestMethod]
    public void CreateGuid_UsesDefaultTimeProvider_GeneratesValidGuid()
    {
        Guid result = GuidUtility.CreateGuid();
        Assert.AreNotEqual(Guid.Empty, result);
        Assert.IsTrue(result.ToString().Length == 36);
    }

    [TestMethod]
    public void CreateGuid_UsesCustomTimeProvider_GeneratesExpectedGuid()
    {
        DateTimeOffset fixedTime = new(2025, 4, 30, 0, 0, 0, TimeSpan.Zero);
        FixedTimeProvider fixedTimeProvider = new(fixedTime);

        Guid result = GuidUtility.CreateGuid(fixedTimeProvider);

        Assert.AreNotEqual(Guid.Empty, result);
        Assert.IsTrue(result.ToString().Length == 36);

        string guidString = result.ToString();
        Assert.AreEqual('7', guidString[14]);

        char variantChar = guidString[19];
        Assert.IsTrue(
            variantChar == '8'
                || variantChar == '9'
                || variantChar == 'a'
                || variantChar == 'b'
                || variantChar == 'A'
                || variantChar == 'B',
            $"Expected variant to be 8, 9, A, or B, but got {variantChar}"
        );

        byte[] guidBytes = result.ToByteArray();
        byte[] uuidBytes = new byte[16];

        uuidBytes[0] = guidBytes[3]; // time_low
        uuidBytes[1] = guidBytes[2];
        uuidBytes[2] = guidBytes[1];
        uuidBytes[3] = guidBytes[0];

        uuidBytes[4] = guidBytes[5]; // time_mid
        uuidBytes[5] = guidBytes[4];

        uuidBytes[6] = guidBytes[7]; // time_hi_and_version
        uuidBytes[7] = guidBytes[6];

        Array.Copy(guidBytes, 8, uuidBytes, 8, 8);

        ulong timestampMs = 0;
        timestampMs |= (ulong)uuidBytes[0] << 40;
        timestampMs |= (ulong)uuidBytes[1] << 32;
        timestampMs |= (ulong)uuidBytes[2] << 24;
        timestampMs |= (ulong)uuidBytes[3] << 16;
        timestampMs |= (ulong)uuidBytes[4] << 8;
        timestampMs |= (ulong)uuidBytes[5];

        ulong expectedTimestampMs = (ulong)fixedTime.ToUnixTimeMilliseconds();

        Assert.IsTrue(
            Math.Abs((long)timestampMs - (long)expectedTimestampMs) < 2,
            $"Expected timestamp close to {expectedTimestampMs}, but got {timestampMs}"
        );
    }

    [TestMethod]
    public void CreateGuid_WithMonotonicTimeProvider_GeneratesAscendingGuids()
    {
        MonotonicTimeProvider monotonicTimeProvider = new();
        Guid[] guids = new Guid[10];

        for (int i = 0; i < guids.Length; i++)
        {
            guids[i] = GuidUtility.CreateGuid(monotonicTimeProvider);
        }

        for (int i = 1; i < guids.Length; i++)
        {
            Assert.IsTrue(
                string.Compare(
                    guids[i - 1].ToString(),
                    guids[i].ToString(),
                    StringComparison.Ordinal
                ) < 0,
                $"GUID at index {i - 1} ({guids[i - 1]}) is not less than GUID at index {i} ({guids[i]})."
            );
        }
    }

    [TestMethod]
    public void CreateGuid_GeneratesUniqueGuids()
    {
        Guid guid1 = GuidUtility.CreateGuid();
        Guid guid2 = GuidUtility.CreateGuid();

        Assert.AreNotEqual(guid1, guid2);
    }

    private class MonotonicTimeProvider : TimeProvider
    {
        private DateTimeOffset _currentTime = new(2025, 4, 30, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow()
        {
            _currentTime = _currentTime.AddMilliseconds(1);
            return _currentTime;
        }
    }

    private class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _fixedTime;

        public FixedTimeProvider(DateTimeOffset fixedTime)
        {
            _fixedTime = fixedTime;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _fixedTime;
        }
    }
}
