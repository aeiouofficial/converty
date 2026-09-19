# AsmJit Readiness Plan — converty

Status: PREPARED_ONLY
Branch: `prep/asmjit-readiness-2026-09-19`
Decision: DO NOT INTEGRATE under current architecture.

Converty delegates media conversion to FFmpeg/provider workers. Codec performance belongs in FFmpeg/native codec implementations, process orchestration, I/O, staging, and worker lifecycle—not in a second custom JIT layer.

## Preferred optimization path
Profile bridge/worker startup, file I/O, FFmpeg invocation, pipeline concurrency, probe overhead, staging/publication, and codec presets.

## Revisit trigger
Only an approved custom runtime-generated compute kernel outside FFmpeg with benchmark evidence.

No implementation, dependency addition, PR, or merge on this branch.
