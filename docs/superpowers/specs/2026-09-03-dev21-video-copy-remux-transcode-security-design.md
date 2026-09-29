# Dev.21 B8 Video Copy / Remux / Transcode security design

## Status

Initial design approval is granted. This committed design is the mandatory pre-code architecture/security gate for `0.1.0-dev.21`. Production behavior must not change until this document passes the committed-spec review gate and the implementation plan is produced with Superpowers `writing-plans`.

Converty remains **NOT CUSTOMER SHIP-READY**.

## Authority base

- Repository: `aeiouofficial/converty`
- Required dev.21 branch: `dev/0.1.0-dev.21`
- Exact frozen base commit: `8a1f46603aa842728247bc11b34fcccf121858fd`
- Exact frozen tree: `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`
- Frozen version: `0.1.0-dev.20`
- Exact-main CI: `33671671714` — `completed/success`
- Existing frozen evidence remains authoritative: `260/260` managed, `103/103` static, `5/5` contract vectors; dev.20 Video `27/27` packaged qualification plus negative/mixed-batch evidence; Audio 36-case and Image 24-case regressions.

This design creates no new release/test evidence by itself.

## Goal

Implement a bounded, typed, auditable Video planning path that selects the existing `ConversionMode.Copy`, `ConversionMode.Remux`, or `ConversionMode.Transcode` only from explicit validated probe facts and target capability policy. The purpose is to avoid unnecessary re-encoding when safety can be proven without weakening the current strict worker/provider boundary, private staging, deterministic cleanup, numbered no-overwrite publication, or existing Audio/Image behavior.

## Existing architecture to preserve

Keep the established trust flow:

`IExplorerCommand DLL -> fixed app-local Converty.Bridge.exe -> strict disposable worker boundary -> typed provider -> fixed app-local FFmpeg -> private staging -> validated transactional numbered no-overwrite publication`

Dev.21 extends this for Video with a dedicated probe boundary:

`private staged input -> strict disposable Converty.ProbeWorker.exe -> fixed app-local ffprobe.exe -> strict bounded MediaProbeResultV1 -> Core VideoPlanningPolicy -> managed Copy OR strict EngineWorker Remux/Transcode -> private staged output -> post-probe TargetMediaContract -> transactional publication`

The following are invariants, not optional implementation details:

- no shell-command construction;
- no raw FFmpeg/ffprobe argument pass-through;
- no PATH lookup or current-working-directory executable discovery;
- no arbitrary executable or plugin discovery;
- no ordinary conversion network dependency;
- no silent Strict-to-Compatibility fallback;
- no hardware acceleration in this tranche;
- no signing private keys in repository, workspace, or ordinary CI;
- no publication before all applicable validation gates pass.

## Reused domain architecture

Do not create a duplicate execution-mode or planner hierarchy.

Reuse:

- `ConversionMode` — existing values include `Copy`, `Remux`, `Transcode`, `Transform`;
- `ConversionPlan.Mode`;
- `CapabilityGraph`;
- `ConversionPlanner`.

`Transform` remains reserved for non-Video families and is not changed by B8.

Add a bounded stream-aware `VideoPlanningPolicy` that is invoked by the existing planner for Video requests. The generic capability graph remains format/provider capability authority; Video-specific stream evidence is handled by the Video policy rather than turning `CapabilityGraph` into a general media-rule engine.

## Chosen design and rejected alternatives

### Chosen: Core VideoPlanningPolicy + minimal isolated ProbeWorker

ProbeWorker translates bounded ffprobe output into strict typed facts. Core owns deterministic mode-selection policy. `Converty.Provider.FFmpeg` owns translation from `(PresetId, ConversionMode)` to an immutable fixed token vector. This gives the planner an auditable semantic result while keeping engine syntax out of Core.

### Rejected: arbitrary stream predicates inside CapabilityGraph

Rejected for dev.21 because it would make the generic capability graph a media-policy language, expand serialization/versioning scope, and reduce decision auditability.

### Rejected: provider/EngineWorker probes and chooses mode internally

Rejected because it hides safety policy inside the execution boundary, prevents Core from emitting an auditable `ConversionPlan`, and couples policy decisions to provider internals.

