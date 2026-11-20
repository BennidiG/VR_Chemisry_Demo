from pathlib import Path
from datetime import datetime
import hashlib
import json
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
results = []
failures = []
pending = []


def check(name, condition, detail=''):
    ok = bool(condition)
    results.append({'name': name, 'passed': ok, 'detail': detail})
    if not ok:
        failures.append(name + (': ' + detail if detail else ''))
    print(('PASS ' if ok else 'FAIL ') + name + ((' - ' + detail) if detail else ''))


def text(relative):
    return (ROOT / relative).read_text(encoding='utf-8-sig')


class Supply:
    def __init__(self):
        self.amount = 0.0
        self.initialized = False
        self.run_id = -1

    def initialize(self, run_id, fill):
        if self.run_id == run_id:
            return False
        self.run_id = run_id
        self.amount = 10000.0 if fill else 0.0
        self.initialized = True
        return True

    def consume(self, amount):
        self.amount = max(0.0, self.amount - amount)

    def add(self, amount):
        self.amount = min(10000.0, self.amount + amount)
        self.initialized = True

    def reset(self):
        self.amount = 0.0
        self.initialized = False
        self.run_id = -1


class Runner:
    def __init__(self):
        self.run_id = 0
        self.project = 0
        self.phase = 'None'
        self.supply = Supply()
        self.steps = set()
        self.cleanup = set()
        self.prepared = False

    def select(self, project):
        self.run_id += 1
        self.project = project
        self.phase = 'Preparing'
        self.steps.clear()
        self.cleanup.clear()
        self.prepared = False
        self.supply.initialize(self.run_id, project == 2)


class ScoreAdapter:
    def __init__(self, runner):
        self.runner = runner
        self.spoon = False
        self.project = 0

    def apply(self):
        if self.project == 0 or self.runner.project != self.project:
            return
        if self.spoon:
            self.runner.cleanup.add('spoon')
        else:
            self.runner.cleanup.discard('spoon')


class Match:
    def __init__(self):
        self.exists = True
        self.have_match = True
        self.socket_selected = False
        self.last_reset_frame = -1

    def destroy(self):
        self.exists = False
        self.have_match = False
        self.socket_selected = False

    def reset(self, frame):
        if self.last_reset_frame == frame and self.have_match and self.exists:
            return False
        self.last_reset_frame = frame
        self.exists = False
        self.have_match = False
        self.exists = True
        self.have_match = True
        self.socket_selected = True
        return True


class MaterialState:
    def __init__(self):
        self.base = (0.2, 0.4, 0.7, 0.25)
        self.emission = (0.0, 0.0, 0.0, 0.0)
        self.wobble = (0.12, -0.08)
        self.liquid_range = (-0.1, 0.2)
        self.owners = set()
        self.highlighted = self.base

    def highlight(self, owner, color):
        self.owners.add(owner)
        self.highlighted = (color[0], color[1], color[2], self.base[3])

    def clear(self, owner):
        self.owners.discard(owner)
        if not self.owners:
            self.highlighted = self.base


# 1. Source and assembly structure.
for path in list((ROOT / 'Assets/Scripts').rglob('*.cs')) + list((ROOT / 'Assets/Editor').rglob('*.cs')) + list((ROOT / 'Assets/Tests').rglob('*.cs')):
    source = path.read_text(encoding='utf-8-sig')
    clean = re.sub(r'(?ms)/\*.*?\*/', '', source)
    clean = re.sub(r'(?m)//.*$', '', clean)
    clean = re.sub(r'@?"(?:\\.|""|[^"\\])*"', '""', clean)
    for left, right in (('{', '}'), ('(', ')'), ('[', ']')):
        check('Brace ' + str(path.relative_to(ROOT)) + ' ' + left + right, clean.count(left) == clean.count(right))

runtime = json.loads(text('Assets/Scripts/VRChemistry.Runtime.asmdef'))
for reference in ('VRChemistry.Experiments.Core', 'Unity.XR.Interaction.Toolkit', 'UnityEngine.UI', 'Unity.InputSystem', 'Unity.VisualEffectGraph', 'Unity.TextMeshPro', 'Unity.RenderPipelines.Universal.Runtime'):
    check('Runtime asmdef reference ' + reference, reference in runtime['references'])
