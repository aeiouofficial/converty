# Converty.Provider.FFmpeg

**Planned batch:** B4/B5 worker migration
**Status in 0.1.0-dev.10:** implementation candidate; qualification pending CI.

Purpose: worker-side FFmpeg provider. This is the only first-party production module that creates the FFmpeg process. It resolves only the fixed app-local `tools/ffmpeg/ffmpeg.exe`, rejects reparse-point trust roots, builds arguments only from the checked-in typed `ProductPresetDefinition`, uses `ProcessStartInfo.ArgumentList` with `UseShellExecute=false`, captures bounded stderr, and enforces a finite execution timeout.

The provider is referenced by `Converty.EngineWorker`, not by Explorer, Bridge, Host, or Core. Worker sandbox/resource containment is owned by the launcher/security boundary and is qualified separately; this module does not claim isolation by itself.