## Threat model

Dev.21 treats input media, container metadata, stream metadata, filenames, paths, ffprobe output, and conversion process behavior as untrusted.

Primary threats addressed by this design:

1. **Parser amplification / unbounded output** — malicious files cause excessive ffprobe stdout/stderr, oversized JSON, extreme counts/strings/numbers, malformed/truncated/trailing content, or duplicate properties.
2. **Process escape / orphaning** — ffprobe/ffmpeg descendants survive cancellation/timeout or escape Job/AppContainer restrictions.
3. **Filesystem widening** — ProbeWorker gains write access or profile/Documents visibility not needed for probing.
4. **Network widening** — probe/engine paths use DNS/TCP/HTTP or non-local protocols.
5. **Argument injection** — media metadata, probe text, user text, preset text, or paths become executable FFmpeg options.
6. **Executable substitution** — PATH/CWD/user-controlled `ffmpeg.exe` or `ffprobe.exe` is selected.
7. **False-success publication** — process exit code is zero but output violates the target contract.
8. **False Copy semantics** — a "copy" path actually invokes FFmpeg or produces bytes different from source.
9. **Ambiguous stream selection** — implicit first-stream selection loses or silently changes content.
10. **Policy-sensitive loss** — subtitles, attachments, data streams, chapters, metadata, HDR, or extra A/V streams are dropped without an explicit policy decision.
11. **Supply-chain drift** — runtime engine bytes or CI Python dependencies drift from declared authority.
12. **Release-authority mutation** — future freeze governance permits force-push/deletion or creates authority SHAs that bypass exact-candidate qualification.

## Probe contract

### MediaProbeResultV1

ProbeWorker emits a strict versioned result object; raw ffprobe JSON never crosses the worker boundary.

The contract must contain:

- schema/version discriminator fixed to V1;
- success/failure state using a closed result model;
- bounded `MediaProbeFactsV1` when successful;
- bounded semantic failure reason when unsuccessful;
- no raw command line, ffprobe JSON, arbitrary metadata/tag dictionaries, packet/frame data, stack traces, or unbounded backend strings.

### MediaProbeFactsV1

The fact model is immutable and bounded. Required semantic facts include only fields needed for B8 policy and output validation:

- canonical container identifier;
- bounded stream collection;
- per-stream index;
- stream kind (`Video`, `Audio`, `Subtitle`, `Data`, `Attachment`, `Unknown`);
- canonical codec identifier;
- bounded profile where policy requires it;
- default and attached-picture disposition;
- Video pixel format, bit depth, color transfer/HDR indicators, and bounded dimensions where applicable;
- Audio sample rate, channel count, and canonical layout where applicable;
- chapter presence;
- global metadata presence;
- bounded policy-relevant stream metadata presence;
- explicit unknown/incomplete state for required facts.

Canonical identifiers use closed enums/IDs plus `Unknown`; unknown backend strings are not preserved as executable or unbounded text.

### Hard validation rules

The serializer/parser must reject rather than normalize away:

- unsupported/future schema version;
- missing required fields;
- duplicate properties;
- unknown/extra properties where the schema does not explicitly permit them;
- trailing content;
- malformed/truncated JSON;
- stream count greater than the hard maximum;
- strings greater than their hard maximum;
- numeric overflow/extreme values outside explicit bounds;
- contradictory HDR/color facts;
- impossible/contradictory stream facts.

Boundaries require tests at exactly-max and max+1.

## ProbeWorker isolation and transport

`Converty.ProbeWorker.exe` is a disposable strict worker dedicated to probing.

Requirements:

- accepts only the exact staged input through a bounded typed invocation contract;
- resolves only fixed app-local `ffprobe.exe`;
- direct process launch only, never shell;
- no PATH/CWD/user binary fallback;
- read-only access to the exact staged input;
- no inherited staging read/write authority from EngineWorker;
- executable/tool paths are read/execute only;
- zero network capability;
- parent and descendants remain under the strict Job/AppContainer model;
- timeout/cancel kills the entire Job and all descendants;
- no orphan processes after success, failure, timeout, cancellation, malformed output, or overflow.

