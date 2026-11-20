from pathlib import Path
from datetime import datetime
import json
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
REPORT = {'timestamp': datetime.now().isoformat(timespec='seconds'), 'root': str(ROOT), 'passes': [], 'pending': []}
CURRENT = None


def begin(name):
    global CURRENT
    CURRENT = {'name': name, 'checks': 0, 'errors': [], 'warnings': [], 'details': []}
    REPORT['passes'].append(CURRENT)
    print('\n=== ' + name + ' ===')


def check(condition, label, detail='', warning=False):
    CURRENT['checks'] += 1
    if condition:
        return True
    entry = label + ((' - ' + detail) if detail else '')
    if warning:
        CURRENT['warnings'].append(entry)
        print('WARN', entry)
    else:
        CURRENT['errors'].append(entry)
        print('FAIL', entry)
    return False


def info(message):
    CURRENT['details'].append(message)
    print(message)


def text(relative):
    return (ROOT / relative).read_text(encoding='utf-8-sig')


def run(command, cwd=ROOT):
    return subprocess.run(command, cwd=str(cwd), text=True, encoding='utf-8', errors='replace', stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=180)


# Pass 1: complete scene structure and ownership.
begin('PASS 1 - Scene structure, component ownership and references')
scene = text('Assets/Scenes/SampleScene.unity')
ids = re.findall(r'(?m)^--- !u!(\d+) &(-?\d+)(?: stripped)?\s*$', scene)
file_ids = [value for _, value in ids]
check(len(file_ids) == len(set(file_ids)), 'Scene fileIDs are unique')
components = re.findall(r'(?m)^  - component: \{fileID: (-?\d+)\}', scene)
check(not (set(components) - set(file_ids)), 'Every component list entry resolves')
scene_documents = list(re.finditer(r'(?ms)^--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=^--- !u!|\Z)', scene))
game_objects = {doc.group(2): doc.group(3) for doc in scene_documents if doc.group(1) == '1'}
component_owners = {doc.group(2): doc.group(3) for doc in scene_documents if 'm_GameObject:' in doc.group(3)}
ownership_errors = []
for component_id, body in component_owners.items():
    match = re.search(r'm_GameObject: \{fileID: (-?\d+)\}', body)
    if not match or match.group(1) == '0':
        continue
    owner = match.group(1)
    if owner not in game_objects:
        ownership_errors.append(component_id + ' missing GameObject ' + owner)
    elif not re.search(r'(?m)^  - component: \{fileID: ' + re.escape(component_id) + r'\}', game_objects[owner]):
        ownership_errors.append(component_id + ' is not listed by owner ' + owner)
check(not ownership_errors, 'Component m_GameObject matches owner component lists', '; '.join(ownership_errors[:5]))
transform_ids = {doc.group(2) for doc in scene_documents if doc.group(1) in ('4', '224')}
father_errors = []
for doc in scene_documents:
    if doc.group(1) != '4':
        continue
    for parent in re.findall(r'(?m)^  m_Father: \{fileID: (-?\d+)\}', doc.group(3)):
        if parent != '0' and parent not in transform_ids:
            father_errors.append(doc.group(2) + ' -> ' + parent)
    for child in re.findall(r'(?m)^  - \{fileID: (-?\d+)\}', doc.group(3)):
        if child not in transform_ids:
            father_errors.append(doc.group(2) + ' child ' + child)
check(not father_errors, 'Transform parent/children references resolve', '; '.join(father_errors[:5]))
required_names = ['Prepare_On_Position', 'Equipment', 'MatchSocket', 'GlassBottle', 'Cork', 'AlcoholLamp', 'TrashCan', 'Score', 'Make_O2_Step7', 'Match_ReFire_Step3', 'Hints', 'Make_O2_Steps', 'Match_ReFire_Steps']
for name in required_names:
    check(re.search(r'(?m)^  m_Name: ' + re.escape(name) + r'$', scene) is not None, 'Required scene object exists: ' + name)
