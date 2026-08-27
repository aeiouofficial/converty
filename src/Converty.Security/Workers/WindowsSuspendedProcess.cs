using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Converty.Security.Workers;

internal sealed class WindowsSuspendedProcess : IDisposable
{
    private SafeKernelHandle? _processHandle;
    private SafeKernelHandle? _threadHandle;

    private WindowsSuspendedProcess(
        SafeKernelHandle processHandle,
        SafeKernelHandle threadHandle,
        uint processId,
        StreamReader standardOutput,
        StreamReader standardError)
    {
        _processHandle = processHandle;
        _threadHandle = threadHandle;
        ProcessId = processId;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    internal uint ProcessId { get; }

    internal StreamReader StandardOutput { get; }

    internal StreamReader StandardError { get; }

    internal SafeKernelHandle ProcessHandle =>
        _processHandle ?? throw new ObjectDisposedException(nameof(WindowsSuspendedProcess));

    internal static WindowsSuspendedProcess Create(WorkerProcessLaunchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        SafeKernelHandle? childStandardInput = null;
        SafeKernelHandle? childStandardOutput = null;
        SafeKernelHandle? childStandardError = null;
        StreamReader? parentStandardOutput = null;
        StreamReader? parentStandardError = null;
        SafeKernelHandle? processHandle = null;
        SafeKernelHandle? threadHandle = null;

        try
        {
            childStandardInput = CreateClosedInputPipe();
            (parentStandardOutput, childStandardOutput) = CreateOutputPipe();
            (parentStandardError, childStandardError) = CreateOutputPipe();

            nint[] inheritedHandles =
            [
                childStandardInput.DangerousGetHandle(),
                childStandardOutput.DangerousGetHandle(),
                childStandardError.DangerousGetHandle(),
            ];

            using var attributes = ProcessThreadAttributeList.Create(inheritedHandles);
            var startupInfo = new WindowsNativeMethods.StartupInfoEx
            {
                StartupInfo = new WindowsNativeMethods.StartupInfo
                {
                    Size = checked((uint)Marshal.SizeOf<WindowsNativeMethods.StartupInfoEx>()),
                    Flags = WindowsNativeMethods.STARTF_USESTDHANDLES,
                    StandardInput = inheritedHandles[0],
                    StandardOutput = inheritedHandles[1],
                    StandardError = inheritedHandles[2],
                },
                AttributeList = attributes.Handle,
            };

            string commandLineText = BuildCommandLine(request.ExecutablePath, request.Arguments);
            nint applicationName = Marshal.StringToHGlobalUni(request.ExecutablePath);
            nint commandLine = Marshal.StringToHGlobalUni(commandLineText);
            nint currentDirectory = Marshal.StringToHGlobalUni(request.WorkingDirectory);
            try
            {
                uint creationFlags =
                    WindowsNativeMethods.CREATE_SUSPENDED |
                    WindowsNativeMethods.EXTENDED_STARTUPINFO_PRESENT |
                    WindowsNativeMethods.CREATE_NO_WINDOW;

                if (!WindowsNativeMethods.CreateProcessW(
                        applicationName,
                        commandLine,
                        nint.Zero,
                        nint.Zero,
                        inheritHandles: true,
                        creationFlags,
                        nint.Zero,
                        currentDirectory,
                        ref startupInfo,
                        out WindowsNativeMethods.ProcessInformation processInformation))
                {
                    throw LastError("Converty could not create the suspended worker process.");
                }

                processHandle = new SafeKernelHandle(processInformation.ProcessHandle, ownsHandle: true);
                threadHandle = new SafeKernelHandle(processInformation.ThreadHandle, ownsHandle: true);

                var result = new WindowsSuspendedProcess(
                    processHandle,
                    threadHandle,
                    processInformation.ProcessId,
                    parentStandardOutput,
                    parentStandardError);
                processHandle = null;
                threadHandle = null;
                parentStandardOutput = null;
                parentStandardError = null;
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(applicationName);
                Marshal.FreeHGlobal(commandLine);
                Marshal.FreeHGlobal(currentDirectory);
            }
        }
        finally
        {
            childStandardInput?.Dispose();
            childStandardOutput?.Dispose();
            childStandardError?.Dispose();
            processHandle?.Dispose();
            threadHandle?.Dispose();
            parentStandardOutput?.Dispose();
            parentStandardError?.Dispose();
        }
    }

    internal void Resume()
    {
        SafeKernelHandle threadHandle =
            _threadHandle ?? throw new ObjectDisposedException(nameof(WindowsSuspendedProcess));
        uint previousSuspendCount = WindowsNativeMethods.ResumeThread(threadHandle.DangerousGetHandle());
        if (previousSuspendCount == uint.MaxValue)
        {
            throw LastError("Converty could not resume the contained worker process.");
        }
    }

    internal void Terminate(uint exitCode)
    {
        SafeKernelHandle? processHandle = _processHandle;
        if (processHandle is null || processHandle.IsClosed)
        {
            return;
        }

        if (!WindowsNativeMethods.TerminateProcess(processHandle.DangerousGetHandle(), exitCode))
        {
            int error = Marshal.GetLastPInvokeError();
            if (error != 0)
            {
                throw new Win32Exception(error, "Converty could not terminate the suspended worker process.");
            }
        }
    }

    public void Dispose()
    {
        StandardOutput.Dispose();
        StandardError.Dispose();
        _threadHandle?.Dispose();
        _threadHandle = null;
        _processHandle?.Dispose();
        _processHandle = null;
    }

    private static SafeKernelHandle CreateClosedInputPipe()
    {
        CreatePipe(out nint childRead, out nint parentWrite);
        var childReadHandle = new SafeKernelHandle(childRead, ownsHandle: true);
        var parentWriteHandle = new SafeKernelHandle(parentWrite, ownsHandle: true);
        try
        {
            ClearInheritance(parentWriteHandle.DangerousGetHandle());
            parentWriteHandle.Dispose();
            return childReadHandle;
        }
        catch
        {
            childReadHandle.Dispose();
            parentWriteHandle.Dispose();
            throw;
        }
    }

    private static (StreamReader ParentRead, SafeKernelHandle ChildWrite) CreateOutputPipe()
    {
        CreatePipe(out nint parentRead, out nint childWrite);
        var parentReadHandle = new SafeFileHandle(parentRead, ownsHandle: true);
        var childWriteHandle = new SafeKernelHandle(childWrite, ownsHandle: true);
        try
        {
            ClearInheritance(parentReadHandle.DangerousGetHandle());
            var stream = new FileStream(
                parentReadHandle,
                FileAccess.Read,
                bufferSize: 4096,
                isAsync: false);
            var reader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 4096,
                leaveOpen: false);
            return (reader, childWriteHandle);
        }
        catch
        {
            parentReadHandle.Dispose();
            childWriteHandle.Dispose();
            throw;
        }
    }

