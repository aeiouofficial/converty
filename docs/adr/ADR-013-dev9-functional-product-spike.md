# ADR-013 — dev.9 minimum-functional-product spike

**Status:** Accepted for `0.1.0-dev.9` development qualification only. This is not release approval.

## Context

The original sequencing required full B2 identity/session closure and B4 worker containment before B3 Explorer integration or B5 media-engine execution. During dev.9, product direction explicitly changed: prove the minimum useful Windows 11 experience first — File Explorer context menu → known preset → conversion → safe output — and postpone final signing, licensing, update infrastructure, and the remaining containment architecture until that path is real and testable.

Continuing to describe B3/B5 as completely blocked would make the repository authority contradict the approved dev.9 implementation and would encourage later work to revert the product spike.

## Decision

Dev.9 may implement and qualify a narrowly bounded functional-product path before B2/B4 are closed, subject to all of these constraints:

1. The Explorer DLL remains trigger-only. It classifies the selected filesystem files, exposes fixed product preset IDs, and launches only the fixed app-local `Converty.Bridge.exe`. It does not parse media, construct FFmpeg arguments, probe media, load codecs/plugins, or perform network work.
2. The dev.9 Bridge/Core path may launch only the fixed app-local `tools/ffmpeg/ffmpeg.exe` through the dedicated `FfmpegProcessLauncher`; PATH lookup, shell execution, command strings, and caller-supplied FFmpeg argument vectors remain forbidden.
3. Conversion arguments come only from typed, checked-in product preset definitions. Explorer knows preset IDs and display metadata only.
4. Output remains non-destructive. The current product spike writes a sibling output and uses numbered-copy collision resolution rather than overwriting an existing destination.
5. The FFmpeg archive pin is development-qualification input only. Its hash must be verified before use; its GPLv3 vendor build is not by itself a production redistribution decision.
6. The MSIX identity currently in the repository is development identity only. Production publisher/signing authority is still deferred.
7. Host remains outside the media-engine path. This spike does not authorize media parsing, codec loading, FFmpeg execution, or arbitrary process launch inside `Converty.Host`.
8. B2 anti-squatting/session closure, B4 disposable-worker containment, network/write canaries, production provider provenance, licensing/notices, self-contained deployment, signing, update/uninstall qualification, and headed Windows 11 acceptance all remain open release work.
9. No B3/B5 checkbox may be marked complete merely because source code exists. Qualification requires executable evidence from the exact source revision; visible Explorer acceptance additionally requires a headed Windows 11 client.

## Qualification target

The dev.9 non-headed functional gate is, in order:

- Release managed build;
- hardened Release MSVC shell DLL build;
- SHA-verified development FFmpeg acquisition;
- development MSIX schema/layout validation;
- direct load of the staged shell DLL, exported COM class-factory activation, `IExplorerCommand` enumeration and `Invoke` producing a real output through the staged Bridge/FFmpeg path;
- loose package registration, packaged COM activation, and the same `Invoke` conversion;
- independent Bridge → FFmpeg Unicode/metacharacter/collision smoke;
- managed/static regression gates.

The headed gate remains separate: Windows 11 File Explorer must actually display the Converty command/flyout for supported selections, hide or omit unsupported actions as designed, survive Explorer restart/failure cases, and create the expected output after a user command invocation.

## Consequences

This decision intentionally trades final containment architecture for earlier end-to-end product evidence in dev.9. The trade is limited to development qualification; it does not lower the final release gates. When B4 is implemented, the conversion execution can move behind the planned disposable-worker boundary without changing the Explorer preset contract or user-facing workflow.