for guid in ['ba1d1234567890abcdef0000000002ab', 'ba1d1234567890abcdef0000000004ab', 'ba1d1234567890abcdef0000000007ab', 'ba1d1234567890abcdef000000001aab']:
    check(scene.count(guid) == 1, 'Core scene script attached once: ' + guid)
info('Scene documents: ' + str(len(scene_documents)) + '; GameObjects: ' + str(len(game_objects)) + '; components checked: ' + str(len(component_owners)))

# Pass 2: UnityEvents, public methods and assemblies.
begin('PASS 2 - UnityEvent methods and assembly references')
script_by_guid = {}
for path in (ROOT / 'Assets').rglob('*.cs'):
    meta = Path(str(path) + '.meta')
    if not meta.exists():
        continue
    match = re.search(r'^guid: ([0-9a-f]+)$', meta.read_text(encoding='utf-8-sig'), re.M)
    if match:
        script_by_guid[match.group(1)] = path
script_by_fileid = {}
for doc in scene_documents:
    if doc.group(1) != '114':
        continue
    match = re.search(r'm_Script: \{fileID: 11500000, guid: ([0-9a-f]+)', doc.group(3))
    if match:
        script_by_fileid[doc.group(2)] = match.group(1)
methods_by_guid = {}
for guid, path in script_by_guid.items():
    methods_by_guid[guid] = set(re.findall(r'public\s+(?:static\s+)?[\w<>\[\],.?]+\s+(\w+)\s*\(', path.read_text(encoding='utf-8-sig')))
builtin_methods = {'SetActive', 'Play', 'Stop', 'Destroy', 'SetText', 'SetColor', 'SetFloat', 'SetBool', 'SetInteger', 'SetTrigger', 'SetActiveSelf', 'SendMessage', 'Start', 'Update', 'Awake', 'OnEnable', 'OnDisable', 'OnDestroy', 'OnTriggerEnter', 'OnTriggerExit', 'OnTriggerStay', 'OnCollisionEnter', 'OnCollisionExit', 'OnCollisionStay', 'OnSelectEntered', 'OnSelectExited', 'OnHoverEntered', 'OnHoverExited', 'set_text', 'set_color', 'set_enabled', 'set_gameObject', 'get_gameObject', 'ToString'}
unknown = []
checked_events = 0
for match in re.finditer(r'm_Target: \{fileID: (-?\d+)\}.*?m_MethodName: ([^\n]+)', scene, re.S):
    target, method = match.group(1), match.group(2).strip()
    if target == '0' or method in builtin_methods:
        continue
    guid = script_by_fileid.get(target)
    if guid is None or guid not in methods_by_guid:
        continue
    checked_events += 1
    if method not in methods_by_guid[guid]:
        unknown.append(method + ' on ' + script_by_guid[guid].name + ' (' + target + ')')
check(not unknown, 'All scene custom UnityEvent methods are public and present', '; '.join(unknown[:10]))
info('Custom UnityEvent methods checked: ' + str(checked_events))
runtime = json.loads(text('Assets/Scripts/VRChemistry.Runtime.asmdef'))
for reference in ['VRChemistry.Chemistry.Core', 'VRChemistry.Experiments.Core', 'Unity.XR.Interaction.Toolkit', 'UnityEngine.UI', 'Unity.InputSystem', 'Unity.VisualEffectGraph', 'Unity.TextMeshPro', 'Unity.RenderPipelines.Universal.Runtime']:
    check(reference in runtime['references'], 'Runtime assembly reference: ' + reference)
play = json.loads(text('Assets/Tests/PlayMode/VRChemistry.PlayModeTests.asmdef'))
for reference in ['VRChemistry.Runtime', 'Unity.XR.Interaction.Toolkit']:
    check(reference in play['references'], 'PlayMode assembly reference: ' + reference)
