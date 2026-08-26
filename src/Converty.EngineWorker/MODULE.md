# Converty.EngineWorker

**Planned batch:** B4/B5 worker migration
**Status in 0.1.0-dev.10:** process-separation implementation candidate; restricted-launch qualification pending B4 containment work.

Purpose: disposable conversion-engine process. The worker accepts only a fixed `--preset <id> --input <absolute staged path> --output <absolute staged path>` surface, reconstructs the preset from the checked-in registry, requires input/output to share one private staging directory, and delegates engine execution to `Converty.Provider.FFmpeg`.

It never receives the final publication path and cannot accept raw FFmpeg argument vectors. The Bridge-side launcher is responsible for creating the disposable process and will be hardened with the strict/compatibility isolation policy, Job Object and resource/network/filesystem controls in the remainder of B4. This module alone is not evidence that strict containment has passed.
