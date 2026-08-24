# FileConvert Security Threat Model and Hardening Checklist

**Status:** planning authority; implementation not started.
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
| Malformed media / decoder RCE | worker-only parsing, AppContainer/isolated profile, Job Object, mitigations | Explorer/Host remain outside parser process |
| Filename/argument injection | structured argument arrays; never shell strings | filename cannot become executable syntax |
| IPC spoofing/flooding | explicit DACL, peer checks, schema/version/size/count/time limits | invalid peer/message creates no job |
| Network exfiltration | no network capability + FFmpeg protocol allowlist | normal conversion has zero intended egress |
| Plugin/engine tamper | signed/hash-pinned artifacts, manifest API gate | tampered provider never executes |
| DLL hijacking | absolute paths + safe DLL search | input/temp directory cannot supply executable code |
| CPU/RAM/process bomb | Job Object limits, process count, timeout, output cap | worker tree is terminated and no final commit |
| Path/reparse race | staging, handle/path identity checks, revalidation before commit | cannot redirect worker/commit to unrelated file |
| Data destruction | temp output, validation, atomic collision/replace policy | original/final preserved on failure |
| Metadata privacy leak | explicit preserve/strip policy; no media in logs | no accidental telemetry/content exfiltration |
| Host crash | atomic journal + idempotent cleanup | restart can identify abandoned temp work |
| Explorer hang | no long work in shell; Bridge handoff | conversion failure cannot block shell UI |

## Security review gates
- Threat model reviewed before B4 worker containment implementation.
- Every new engine/provider requires a provider-specific threat-model delta.
- Any sandbox fallback requires explicit `IsolationLevel` and must not silently downgrade `StrictRequired`.
- Any new network capability requires separate user-facing feature, policy, and threat review.
- Any runtime download/update path requires signature verification, rollback strategy, and new supply-chain ADR.

## Build hardening checklist
- [ ] Native shell/bridge compiled with stack protection, CFG, ASLR, DEP/NX, CET compatibility where supported, SDL checks, warnings-as-errors.
- [ ] .NET analyzers enabled; nullable enabled; unsafe code prohibited except audited modules.
- [ ] Safe DLL search policy established before dynamic loading.
- [ ] All converter binaries invoked by absolute path from protected install location.
- [ ] Dependency versions locked and SBOM generated.
- [ ] Release artifacts signed; hashes published in release manifest.
- [ ] No secrets/API keys required for offline conversion.

## Runtime hardening checklist
- [ ] Explicit named-pipe DACL, never default [MS-2].
- [ ] Peer identity validation implemented and tested.
- [ ] IPC length-prefixed/framed protocol with hard maximum sizes.
- [ ] Worker launched with explicit environment, cwd and handle-inheritance policy.
- [ ] Worker Job Object uses kill-on-close [MS-3].
- [ ] Network disabled in strict profile.
- [ ] Strict worker filesystem access limited to staging/input/output scope.
- [ ] FFmpeg protocol allowlist applied [FF-1].
- [ ] stdout/stderr/progress bounded.
- [ ] final output unreachable to worker until commit.
- [ ] temp artifacts uniquely tied to job ID.
- [ ] commit revalidates destination identity/policy.

## Security acceptance evidence
For each release retain machine-readable evidence for:
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

## External reference authority

- **[MS-1] Windows 11 File Explorer context menu integration** — Microsoft Learn, updated 2026-07-16: https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/integrate-packaged-app-with-file-explorer
- **[MS-2] Named Pipe Security and Access Rights** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipe-security-and-access-rights
- **[MS-3] Job Objects** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/procthread/job-objects
- **[MS-4] AppContainer isolation** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/secauthz/appcontainer-isolation
- **[MS-5] Launch an AppContainer** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/secauthz/implementing-an-appcontainer
- **[MS-6] Create Process In Sandbox APIs** — Microsoft Learn. Treat as an optional/experimental hardening path until production support is confirmed: https://learn.microsoft.com/en-us/windows/win32/secauthz/createprocessinsandbox
- **[MS-7] Process creation mitigation policy / UpdateProcThreadAttribute** — Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-updateprocthreadattribute
- **[FF-1] FFmpeg Protocols / protocol whitelist controls** — FFmpeg documentation: https://ffmpeg.org/ffmpeg-protocols.html

The original workspace planning note is preserved at `source/original_audio_plan.md`. The new plan intentionally generalizes that audio-only architecture into a generic conversion platform while retaining its most important rule: Explorer must never perform conversion work.