score = text('Assets/Scripts/InitialDefinition/Score.cs')
check(not re.search(r'runner\.Set(?:Step|Cleanup)Completed', score), 'Score has no direct Runner writes')
check('public event Action LegacyStateChanged' in score, 'Score exposes legacy event')
check('score.LegacyStateChanged += Apply' in text('Assets/Scripts/InitialDefinition/LegacyExperimentAdapter.cs'), 'Adapter subscribes to Score event')
edit_test_count = sum(len(re.findall(r'\[Test\]', path.read_text(encoding='utf-8-sig'))) for path in (ROOT / 'Assets/Tests/EditMode').rglob('*.cs'))
check(edit_test_count == 37, 'Thirty-seven EditMode tests including exact reaction process boundaries')
play_tests = text('Assets/Tests/PlayMode/ExperimentSceneSmokeTests.cs')
check(len(re.findall(r'\[UnityTest\]', play_tests)) == 11, 'Eleven PlayMode tests including chemistry integration')

# Pass 3: experiment/state/material simulation and P2 runtime paths.
begin('PASS 3 - Experiment logic, reset, material state and runtime simulation')
python = sys.executable
simulation = run([python, str(ROOT / 'Tools/simulate_p0_p1.py')])
check(simulation.returncode == 0, 'P0/P1 simulation passes', simulation.stdout[-500:])
summary = re.search(r'SIMULATION SUMMARY: (.+)', simulation.stdout)
info(summary.group(1) if summary else 'Simulation completed')
chemistry = run([python, str(ROOT / 'Tools/simulate_chemistry.py')])
check(chemistry.returncode == 0, 'Chemistry core simulation passes', chemistry.stdout[-400:])
chemistry_summary = re.search(r'CHEMISTRY SIMULATION: (.+)', chemistry.stdout)
info('Chemistry simulation: ' + (chemistry_summary.group(1) if chemistry_summary else 'completed'))
runner = text('Assets/Scripts/Experiments/Core/ExperimentRunner.cs')
for token in ['ExperimentRunId++', 'event Action ScoreChanged', 'InitializeForExperiment(ExperimentRunId, CurrentProject == 2)']:
    check(token in runner, 'Runner lifecycle token: ' + token)
for token in ['EvaluateMakeOxygen', 'EvaluateMatchRefire', 'AddProducedOxygen', 'GasUnitsPerMole']:
    check(token in text('Assets/Scripts/ExperimentalFlowCode/ChemistryLegacyAdapter.cs'), 'Chemistry adapter token: ' + token)
for token in ['ReactionEngine.Evaluate', 'ReactionResult', 'ChemistryDatabase.GetReaction']:
    check(token in text('Assets/Scripts/Chemistry/Runtime/ChemistrySimulationService.cs'), 'Chemistry service token: ' + token)
check('AddOxygenMol' in text('Assets/Scripts/Experiments/Core/OxygenSupply.cs'), 'Oxygen supply supports molar amounts')
check('ChemistryUnits.GasUnitsPerMole' in text('Assets/Scripts/Experiments/Core/OxygenSupply.cs'), 'Oxygen units use lower shared assembly')
check('float.IsNaN(amount)' in text('Assets/Scripts/Experiments/Core/OxygenSupply.cs') and 'float.IsInfinity(amount)' in text('Assets/Scripts/Experiments/Core/OxygenSupply.cs'), 'Oxygen supply rejects nonfinite amounts')
check('score.If_Make_O2_Step4()' in text('Assets/Scripts/ExperimentalFlowCode/MakeO2.cs'), 'MakeO2 uses actual gas connection state')
check('Match_ReFire_Score.If_Make_O2_Step4()' in text('Assets/Scripts/ExperimentalFlowCode/O2IntoBottle.cs'), 'Oxygen production uses actual connection state')
check('LiquidProduced' in text('Assets/Scripts/Chemistry/Core/ReactionResult.cs'), 'Liquid products are tracked separately')
for token in ['ReactionRunStatus', 'CanApply', 'Reset(bool restoreMixture = true)', 'ReactionRunStatus.Paused', 'ReactionRunStatus.Completed', 'beforeTick', 'activeDeltaTime', 'actual != amount', 'Progress >= 1F', 'totalGas']:
    check(token in text('Assets/Scripts/Chemistry/Core/ReactionProcess.cs'), 'Reaction process lifecycle token: ' + token)
for token in ['GasComponents', 'SolidComponents', 'CanAddProduct', 'CanConsume', 'Clone()']:
    check(token in text('Assets/Scripts/Chemistry/Core/MixtureState.cs'), 'Mixture state commit token: ' + token)
