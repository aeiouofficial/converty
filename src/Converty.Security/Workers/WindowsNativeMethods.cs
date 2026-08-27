using System.Runtime.InteropServices;

namespace Converty.Security.Workers;

internal static partial class WindowsNativeMethods
{
    internal const uint CREATE_SUSPENDED = 0x00000004;
    internal const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
    internal const uint CREATE_NO_WINDOW = 0x08000000;
    internal const uint STARTF_USESTDHANDLES = 0x00000100;
    internal const uint HANDLE_FLAG_INHERIT = 0x00000001;
    internal const nuint PROC_THREAD_ATTRIBUTE_HANDLE_LIST = 0x00020002;
    internal const nuint PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES = 0x00020009;
    internal const uint STILL_ACTIVE = 259;

    internal const uint SE_FILE_OBJECT = 1;
    internal const uint DACL_SECURITY_INFORMATION = 0x00000004;
    internal const uint FILE_GENERIC_READ = 0x00120089;
    internal const uint FILE_GENERIC_WRITE = 0x00120116;
    internal const uint FILE_GENERIC_EXECUTE = 0x001200A0;
    internal const uint OBJECT_INHERIT_ACE = 0x00000001;
    internal const uint CONTAINER_INHERIT_ACE = 0x00000002;
    internal const uint GRANT_ACCESS = 1;
    internal const uint REVOKE_ACCESS = 4;
    internal const uint TRUSTEE_IS_SID = 0;
    internal const uint TRUSTEE_IS_UNKNOWN = 0;
    internal const int ERROR_ALREADY_EXISTS_HRESULT = unchecked((int)0x800700B7);

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityAttributes
    {
        internal uint Length;
        internal nint SecurityDescriptor;
        internal int InheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StartupInfo
    {
        internal uint Size;
        internal nint Reserved;
        internal nint Desktop;
        internal nint Title;
        internal uint X;
        internal uint Y;
        internal uint XSize;
        internal uint YSize;
        internal uint XCountChars;
        internal uint YCountChars;
        internal uint FillAttribute;
        internal uint Flags;
        internal ushort ShowWindow;
        internal ushort Reserved2Count;
        internal nint Reserved2;
        internal nint StandardInput;
        internal nint StandardOutput;
        internal nint StandardError;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StartupInfoEx
    {
        internal StartupInfo StartupInfo;
        internal nint AttributeList;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ProcessInformation
    {
        internal nint ProcessHandle;
        internal nint ThreadHandle;
        internal uint ProcessId;
        internal uint ThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityCapabilities
    {
        internal nint AppContainerSid;
        internal nint Capabilities;
        internal uint CapabilityCount;
        internal uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Trustee
    {
        internal nint MultipleTrustee;
        internal int MultipleTrusteeOperation;
        internal int TrusteeForm;
        internal int TrusteeType;
        internal nint Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ExplicitAccess
    {
        internal uint AccessPermissions;
        internal uint AccessMode;
        internal uint Inheritance;
        internal Trustee Trustee;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(nint handle);

    [LibraryImport("kernel32.dll", EntryPoint = "CreatePipe", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CreatePipe(
        out nint readPipe,
        out nint writePipe,
        ref SecurityAttributes pipeAttributes,
        uint size);

    [LibraryImport("kernel32.dll", EntryPoint = "SetHandleInformation", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetHandleInformation(nint handle, uint mask, uint flags);

    [LibraryImport("kernel32.dll", EntryPoint = "InitializeProcThreadAttributeList", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool InitializeProcThreadAttributeList(
        nint attributeList,
        int attributeCount,
        uint flags,
        ref nuint size);

    [LibraryImport("kernel32.dll", EntryPoint = "UpdateProcThreadAttribute", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UpdateProcThreadAttribute(
        nint attributeList,
        uint flags,
        nuint attribute,
        nint value,
        nuint size,
        nint previousValue,
        nint returnSize);

    [LibraryImport("kernel32.dll", EntryPoint = "DeleteProcThreadAttributeList")]
    internal static partial void DeleteProcThreadAttributeList(nint attributeList);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateProcessW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CreateProcessW(
        nint applicationName,
        nint commandLine,
        nint processAttributes,
        nint threadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandles,
        uint creationFlags,
        nint environment,
        nint currentDirectory,
        ref StartupInfoEx startupInfo,
        out ProcessInformation processInformation);

    [LibraryImport("kernel32.dll", EntryPoint = "ResumeThread", SetLastError = true)]
    internal static partial uint ResumeThread(nint threadHandle);

    [LibraryImport("kernel32.dll", EntryPoint = "TerminateProcess", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool TerminateProcess(nint processHandle, uint exitCode);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateJobObjectW", SetLastError = true)]
    internal static partial nint CreateJobObject(nint jobAttributes, nint name);

    [LibraryImport("kernel32.dll", EntryPoint = "SetInformationJobObject", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetInformationJobObject(
        nint jobHandle,
        int informationClass,
        nint information,
        uint informationLength);

    [LibraryImport("kernel32.dll", EntryPoint = "AssignProcessToJobObject", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool AssignProcessToJobObject(nint jobHandle, nint processHandle);

    [LibraryImport("kernel32.dll", EntryPoint = "TerminateJobObject", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool TerminateJobObject(nint jobHandle, uint exitCode);

    [LibraryImport("kernel32.dll", EntryPoint = "LocalFree")]
    internal static partial nint LocalFree(nint memory);

    [LibraryImport("advapi32.dll", EntryPoint = "FreeSid")]
    internal static partial nint FreeSid(nint sid);

    [LibraryImport("userenv.dll", EntryPoint = "CreateAppContainerProfile", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int CreateAppContainerProfile(
        string appContainerName,
        string displayName,
        string description,
        nint capabilities,
        uint capabilityCount,
        out nint appContainerSid);

    [LibraryImport("userenv.dll", EntryPoint = "DeriveAppContainerSidFromAppContainerName", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int DeriveAppContainerSidFromAppContainerName(
        string appContainerName,
        out nint appContainerSid);

    [LibraryImport("userenv.dll", EntryPoint = "DeleteAppContainerProfile", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int DeleteAppContainerProfile(string appContainerName);

    [LibraryImport("advapi32.dll", EntryPoint = "GetNamedSecurityInfoW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial uint GetNamedSecurityInfoW(
        string objectName,
        uint objectType,
        uint securityInfo,
        out nint owner,
        out nint group,
        out nint dacl,
        out nint sacl,
        out nint securityDescriptor);

    [LibraryImport("advapi32.dll", EntryPoint = "SetEntriesInAclW")]
    internal static partial uint SetEntriesInAclW(
        uint countOfExplicitEntries,
        ref ExplicitAccess explicitEntry,
        nint oldAcl,
        out nint newAcl);

    [LibraryImport("advapi32.dll", EntryPoint = "SetNamedSecurityInfoW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial uint SetNamedSecurityInfoW(
        string objectName,
        uint objectType,
        uint securityInfo,
        nint owner,
        nint group,
        nint dacl,
        nint sacl);
}
