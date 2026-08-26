from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "packaging/Converty.Package/AppxManifest.xml"
CLSID = "20E7C5C1-3E5F-4D0F-9C56-2E9F2A978A10"

NS = {
    "foundation": "http://schemas.microsoft.com/appx/manifest/foundation/windows10",
    "com": "http://schemas.microsoft.com/appx/manifest/com/windows10",
    "desktop4": "http://schemas.microsoft.com/appx/manifest/desktop/windows10/4",
    "desktop5": "http://schemas.microsoft.com/appx/manifest/desktop/windows10/5",
    "rescap": "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities",
}


def test_development_package_registers_shell_com_class_and_modern_context_menu() -> None:
    assert MANIFEST.is_file()
    root = ET.parse(MANIFEST).getroot()

    identity = root.find("foundation:Identity", NS)
    assert identity is not None
    assert identity.attrib["Name"] == "Converty.Dev"
    assert identity.attrib["Publisher"] == "CN=Converty Development"

    com_class = root.find(
        ".//com:Extension[@Category='windows.comServer']/com:ComServer/com:SurrogateServer/com:Class",
        NS,
    )
    assert com_class is not None
    assert com_class.attrib["Id"].upper() == CLSID
    assert com_class.attrib["Path"] == "Converty.ShellExtension.dll"
    assert com_class.attrib["ThreadingModel"] == "STA"

    menu_extension = root.find(
        ".//desktop4:Extension[@Category='windows.fileExplorerContextMenus']",
        NS,
    )
    assert menu_extension is not None
    item_type = menu_extension.find("desktop4:FileExplorerContextMenus/desktop5:ItemType", NS)
    assert item_type is not None
    assert item_type.attrib["Type"] == "*"
    verb = item_type.find("desktop5:Verb", NS)
    assert verb is not None
    assert verb.attrib["Id"] == "Converty.Convert"
    assert verb.attrib["Clsid"].upper() == CLSID

    full_trust = root.find("foundation:Capabilities/rescap:Capability[@Name='runFullTrust']", NS)
    assert full_trust is not None


def test_package_registration_is_not_a_legacy_registry_verb() -> None:
    source = MANIFEST.read_text(encoding="utf-8").lower()
    assert "windows.comserver" in source
    assert "windows.fileexplorercontextmenus" in source
    for forbidden in ("windows.registry", "shell\\", "delegateexecute", "shellexecute"):
        assert forbidden not in source