### Streaming stdout budget

ProbeWorker transport must consume stdout incrementally with a hard byte budget. It must not use unlimited `ReadToEnd()` followed by a size check.

- exactly `MaxStdoutBytes` is valid when the payload is otherwise valid;
- byte `MaxStdoutBytes + 1` immediately causes fail-closed termination of the Job;
- stderr has a separate independent hard budget;
- stdout overflow, stderr overflow, timeout, cancellation, malformed worker output, or transport failure produces no planning facts and no publication.

## Probe engine protocol/demuxer policy

Ordinary staged-file conversion must qualify a `file`-only protocol posture against the exact pinned ffmpeg/ffprobe engine before freeze. An exact demuxer/format whitelist must also be qualified against that engine.

Do not guess demuxer names in production policy before qualification evidence exists. Until qualification passes, the corresponding freeze gate remains open.

## Typed planning model

`ProbedFileDescriptor` receives an additive media-facts association/overload so existing Audio/Image callers remain source-compatible. Video B8 requires complete `MediaProbeFactsV1`.

Add a semantic `VideoExecutionDecision` containing:

- existing `ConversionMode`;
- bounded deterministic reason code;
- immutable target profile/contract identifier.

No process tokens, shell fragments, command fragments, user text, or raw media metadata are plan data.

Unknown, incomplete, ambiguous, contradictory, or unqualified evidence never triggers "try FFmpeg" behavior. It rejects deterministically unless the decision table explicitly identifies an allowlisted Transcode path.

## First-tranche global Video preconditions

For `video.mp4.h264` and `video.webm.vp9`, planning requires:

- exactly one non-attached-picture primary Video stream;
- zero or one primary Audio stream;
- no secondary Video or Audio streams;
- no Subtitle, Data, or Attachment streams;
- no HDR/PQ/HLG or other bounded HDR indicator;
- no high-bit-depth input outside the first-tranche policy;
- complete required codec/pixel/audio facts;
- codecs must be in the dev.20-qualified decoder allowlist for any Transcode decision.

Violation causes deterministic rejection. There is no implicit "first stream wins" or opportunistic conversion.

## Dev.20-qualified decoder allowlist for B8

Video codecs:

- `h264`
- `vp9`
- `mpeg4`
- `mpeg2video`
- `wmv2`

Audio codecs:

- `aac`
- `opus`
- `mp3`
- `mp2`
- `wmav2`

This allowlist is a dev.21 planning bound derived from already-qualified dev.20 fixtures. HEVC, AV1, and other codecs remain out of scope until separately qualified.

## Decision table: video.mp4.h264

| Preconditions | Decision | Required result |
| --- | --- | --- |
| Source container `MP4`; Video `h264`; `yuv420p`; Audio absent or `aac`, 1-2 channels, sample rate 44100 or 48000; global preconditions pass | `Copy` | Managed byte-exact staged copy; no FFmpeg process; SHA-256 equality required before publication |
| Source container is not `MP4`; same H.264/yuv420p + optional compatible AAC stream contract; global preconditions pass | `Remux` | Provider-owned fixed stream-copy token vector; no decode/re-encode |
| Passthrough contract fails, but Video/Audio codecs are within the qualified decoder allowlist and all other first-tranche facts are supported | `Transcode` | Fixed MP4 compatibility profile |
| Unknown, incomplete, ambiguous, unsupported, unqualified, HDR/high-bit-depth, extra policy-sensitive streams | Reject | No engine start and no publication |

## Decision table: video.webm.vp9

| Preconditions | Decision | Required result |
| --- | --- | --- |
| Source container `WebM`; Video `vp9`; `yuv420p`; Audio absent or `opus`, 1-2 channels, sample rate 48000; global preconditions pass | `Copy` | Managed byte-exact staged copy; no FFmpeg process; SHA-256 equality required before publication |
| Source container is not `WebM`; same VP9/yuv420p + optional compatible Opus stream contract; global preconditions pass | `Remux` | Provider-owned fixed stream-copy token vector; no decode/re-encode |
| Passthrough contract fails, but Video/Audio codecs are within the qualified decoder allowlist and all other first-tranche facts are supported | `Transcode` | Fixed WebM compatibility profile |
| Unknown, incomplete, ambiguous, unsupported, unqualified, HDR/high-bit-depth, extra policy-sensitive streams | Reject | No engine start and no publication |

