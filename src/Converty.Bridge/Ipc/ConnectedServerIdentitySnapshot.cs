namespace Converty.Bridge.Ipc;

public sealed record ConnectedServerIdentitySnapshot(
    uint ServerProcessId,
    string ImagePath,
    string? PackageFamilyName,
    uint ConfirmedServerProcessId);