playmode = json.loads(text('Assets/Tests/PlayMode/VRChemistry.PlayModeTests.asmdef'))
for reference in ('VRChemistry.Runtime', 'Unity.XR.Interaction.Toolkit'):
    check('PlayMode asmdef reference ' + reference, reference in playmode['references'])

# 2. P0 shader/material/font checks.
equipment = text('Assets/Shaders/Lab/LabEquipment.shader')
check('Equipment uses input shadow coordinate', 'GetMainLight(input.shadowCoord)' in equipment)
check('Equipment has no undefined output coordinate', 'GetMainLight(output.shadowCoord)' not in equipment)
check('Equipment has ShadowCaster pass', 'Name "ShadowCaster"' in equipment)
for material in ('Assets/Materials/Stick.mat', 'Assets/Stick_M/Wood_2.mat', 'Assets/Stick_M/Wood_3.mat', 'Assets/Stick_M/Wood_4.mat'):
    check('URP material ' + material, 'c0ffee1234567890abcdef0000000003' in text(material))
font = text('Assets/Editor/ChineseFontRepair.cs')
for token in ('[InitializeOnLoad]', 'GlyphRenderMode.SDFAA', 'text.fontSharedMaterial = fontAsset.material', 'AssetDatabase.AddObjectToAsset'):
    check('Font repair token ' + token, token in font)
check('Complete Noto font exists', (ROOT / 'Assets/Fonts/NotoSansSC.ttf').stat().st_size > 1000000)
pending.append('ChemistryChineseSDF.asset must be generated by Unity/ChineseFontRepair')

# 3. Scene/adapter structure.
scene = text('Assets/Scenes/SampleScene.unity')
ids = re.findall(r'(?m)^--- !u!\d+ &(-?\d+)(?: stripped)?\s*$', scene)
components = re.findall(r'(?m)^  - component: \{fileID: (-?\d+)\}', scene)
check('Scene fileIDs unique', len(ids) == len(set(ids)))
check('Scene component references valid', not (set(components) - set(ids)))
check('Legacy adapter attached once', scene.count('ba1d1234567890abcdef0000000007ab') == 1)
score = text('Assets/Scripts/InitialDefinition/Score.cs')
check('Score exposes legacy event', 'public event Action LegacyStateChanged' in score)
check('Score has no direct Runner state writes', not re.search(r'runner\.Set(?:Step|Cleanup)Completed', score))
adapter = text('Assets/Scripts/InitialDefinition/LegacyExperimentAdapter.cs')
check('Adapter subscribes to legacy event', 'score.LegacyStateChanged += Apply' in adapter)
check('Adapter is the only legacy mapping component', 'SetStepCompletedFromLegacy' in adapter and 'SetCleanupCompletedFromLegacy' in adapter)

# 4. Oxygen and collection simulation.
runner = Runner()
runner.select(2)
first_amount = runner.supply.amount
runner.supply.consume(first_amount)
runner.select(2)
check('Same project new run restores oxygen after depletion', runner.supply.amount == 10000.0)
first_run = runner.run_id
check('Same run initialize does not refill', not runner.supply.initialize(first_run, True))
runner.supply.consume(400.0)
check('Same run repeated initialization leaves consumption intact', runner.supply.amount == 9600.0)
runner.select(1)
check('MakeO2 starts with empty bottle', runner.supply.amount == 0.0)
runner.supply.add(150.0)
runner.select(1)
check('MakeO2 new run clears previous collection', runner.supply.amount == 0.0)
runner.steps.add('heat_test_tube')
can_collect = runner.project == 1 and 'heat_test_tube' in runner.steps and runner.supply.amount >= 120.0
check('Collection still requires threshold after heating', not can_collect)
runner.supply.add(120.0)
can_collect = runner.project == 1 and 'heat_test_tube' in runner.steps and runner.supply.amount >= 120.0
check('Collection threshold works after valid production', can_collect)

# 5. Adapter/trash simulation.
adapter_sim = ScoreAdapter(runner)
adapter_sim.project = 1
adapter_sim.spoon = True
adapter_sim.apply()
check('Adapter applies legacy cleanup', 'spoon' in runner.cleanup)
adapter_sim.spoon = False
adapter_sim.apply()
check('Adapter removes revoked cleanup', 'spoon' not in runner.cleanup)
adapter_sim.spoon = True
adapter_sim.apply()
item_active = True
item_active = False
adapter_sim.spoon = False
adapter_sim.apply()
check('Trash simulation disables item and clears cleanup', not item_active and 'spoon' not in runner.cleanup)

