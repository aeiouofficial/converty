using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Converty.Security.Ipc;

[SupportedOSPlatform("windows")]
public static class PipeEndpointName
{
    private const string Prefix = "converty.v1.";

    public static string ForUser(SecurityIdentifier userSid)
    {
        ArgumentNullException.ThrowIfNull(userSid);

        byte[] sidBytes = new byte[userSid.BinaryLength];
        userSid.GetBinaryForm(sidBytes, 0);
        byte[] digest = SHA256.HashData(sidBytes);
        string suffix = Convert.ToHexString(digest.AsSpan(0, 20)).ToLowerInvariant();
        return Prefix + suffix;
    }
}