    private static void CreatePipe(out nint readPipe, out nint writePipe)
    {
        var attributes = new WindowsNativeMethods.SecurityAttributes
        {
            Length = checked((uint)Marshal.SizeOf<WindowsNativeMethods.SecurityAttributes>()),
            InheritHandle = 1,
        };
        if (!WindowsNativeMethods.CreatePipe(out readPipe, out writePipe, ref attributes, size: 0))
        {
            throw LastError("Converty could not create worker standard-I/O pipes.");
        }
    }

    private static void ClearInheritance(nint handle)
    {
        if (!WindowsNativeMethods.SetHandleInformation(
                handle,
                WindowsNativeMethods.HANDLE_FLAG_INHERIT,
                flags: 0))
        {
            throw LastError("Converty could not restrict worker pipe inheritance.");
        }
    }

    private static string BuildCommandLine(string executablePath, IReadOnlyList<string> arguments)
    {
        var commandLine = new StringBuilder();
        AppendWindowsArgument(commandLine, executablePath);
        foreach (string argument in arguments)
        {
            commandLine.Append(' ');
            AppendWindowsArgument(commandLine, argument);
        }
        return commandLine.ToString();
    }

    private static void AppendWindowsArgument(StringBuilder destination, string argument)
    {
        bool requiresQuotes =
            argument.Length == 0 ||
            argument.Any(char.IsWhiteSpace) ||
            argument.Contains('"', StringComparison.Ordinal);
        if (!requiresQuotes)
        {
            destination.Append(argument);
            return;
        }

        destination.Append('"');
        int backslashCount = 0;
        foreach (char value in argument)
        {
            if (value == '\\')
            {
                backslashCount++;
                continue;
            }

            if (value == '"')
            {
                destination.Append('\\', checked((backslashCount * 2) + 1));
                destination.Append('"');
                backslashCount = 0;
                continue;
            }

            destination.Append('\\', backslashCount);
            backslashCount = 0;
            destination.Append(value);
        }

        destination.Append('\\', checked(backslashCount * 2));
        destination.Append('"');
    }