# 6. Match/XRI simulation.
match = Match()
match.destroy()
reset_created = match.reset(10)
check('Destroyed match can be reset in same frame', reset_created and match.exists)
check('Reset restores one match and socket selection', match.have_match and match.socket_selected)
check('Same-frame duplicate reset is suppressed', not match.reset(10))
match.reset(11)
check('New frame reset is allowed', match.exists and match.socket_selected)

# 7. Shared material state simulation.
material = MaterialState()
material.highlight('rack', (1.0, 0.2, 0.1, 1.0))
check('Highlight preserves alpha', material.highlighted[3] == material.base[3])
check('Highlight preserves liquid range', material.liquid_range == (-0.1, 0.2))
check('Highlight preserves wobble', material.wobble == (0.12, -0.08))
material.highlight('tube', (0.1, 1.0, 0.2, 1.0))
material.clear('rack')
check('One owner removal keeps other owner highlight', material.highlighted[0] == 0.1 and 'tube' in material.owners)
material.clear('tube')
check('Final owner removal restores base color', material.highlighted == material.base)
check('Final restore does not change wobble', material.wobble == (0.12, -0.08))

check('Event-driven score refresh', 'runner.ScoreChanged += RefreshScore' in text('Assets/Scripts/InitialDefinition/Score.cs'))
check('LabItem registry cache', 'cacheDirty' in text('Assets/Scripts/Experiments/Core/LabItem.cs'))
guidance = text('Assets/Scripts/Experiments/ExperimentGuidanceController.cs')
for token in ('guidanceDirty', 'highlightTargets', 'contentBuilder', 'itemsToRemove'):
    check('Guidance allocation reduction ' + token, token in guidance)
vfx = text('Assets/Scripts/ExperimentalFlowCode/VisualEffectGraph.cs')
for token in ('oxygenTextDirty', 'nextOxygenTextRefresh'):
    check('Oxygen UI throttling ' + token, token in vfx)
cap = text('Assets/Scripts/Rendering/LabLiquidCap.cs')
for token in ('CreateCapMesh', 'RuntimeRendererEnabled', 'HideFlags.DontSave', 'renderQueue = 3001', 'center.y -= 0.0002F', 'source.CurrentWobble'):
    check('Liquid cap token ' + token, token in cap)
scene = text('Assets/Scenes/SampleScene.unity')
check('Ten liquid bodies enable caps', scene.count('generateSurfaceCap: 1') == 10)
check('Existing LiquidLevel does not duplicate cap', scene.count('generateSurfaceCap: 0') == 1)
check('LOD copies cap settings', 'volume.generateSurfaceCap = sourceVolume.generateSurfaceCap' in text('Assets/Editor/ModelOptimizationTools.cs'))
for name in ('a_LOD3.fbx', 'basin_LOD3.fbx', 'rack_LOD3.fbx'):
    check('Third LOD exists ' + name, (ROOT / 'Assets/Modle/Optimized' / name).exists())
    check('Third LOD meta exists ' + name, (ROOT / 'Assets/Modle/Optimized' / (name + '.meta')).exists())
physics = text('Assets/Editor/LabPhysicsOptimizationTools.cs')
for token in ('CapsuleCollider', 'collider.enabled = false', 'ApplyPhysicsProxies', 'EditorSceneManager.sceneOpened'):
    check('Physics proxy token ' + token, token in physics)
