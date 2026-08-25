using System.Runtime.Versioning;
using System.Security.Principal;
using Converty.Host.Runtime;

namespace Converty.Host.Tests.Runtime;

[SupportedOSPlatform("windows")]
public sealed class HostSingleInstanceLeaseTests
{
    private static readonly SecurityIdentifier UserOne = new("S-1-5-21-111111111-222222222-333333333-1001");
    private static readonly SecurityIdentifier UserTwo = new("S-1-5-21-111111111-222222222-333333333-1002");

    [Fact]
    public void SecondLeaseForSameUserIsRejectedUntilFirstIsDisposed()
    {
        Assert.True(HostSingleInstanceLease.TryAcquire(UserOne, out HostSingleInstanceLease? first));
        Assert.NotNull(first);

        try
        {
            Assert.False(HostSingleInstanceLease.TryAcquire(UserOne, out HostSingleInstanceLease? second));
            Assert.Null(second);
        }
        finally
        {
            first.Dispose();
        }

        Assert.True(HostSingleInstanceLease.TryAcquire(UserOne, out HostSingleInstanceLease? reacquired));
        reacquired!.Dispose();
    }

    [Fact]
    public void DifferentUsersUseDifferentInstanceNames()
    {
        string first = HostSingleInstanceLease.NameForUser(UserOne);
        string second = HostSingleInstanceLease.NameForUser(UserTwo);

        Assert.NotEqual(first, second);
        Assert.StartsWith(@"Local\Converty.Host.v1.", first, StringComparison.Ordinal);
        Assert.DoesNotContain(UserOne.Value!, first, StringComparison.Ordinal);
    }

    [Fact]
    public void NameForUserRejectsNullSid()
    {
        Assert.Throws<ArgumentNullException>(() => HostSingleInstanceLease.NameForUser(null!));
    }
}
