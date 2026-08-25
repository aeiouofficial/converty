using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.Principal;
using Converty.Ipc.Protocol;
using Converty.Security.Ipc;

namespace Converty.Host.Ipc;

public enum HostPipeSessionResult
{
    RequestHandled = 0,
    UnauthorizedPeer = 1,
    InvalidFrame = 2,
    TransportClosed = 3,
}

[SupportedOSPlatform("windows")]
public sealed class HostPipeServer
{
    private const int PipeBufferBytes = 64 * 1024;

    private readonly string _pipeName;
    private readonly SecurityIdentifier _expectedUser;
    private readonly IConnectedPeerValidator _peerValidator;
    private readonly HostRequestHandler _requestHandler;

    public HostPipeServer(
        string pipeName,
        SecurityIdentifier expectedUser,
        IConnectedPeerValidator peerValidator,
        HostRequestHandler requestHandler)
    {
        if (string.IsNullOrWhiteSpace(pipeName))
        {
            throw new ArgumentException("Pipe name is required.", nameof(pipeName));
        }

        _pipeName = pipeName;
        _expectedUser = expectedUser ?? throw new ArgumentNullException(nameof(expectedUser));
        _peerValidator = peerValidator ?? throw new ArgumentNullException(nameof(peerValidator));
        _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
    }

    public async Task<HostPipeSessionResult> RunSingleConnectionAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        PipeSecurity security = CurrentUserPipeSecurity.Create(_expectedUser);
        await using NamedPipeServerStream pipe = NamedPipeServerStreamAcl.Create(
            _pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            PipeBufferBytes,
            PipeBufferBytes,
            security,
            HandleInheritability.None,
            additionalAccessRights: 0);

        await pipe.WaitForConnectionAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_peerValidator.IsExpectedUser(pipe, _expectedUser))
        {
            return HostPipeSessionResult.UnauthorizedPeer;
        }

        ProtocolFrame request;
        try
        {
            request = await ProtocolFrameCodec.ReadAsync(pipe, cancellationToken);
        }
        catch (ProtocolException)
        {
            return HostPipeSessionResult.InvalidFrame;
        }
        catch (IOException)
        {
            return HostPipeSessionResult.TransportClosed;
        }

        byte[] response = await _requestHandler.HandleAsync(
            request.Payload,
            PeerAuthorization.ExpectedUser,
            cancellationToken);

        try
        {
            await ProtocolFrameCodec.WriteAsync(pipe, response, cancellationToken);
            await pipe.FlushAsync(cancellationToken);
        }
        catch (IOException)
        {
            return HostPipeSessionResult.TransportClosed;
        }

        return HostPipeSessionResult.RequestHandled;
    }
}