## Decision table: extract.audio.mp3

| Preconditions | Decision | Required result |
| --- | --- | --- |
| Exactly one Audio stream; codec `mp3`; 1-2 channels; sample rate 32000, 44100, or 48000 | `Remux` | Fixed provider extraction/stream-copy path to MP3 |
| Exactly one Audio stream; codec in `{aac, opus, mp3, mp2, wmav2}` but MP3 passthrough contract fails | `Transcode` | Fixed MP3 compatibility profile |
| No Audio stream, more than one Audio stream, unknown/unqualified Audio codec/facts | Reject | No engine start and no publication |

`Copy` is not applicable to `extract.audio.mp3` because the advertised action consumes Video-family input and produces an audio-only MP3 output. Non-audio streams are intentionally excluded by this action; multiple Audio streams remain ambiguous and reject.

## Transcode compatibility profiles

### MP4 target

- Video: H.264 via `libx264`
- Pixel format: explicit `yuv420p`
- Encoder preset: `medium`
- CRF: `23`
- Audio when source Audio exists: AAC, 48000 Hz, stereo, 192k
- `+faststart`
- no synthetic Audio stream when source has none

### WebM target

- Video: VP9 via `libvpx-vp9`
- Pixel format: explicit `yuv420p`
- CRF: `32`
- `b:v 0`
- Audio when source Audio exists: Opus, 48000 Hz, stereo, 128k
- no synthetic Audio stream when source has none

### MP3 target

- `libmp3lame`
- 44100 Hz
- stereo
- 192k

These are deterministic compatibility outputs, not source-fidelity guarantees. Higher-channel layouts are deterministically downmixed only on an otherwise-allowed Transcode path.

## Subtitle, HDR, metadata, chapter, and non-primary policy

### Video output presets

- Subtitle streams reject.
- Secondary Video/Audio streams reject.
- Data streams reject.
- Attachment streams reject.
- HDR/PQ/HLG and other explicit HDR indicators reject.
- High-bit-depth content outside the approved first-tranche contract rejects.
- No HDR preservation or tone mapping is claimed.

### Audio extraction

Video, Subtitle, Data, and Attachment streams are intentionally excluded. Multiple Audio streams reject because no selection UX/policy exists in dev.21.

### Metadata and chapters

- `Copy` preserves all bytes, therefore metadata and chapters are preserved exactly as source bytes.
- `Remux` and `Transcode` strip global/stream metadata and chapters in this first tranche using fixed provider policy.
- Metadata preservation/settings are a later explicit UX/Settings tranche; dev.21 makes no preservation claim for Remux/Transcode.

## FFmpeg provider ownership

Core must contain no FFmpeg command policy after dev.21.

`Converty.Provider.FFmpeg` is the sole owner of a closed compiler:

`(PresetId, ConversionMode) -> immutable ArgumentList token vector`

Rules:

- closed switch/table only;
- exact supported preset/mode tuples only;
- unsupported tuple rejects before process start;
- no caller-controlled arbitrary token ingress;
- no probe/media/user string becomes a process flag;
- input/output paths remain typed positional data passed through the established safe process API;
- Remux token vectors use explicit mapping + stream copy;
- Transcode token vectors use the fixed profiles above;
- metadata/chapter stripping is explicit in provider tokens;
- no hardware-acceleration tokens.

`ProductPresetDefinition` / `ProductPresetRegistry` remain product/menu/domain declarations and lose ownership of engine token vectors.

## Copy execution semantics

`Copy` is a managed byte-for-byte staging copy. FFmpeg is never launched.

Required sequence:

1. copy exact staged input bytes to staged output using managed filesystem APIs under the existing private staging/publication model;
2. compute SHA-256 of staged input and staged output;
3. require equality;
4. require existing path/publication invariants;
5. publish only after equality succeeds.

Any hash mismatch is a conversion failure with no publication. The original source and pre-existing destinations remain unchanged.

