using Converty.Security.Workers;

namespace Converty.Security.Tests.Workers;

public sealed class WorkerResourceLimitsTests
{
    [Fact]
    public void ConversionDefaultUsesFiniteBoundedCeilings()
    {
        WorkerResourceLimits limits = WorkerResourceLimits.ConversionDefault;

        Assert.InRange(limits.MaximumActiveProcesses, 1u, 8u);
        Assert.InRange(limits.MaximumProcessMemoryBytes, 256L * 1024 * 1024, 4L * 1024 * 1024 * 1024);
        Assert.InRange(limits.MaximumJobMemoryBytes, limits.MaximumProcessMemoryBytes, 6L * 1024 * 1024 * 1024);
        Assert.InRange(limits.MaximumCpuRatePercent, 1u, 100u);
    }

    [Theory]
    [InlineData(0u, 1024L, 2048L, 50u)]
    [InlineData(1u, 0L, 2048L, 50u)]
    [InlineData(1u, 2048L, 0L, 50u)]
    [InlineData(1u, 4096L, 2048L, 50u)]
    [InlineData(1u, 1024L, 2048L, 0u)]
    [InlineData(1u, 1024L, 2048L, 101u)]
    public void ConstructorRejectsNonFiniteOrIncoherentCeilings(
        uint maximumActiveProcesses,
        long maximumProcessMemoryBytes,
        long maximumJobMemoryBytes,
        uint maximumCpuRatePercent)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WorkerResourceLimits(
            maximumActiveProcesses,
            maximumProcessMemoryBytes,
            maximumJobMemoryBytes,
            maximumCpuRatePercent));
    }

    [Fact]
    public void IsolationLevelKeepsStrictAndCompatibilityDistinct()
    {
        Assert.NotEqual(WorkerIsolationLevel.Strict, WorkerIsolationLevel.Compatibility);
    }
}
