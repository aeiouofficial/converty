from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SMOKE = ROOT / "build" / "image-input-acceptance-smoke.ps1"
CI = ROOT / ".github" / "workflows" / "ci.yml"
REGISTRY = ROOT / "src" / "Converty.Core" / "Presets" / "ProductPresetRegistry.cs"
NATIVE = ROOT / "native" / "Converty.ShellExtension" / "ConvertyShellExtension.cpp"


def test_dev18_image_acceptance_smoke_exists_and_is_wired_into_ci():
    """Dev.18 must add one ordinary Windows product gate for the fixed Image matrix."""
    assert SMOKE.is_file(), "dev.18 Image input acceptance smoke is missing"
    ci = CI.read_text(encoding="utf-8")
    assert "Image source and malformed-input acceptance" in ci
    assert "./build/image-input-acceptance-smoke.ps1" in ci


def test_dev18_smoke_covers_all_advertised_image_extensions_and_fixed_actions():
    assert SMOKE.is_file(), "dev.18 Image input acceptance smoke is missing"
    smoke = SMOKE.read_text(encoding="utf-8").lower()

    for source_extension in ("png", "jpg", "jpeg", "webp", "bmp", "gif", "tif", "tiff"):
        assert f"id = '{source_extension}'" in smoke

    for preset_id in ("image.png", "image.jpeg", "image.webp"):
        assert preset_id in smoke

    for token in (
        "--preset",
        "converty_bridge_noninteractive",
        "argumentlist.add($source.path)",
        "waitforexit(30000)",
        "codec_name,width,height",
        "malformed",
        "truncated",
    ):
        assert token in smoke


def test_dev18_smoke_locks_transactional_image_invariants():
    assert SMOKE.is_file(), "dev.18 Image input acceptance smoke is missing"
    smoke = SMOKE.read_text(encoding="utf-8").lower()

    for token in (
        "get-filehash",
        "pre-existing destination",
        "numbered",
        ".converty-*.partial.*",
        "source preserved",
        "exit code 4",
    ):
        assert token in smoke


def test_dev18_keeps_exact_fixed_image_registry_and_native_surface():
    registry = REGISTRY.read_text(encoding="utf-8")
    native = NATIVE.read_text(encoding="utf-8")

    for preset_id, title, extension in (
        ("image.png", "Convert to PNG", ".png"),
        ("image.jpeg", "Convert to JPEG", ".jpg"),
        ("image.webp", "Convert to WebP", ".webp"),
    ):
        assert preset_id in registry
        assert title in registry
        assert f'"{extension}"' in registry
        assert f'L"{preset_id}"' in native
        assert f'L"{title}"' in native
        assert f'L"{extension}"' in native

    assert 'PresetId.Parse("image.png")' in registry
    assert 'PresetId.Parse("image.jpeg")' in registry
    assert 'PresetId.Parse("image.webp")' in registry
