# Converty dev.20 Video Qualification Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Qualify Converty's existing nine-extension, three-action Video product surface through the packaged strict Bridge/Worker/FFmpeg path with deterministic positive, negative, mixed-batch, codec, preservation, cleanup, CI, and exact-main evidence.

**Architecture:** Keep the existing production architecture unchanged. Add test/evidence surfaces around the fixed `ProductPresetRegistry` Video presets, generate tiny deterministic development fixtures with the pinned development FFmpeg, invoke only the staged `Converty.Bridge.exe`, validate outputs with pinned `ffprobe`, and preserve the existing transactional publication and worker-isolation boundaries.

**Tech Stack:** .NET 10 / xUnit, PowerShell 7 on `windows-2025`, Python 3.13 / pytest static contracts, FFmpeg/ffprobe pinned by the existing development preparation script, GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-09-02-dev20-video-qualification-design.md`

## Global Constraints

- Required branch base: `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`, tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`.
- Target version: `0.1.0-dev.20`.
- Source extensions: `.mp4`, `.mov`, `.mkv`, `.avi`, `.webm`, `.m4v`, `.mpeg`, `.mpg`, `.wmv`.
- Fixed actions only: `video.mp4.h264`, `video.webm.vp9`, `extract.audio.mp3`.
- Preserve `IExplorerCommand DLL -> fixed app-local Bridge -> strict disposable EngineWorker/provider -> fixed app-local FFmpeg -> private staging -> validated transactional numbered no-overwrite publication`.
- Never introduce shell command construction, raw FFmpeg pass-through, PATH lookup, arbitrary converter/plugin discovery, ordinary conversion network dependency, silent Strict-to-Compatibility fallback, or repository signing keys.
- Preserve all Audio and Image acceptance/batch gates.
- Preserve failed/RED evidence. A green side branch is not release authority.

---

### Task 1: Commit the RED dev.20 static contract

**Files:**
- Create: `tests/static/test_dev20_video_qualification.py`
- Reference: `build/audio-input-acceptance-smoke.ps1`
- Reference: `build/image-batch-isolation-smoke.ps1`
- Reference: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: repository file layout and fixed dev.20 spec.
- Produces: static requirements that fail until both Video harnesses and CI wiring exist.

- [ ] **Step 1: Write the failing static test**

Create `tests/static/test_dev20_video_qualification.py` with explicit assertions for:

```python
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ACCEPTANCE = ROOT / "build" / "video-input-acceptance-smoke.ps1"
BATCH = ROOT / "build" / "video-batch-isolation-smoke.ps1"
CI = ROOT / ".github" / "workflows" / "ci.yml"

VIDEO_EXTENSIONS = (".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v", ".mpeg", ".mpg", ".wmv")
VIDEO_PRESETS = ("video.mp4.h264", "video.webm.vp9", "extract.audio.mp3")


def test_dev20_video_acceptance_and_batch_gates_exist_and_are_wired():
    assert ACCEPTANCE.is_file(), "dev.20 Video source acceptance smoke is missing"
    assert BATCH.is_file(), "dev.20 Video mixed-batch smoke is missing"
    ci = CI.read_text(encoding="utf-8")
    assert "Video source and malformed-input acceptance" in ci
    assert "./build/video-input-acceptance-smoke.ps1" in ci
    assert "Video mixed-batch failure isolation" in ci
    assert "./build/video-batch-isolation-smoke.ps1" in ci


def test_dev20_acceptance_covers_all_sources_actions_and_probe_contracts():
    smoke = ACCEPTANCE.read_text(encoding="utf-8").lower()
    for extension in VIDEO_EXTENSIONS:
        assert extension in smoke
    for preset in VIDEO_PRESETS:
        assert preset in smoke
    for token in ("ffprobe", "codec_name", "h264", "aac", "vp9", "opus", "mp3", "malformed", "truncated", "foreach ($attempt in 1..2)"):
        assert token in smoke


def test_dev20_acceptance_locks_transactional_path_invariants():
    smoke = ACCEPTANCE.read_text(encoding="utf-8").lower()
    for token in ("hör", "& semi;", "get-filehash", "pre-existing destination", "numbered", ".converty-*.partial.*", "argumentlist.add('--preset')", "converty_bridge_noninteractive"):
        assert token in smoke


def test_dev20_batch_locks_failure_isolation_and_orphan_cleanup():
    smoke = BATCH.read_text(encoding="utf-8").lower()
    for token in ("valid-mp4", "malformed-avi", "valid-mov", "truncated-mkv", "valid-webm", "foreach ($attempt in 1..2)", "exit code 4", ".converty-*.partial.*", "get-ciminstance win32_process", "converty.engineworker", "ffmpeg"):
        assert token in smoke
```