    private static Win32Exception LastError(string message) =>
        new(Marshal.GetLastPInvokeError(), message);

    private sealed class ProcessThreadAttributeList : IDisposable
    {
        private nint _attributeList;
        private nint _handleList;

        private ProcessThreadAttributeList(nint attributeList, nint handleList)
        {
            _attributeList = attributeList;
            _handleList = handleList;
        }

        internal nint Handle =>
            _attributeList != nint.Zero
                ? _attributeList
                : throw new ObjectDisposedException(nameof(ProcessThreadAttributeList));

        internal static ProcessThreadAttributeList Create(nint[] inheritedHandles)
        {
            ArgumentNullException.ThrowIfNull(inheritedHandles);
            if (inheritedHandles.Length == 0)
            {
                throw new ArgumentException("At least one inherited worker handle is required.", nameof(inheritedHandles));
            }

            nuint requiredBytes = 0;
            _ = WindowsNativeMethods.InitializeProcThreadAttributeList(
                nint.Zero,
                attributeCount: 1,
                flags: 0,
                ref requiredBytes);
            if (requiredBytes == 0 || requiredBytes > int.MaxValue)
            {
                throw LastError("Converty could not size the worker process attribute list.");
            }

            nint attributeList = Marshal.AllocHGlobal(checked((int)requiredBytes));
            nint handleList = nint.Zero;
            try
            {
                if (!WindowsNativeMethods.InitializeProcThreadAttributeList(
                        attributeList,
                        attributeCount: 1,
                        flags: 0,
                        ref requiredBytes))
                {
                    throw LastError("Converty could not initialize the worker process attribute list.");
                }

                int handleListBytes = checked(inheritedHandles.Length * nint.Size);
                handleList = Marshal.AllocHGlobal(handleListBytes);
                for (int index = 0; index < inheritedHandles.Length; index++)
                {
                    Marshal.WriteIntPtr(handleList, checked(index * nint.Size), inheritedHandles[index]);
                }

                if (!WindowsNativeMethods.UpdateProcThreadAttribute(
                        attributeList,
                        flags: 0,
                        WindowsNativeMethods.PROC_THREAD_ATTRIBUTE_HANDLE_LIST,
                        handleList,
                        checked((nuint)handleListBytes),
                        nint.Zero,
                        nint.Zero))
                {
                    throw LastError("Converty could not restrict worker inherited handles.");
                }

                return new ProcessThreadAttributeList(attributeList, handleList);
            }
            catch
            {
                if (attributeList != nint.Zero)
                {
                    WindowsNativeMethods.DeleteProcThreadAttributeList(attributeList);
                    Marshal.FreeHGlobal(attributeList);
                }
                if (handleList != nint.Zero)
                {
                    Marshal.FreeHGlobal(handleList);
                }
                throw;
            }
        }

        public void Dispose()
        {
            if (_attributeList != nint.Zero)
            {
                WindowsNativeMethods.DeleteProcThreadAttributeList(_attributeList);
                Marshal.FreeHGlobal(_attributeList);
                _attributeList = nint.Zero;
            }
            if (_handleList != nint.Zero)
            {
                Marshal.FreeHGlobal(_handleList);
                _handleList = nint.Zero;
            }
        }
    }
}