check('StartProcess' in text('Assets/Scripts/Chemistry/Runtime/ChemistrySimulationService.cs'), 'Unity service exposes reaction process startup')
supply = text('Assets/Scripts/Experiments/Core/OxygenSupply.cs')
for token in ['initializedRunId', 'InitializeForExperiment(int runId, bool fillBottle)', 'AmountChanged?.Invoke(Amount)']:
    check(token in supply, 'Oxygen lifecycle token: ' + token)
collector = text('Assets/Scripts/ExperimentalFlowCode/O2IntoBottle.cs')
for token in ['CanCollectOxygen', 'If_Make_O2_Step6', 'minimumCollectionAmount = 120F']:
    check(token in collector, 'Collection guard token: ' + token)
layout = text('Assets/Scripts/InitialDefinition/LaboratoryLayoutController.cs')
for token in ['SelectExit', 'HoverExit', 'ExitXrSelection', 'grabEnabled']:
    check(token in layout, 'XRI reset token: ' + token)
create = text('Assets/Scripts/InitialDefinition/CreateMatch.cs')
for token in ['SelectMatchAfterReset', 'SelectEnter', 'RefreshMatchRelay', 'public bool Have_Match()']:
    check(token in create, 'Match reset token: ' + token)
material = text('Assets/Scripts/Experiments/Core/LabMaterialState.cs')
for token in ['HighlightOwners', 'ClearHighlight', 'SetWobble', 'SetDissolve', 'ClearRenderer']:
    check(token in material, 'Shared material state token: ' + token)
check('runner.ScoreChanged += RefreshScore' in score and 'CalculateScore' not in score, 'Score is event refreshed')
check('cacheDirty' in text('Assets/Scripts/Experiments/Core/LabItem.cs'), 'LabItem cache is present')
check('lastPosition' not in text('Assets/Scripts/Rendering/LabLiquidVolume.cs'), 'Liquid pure translation recalculation removed')
check('QualitySettings.renderPipeline as UniversalRenderPipelineAsset' in text('Assets/Scripts/Diagnostics/LabPerformanceMonitor.cs'), 'Performance monitor uses current quality pipeline')
for token in ['guidanceDirty', 'highlightTargets', 'contentBuilder', 'itemsToRemove']:
    check(token in text('Assets/Scripts/Experiments/ExperimentGuidanceController.cs'), 'Guidance allocation reduction: ' + token)
check('nextHighlightUpdate' in text('Assets/Scripts/Experiments/Core/LabItem.cs'), 'Highlight update throttling is present')
check('nextWobbleUpdate' in text('Assets/Scripts/Wobble.cs'), 'Wobble update throttling is present')
check('NeedsGeometryUpdate' in text('Assets/Scripts/Rendering/LabLiquidCap.cs'), 'Cap updates are change-driven')
check('UnityEditor.UnityStats' in text('Assets/Scripts/Diagnostics/LabPerformanceMonitor.cs'), 'Editor draw statistics are sampled')
check('ToggleProfile' in text('Assets/Scripts/Diagnostics/LabPerformanceBudget.cs'), 'QuestLow profile switch exists')
check('maxWaveAmplitude' in text('Assets/Scripts/Rendering/LabQuestMaterialTuner.cs'), 'Android material tuning exists')
physics = text('Assets/Editor/LabPhysicsOptimizationTools.cs')
check('body.solverIterations = 4' in physics, 'Physics solver budget is limited to proxy bodies')
check('bubblePlayRate' in text('Assets/Scripts/Rendering/LabQuestMaterialTuner.cs'), 'Android VFX playback budget exists')
check('IPreprocessBuildWithReport' in text('Assets/Editor/LabQuestBuildPreprocessor.cs'), 'Quest build stripping preprocessor exists')
check('RestoreBuildState' in text('Assets/Editor/LabQuestBuildPreprocessor.cs'), 'Quest build exception restoration exists')
for token in ['oxygenTextDirty', 'nextOxygenTextRefresh']:
    check(token in text('Assets/Scripts/ExperimentalFlowCode/VisualEffectGraph.cs'), 'Oxygen UI throttling: ' + token)
