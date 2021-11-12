// This file isn't generated, but this comment is necessary to exclude it from StyleCop analysis.
// <auto-generated/>

using Xunit;

namespace HdrHistogram.UnitTests.Recording
{
    
    internal sealed class RecorderTestWithIntConcurrentHistogram : RecorderTestsBase
    {
        internal override HistogramBase CreateHistogram(long id, long min, long max, int sf)
        {
            //return new IntConcurrentHistogram(id, min, max, sf);
            return HistogramFactory.With32BitBucketSize()
                .WithValuesFrom(min)
                .WithValuesUpTo(max)
                .WithPrecisionOf(sf)
                .WithThreadSafeWrites()
                .Create();
        }

        internal override Recorder Create(long min, long max, int sf)
        {
            return HistogramFactory.With32BitBucketSize()
                .WithValuesFrom(min)
                .WithValuesUpTo(max)
                .WithPrecisionOf(sf)
                .WithThreadSafeWrites()
                .WithThreadSafeReads()
                .Create();
        }
    }
}