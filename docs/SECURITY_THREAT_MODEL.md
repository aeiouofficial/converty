# Converty Security Threat Model and Hardening Checklist

**Status:** B4 disposable-worker containment implemented and behavior-qualified in `0.1.0-dev.10`; final release hardening/acceptance remains open.
**Security objective:** treat file contents and conversion engines as potentially hostile, minimize authority, contain compromise, fail closed, and preserve original data.

## Trust zones
1. Explorer shell path — highly sensitive, minimal code, no parsing.
2. Bridge/coordinator — trusted orchestration, no codec/plugin code.
3. Disposable worker — hostile-parser zone; sandbox/resource restricted.
4. Staging/output transaction — private temp, validated commit only.
5. Supply chain — signed/pinned artifacts, SBOM, no arbitrary loading.

## Threats → required mitigation

| Threat | Primary controls | Failure invariant |
|---|---|---|
| Malformed media / decoder RCE | worker-only parsing, AppContainer strict profile, Job Object | Explorer/Host remain outside parser process |
| Filename/argument injection | typed presets, structured argument arrays, no shell | filename cannot become executable syntax |
| IPC spoofing/flooding | explicit DACL, peer checks, schema/version/size/count/time limits | invalid peer/message creates no job |
| Network exfiltration | zero-capability AppContainer strict profile; future protocol allowlist defense-in-depth | strict local conversion has no intended egress |
| Plugin/engine tamper | signed/hash-pinned production artifacts, manifest/API gate | tampered provider never executes |
| DLL hijacking | absolute app-local paths, reparse rejection, protected install authority | input/temp directory cannot supply executable code |
| CPU/RAM/process/output bomb | Job Object limits, process count, timeout, output-growth cap | worker tree is terminated and no final commit |
| Path/reparse race | private staging, reparse rejection, no-overwrite publication | cannot redirect worker/commit to unrelated file |
| Data destruction | private output, validation, atomic numbered policy | original/final preserved on failure |
| Metadata privacy leak | explicit preserve/strip policy; no media in logs | no accidental telemetry/content exfiltration |
| Host crash | atomic journal + idempotent cleanup | restart can identify abandoned temp work |
| Explorer hang | no long work in shell; fixed Bridge handoff | conversion failure cannot block shell UI |

## B4 dev.10 evidence
Behavior authority `f221563c790057344a94b4e60c309d4512a77c38`, run `33028554361` (managed `98375493893`, static `98375494099`) proves the real strict Bridge→EngineWorker→FFmpeg MVP plus the Windows executable security suite. The earlier direct strict-canary run `33027104465` (managed `98370929641`, static `98370929814`) specifically qualified staging-only file access, outside-scope denial, loopback-network denial and descendant/resource containment. The dev.10 Windows suite has 0 skipped tests and includes the finite-output-growth termination canary.

## Security review gates
- [x] Threat model reviewed before and during B4 worker containment implementation.
- [ ] Every new production engine/provider must retain a provider-specific threat-model delta before shipping.
- [x] Isolation level is explicit; Strict failure never silently downgrades to Compatibility.
- [ ] Any new network capability requires separate user-facing feature, policy, and threat review.
- [ ] Any runtime download/update path requires signature verification, rollback strategy, and new supply-chain ADR.

## Build hardening checklist
- [x] Native Explorer Release target compiled with repository hardening/warnings-as-errors policy in automated qualification.
- [x] .NET analyzers enabled; nullable enabled; warnings/analyzer findings fail the Release build.
- [ ] Final production safe-DLL-search/signing acceptance across installed package remains open.
- [x] Development converter/worker binaries are invoked by fixed absolute app-local paths; trust-root reparse substitution is rejected.
- [x] Dependency versions locked and SBOM tooling present; 18/18 locked restore and zero-vulnerability audit pass at behavior head.
- [ ] Release artifacts signed; hashes published in final release manifest.
- [x] No secrets/API keys required for offline conversion.

## Runtime hardening checklist
- [x] Explicit named-pipe DACL, never default.
- [x] Peer identity validation implemented and tested before application-frame parsing.
- [x] IPC length-prefixed/framed protocol with hard maximum sizes.
- [x] Worker launched with fixed executable/cwd, no shell, suspended creation and explicit inherited-handle list.
- [x] Worker Job Object uses kill-on-close.
- [x] Network disabled in strict profile via zero-capability AppContainer and executable canary.
- [x] Strict worker filesystem access limited to application read/execute and private staging read/write; outside-scope canary denied.
- [ ] FFmpeg protocol allowlist defense-in-depth is not yet claimed as a qualified production gate.
- [x] stderr capture and worker execution/output growth are bounded.
- [x] Final publication destination is unreachable to the worker; worker writes only private staging.
- [x] Temp artifacts are unique per conversion job and owned cleanup runs in `finally`.
- [x] Publication re-resolves numbered destination and uses no-overwrite transactional move semantics.

## Security acceptance evidence required before shipping
Retain machine-readable evidence for:
- binary/signature/hash manifest;
- SBOM;
- dependency vulnerability scan;
- static analysis results;
- IPC fuzz run;
- media fuzz/corpus run per provider;
- sandbox network/file canary tests;
- process-tree kill test;
- tampered provider rejection test;
- transactional-output crash matrix.

B4 canary evidence exists; the final production signing/provider/fuzz/clean-VM evidence above remains open where not explicitly qualified.

## External reference authority
- **[MS-1] Windows 11 File Explorer context menu integration** — Microsoft Learn, updated 2026-07-16: https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/integrate-packaged-app-with-file-explorer
- **[MS-2] Named Pipe Security and Access Rights** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipe-security-and-access-rights
- **[MS-3] Job Objects** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/procthread/job-objects
- **[MS-4] AppContainer isolation** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/secauthz/appcontainer-isolation
- **[MS-5] Launch an AppContainer** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/secauthz/implementing-an-appcontainer
- **[MS-6] Create Process In Sandbox APIs** — Microsoft Learn. Treat as optional/experimental until production support is confirmed: https://learn.microsoft.com/en-us/windows/win32/secauthz/createprocessinsandbox
- **[MS-7] Process creation mitigation policy / UpdateProcThreadAttribute** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-updateprocthreadattribute
- **[FF-1] FFmpeg Protocols / protocol whitelist controls** — FFmpeg documentation: https://ffmpeg.org/ffmpeg-protocols.html

The original workspace planning note is preserved at `source/original_audio_plan.md`. Explorer must never perform conversion work.
