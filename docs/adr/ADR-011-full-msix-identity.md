# Full MSIX as the production Windows identity authority

**Status:** Accepted for Converty production packaging and B2 server-identity design.

## Decision

Converty will use a **full-trust MSIX package** as the production Windows package-identity authority for the native Explorer command, Bridge and Host payload.

The production Bridge must authenticate each connected Host pipe session against the same package-family identity as the packaged Bridge and the exact trusted `Converty.Host.exe` installation path before it writes the first application frame.

The package publisher/signing identity remains externally provisioned release authority. Production signing acceptance is a separate release gate; private signing keys never enter the repository or workspace, and unsigned CI binaries are never described as production-signed.

## Rationale

Windows 11 modern File Explorer context-menu integration already uses packaged app identity for `IExplorerCommand`, `windows.comServer` and `windows.fileExplorerContextMenus`. A full MSIX keeps the executable payload inside the package instead of splitting package identity from an externally installed executable location as sparse/external-location packaging does.

Windows package-family identity is stable across package versions and binds the package name to publisher identity. For a full package, the Host executable path and package identity therefore form one OS-backed installation authority rather than an application-defined shared secret.

## Consequences

- B2 reciprocal server authentication may rely on the connected pipe's server PID, exact trusted Host image path and matching package-family identity.
- An unpackaged process is not production-authenticated Converty and must fail closed in the production verifier.
- Development/unit tests use injected verifier fakes; this is test infrastructure, not a production bypass.
- A package-family match alone is insufficient; exact trusted Host path is also required.
- A trusted path alone is insufficient; the connected server process must also carry the expected package identity.
- Sparse/external-location packaging is not the production default. Reversing that choice requires a new ADR and threat-model/server-auth review.
- Release signing and timestamp verification remain distinct supply-chain gates.

## Platform references

- https://learn.microsoft.com/windows/apps/desktop/modernize/integrate-packaged-app-with-file-explorer
- https://learn.microsoft.com/windows/apps/package-and-deploy/packaging/
- https://learn.microsoft.com/windows/msix/desktop/desktop-to-uwp-behind-the-scenes
- https://learn.microsoft.com/windows/msix/package/sign-msix-package-guide
- https://learn.microsoft.com/windows/win32/api/winbase/nf-winbase-getnamedpipeserverprocessid
- https://learn.microsoft.com/windows/win32/api/appmodel/nf-appmodel-getpackagefamilyname
- https://learn.microsoft.com/windows/win32/api/appmodel/nf-appmodel-getcurrentpackagefamilyname
