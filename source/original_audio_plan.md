For the exact UX you described, I’d target **Windows 11 first** and make it feel native:

> Right-click `chapter-2.wav` → **Convert to MP3 (HQ)** → conversion starts → `chapter-2.mp3` appears beside it.

The key architectural rule is: **Explorer integration should only receive the click and hand the job off. FFmpeg must run in a separate process.** Microsoft explicitly recommends keeping Explorer shell-extension methods fast and doing expensive work only after invocation.

This preserved historical planning note is the audio-only precursor to the generic FileConvert architecture. Its key decisions remain authoritative where the newer master plan does not supersede them: native Windows 11 `IExplorerCommand`, Explorer trigger-only behavior, external worker execution, same-folder safe output, numbered-copy collision handling, multi-select, bundled/pinned FFmpeg, hidden console, machine-readable progress, and MSIX/package identity for the modern menu.

See `docs/FileConvert_Master_Build_Plan.md` for the current generalized audio/image/video/future-filetype architecture and security boundaries.