Also assert the new Video CI steps occur after the existing Image mixed-batch step and before `- name: Test` by comparing `str.index` positions.

- [ ] **Step 2: Run the static contract and verify RED**

Run in CI/static environment:

```bash
python -m pytest -q tests/static/test_dev20_video_qualification.py
```

Expected: FAIL because `build/video-input-acceptance-smoke.ps1` and `build/video-batch-isolation-smoke.ps1` do not yet exist and CI has no Video steps.

- [ ] **Step 3: Commit preserved RED evidence source**

```bash
git add tests/static/test_dev20_video_qualification.py
git commit -m "test: define dev20 video qualification contract"
```

Record the exact commit and failing workflow/job IDs before moving to GREEN.

---

### Task 2: Pin existing Video preset behavior with managed characterization

**Files:**
- Modify: `tests/Converty.Core.Tests/Presets/ProductPresetRegistryTests.cs`
- Production reference only: `src/Converty.Core/Presets/ProductPresetRegistry.cs`
- Production reference only: `src/Converty.Core/Presets/ProductPresetDefinition.cs`

**Interfaces:**
- Consumes: existing `ProductPresetRegistry.Default` and `ProductPresetDefinition` public properties.
- Produces: managed regression authority for exact Video extensions, target output extensions, and FFmpeg token sequences.

- [ ] **Step 1: Add characterization tests before any production-code edit**

Add tests equivalent to:

```csharp
[Fact]
public void VideoPresetsSupportExactlyTheNineAdvertisedSourceExtensions()
{
    string[] expected = [".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v", ".mpeg", ".mpg", ".wmv"];
    foreach (string id in new[] { "video.mp4.h264", "video.webm.vp9", "extract.audio.mp3" })
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse(id));
        Assert.Equal(expected, preset.InputExtensions);
        Assert.All(expected, extension => Assert.True(preset.SupportsPath("clip" + extension)));
    }
}

[Fact]
public void VideoMp4PresetUsesExactDev20EncodingContract()
{
    ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.mp4.h264"));
    Assert.Equal(".mp4", preset.OutputExtension);
    Assert.Equal(
        ["-map", "0:v:0?", "-map", "0:a:0?", "-c:v", "libx264", "-preset", "medium", "-crf", "23", "-c:a", "aac", "-b:a", "192k", "-movflags", "+faststart"],
        preset.FfmpegArgumentsAfterInput);
}

[Fact]
public void VideoWebmPresetUsesExactDev20EncodingContract()
{
    ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.webm.vp9"));
    Assert.Equal(".webm", preset.OutputExtension);
    Assert.Equal(
        ["-map", "0:v:0?", "-map", "0:a:0?", "-c:v", "libvpx-vp9", "-crf", "32", "-b:v", "0", "-c:a", "libopus", "-b:a", "128k"],
        preset.FfmpegArgumentsAfterInput);
}

[Fact]
public void ExtractAudioMp3PresetUsesExactDev20EncodingContract()
{
    ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("extract.audio.mp3"));
    Assert.Equal(".mp3", preset.OutputExtension);
    Assert.Equal(["-vn", "-c:a", "libmp3lame", "-b:a", "192k"], preset.FfmpegArgumentsAfterInput);
}
```

Extend the existing path-token test or add a Video-specific equivalent using a Unicode/metacharacter input/output path and assert each literal path appears exactly once, with no `cmd.exe` or `powershell` token.

- [ ] **Step 2: Run managed tests**

```powershell
./build/test.ps1 -Configuration Release
```

Expected: the new tests pass against the already-existing fixed Video registry. If a test exposes a real contract mismatch, stop and enter a normal RED->minimal production fix cycle; do not weaken the test.

