using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Converty.Security.Workers;

internal sealed class WindowsJobObject : IDisposable
{
    internal const uint JOB_OBJECT_LIMIT_ACTIVE_PROCESS = 0x00000008;
    internal const uint JOB_OBJECT_LIMIT_PROCESS_MEMORY = 0x00000100;
    internal const uint JOB_OBJECT_LIMIT_JOB_MEMORY = 0x00000200;
    internal const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000;
    internal const uint JOB_OBJECT_CPU_RATE_CONTROL_ENABLE = 0x00000001;
    internal const uint JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP = 0x00000004;

    private const int JobObjectExtendedLimitInformationClass = 9;
    private const int JOBOBJECT_CPU_RATE_CONTROL_INFORMATION_CLASS = 15;

    private SafeKernelHandle? _handle;

    private WindowsJobObject(SafeKernelHandle handle)
    {
        _handle = handle;
    }

    internal static WindowsJobObject Create(WorkerResourceLimits limits)
    {
        ArgumentNullException.ThrowIfNull(limits);

        nint rawHandle = WindowsNativeMethods.CreateJobObject(nint.Zero, nint.Zero);
        if (rawHandle == nint.Zero)
        {
            throw LastError("Converty could not create the worker Job Object.");
        }

        var handle = new SafeKernelHandle(rawHandle, ownsHandle: true);
        try
        {
            ConfigureExtendedLimits(handle, limits);
            ConfigureCpuLimit(handle, limits.MaximumCpuRatePercent);
            return new WindowsJobObject(handle);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    internal void AssignProcess(SafeKernelHandle processHandle)
    {
        ArgumentNullException.ThrowIfNull(processHandle);
        SafeKernelHandle handle = GetHandle();
        if (!WindowsNativeMethods.AssignProcessToJobObject(
                handle.DangerousGetHandle(),
                processHandle.DangerousGetHandle()))
        {
            throw LastError("Converty could not assign the suspended worker to its Job Object.");
        }
    }

    internal void Terminate(uint exitCode)
    {
        SafeKernelHandle handle = GetHandle();
        if (!WindowsNativeMethods.TerminateJobObject(handle.DangerousGetHandle(), exitCode))
        {
            int error = Marshal.GetLastPInvokeError();
            if (error != 0)
            {
                throw new Win32Exception(error, "Converty could not terminate the worker Job Object.");
            }
        }
    }

    public void Dispose()
    {
        _handle?.Dispose();
        _handle = null;
    }

    private SafeKernelHandle GetHandle() =>
        _handle ?? throw new ObjectDisposedException(nameof(WindowsJobObject));

    private static void ConfigureExtendedLimits(SafeKernelHandle handle, WorkerResourceLimits limits)
    {
        var information = new JobObjectExtendedLimitInformation
        {
            BasicLimitInformation = new JobObjectBasicLimitInformation
            {
                LimitFlags =
                    JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE |
                    JOB_OBJECT_LIMIT_ACTIVE_PROCESS |
                    JOB_OBJECT_LIMIT_PROCESS_MEMORY |
                    JOB_OBJECT_LIMIT_JOB_MEMORY,
                ActiveProcessLimit = limits.MaximumActiveProcesses,
            },
            ProcessMemoryLimit = checked((nuint)limits.MaximumProcessMemoryBytes),
            JobMemoryLimit = checked((nuint)limits.MaximumJobMemoryBytes),
        };

        SetInformation(handle, JobObjectExtendedLimitInformationClass, information);
    }

    private static void ConfigureCpuLimit(SafeKernelHandle handle, uint maximumCpuRatePercent)
    {
        var information = new JobObjectCpuRateControlInformation
        {
            ControlFlags = JOB_OBJECT_CPU_RATE_CONTROL_ENABLE | JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP,
            CpuRate = checked(maximumCpuRatePercent * 100),
        };

        SetInformation(handle, JOBOBJECT_CPU_RATE_CONTROL_INFORMATION_CLASS, information);
    }

    private static void SetInformation<T>(SafeKernelHandle handle, int informationClass, T information)
        where T : struct
    {
        int byteCount = Marshal.SizeOf<T>();
        nint buffer = Marshal.AllocHGlobal(byteCount);
        try
        {
            Marshal.StructureToPtr(information, buffer, fDeleteOld: false);
            if (!WindowsNativeMethods.SetInformationJobObject(
                    handle.DangerousGetHandle(),
                    informationClass,
                    buffer,
                    checked((uint)byteCount)))
            {
                throw LastError("Converty could not configure worker Job Object limits.");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static Win32Exception LastError(string message) =>
        new(Marshal.GetLastPInvokeError(), message);

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectBasicLimitInformation
    {
        internal long PerProcessUserTimeLimit;
        internal long PerJobUserTimeLimit;
        internal uint LimitFlags;
        internal nuint MinimumWorkingSetSize;
        internal nuint MaximumWorkingSetSize;
        internal uint ActiveProcessLimit;
        internal nuint Affinity;
        internal uint PriorityClass;
        internal uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IoCounters
    {
        internal ulong ReadOperationCount;
        internal ulong WriteOperationCount;
        internal ulong OtherOperationCount;
        internal ulong ReadTransferCount;
        internal ulong WriteTransferCount;
        internal ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectExtendedLimitInformation
    {
        internal JobObjectBasicLimitInformation BasicLimitInformation;
        internal IoCounters IoInfo;
        internal nuint ProcessMemoryLimit;
        internal nuint JobMemoryLimit;
        internal nuint PeakProcessMemoryUsed;
        internal nuint PeakJobMemoryUsed;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectCpuRateControlInformation
    {
        internal uint ControlFlags;
        internal uint CpuRate;
    }
}
