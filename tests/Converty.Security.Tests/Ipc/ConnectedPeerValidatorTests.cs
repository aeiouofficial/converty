using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.Principal;
using Converty.Security.Ipc;

namespace Converty.Security.Tests.Ipc;

[SupportedOSPlatform("windows")]
public sealed class ConnectedPeerValidatorTests
{
    private static readonly SecurityIdentifier ExpectedUser = new("S-1-5-21-111111111-222222222-333333333-1001");
    private static readonly SecurityIdentifier OtherUser = new("S-1-5-21-111111111-222222222-333333333-1002");

    [Fact]
    public void ExpectedSidIsAuthorized()
    {
        using NamedPipeServerStream pipe = CreateDisconnectedPipe();
        var validator = new ConnectedPeerValidator(new FakeReader(ExpectedUser));

        Assert.True(validator.IsExpectedUser(pipe, ExpectedUser));
    }

    [Fact]
    public void DifferentSidIsRejected()
    {
        using NamedPipeServerStream pipe = CreateDisconnectedPipe();
        var validator = new ConnectedPeerValidator(new FakeReader(OtherUser));

        Assert.False(validator.IsExpectedUser(pipe, ExpectedUser));
    }

    [Fact]
    public void MissingSidIsRejected()
    {
        using NamedPipeServerStream pipe = CreateDisconnectedPipe();
        var validator = new ConnectedPeerValidator(new FakeReader(null));

        Assert.False(validator.IsExpectedUser(pipe, ExpectedUser));
    }

    [Fact]
    public void IdentityReadFailureIsRejected()
    {
        using NamedPipeServerStream pipe = CreateDisconnectedPipe();
        var validator = new ConnectedPeerValidator(new ThrowingReader(new IOException("peer identity unavailable")));

        Assert.False(validator.IsExpectedUser(pipe, ExpectedUser));
    }

    [Fact]
    public void ConstructorRejectsNullReader()
    {
        Assert.Throws<ArgumentNullException>(() => new ConnectedPeerValidator(null!));
    }

    [Fact]
    public void ValidatorRejectsNullArguments()
    {
        using NamedPipeServerStream pipe = CreateDisconnectedPipe();
        var validator = new ConnectedPeerValidator(new FakeReader(ExpectedUser));

        Assert.Throws<ArgumentNullException>(() => validator.IsExpectedUser(null!, ExpectedUser));
        Assert.Throws<ArgumentNullException>(() => validator.IsExpectedUser(pipe, null!));
    }

    private static NamedPipeServerStream CreateDisconnectedPipe() =>
        new(
            $"converty-security-test-{Guid.NewGuid():N}",
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

    private sealed class FakeReader(SecurityIdentifier? sid) : IConnectedPeerIdentityReader
    {
        public SecurityIdentifier? ReadClientSid(NamedPipeServerStream pipe) => sid;
    }

    private sealed class ThrowingReader(Exception exception) : IConnectedPeerIdentityReader
    {
        public SecurityIdentifier? ReadClientSid(NamedPipeServerStream pipe) => throw exception;
    }
}