- [ ] **Step 3: Commit characterization**

```bash
git add tests/Converty.Core.Tests/Presets/ProductPresetRegistryTests.cs
git commit -m "test: pin dev20 video preset contracts"
```

---

### Task 3: Implement the 27-case packaged Video acceptance harness

**Files:**
- Create: `build/video-input-acceptance-smoke.ps1`
- Reference: `build/audio-input-acceptance-smoke.ps1`

**Interfaces:**
- Consumes: staged `Converty.Bridge.exe`, staged app-local `tools/ffmpeg/ffmpeg.exe`, pinned development `ffprobe.exe`.
- Produces: 27 successful packaged conversions plus two repeated deterministic negative cases.

- [ ] **Step 1: Build structured-process helpers**

Reuse the established no-shell `System.Diagnostics.ProcessStartInfo` pattern with `UseShellExecute = $false`, `ArgumentList.Add(...)`, redirected stdout/stderr, 30-second bounded waits, and `CONVERTY_BRIDGE_NONINTERACTIVE=1` for Bridge invocations.

Define helpers:

```powershell
Invoke-StructuredProcess -FileName <path> -Arguments <string[]> -WorkingDirectory <path>
Invoke-Bridge -PresetId <id> -InputPath <path>
Assert-NoPartialOutputs -Directory <path>
Get-StreamCodecNames -Path <path>
```

`Get-StreamCodecNames` must use pinned `ffprobe` and return video and audio codec arrays separately.

- [ ] **Step 2: Generate nine real deterministic fixtures**

Use lavfi sources:

```text
video: testsrc2=size=64x48:rate=10
 audio: sine=frequency=440:sample_rate=44100
 duration: 0.25 seconds
```

Use structured FFmpeg tokens with `-shortest -t 0.25 -y`. Encode/mux each extension exactly as defined in the spec. Do not create one payload and rename it.

- [ ] **Step 3: Define three targets**

```powershell
$targets = @(
    [pscustomobject]@{ PresetId='video.mp4.h264'; Extension='.mp4'; VideoCodec='h264'; AudioCodec='aac'; ExpectVideo=$true },
    [pscustomobject]@{ PresetId='video.webm.vp9'; Extension='.webm'; VideoCodec='vp9'; AudioCodec='opus'; ExpectVideo=$true },
    [pscustomobject]@{ PresetId='extract.audio.mp3'; Extension='.mp3'; VideoCodec=$null; AudioCodec='mp3'; ExpectVideo=$false }
)
```

- [ ] **Step 4: Run the full 27-case matrix**

For each fixture/target pair, copy to a case directory as `Hör clip & semi; -dash [x]<extension>`, hash source, reserve a base destination only when it does not alias the source, invoke Bridge, require exit `0`, and require numbered publication.

The collision guard must be equivalent to:

```powershell
if (-not [string]::Equals($baseOutput, $inputPath, [StringComparison]::OrdinalIgnoreCase)) {
    [System.IO.File]::WriteAllBytes($baseOutput, [byte[]](17,34,51,68))
}
```

Require source and reserved destination hashes unchanged and zero partials.

- [ ] **Step 5: ffprobe every successful output**

Require:

```text
video.mp4.h264  -> video h264, audio aac
video.webm.vp9  -> video vp9, audio opus
extract.audio.mp3 -> no video stream, audio mp3
```

- [ ] **Step 6: Add repeated malformed and physically truncated negatives**

Malformed: invalid bytes in an advertised `.avi` file.

Truncated: create a valid `.mkv` fixture, then write only a small leading prefix (for example first 16 bytes) to a separate `.mkv` input.

Each negative runs twice and must preserve source/reserved destination hashes, publish nothing, leave zero partials, and return the same non-zero code both times.

- [ ] **Step 7: Execute on Windows CI and make the acceptance portion GREEN**

The static test remains RED until CI wiring and the batch harness are also present. Use the standalone PowerShell script in a Windows job or temporary diagnostic commit if necessary; preserve any failed run evidence.

- [ ] **Step 8: Commit the acceptance harness**

```bash
git add build/video-input-acceptance-smoke.ps1
git commit -m "test: qualify packaged video source conversions"
```

