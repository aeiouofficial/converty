# Converty.ProbeWorker

**Planned batch:** dev.21 B8 Video Copy/Remux/Transcode security foundation
**Status:** implementation candidate; real packaged ffprobe containment/protocol qualification remains required before dev.21 freeze.

Purpose: disposable strict media-probe process. The worker accepts only `--input <absolute staged path>`, receives read-only access to that exact staged file from the Bridge-side strict worker launcher, resolves only the fixed app-local `tools/ffmpeg/ffprobe.exe`, and converts bounded raw ffprobe JSON into the closed `MediaProbeResultV1` contract.

Raw ffprobe JSON, backend metadata text, shell fragments, PATH lookup and arbitrary process options never cross this worker boundary. The provider owns the fixed ffprobe token vector, including the `file`-only protocol whitelist. Timeout/output-overflow failures produce bounded semantic results and the surrounding strict Job/AppContainer boundary remains responsible for descendant containment. This module is not by itself evidence that the final real ffprobe containment/protocol gates have passed.