check('cacheDirty' in text('Assets/Scripts/Experiments/Core/LabItem.cs'), 'LabItem registry cache is present')

# Pass 4: shaders, materials, LOD files and Blender name validation.
begin('PASS 4 - Shader, materials, LOD files and model-name validation')
equipment = text('Assets/Shaders/Lab/LabEquipment.shader')
check('GetMainLight(input.shadowCoord)' in equipment, 'Equipment uses input shadow coordinate')
check('GetMainLight(output.shadowCoord)' not in equipment, 'No undefined fragment output coordinate')
check('Name "ShadowCaster"' in equipment, 'Equipment ShadowCaster exists')
for path in (ROOT / 'Assets/Shaders/Lab').glob('*.shader'):
    source = path.read_text(encoding='utf-8-sig')
    check('CBUFFER_START(UnityPerMaterial)' in source and 'CBUFFER_END' in source, 'UnityPerMaterial: ' + path.name)
    check('ShaderLibrary/Unlit.hlsl' not in source, 'No incompatible URP include: ' + path.name)
for path in ['Assets/Materials/Stick.mat', 'Assets/Stick_M/Wood_2.mat', 'Assets/Stick_M/Wood_3.mat', 'Assets/Stick_M/Wood_4.mat']:
    check('c0ffee1234567890abcdef0000000003' in text(path), 'Wood material uses URP Lab Equipment: ' + path)
for path in ['Assets/Resources/Chemistry/Reactions/make_oxygen.asset', 'Assets/Resources/Chemistry/Reactions/match_refire.asset', 'Assets/Resources/Chemistry/Reactions/neutralization.asset']:
    check((ROOT / path).exists(), 'Reaction asset exists: ' + path)
    check((ROOT / (path + '.meta')).exists(), 'Reaction asset meta exists: ' + path)
check('Coefficient: 2' in text('Assets/Resources/Chemistry/Reactions/make_oxygen.asset'), 'Oxygen reaction has stoichiometric coefficients')
check('Coefficient: 3' in text('Assets/Resources/Chemistry/Reactions/make_oxygen.asset'), 'Oxygen gas coefficient is present')
check('RequiresHotEmber: 1' in text('Assets/Resources/Chemistry/Reactions/match_refire.asset'), 'Refire requires hot ember')
check('RequiresOxygen: 1' in text('Assets/Resources/Chemistry/Reactions/match_refire.asset'), 'Refire requires oxygen')
cap = text('Assets/Scripts/Rendering/LabLiquidCap.cs')
for token in ['CreateCapMesh', 'RuntimeRendererEnabled', 'HideFlags.DontSave', 'renderQueue = 3001', 'center.y -= 0.0002F', 'source.CurrentWobble', 'sharedCapMaterials', 'capMaterialUsers', 'AcquireCapMaterial', 'ReleaseCapMaterial']:
    check(token in cap, 'Liquid cap token: ' + token)
check(scene.count('generateSurfaceCap: 1') == 10, 'Ten liquid bodies enable surface caps')
check(scene.count('generateSurfaceCap: 0') == 1, 'LiquidLevel does not duplicate its existing surface')
check('volume.generateSurfaceCap = sourceVolume.generateSurfaceCap' in text('Assets/Editor/ModelOptimizationTools.cs'), 'LOD inherits cap settings')
check('levels[2].renderers' in text('Assets/Scripts/Rendering/LabQuestLodController.cs'), 'Android uses LOD2 for near geometry')
check('Application.platform == RuntimePlatform.Android' in text('Assets/Scripts/Rendering/LabQuestLodController.cs'), 'Quest LOD switch is Android-only')
check('Mathf.Min(segments, 24)' in text('Assets/Scripts/Rendering/LabLiquidCap.cs'), 'Android cap segments are limited')
report = json.loads(text('Assets/Modle/Optimized/optimization_report.json'))
for item in report:
    output = ROOT / item['output']
    check(output.exists(), 'LOD/proxy output exists: ' + item['output'])
    check(item['after'][1] < item['before'][1], 'Triangle count reduced: ' + item['output'])
