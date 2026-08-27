namespace Converty.Security.Workers;

public sealed record WorkerResourceLimits
{
    private const long Gibibyte = 1024L * 1024 * 1024;

    public static WorkerResourceLimits ConversionDefault { get; } = new(
        maximumActiveProcesses: 4,
        maximumProcessMemoryBytes: 2 * Gibibyte,
        maximumJobMemoryBytes: 3 * Gibibyte,
        maximumCpuRatePercent: 80);

    public WorkerResourceLimits(
        uint maximumActiveProcesses,
        long maximumProcessMemoryBytes,
        long maximumJobMemoryBytes,
        uint maximumCpuRatePercent)
    {
        ArgumentOutOfRangeException.ThrowIfZero(maximumActiveProcesses);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maximumActiveProcesses, 8u);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumProcessMemoryBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumJobMemoryBytes);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumJobMemoryBytes, maximumProcessMemoryBytes);
        ArgumentOutOfRangeException.ThrowIfZero(maximumCpuRatePercent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maximumCpuRatePercent, 100u);

        MaximumActiveProcesses = maximumActiveProcesses;
        MaximumProcessMemoryBytes = maximumProcessMemoryBytes;
        MaximumJobMemoryBytes = maximumJobMemoryBytes;
        MaximumCpuRatePercent = maximumCpuRatePercent;
    }

    public uint MaximumActiveProcesses { get; }

    public long MaximumProcessMemoryBytes { get; }

    public long MaximumJobMemoryBytes { get; }

    public uint MaximumCpuRatePercent { get; }
}
