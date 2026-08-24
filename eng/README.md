# Engineering pins

`toolchain.json`, `global.json`, `Directory.Build.props`, and `Directory.Packages.props` are the machine-readable bootstrap authority.

Do not silently upgrade SDKs, test frameworks, native toolchains, or converter binaries inside an unrelated feature batch. Toolchain changes require a versioned change, clean restore/build/test evidence, dependency audit, and handover update.