for path in (ROOT / 'Assets/Modle/Optimized').glob('*.fbx'):
    check(Path(str(path) + '.meta').exists(), 'Model meta exists: ' + path.name)
blender = Path.home() / 'AppData/Local/Temp/VR_Chemistry_blender/blender-4.2.3-windows-x64/blender.exe'
if blender.exists():
    result = run([str(blender), '-b', '--factory-startup', '--python', str(ROOT / 'Tools/check_lod_names.py')])
    check(result.returncode == 0, 'Blender LOD name validation exits successfully', result.stdout[-300:])
    bad_lines = [line for line in result.stdout.splitlines() if 'missing=' in line and ('missing= []' not in line or 'ambiguous= []' not in line)]
    check(not bad_lines, 'All original/LOD mesh names match without ambiguity', '; '.join(bad_lines[:3]))
    for line in result.stdout.splitlines():
        if 'missing=' in line:
            info(line)
else:
    REPORT['pending'].append('Blender executable unavailable for this run; LOD name validation not repeated')

# Pass 5: validation/build/CI/release/package and delivery metadata.
begin('PASS 5 - Build, CI, Quest, release and delivery metadata')
validation = text('Assets/Editor/LabProjectValidation.cs')
for token in ['LightRenderingMode.PerVertex', 'CheckDynamicCollidersAndLights', 'CheckChemistryDefinitions', 'CheckFont', 'CheckLodAssets', 'CheckQuestAndAndroid', 'NamedBuildTarget.Android']:
    check(token in validation, 'Project validation token: ' + token)
build = text('Assets/Editor/ProjectBuildValidation.cs')
for token in ['EditorSceneManager.OpenScene', 'Application.isBatchMode', 'BuildSignedAndroidApk', 'WriteBuildInfo', 'PrepareAssetsForBuild', 'LabBuildInfo']:
    check(token in build, 'Build entry token: ' + token)
manifest = json.loads(text('Packages/manifest.json'))
lock = json.loads(text('Packages/packages-lock.json'))
for package in ['com.unity.xr.arfoundation', 'com.unity.xr.openxr']:
    check(package not in manifest['dependencies'], 'Removed unused/conflicting package: ' + package)
check('com.unity.xr.arsubsystems' not in lock['dependencies'], 'Isolated ARSubsystems lock entry removed')
check('com.unity.subsystemregistration' in lock['dependencies'], 'Required SubsystemRegistration retained')
for name in ['Open XR Loader.asset', 'Open XR Loader No Pre Init.asset', 'Open XR Package Settings.asset']:
    check(not (ROOT / 'Assets/XR/Loaders' / name).exists() and not (ROOT / 'Assets/XR/Settings' / name).exists(), 'OpenXR residual archived: ' + name)
oculus = text('Assets/XR/Settings/Oculus Settings.asset')
check('m_StereoRenderingModeAndroid: 1' in oculus, 'Quest Android Multiview enabled')
check('SubsampledLayout: 1' in oculus, 'Quest Subsampled Layout enabled')
quality = text('ProjectSettings/QualitySettings.asset')
check('name: Quest' in quality and 'Android: 3' in quality, 'Android selects Quest quality')
check('Standalone: 2' in quality, 'Standalone retains High quality')
check('name: QuestLow' in quality, 'QuestLow quality profile exists')
check('5ca1e1234567890abcdef00000000011' in quality, 'QuestLow pipeline reference exists')
check('LabPhysicsOptimizationTools.ApplyPhysicsProxies' in build, 'Build applies physics proxies')
check('LabTextureOptimization.ApplyAndroidBudgets' in build, 'Build applies Android texture budgets')
check('EditorSceneManager.sceneOpened' in text('Assets/Editor/LabPhysicsOptimizationTools.cs'), 'Physics proxies support sceneOpened')
ci = ROOT / 'Tools/run_unity_validation.ps1'
pwsh = shutil.which('pwsh')
if pwsh:
    parser = "$errors = $null; [System.Management.Automation.Language.Parser]::ParseFile('" + str(ci).replace("'", "''") + "', [ref]$null, [ref]$errors) | Out-Null; if ($errors.Count -gt 0) { $errors | ForEach-Object { $_.Message }; exit 1 }"
    result = run([pwsh, '-NoProfile', '-Command', parser])
    check(result.returncode == 0, 'CI PowerShell syntax parses', result.stdout[-300:])
