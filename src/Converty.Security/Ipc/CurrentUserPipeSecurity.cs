using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Converty.Security.Ipc;

[SupportedOSPlatform("windows")]
public static class CurrentUserPipeSecurity
{
    public static PipeSecurity Create(SecurityIdentifier userSid)
    {
        ArgumentNullException.ThrowIfNull(userSid);

        var security = new PipeSecurity();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.SetOwner(userSid);
        security.AddAccessRule(new PipeAccessRule(
            userSid,
            PipeAccessRights.FullControl,
            AccessControlType.Allow));

        return security;
    }
}
