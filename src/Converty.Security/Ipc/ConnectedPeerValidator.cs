using System.ComponentModel;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Principal;

namespace Converty.Security.Ipc;

[SupportedOSPlatform("windows")]
public interface IConnectedPeerIdentityReader
{
    SecurityIdentifier? ReadClientSid(NamedPipeServerStream pipe);
}

[SupportedOSPlatform("windows")]
public interface IConnectedPeerValidator
{
    bool IsExpectedUser(NamedPipeServerStream pipe, SecurityIdentifier expectedUser);
}

[SupportedOSPlatform("windows")]
public sealed class WindowsConnectedPeerIdentityReader : IConnectedPeerIdentityReader
{
    public SecurityIdentifier? ReadClientSid(NamedPipeServerStream pipe)
    {
        ArgumentNullException.ThrowIfNull(pipe);

        SecurityIdentifier? sid = null;
        pipe.RunAsClient(() =>
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query);
            sid = identity.User;
        });
        return sid;
    }
}

[SupportedOSPlatform("windows")]
public sealed class ConnectedPeerValidator : IConnectedPeerValidator
{
    private readonly IConnectedPeerIdentityReader _identityReader;

    public ConnectedPeerValidator(IConnectedPeerIdentityReader identityReader)
    {
        _identityReader = identityReader ?? throw new ArgumentNullException(nameof(identityReader));
    }

    public bool IsExpectedUser(NamedPipeServerStream pipe, SecurityIdentifier expectedUser)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        ArgumentNullException.ThrowIfNull(expectedUser);

        try
        {
            SecurityIdentifier? actualUser = _identityReader.ReadClientSid(pipe);
            return actualUser is not null && actualUser.Equals(expectedUser);
        }
        catch (Exception error) when (error is IOException
                                      or UnauthorizedAccessException
                                      or InvalidOperationException
                                      or SecurityException
                                      or Win32Exception)
        {
            return false;
        }
    }
}