---

### Task 4: Implement repeated mixed Video batch failure isolation

**Files:**
- Create: `build/video-batch-isolation-smoke.ps1`
- Reference: `build/image-batch-isolation-smoke.ps1`
- Reference: `build/audio-batch-isolation-smoke.ps1`

**Interfaces:**
- Consumes: staged Bridge and deterministic Video fixtures.
- Produces: two runs of one five-member Bridge batch with aggregate exit `4` and post-run process/staging checks.

- [ ] **Step 1: Create the five ordered sources**

Use case names containing these stable tokens for static evidence:

```text
valid-mp4
malformed-avi
valid-mov
truncated-mkv
valid-webm
```

All valid files contain video+audio. Malformed/truncated inputs use real advertised extensions.

- [ ] **Step 2: Invoke one Bridge process for all five members**

Build arguments as:

```text
--preset video.mp4.h264 -- <path1> <path2> <path3> <path4> <path5>
```

Use `ArgumentList.Add` for every token/path and `CONVERTY_BRIDGE_NONINTERACTIVE=1`.

- [ ] **Step 3: Repeat the batch twice**

Use `foreach ($attempt in 1..2)` and require bounded completion.

- [ ] **Step 4: Assert failure isolation**

For each attempt require exactly `exit code 4`, successful output for valid members before and after failures, no output for malformed/truncated members, source/reserved destination preservation, zero partials, and no abort before the later valid members publish.

- [ ] **Step 5: Assert no orphan worker/FFmpeg processes**

Snapshot or inspect `Win32_Process` after the Bridge exits. Fail if any process whose executable/name or command line identifies the case-local `Converty.EngineWorker` or staged `ffmpeg` remains.

- [ ] **Step 6: Commit mixed-batch harness**

```bash
git add build/video-batch-isolation-smoke.ps1
git commit -m "test: prove video mixed-batch failure isolation"
```

---

### Task 5: Wire Video gates into Windows CI and close static RED

**Files:**
- Modify: `.github/workflows/ci.yml`
- Test: `tests/static/test_dev20_video_qualification.py`

**Interfaces:**
- Consumes: new Video PowerShell harnesses.
- Produces: ordinary push/PR CI execution order with Video gates after Image and before managed tests.

- [ ] **Step 1: Add two managed-job steps**

Insert after Image mixed-batch isolation and before `Test`:

```yaml
      - name: Video source and malformed-input acceptance
        shell: pwsh
        run: ./build/video-input-acceptance-smoke.ps1
      - name: Video mixed-batch failure isolation
        shell: pwsh
        run: ./build/video-batch-isolation-smoke.ps1
```

- [ ] **Step 2: Run static test and full static suite**

```bash
python -m pytest -q tests/static/test_dev20_video_qualification.py
python -m pytest -q tests/static
```

Expected: PASS.

- [ ] **Step 3: Commit CI wiring**

```bash
git add .github/workflows/ci.yml
git commit -m "ci: gate packaged video qualification"
```

---

### Task 6: Run branch behavior qualification and fix only evidence-backed defects

**Files:**
- Modify only files implicated by failed tests.
- If a production defect is found, add/revise a failing test before the minimal production fix.

**Interfaces:**
- Consumes: ordinary branch CI.
- Produces: exact branch SHA plus continuity/static/managed run/job evidence.

- [ ] **Step 1: Wait for ordinary push CI on the exact candidate SHA**

Require all three jobs to finish; a side-branch continuity job may validate branch ancestry but is not final main authority.

- [ ] **Step 2: Inspect any Video failure with systematic debugging**

Do not guess. Preserve failing run/job IDs and identify whether the defect is fixture generation, harness expectation, product behavior, or pinned FFmpeg capability.

- [ ] **Step 3: For any product defect, write a failing regression test first**

Run/observe RED, apply the smallest production fix, then rerun affected and full gates.

- [ ] **Step 4: Require Audio/Image regressions remain green**

Explicitly verify the existing Audio 36-case acceptance + mixed batch and Image 24-case acceptance + mixed batch steps still pass in the same Windows job.

---

### Task 7: Bump dev.20 metadata and generate deterministic authority

