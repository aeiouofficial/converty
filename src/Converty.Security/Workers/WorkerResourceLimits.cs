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
        if (maximumActiveProcesses is 0 or > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumActiveProcesses));
        }
        if (maximumProcessMemoryBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumProcessMemoryBytes));
        }
        if (maximumJobMemoryBytes <= 0 || maximumJobMemoryBytes < maximumProcessMemoryBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumJobMemoryBytes));
        }
        if (maximumCpuRatePercent is 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumCpuRatePercent));
        }

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