## Remux / Transcode execution semantics

EngineWorker receives only bounded validated fields such as `--preset`, `--mode`, `--input`, and `--output`. It does not accept raw FFmpeg options.

For `Remux` / `Transcode`:

1. EngineWorker validates preset/mode combination;
2. provider compiles the exact immutable token vector;
3. fixed app-local FFmpeg launches through the strict process boundary;
4. exit code `0` alone is not authorization to publish;
5. staged output is post-probed;
6. immutable `TargetMediaContract` validates container/stream topology/codecs/pixel/audio/HDR policy;
7. only a valid target proceeds to existing transactional numbered no-overwrite publication.

A successful process that creates the wrong target is a failure and publishes nothing.

## TargetMediaContract

Each user-visible Video action has an immutable target contract. The post-probe validator must verify every field the action promises, including at minimum:

- canonical target container;
- expected primary stream count/topology;
- Video codec where applicable;
- explicit `yuv420p` where applicable;
- Audio codec/sample-rate/channel policy where applicable;
- absence of unexpected Subtitle/Data/Attachment/secondary A/V streams;
- absence of disallowed HDR/high-bit-depth state;
- audio-only shape for MP3 extraction.

The target contract is semantic data, not FFmpeg syntax.

## Fixed engine trust and package binding

Both `ffmpeg.exe` and `ffprobe.exe` must be fixed app-local paths. Missing engines, path reparse/substitution, or digest mismatch fail closed. There is no PATH fallback.

Before customer release, runtime execution must bind the engine bytes to package-manifest digest/provenance authority. Dev.21 may use the pinned development engine for qualification, but that does not close production redistribution, licensing, provenance, signature, notices, or release approval.

## Child-containment canaries

Before dev.21 merge/freeze, tests must prove that the **actual** ffmpeg/ffprobe descendants:

- remain in the expected strict Job/AppContainer containment;
- have no network capability;
- cannot read prohibited profile/Documents locations;
- cannot write outside authorized scope;
- cannot mutate ProbeWorker's read-only staged input;
- are terminated with the Job on timeout/cancel/failure;
- leave no orphan descendants.

Wrapper-only tests are insufficient for this gate.

## Repository / CI / release-path hardening

These audit findings are part of dev.21 security acceptance but must not falsify historical authority.

### Main governance

Historical dev.20 remains unsigned history. Future main/freeze governance should be compatible with the exact-candidate-SHA workflow:

- block branch deletion;
- block force-push;
- require the exact status checks used by the freeze protocol;
- preserve non-force exact-candidate fast-forward semantics;
- require verified signatures for future frozen-authority commits in a way that does not introduce a merge-generated SHA after qualification;
- restrict updates consistently with the release authority model.

### CI dependencies

Keep least-privilege GitHub Actions permissions and full-SHA action pins. Python CI dependencies added/used by the release path must be hash-locked and installed with hash verification.

### Artifact attestations

Where supported, release artifacts should gain attestations/provenance verification as a supplement to existing SBOM/hash/package authority, not a replacement.

Long-lived signing keys must not enter repository/workspace/ordinary CI. Protected release signing should use short-lived workload identity/OIDC where the signing service supports it.

## RED acceptance matrix — required before GREEN implementation

### Contracts / serialization

- exactly-max stream count accepted; max+1 rejected;
- exactly-max string accepted; max+1 rejected;
- oversized numeric/extreme dimensions rejected;
- unknown vs missing vs unsupported facts remain distinguishable;
- duplicate properties rejected;
- extra properties rejected unless explicitly schema-approved;
- future schema rejected;
- malformed/truncated/trailing JSON rejected;
- contradictory HDR/color facts rejected;
- existing Audio/Image contracts remain unchanged.

### ProbeWorker / strict launcher

- fixed app-local ffprobe only;
- PATH/CWD/user binary substitution rejected;
- exact staged input readable but not writable;
- profile/Documents read denied;
- write outside probe scope denied;
- DNS/TCP/HTTP denied;
- Unicode/metacharacter paths remain inert data;
- stdout exactly max succeeds;
- stdout max+1 kills Job and fails closed;
- stderr overflow fails closed;
- timeout kills complete Job;
- cancellation kills complete Job;
- malformed/oversized backend JSON rejects;
- no orphan processes after all outcomes;
- prohibited protocol/demuxer fails deterministically once exact whitelist is qualified.

