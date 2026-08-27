# Test and Release Gates

## Gate rule
A release gate is binary. “Mostly works” is failure. Evidence must be retained with the build artifact. Automated Windows Server COM/product evidence does not substitute for headed Windows 11 Explorer evidence.

## Current dev.10 B4 automated evidence
Behavior head `f221563c790057344a94b4e60c309d4512a77c38`, run `33028554361`:
- 18/18 locked restore PASS; vulnerability audit 0 vulnerable-result packages.
- Release build PASS, 0 warnings, 0 errors.
- native Explorer, unsigned dev package, direct DLL Invoke and packaged COM activation/Invoke PASS.
- strict Bridge→EngineWorker→FFmpeg product conversion PASS with source/external destination preservation, numbered output and MP3 exactly 320000 bit/s.
- managed tests 190/190 PASS, 0 skipped; static tests 66/66 PASS; contract vectors 5/5 PASS.
- strict output-growth termination canary PASS as part of the zero-skip Windows security suite.
- direct strict-canary run `33027104465` proves private-staging write allowed, outside-scope write denied, loopback network denied and descendant/resource containment.
- behavior-head workspace ZIP double build is deterministic, but generated-authority freshness/embedded package-manifest verification was intentionally still open before dev.10 authority synchronization.

## Shell gate
- [ ] Modern Windows 11 menu entry verified on clean interactive VM with exact-build screenshots.
- [x] No conversion/probe/network in Explorer-loaded DLL by architecture/static product gates.
- [ ] Headed Explorer remains responsive if Host/Bridge/worker are absent, crash, or reject request.

## IPC gate
- [x] Default pipe descriptor is not used.
- [x] Unauthorized client/peer identity checks exist before application-frame parsing.
- [x] Oversize/malformed framing corpus is bounded and automated tests pass.
- [ ] Final connected-server anti-squatting/authentication acceptance under production package/signing authority.
- [ ] Final status/cancel wire decision and replay/disconnect/reconnect/session matrix.

## Worker isolation gate
- [x] No intended network egress under strict profile; loopback connection canary denied.
- [x] Canary file outside job scope cannot be created/modified.
- [x] Child process tree terminates on Job Object close/cancellation path.
- [x] Memory/CPU/process/time ceilings are finite and tested.
- [x] Output-growth ceiling terminates an over-budget strict worker; default 8 GiB, hard configuration max 16 GiB.
- [x] Strict launch uses zero-capability AppContainer plus application read/execute and private-staging read/write ACLs.
- [x] Strict failure has no silent Compatibility retry.

## Transaction gate
- [x] Worker receives private staging, not final publication destination.
- [x] No final output is reported successful before non-empty validation and no-overwrite publication.
- [x] Collision policy is race-safe for the qualified numbered-copy path and preserves externally created destination.
- [x] Owned staging cleanup runs on failure/success paths covered by managed regressions.
- [ ] Full crash-at-every-lifecycle-state release matrix remains open.

## Supply-chain gate
- [x] Every external GitHub Action reference is pinned to an approved full 40-character commit SHA and passes `scripts/verify_ci_actions.py`.
- [x] CI checkout credentials are not persisted and every job has an explicit finite timeout.
- [x] Reviewed `packages.lock.json` files exist for every current managed project and locked restore succeeds.
- [x] `./build/dependency-audit.ps1` produces a machine-readable transitive vulnerability report and verifier acceptance passes.
- [ ] Production engine/provider binaries require final signed/hash manifest approval; development FFmpeg hash qualification is not production approval.
- [ ] Tampered production binary/provider rejection gate.
- [ ] Final release-mode SBOM/notices/signing review for shipping package.

## Provider gate
Every production provider must prove source/target matrix, malformed corpus handling, resource bounds, deterministic errors, metadata/privacy behavior, sandbox compatibility, and license/redistribution compliance. The dev.10 FFmpeg MVP proves strict-worker WAV→MP3 320k behavior only; it does not close production redistribution/compliance.

## Release-candidate gate
- [ ] Clean x64 Windows 11 VM install → use → update → uninstall.
- [ ] Headed Explorer acceptance.
- [ ] Final unit/integration/security/fuzz/chaos suites pass on the release candidate.
- [ ] Release package signed.
- [ ] Final release hash manifest, SBOM, third-party notices, known limitations, and shipping handover complete.
