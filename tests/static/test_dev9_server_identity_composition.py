from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def read(relative: str) -> str:
    return (ROOT / relative).read_text(encoding="utf-8")


def test_packaged_bridge_composition_uses_one_trusted_host_path_for_launch_and_authentication():
    source = read("src/Converty.Bridge/Startup/PackagedBridgeRuntimeFactory.cs")

    assert "TrustedHostPath.FromApplicationBaseDirectory()" in source
    assert "WindowsCurrentPackageFamilyName.GetRequired()" in source
    assert "new WindowsConnectedServerIdentityProbe()" in source
    assert "new WindowsConnectedServerIdentityVerifier(" in source
    assert "trustedHost.ExecutablePath" in source
    assert "new InstalledHostProcessLauncher(trustedHost)" in source
    assert "BridgeClient.ForCurrentUser(connectTimeout, verifier)" in source


def test_packaged_bridge_factory_does_not_accept_executable_or_install_path_from_callers():
    source = read("src/Converty.Bridge/Startup/PackagedBridgeRuntimeFactory.cs")

    signature_start = source.index("CreateForCurrentUser(")
    signature_end = source.index(")", signature_start)
    signature = source[signature_start:signature_end]

    assert "string" not in signature
    assert "path" not in signature.lower()
    assert "executable" not in signature.lower()


def test_current_package_family_is_derived_from_windows_not_configuration_text():
    source = read("src/Converty.Bridge/Ipc/WindowsCurrentPackageFamilyName.cs")

    assert "GetCurrentPackageFamilyName" in source
    assert "Environment.GetEnvironmentVariable" not in source
    assert "Configuration" not in source