### Core planner

Table-driven witnesses are required for:

- MP4 `Copy`;
- MP4 `Remux`;
- MP4 `Transcode`;
- WebM `Copy`;
- WebM `Remux`;
- WebM `Transcode`;
- MP3 extract `Remux`;
- MP3 extract `Transcode`;
- unsupported codec reject;
- unknown container reject;
- unknown required fact reject;
- multiple primary Video reject;
- multiple primary Audio reject where policy requires uniqueness;
- Video target Subtitle/Data/Attachment reject;
- HDR reject;
- high-bit-depth reject;
- incompatible pixfmt/layout selects Transcode only when decoder allowlisted;
- missing Audio extraction reject;
- deterministic bounded reason codes.

### Provider / EngineWorker

- exact token assertion for every supported `(PresetId, ConversionMode)` tuple;
- unsupported tuple fails before process start;
- no caller/probe/media token ingress;
- `Copy` launches no FFmpeg process;
- `Copy` output byte-identical;
- Copy hash mismatch prevents publication;
- `Remux` uses explicit stream mapping + streamcopy;
- `Transcode` uses fixed codecs/pixfmt/audio defaults;
- metadata/chapter stripping explicit;
- no hardware-acceleration tokens;
- no PATH/network fallback.

### Output authorization

- engine exit 0 + wrong container => no publication;
- wrong codec => no publication;
- wrong stream topology => no publication;
- wrong pixel format => no publication;
- wrong Audio contract => no publication;
- unexpected HDR => no publication;
- corrupt staged output => no publication;
- post-probe timeout/failure => no publication;
- no partial files;
- no orphan processes.

### Packaged / regression gates

Retain all dev.20 qualification and add mode witnesses:

- dev.20 Video `27/27` packaged matrix;
- explicit real packaged `Copy`, `Remux`, and `Transcode` witness fixtures;
- repeated malformed/truncated rejection;
- twice-run mixed valid/invalid batch continuation;
- source preservation;
- pre-existing destination preservation;
- Unicode/metacharacter paths;
- numbered no-overwrite publication;
- zero partial/orphan processes;
- Audio 36-case regression;
- Image 24-case regression;
- all managed/static/vector gates.

Static scans must expand to cover ffprobe/ProbeWorker fixed-path use, provider-only token ownership, no raw argument ingress, no hardware acceleration, no PATH fallback, and no ordinary network dependency.

## Implementation file map after committed-spec review

### Contracts

- extend `src/Converty.Contracts/Conversion/ProbedFileDescriptor.cs` additively;
- add bounded media-probe fact/result V1 types;
- keep existing `ConversionMode.cs` and `ConversionPlan.cs`.

### Serialization

- add strict ProbeResult V1 serialization/deserialization and boundary tests.

### Core planning

- add `VideoPlanningPolicy`;
- add `VideoExecutionDecision` and bounded reason codes;
- update `ConversionPlanner` / planning request only as needed to delegate Video stream policy while preserving generic capability/provider logic.

### Core execution

- add `IMediaProbeClient`;
- update `ConversionBatchRunner` for Video: stage -> probe -> plan -> execute -> post-probe/validate -> publish;
- preserve Audio/Image execution behavior.

### Core presets

- remove FFmpeg token ownership from `ProductPresetDefinition` / `ProductPresetRegistry` while preserving display/menu/source-extension/output semantics.

### ProbeWorker

- implement bounded fixed-input `Converty.ProbeWorker` program;
- add fixed ffprobe adapter boundary and strict result emission.

### Bridge / worker clients

- add `ProbeWorkerClient` using purpose-specific read-only staging ACL and bounded stdout transport;
- update `EngineWorkerClient` only for the bounded mode field;
- keep Bridge media-policy neutral.

### EngineWorker

- validate the mode;
- dispatch managed Copy vs provider Remux/Transcode;
- expose no arbitrary FFmpeg option surface.

