using System.Runtime.Versioning;
using System.Security.Principal;
using Converty.Security.Ipc;

namespace Converty.Security.Tests.Ipc;

[SupportedOSPlatform("windows")]
public sealed class PipeEndpointNameTests
{
    private static readonly SecurityIdentifier UserOne = new("S-1-5-21-111111111-222222222-333333333-1001");
    private static readonly SecurityIdentifier UserTwo = new("S-1-5-21-111111111-222222222-333333333-1002");

    [Fact]
    public void ForUserIsDeterministicBoundedAndDoesNotExposeRawSid()
    {
        string first = PipeEndpointName.ForUser(UserOne);
        string second = PipeEndpointName.ForUser(UserOne);

        Assert.Equal(first, second);
        Assert.StartsWith("converty.v1.", first, StringComparison.Ordinal);
        Assert.True(first.Length <= 96);
        Assert.DoesNotContain('\\', first);
        Assert.DoesNotContain('/', first);
        Assert.DoesNotContain(UserOne.Value!, first, StringComparison.Ordinal);
    }

    [Fact]
    public void DifferentUsersGetDifferentEndpointNames()
    {
        Assert.NotEqual(PipeEndpointName.ForUser(UserOne), PipeEndpointName.ForUser(UserTwo));
    }

    [Fact]
    public void ForUserRejectsNullSid()
    {
        Assert.Throws<ArgumentNullException>(() => PipeEndpointName.ForUser(null!));
    }
}