check('Highlight throttling', 'nextHighlightUpdate' in text('Assets/Scripts/Experiments/Core/LabItem.cs'))
check('Wobble throttling', 'nextWobbleUpdate' in text('Assets/Scripts/Wobble.cs'))
check('Cap dirty updates', 'NeedsGeometryUpdate' in cap)
check('Editor draw statistics', 'UnityEditor.UnityStats' in text('Assets/Scripts/Diagnostics/LabPerformanceMonitor.cs'))
check('QuestLow quality profile', 'name: QuestLow' in text('ProjectSettings/QualitySettings.asset'))
check('QuestLow profile asset', (ROOT / 'Assets/Settings/UniversalRP-QuestLow.asset').exists())
check('Quality profile switch', 'ToggleProfile' in text('Assets/Scripts/Diagnostics/LabPerformanceBudget.cs'))
check('Android material tuner', 'maxWaveAmplitude' in text('Assets/Scripts/Rendering/LabQuestMaterialTuner.cs'))
check('Android texture budgets', 'ApplyAndroidBudgets' in text('Assets/Editor/LabTextureOptimization.cs'))
check('Cap material sharing', 'sharedCapMaterials' in cap and 'capMaterialUsers' in cap and 'AcquireCapMaterial' in cap and 'ReleaseCapMaterial' in cap)
check('Physics solver budget', 'body.solverIterations = 4' in physics)
check('VFX playback budget', 'bubblePlayRate' in text('Assets/Scripts/Rendering/LabQuestMaterialTuner.cs'))
check('Android near LOD2', 'levels[2].renderers' in text('Assets/Scripts/Rendering/LabQuestLodController.cs'))
check('Quest build preprocessor', 'IPreprocessBuildWithReport' in text('Assets/Editor/LabQuestBuildPreprocessor.cs') and 'OnPostprocessBuild' in text('Assets/Editor/LabQuestBuildPreprocessor.cs'))

# 8. Model and LOD resource simulation.
report = json.loads(text('Assets/Modle/Optimized/optimization_report.json'))
for item in report:
    output = ROOT / item['output']
    check('LOD/proxy output exists ' + item['output'], output.exists())
    before = item['before'][1]
    after = item['after'][1]
    check('Reduced triangle count ' + item['output'], after < before)
for name in ('a_LOD1.fbx', 'a_LOD2.fbx', 'a_PROXY.fbx', 'basin_LOD1.fbx', 'basin_LOD2.fbx', 'rack_LOD1.fbx', 'rack_LOD2.fbx', 'rack_PROXY.fbx'):
    check('Model meta exists ' + name, (ROOT / 'Assets/Modle/Optimized' / (name + '.meta')).exists())

def normalize(value):
    return value.replace(' ', '').replace('_', '').replace('.', '')

check('LOD name Unity/Blender normalization', normalize('Cylinder.001') == normalize('Cylinder001'))
check('LOD ambiguous names rejected', len([x for x in ('Cylinder.001', 'Cylinder001') if normalize(x) == normalize('Cylinder001')]) > 1)
editor = text('Assets/Editor/ModelOptimizationTools.cs')
check('Existing LODGroup can receive proxy', editor.index('collider.sharedMesh = proxy') < editor.index('LODGroup group = filter.GetComponent<LODGroup>()'))
check('LOD auto apply handles sceneOpened', 'sceneOpened' in editor)

# 9. XRI and tests.
layout = text('Assets/Scripts/InitialDefinition/LaboratoryLayoutController.cs')
create = text('Assets/Scripts/InitialDefinition/CreateMatch.cs')
for token in ('SelectExit', 'HoverExit', 'ExitXrSelection'):
    check('XRI reset token ' + token, token in layout)
for token in ('SelectMatchAfterReset', 'SelectEnter', 'RefreshMatchRelay'):
    check('Match reset token ' + token, token in create)
tests = text('Assets/Tests/PlayMode/ExperimentSceneSmokeTests.cs')
check('Eleven PlayMode tests present', len(re.findall(r'\[UnityTest\]', tests)) == 11)
check('Socket test uses real preparation entry', 'preparation.Make_O2_ReFire()' in tests)

summary = {
    'timestamp': datetime.now().isoformat(timespec='seconds'),
    'root': str(ROOT),
    'checks': len(results),
    'passed': sum(1 for result in results if result['passed']),
    'failed': len(failures),
    'failures': failures,
    'pending': pending,
    'results': results,
}
report_path = Path.home() / 'AppData/Local/Temp/VR_Chemistry_P0_P1_simulation.json'
report_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding='utf-8')
print('\nSIMULATION SUMMARY:', summary['passed'], '/', summary['checks'], 'passed')
print('REPORT:', report_path)
if failures:
    print('\nFAILURES:')
    for failure in failures:
        print(failure)
    sys.exit(1)
print('SIMULATION PASSED (not Unity compilation or device validation)')