### Converty.Provider.FFmpeg

- add fixed ffprobe path/launcher;
- add closed preset+mode argument compiler;
- update `FfmpegProcessLauncher` only within strict fixed-token/fixed-path constraints.

### Build/package

- stage ProbeWorker and ffprobe for dev.21 qualification;
- validate both fixed executable locations;
- add runtime digest/package binding as required by the production gate;
- update generated package authority only through the existing guarded workflows.

### Tests / CI / docs

- add contract/core/worker/provider/static tests and B8 packaged acceptance;
- preserve every dev.20 regression;
- update architecture/module/security documentation after verified behavior changes;
- curate release/test evidence only from actually executed gates.

## Natural implementation order after this spec is approved

1. Run Superpowers `writing-plans` and commit a detailed implementation plan.
2. RED: strict versioned probe contracts and bounded serializer.
3. RED: bounded streaming worker stdout/stderr, read-only probe ACL, zero network, timeout/cancel/overflow containment.
4. RED: provider token ownership and unsupported tuple rejection; prove Core no longer owns FFmpeg syntax.
5. GREEN: security foundations above with no planner behavior expansion beyond what RED requires.
6. RED: table-driven VideoPlanningPolicy Copy/Remux/Transcode/Reject matrix.
7. GREEN: ProbeWorker -> planner -> provider -> managed Copy / Remux / Transcode -> post-validation flow.
8. RED/GREEN: actual ffprobe/ffmpeg child-containment canaries, `file` protocol + exact demuxer qualification, engine digest binding as applicable.
9. Full managed/static/vector + packaged Video + Audio/Image regression qualification.
10. Curate version/metadata/evidence; generated authority only through guarded workflow; exact-candidate CI; independent artifact verification.
11. Freeze only through the existing non-force exact-SHA protocol after every required gate is green.
12. Fresh-read GitHub authority, reconcile Drive/Slack, process the current handover, and publish exactly one successor OPEN handover.

## Out of scope

- hardware acceleration;
- arbitrary codec auto-discovery;
- HEVC/AV1 expansion without new qualification fixtures;
- subtitle conversion/preservation;
- HDR preservation or HDR->SDR tone mapping;
- metadata preservation settings;
- multi-Audio selection UX;
- arbitrary plugin/executable discovery;
- Plugin SDK implementation;
- production FFmpeg/ffprobe redistribution approval;
- production signing/B2/MSIX lifecycle;
- headed Explorer acceptance;
- final fuzz/chaos/security/end-user release acceptance.

The Plugin SDK remains quarantined until a separate versioned manifest/API, publisher/signature/hash gate, compatibility gate, and worker-only execution trust architecture is approved. No in-process untrusted plugins.

## Release limitations that remain open

Dev.21 is not a customer-ship gate by itself. The following remain open even if B8 passes:

- production ffmpeg+ffprobe source/version/hash/signature/provenance/license/notices/redistribution approval;
- runtime engine package-digest/provenance binding evidence;
- signed-package B2 identity/authentication requalification;
- signed MSIX clean-Windows install/update/uninstall/rollback lifecycle;
- headed Windows 11 modern Explorer exact-build/crash/hang/failure evidence;
- production artifact-attestation evidence;
- final fuzz/property/chaos/security/release/end-user acceptance;
- separately approved Plugin SDK trust architecture.

## Completion criteria for dev.21

Dev.21 must not be marked frozen until:

- this design passed committed-spec review;
- the implementation plan was committed;
- RED evidence preceded corresponding GREEN implementation;
- all planner/security/containment/output-validation gates passed;
- complete dev.20 regressions passed;
- version/metadata/generated authority are coherent;
- branch zero-diff/deterministic qualification required by the freeze protocol passed;
- exact-candidate release evidence is verified;
- non-force exact-SHA promotion rules are satisfied;
- fresh exact-main continuity/static/managed qualification succeeds;
- final workspace/delivery artifacts are independently verified;
- GitHub, Drive, Slack, evidence, changelog, roadmap, plan, tasks, and recursive handover are reconciled.

No release or test evidence may be inferred from this design document.
