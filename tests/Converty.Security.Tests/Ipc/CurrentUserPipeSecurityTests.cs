using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using Converty.Security.Ipc;

namespace Converty.Security.Tests.Ipc;

[SupportedOSPlatform("windows")]
public sealed class CurrentUserPipeSecurityTests
{
    private static readonly SecurityIdentifier ExpectedUser = new("S-1-5-21-111111111-222222222-333333333-1001");

    [Fact]
    public void CreateProducesProtectedSingleUserDacl()
    {
        PipeSecurity security = CurrentUserPipeSecurity.Create(ExpectedUser);

        Assert.True(security.AreAccessRulesProtected);
        Assert.Equal(ExpectedUser, security.GetOwner(typeof(SecurityIdentifier)));

        PipeAccessRule rule = Assert.Single(
            security.GetAccessRules(includeExplicit: true, includeInherited: false, typeof(SecurityIdentifier))
                .Cast<PipeAccessRule>());

        Assert.Equal(ExpectedUser, rule.IdentityReference);
        Assert.Equal(AccessControlType.Allow, rule.AccessControlType);
        Assert.Equal(PipeAccessRights.FullControl, rule.PipeAccessRights);
        Assert.False(rule.IsInherited);
    }

    [Fact]
    public void CreateDoesNotGrantBroadWellKnownIdentities()
    {
        PipeSecurity security = CurrentUserPipeSecurity.Create(ExpectedUser);
        SecurityIdentifier[] granted = security
            .GetAccessRules(includeExplicit: true, includeInherited: false, typeof(SecurityIdentifier))
            .Cast<PipeAccessRule>()
            .Select(rule => (SecurityIdentifier)rule.IdentityReference)
            .ToArray();

        Assert.DoesNotContain(new SecurityIdentifier(WellKnownSidType.WorldSid, null), granted);
        Assert.DoesNotContain(new SecurityIdentifier(WellKnownSidType.AnonymousSid, null), granted);
        Assert.DoesNotContain(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), granted);
    }

    [Fact]
    public void CreateRejectsNullSid()
    {
        Assert.Throws<ArgumentNullException>(() => CurrentUserPipeSecurity.Create(null!));
    }
}
