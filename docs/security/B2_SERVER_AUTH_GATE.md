# B2 connected-server identity gate

B2 connected-server authentication is executable-qualified for the current full-package identity model as of `0.1.0-dev.11` development evidence. This qualification does not approve production signing, FFmpeg redistribution, or final Windows 11 headed acceptance.

## Authentication rule

Bridge must authenticate every newly connected Host pipe session before any application request frame is serialized or written. Acceptance requires all of the following to agree with installed package authority:

1. the server PID obtained from the connected named pipe,
2. the exact canonical installed `Converty.Host.exe` image path,
3. the expected Converty package family name,
4. the same server PID before and after process identity queries.

`WindowsConnectedServerIdentityProbe`, `WindowsConnectedServerIdentityVerifier`, and `BridgeClient` implement this rule. Missing package identity, a wrong Host image, a wrong package family, or a PID race fails closed. Current-user pipe ACLs and Host-side client SID validation remain defense-in-depth; fixed path, pipe name, or same-user ownership alone are not treated as server authentication.

## Development acceptance evidence

The development package identity observed in the qualified runs was:

`Converty.Dev_yr4ybytcyx7nj`

The following independent executable boundaries were qualified on Windows Server 2025 (`10.0.26100`, `windows-2025-vs2026`, image `20260824.214.3`, .NET SDK `10.0.400`):

- Run `33211928010`, job `98986920905`: an AUMID-activated package-identified parent launched the exact staged sibling `Converty.Host.exe` with the existing direct `Process.Start` semantics; parent and Host child both reported `Converty.Dev_yr4ybytcyx7nj`. This proves the existing `InstalledHostProcessLauncher` does not require a separate Host AUMID.
- Run `33218030168`, job `99005949641`: the real registered package COM `IExplorerCommand` path invoked the production shell extension, which launched the exact staged sibling `Converty.Bridge.exe` through its existing `CreateProcessW` path. The observed Bridge PID was `7332`, its image was the exact staged Bridge path, `GetCurrentPackageFamilyName` returned `0`, and the Bridge PFN exactly matched `Converty.Dev_yr4ybytcyx7nj`.
- Run `33218498644`, job `99007347897`: the same real registered COM → `IExplorerCommand::Invoke` → shell `CreateProcessW` → packaged Bridge path instantiated the existing `PackagedBridgeRuntimeFactory`. The exact sibling Host was started through `InstalledHostProcessLauncher`; `BridgeClient` connected and authenticated the server before the request frame; Host accepted the request and returned job ID `5bd48925-8c88-48d2-bbd7-a62c2ba03e3e`. The run emitted `B2_PACKAGED_BRIDGE_HOST_AUTH_RESULT=ACCEPTED`.

The normal managed suite retains negative coverage for wrong Host path, wrong or missing package family, server PID race, fresh per-session probing, native probe failure, unpackaged same-user server rejection, and the requirement that identity rejection writes no application frame. Do not duplicate those checks with a weaker PowerShell-only positive model.

## Architecture consequences

The qualified result preserves the product-first architecture:

- keep the shell extension's direct exact-path `CreateProcessW` launch of `Converty.Bridge.exe`;
- keep the Bridge's existing exact-path `InstalledHostProcessLauncher` semantics;
- do not add a second permanent Host application declaration solely for B2;
- do not add a parallel activation subsystem;
- do not route the live Explorer conversion path through Host. The conversion product path remains Bridge → disposable EngineWorker → provider/FFmpeg.

The temporary diagnostic instrumentation used to capture the executable evidence is not product behavior and must not remain in the final dev.11 product tree.

## Remaining release authority

Production package signing is still a release blocker. Production acceptance must use the final publisher/package family identity and signed MSIX, with private signing keys kept outside the repository/workspace. The development evidence above establishes that the selected Windows package identity model and the existing Bridge/Host launch mechanics support the required fail-closed connected-server authentication rule; it is not a substitute for final signed-package and headed Windows 11 qualification.
