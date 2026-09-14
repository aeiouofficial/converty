from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def test_task8_video_pipeline_contract_and_validator_exist():
    assert (ROOT / "src/Converty.Core/Execution/TargetMediaContract.cs").is_file()
    assert (ROOT / "src/Converty.Core/Execution/TargetMediaContractValidator.cs").is_file()


def test_task8_batch_runner_owns_probe_plan_mode_and_post_validation_pipeline():
    text = (ROOT / "src/Converty.Core/Execution/ConversionBatchRunner.cs").read_text(encoding="utf-8")
    assert "IMediaProbeClient" in text
    assert "ConversionPlanner" in text
    assert "plan.Mode" in text
    assert "TargetMediaContractValidator" in text
    assert text.count("ProbeAsync(") >= 2


def test_task8_bridge_composition_root_wires_probe_and_video_planner():
    text = (ROOT / "src/Converty.Bridge/Program.cs").read_text(encoding="utf-8")
    assert "ProbeWorkerClient.CreateForApplicationBaseDirectory()" in text
    assert "VideoProductCapabilityCatalog.CreatePlanner()" in text


def test_task8_copy_hash_is_reverified_before_publication():
    text = (ROOT / "src/Converty.Core/Execution/ConversionBatchRunner.cs").read_text(encoding="utf-8")
    assert "ConversionMode.Copy" in text
    assert "SHA256" in text
    assert "FixedTimeEquals" in text
