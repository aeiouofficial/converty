using Converty.Contracts.Identifiers;

namespace Converty.Bridge.Shell;

public sealed record ShellConversionRequest(PresetId PresetId, IReadOnlyList<string> InputPaths);
