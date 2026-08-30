# Dev.16 Audio input acceptance — TDD and defect evidence

## Scope
Qualify representative Audio source formats and malformed/truncated inputs across the fixed Audio action matrix through the packaged Bridge→Strict Worker/provider→FFmpeg path. Preserve all trust boundaries and transactional publication invariants.

## RED #1 — missing acceptance capability
- Commit: `251b1c54901d212e03961e6bed947bc828df6bc7`
- CI run: `33339926916`
- Static job: `99333594749`
- Result: 81 existing static tests PASS; exactly 3 new dev.16 assertions FAIL because the dedicated acceptance smoke/CI wiring did not exist.

## Initial implementation and discovered defect
- Commit: `86c6352ce12f2c492d4065b4c15a78223d1d2aab`
- CI run: `33340046236`
- Static: 84/84 PASS before expected generated-authority freshness failure.
- Windows product path reached the new matrix after restore/audit/build/native/package/COM/product gates passed.
- All 36 valid source/action combinations passed.
- First malformed WAV conversion legitimately failed inside the conversion path, but `Converty.Bridge.exe` remained alive beyond 30 seconds.

## Root cause
`Program.cs` correctly caught the failed conversion and called `BridgeErrorDialog.Show`. That function synchronously invoked Win32 `MessageBoxW`. On a noninteractive runner there was no user to dismiss the dialog, so automation observed a process hang even though worker/provider conversion failure had already returned. Worker containment, FFmpeg stdin policy and process timeout logic were not the cause.

## RED #2 — lock the lifecycle requirement
- Commit: `673f92e43738554db364a8db5ea44a00cdd903b7`
- CI run: `33340234688`
- Static job: `99334412200`
- Result: 84 existing static tests PASS; exactly 1 new assertion FAIL because an explicit noninteractive error reporter did not yet exist.

## Fix
- `066925fa5c90f4bcaf581590c5193a44b64cb4e9` — `BridgeErrorDialog` recognizes exact opt-in `CONVERTY_BRIDGE_NONINTERACTIVE=1`; it writes the already-bounded message to stderr and returns. Default Explorer behavior still calls `MessageBoxW`.
- `061ad75600fee6fd4b34e4a24bd8d571ac17ce90` — the acceptance harness opts its child Bridge processes into noninteractive reporting. Conversion arguments remain structured `ArgumentList` values.

## GREEN behavior evidence
- Run: `33340338502`
- Managed job: `99334697033`
- Static job: `99334696969`
- 36/36 valid conversions PASS: six sources × six fixed Audio actions.
- Malformed WAV: two attempts, deterministic exit code 4.
- Truncated FLAC: two attempts, deterministic exit code 4.
- Every positive and negative case preserves source bytes; pre-existing destinations remain unchanged; negative cases publish nothing; no `.converty-*.partial.*` remains.
- Managed tests: 253/253 PASS, 0 skipped.
- Static tests: 85/85 PASS.
- Contract vectors: 5/5 PASS.
- Dependency audit: 18 projects / 18 frameworks / 0 vulnerable-result packages.
- Release build: 0 warnings / 0 errors.
- Native Explorer, MakeAppx development package, direct shell Invoke, registered COM Invoke and existing four-target product smoke PASS.
- Deterministic pre-authority workspace A/B SHA-256: `27b6f96ea8c42afee8de2d67a2ea9d43f48607ab13a4b124cbab6acd3b55a643`, 436519 bytes, 358 entries. Integrity then failed at `.github/workflows/ci.yml` only because generated authority still described dev.15.

## Security conclusion
The defect fix changes only how an already-detected Bridge failure is reported to an explicitly noninteractive caller. It does not add media parsing to Bridge, expose FFmpeg arguments, permit arbitrary executables/PATH lookup, add network behavior, weaken worker containment, change publication semantics or disable the user-facing Explorer dialog.
