namespace Converty.Bridge.Ipc;

public readonly record struct BridgeSubmissionResult(bool Accepted, Guid? JobId, string? Reason);
