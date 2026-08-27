namespace Converty.Security.Workers;

public sealed record WorkerResourceLimits
{
    private const long Gibibyte = 1024L * 1024 * 1024;
    private const long DefaultMaximumOutputBytes = 8 * Gibibyte;
    private const long MaximumAllowedOutputBytes = 16 * Gibibyte;

    public static WorkerResourceLimits ConversionDefault { get; } = new(
        maximumActiveProcesses: 4,
        maximumProcessMemoryBytes: 2 * Gibibyte,
        maximumJobMemoryBytes: 3 * Gibibyte,
        maximumOutputBytes: DefaultMaximumOutputBytes,
        maximumCpuRatePercent: 80);

    public WorkerResourceLimits(
        uint maximumActiveProcesses,
        long maximumProcessMemoryBytes,
        long maximumJobMemoryBytes,
        uint maximumCpuRatePercent)
        : this(
            maximumActiveProcesses,
            maximumProcessMemoryBytes,
            maximumJobMemoryBytes,
            DefaultMaximumOutputBytes,
            maximumCpuRatePercent)
    {
    }

    public WorkerResourceLimits(
        uint maximumActiveProcesses,
        long maximumProcessMemoryBytes,
        long maximumJobMemoryBytes,
        long maximumOutputBytes,
        uint maximumCpuRatePercent)
    {
        ArgumentOutOfRangeException.ThrowIfZero(maximumActiveProcesses);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maximumActiveProcesses, 8u);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumProcessMemoryBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumJobMemoryBytes);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumJobMemoryBytes, maximumProcessMemoryBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumOutputBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maximumOutputBytes, MaximumAllowedOutputBytes);
        ArgumentOutOfRangeException.ThrowIfZero(maximumCpuRatePercent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maximumCpuRatePercent, 100u);

        MaximumActiveProcesses = maximumActiveProcesses;
        MaximumProcessMemoryBytes = maximumProcessMemoryBytes;
        MaximumJobMemoryBytes = maximumJobMemoryBytes;
        MaximumOutputBytes = maximumOutputBytes;
        MaximumCpuRatePercent = maximumCpuRatePercent;
    }

    public uint MaximumActiveProcesses { get; }

    public long MaximumProcessMemoryBytes { get; }

    public long MaximumJobMemoryBytes { get; }

    public long MaximumOutputBytes { get; }

    public uint MaximumCpuRatePercent { get; }
}
