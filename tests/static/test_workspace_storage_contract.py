from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[2]

class WorkspaceStorageContractTests(unittest.TestCase):
    def test_agents_describes_frozen_main_and_dev_branch_authority(self):
        text = (ROOT / 'AGENTS.md').read_text(encoding='utf-8').lower()
        self.assertIn('frozen main', text)
        self.assertIn('development branch', text)
        self.assertIn('not customer ship-ready', text)

    def test_workspace_script_enforces_non_c_project_temp(self):
        script = ROOT / 'build' / 'use-workspace-temp.ps1'
        self.assertTrue(script.is_file(), 'workspace temp bootstrap missing')
        s = script.read_text(encoding='utf-8')
        for item in ('CONVERTY_WORKSPACE_ROOT','TEMP','TMP','TMPDIR','PIP_CACHE_DIR','NUGET_PACKAGES','NPM_CONFIG_CACHE','DOTNET_CLI_HOME', "'C:\\'"):
            self.assertIn(item, s)
        policy = (ROOT / 'AGENTS.md').read_text(encoding='utf-8')
        self.assertIn('D:\\converty\\_temp', policy)
        self.assertIn('build/use-workspace-temp.ps1', policy)

    def test_runtime_staging_honors_workspace_root_before_local_app_data(self):
        staging = (ROOT / 'src' / 'Converty.Core' / 'Execution' / 'ConversionStagingDirectory.cs').read_text(encoding='utf-8')
        self.assertIn('CONVERTY_WORKSPACE_ROOT', staging)
        workspace_index = staging.index('CONVERTY_WORKSPACE_ROOT')
        local_app_data_index = staging.index('Environment.SpecialFolder.LocalApplicationData')
        self.assertLess(workspace_index, local_app_data_index)
        self.assertIn('Path.Combine(workspaceRoot, "_temp", "runtime", ProductDirectoryName, StagingDirectoryName)', staging)

    def test_host_state_honors_workspace_root_before_local_app_data(self):
        program = (ROOT / 'src' / 'Converty.Host' / 'Program.cs').read_text(encoding='utf-8')
        self.assertIn('CONVERTY_WORKSPACE_ROOT', program)
        workspace_index = program.index('CONVERTY_WORKSPACE_ROOT')
        local_app_data_index = program.index('Environment.SpecialFolder.LocalApplicationData')
        self.assertLess(workspace_index, local_app_data_index)
        self.assertIn('Path.Combine(workspaceRoot, "_temp", "runtime", "Converty", "Host", "state")', program)