else:
    REPORT['pending'].append('pwsh unavailable for CI syntax check in this run')
for path in ['Assets/Editor/LabProjectValidation.cs.meta', 'Assets/Editor/ProjectBuildValidation.cs.meta', 'Assets/Editor/ModelOptimizationTools.cs.meta', 'Assets/Editor/LabPhysicsOptimizationTools.cs.meta', 'Assets/Editor/LabTextureOptimization.cs.meta', 'Assets/Editor/LabQuestBuildPreprocessor.cs.meta', 'Assets/Scripts/Diagnostics/LabPerformanceMonitor.cs.meta', 'Assets/Scripts/Diagnostics/LabPerformanceBudget.cs.meta', 'Assets/Scripts/Rendering/LabLiquidCap.cs.meta', 'Assets/Scripts/Rendering/LabQuestLodController.cs.meta', 'Assets/Scripts/Rendering/LabQuestMaterialTuner.cs.meta', 'Assets/Scripts/Chemistry/Runtime/ChemistrySimulationService.cs.meta', 'Assets/Scripts/Chemistry/Runtime/SubstanceDefinitionAsset.cs.meta', 'Assets/Scripts/Chemistry/Runtime/ReactionDefinitionAsset.cs.meta', 'Assets/Scripts/Chemistry/Runtime/ChemistryDatabase.cs.meta', 'Assets/Scripts/ExperimentalFlowCode/ChemistryLegacyAdapter.cs.meta', 'Assets/Scripts/Experiments/Core/ChemistryUnits.cs.meta', 'Assets/Scripts/Chemistry/Core/VRChemistry.Chemistry.Core.asmdef.meta', 'Assets/Scripts/Chemistry/Core/ReactionProcess.cs.meta', 'Assets/Tests/EditMode/Chemistry/ChemistryCoreTests.cs.meta', 'Assets/Scripts/VRChemistry.Runtime.asmdef.meta', 'Assets/Modle/Optimized.meta', 'Assets/Modle/Optimized/optimization_report.json.meta', 'Assets/Settings/UniversalRP-QuestLow.asset.meta']:
    check((ROOT / path).exists(), 'Delivery meta exists: ' + path)
REPORT['pending'].extend([
    'Unity C# and Shader compilation has not been run in this environment.',
    'Unity Test Runner, Android build and Quest device validation have not been run.',
    'ChemistryChineseSDF.asset still needs Unity font generation if not already created by the editor tool.'
])

for item in REPORT['passes']:
    item['passed'] = not item['errors']
    print('\n' + item['name'] + ': ' + ('PASS' if item['passed'] else 'FAIL') + ' | checks=' + str(item['checks']) + ' errors=' + str(len(item['errors'])) + ' warnings=' + str(len(item['warnings'])))

REPORT['passed'] = all(item['passed'] for item in REPORT['passes'])
REPORT['totalChecks'] = sum(item['checks'] for item in REPORT['passes'])
REPORT['totalErrors'] = sum(len(item['errors']) for item in REPORT['passes'])
REPORT['totalWarnings'] = sum(len(item['warnings']) for item in REPORT['passes'])
(ROOT / 'Logs').mkdir(exist_ok=True)
(ROOT / 'Logs/full_scene_check.json').write_text(json.dumps(REPORT, ensure_ascii=False, indent=2), encoding='utf-8')
print('\nFULL SCENE CHECK:', 'PASS' if REPORT['passed'] else 'FAIL', '| checks=', REPORT['totalChecks'], '| errors=', REPORT['totalErrors'])
print('REPORT:', ROOT / 'Logs/full_scene_check.json')
if not REPORT['passed']:
    sys.exit(1)
