# FileConvert.ShellExtension — planned B3 module

**Status:** topology only in `0.1.0-dev.4`; no DLL or COM implementation is claimed.

Locked responsibilities:
- Native C++ `IExplorerCommand` implementation for the modern Windows 11 context menu.
- Cheap title/icon/state/subcommand work only.
- Gather selected shell items and hand a bounded request to the signed Bridge.

Forbidden responsibilities:
- Media probing/parsing.
- FFmpeg/WIC/plugin loading.
- Network access.
- Long-running work.
- Writing user outputs.

B3 must add headed Explorer acceptance evidence before this module can be called implemented.