**Files:**
- Modify: `VERSION`
- Modify curated release/evidence docs that explicitly carry the version only when required by repository conventions.
- Generated by CI only: `machine-readable/source_sbom.spdx.json`
- Generated by CI only: `machine-readable/release_sbom.spdx.json`
- Generated by CI only: `machine-readable/package_manifest.json`
- Generated by CI only: `SHA256SUMS.txt`

**Interfaces:**
- Consumes: green behavior candidate.
- Produces: `0.1.0-dev.20` generated authority candidate.

- [ ] **Step 1: Set `VERSION` to exactly `0.1.0-dev.20`**

Do not hand-edit generated authority files.

- [ ] **Step 2: Run ordinary CI generation**

Require the supply-chain/static job to upload `converty-generated-authority-<sha>` containing exactly the four generated authority files.

- [ ] **Step 3: Independently verify artifact**

Verify digest, archive CRC, exact four-member set, and embedded version before synchronization.

- [ ] **Step 4: Synchronize only through the guarded exact-parent authority workflow**

Use the repository's existing exact-parent/self-deleting synchronization mechanism. Reject stale-parent or extra-file changes.

- [ ] **Step 5: Require generated-authority zero-diff on the synchronized branch**

Ordinary CI must regenerate and report no diff for all four tracked generated files.

---

### Task 8: Final branch qualification, non-force main promotion, and exact-main proof

**Files:**
- No new implementation files unless an evidence-backed failure requires a TDD fix.

**Interfaces:**
- Consumes: synchronized green branch candidate.
- Produces: exact promoted `main` SHA/tree, fresh three-job exact-main evidence, independently verified workspace and delivery artifacts.

- [ ] **Step 1: Require full branch qualification**

Record exact branch SHA/tree, continuity/static/managed workflow/job IDs, managed test count, Video 27-case/negative/mixed results, Audio/Image regression results, workspace hash/bytes/entry counts, generated-authority artifact ID/digest, and verified-delivery candidate ID/digest.

- [ ] **Step 2: Re-read live `main` immediately before promotion**

Require it still equals the expected base/authority parent. Verify candidate is a strict descendant.

- [ ] **Step 3: Fast-forward `main` non-force only**

Do not force push and do not merge an unrelated lineage.

- [ ] **Step 4: Run fresh ordinary CI on exact promoted `main`**

Require continuity + static/supply-chain + managed SUCCESS on the exact same SHA.

- [ ] **Step 5: Independently verify exact-main final workspace and delivery**

Verify generated authority and delivery artifact identity/digests against the exact-main SHA, not a side branch.

- [ ] **Step 6: Re-read live refs**

Only now may dev.20 be called frozen. Do not call Converty generally ship-ready; the remaining production/headed/security/end-user gates stay open.

---

### Task 9: Reconcile persistent documentation and rotate the single OPEN handover

**Files/Surfaces:**
- GitHub repository documentation map where applicable
- Slack `#proj-converty`, `#roadmap-converty`, `#plan-converty`, `#tasks-converty`, `#changelog-converty`, `#handover-open-converty`
- Google Drive canonical Authority, Roadmap, Current Implementation Plan, Tasks/Gates, Changelog, Release/Test Evidence, Recursive Handover

**Interfaces:**
- Consumes: freshly re-read final GitHub authority and CI evidence.
- Produces: coherent cloud state with exactly one successor OPEN handover.

- [ ] **Step 1: Re-read GitHub authority and CI one final time**

- [ ] **Step 2: Update canonical Slack anchors and Drive documents in place**

Record exact SHA/tree/version/run/job/artifact/digest/test evidence and what remains unverified.

- [ ] **Step 3: Mark the current OPEN handover `PROCESSED`**

Include the exact successor handover number and Slack timestamp/reference.

- [ ] **Step 4: Publish exactly one new `OPEN` handover**

It must be context-free and include current authority, completed work, tests/evidence, blockers, invariants, precise next executable task, acceptance criteria, and Slack/Drive/GitHub references.

- [ ] **Step 5: Verify there is exactly one OPEN handover**

Read the handover channel and Recursive Handover again. Correct any duplicate/current-state conflict before ending the work block.
