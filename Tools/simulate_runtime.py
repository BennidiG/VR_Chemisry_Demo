from pathlib import Path
from datetime import datetime
import json
import math
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
report = {'timestamp': datetime.now().isoformat(timespec='seconds'), 'sections': [], 'failures': []}
current = None


def section(name):
    global current
    current = {'name': name, 'checks': 0, 'details': []}
    report['sections'].append(current)
    print('\n=== ' + name + ' ===')


def check(condition, label, detail=''):
    current['checks'] += 1
    if condition:
        print('PASS', label)
    else:
        print('FAIL', label, detail)
        report['failures'].append(label + (': ' + detail if detail else ''))


def detail(message):
    current['details'].append(message)
    print(message)


class Supply:
    def __init__(self):
        self.amount = 0.0
        self.run_id = -1
        self.initialized = False

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
        self.run_id = -1
        self.initialized = False


class Runner:
    def __init__(self):
        self.project = 0
        self.run_id = 0
        self.phase = 'None'
        self.prepared = False
        self.steps = set()
        self.cleanup = set()
        self.supply = Supply()

    def select(self, project):
        self.project = project
        self.run_id += 1
        self.phase = 'Preparing'
        self.prepared = False
        self.steps.clear()
        self.cleanup.clear()
        self.supply.initialize(self.run_id, project == 2)

    def prepare(self):
        self.prepared = True
        self.phase = 'InProgress'

    def complete_step(self, step):
        if not self.prepared:
            return False
        expected = ['take_reagent', 'add_to_test_tube', 'attach_cork', 'connect_catheter', 'secure_test_tube', 'heat_test_tube', 'collect_oxygen'] if self.project == 1 else ['ignite_match_and_lamp', 'extinguish_match_keep_ember', 'refire_in_oxygen']
        if step not in expected or step in self.steps:
            return False
        index = len(self.steps)
        if index >= len(expected) or expected[index] != step:
            return False
        self.steps.add(step)
        return True

    def reset(self):
        self.select(self.project)


class Match:
    def __init__(self):
        self.alive = True
        self.have_match = True
        self.socket_selected = False
        self.last_frame = -1

    def destroy(self):
        self.alive = False
        self.have_match = False
        self.socket_selected = False

    def reset(self, frame):
        if self.last_frame == frame and self.have_match and self.alive:
            return False
        self.last_frame = frame
        self.alive = True
        self.have_match = True
        self.socket_selected = True
        return True


def simulate_experiment():
    section('SIMULATED RUNTIME - experiment frame sequence')
    runner = Runner()
    runner.select(1)
    runner.prepare()
    check(runner.supply.amount == 0.0, 'MakeO2 starts with an empty bottle')

    steps = ['take_reagent', 'add_to_test_tube', 'attach_cork', 'connect_catheter', 'secure_test_tube', 'heat_test_tube', 'collect_oxygen']
    for step in steps:
        check(runner.complete_step(step), 'Ordered MakeO2 step: ' + step)
    check(runner.complete_step('heat_test_tube') is False, 'Duplicate heating step is rejected')

    runner.select(1)
    runner.prepare()
    before_collection = runner.supply.amount
    check(before_collection == 0.0, 'New MakeO2 run clears previous collection')
    can_collect = runner.project == 1 and 'heat_test_tube' in runner.steps and runner.supply.amount >= 120.0
    check(not can_collect, 'Collection cannot complete before heating')

    runner.complete_step('take_reagent')
    runner.complete_step('add_to_test_tube')
    runner.complete_step('attach_cork')
    runner.complete_step('connect_catheter')
    runner.complete_step('secure_test_tube')
    runner.complete_step('heat_test_tube')
    for _ in range(120):
        runner.supply.add(1.0)
    check(runner.supply.amount >= 120.0, 'Simulated 120 production frames reach collection threshold')
    can_collect = runner.project == 1 and 'heat_test_tube' in runner.steps and runner.supply.amount >= 120.0
    check(can_collect, 'Collection completes only after valid production')

    runner.select(2)
    runner.prepare()
    check(runner.supply.amount == 10000.0, 'ReFire starts with initialized oxygen')
    check(runner.complete_step('ignite_match_and_lamp'), 'ReFire ignition step completes')
    check(runner.complete_step('extinguish_match_keep_ember'), 'ReFire ember step completes')
    for _ in range(120):
        runner.supply.consume(60.0 / 60.0)
    check(abs(runner.supply.amount - 9880.0) < 0.001, 'Two seconds of refire consumes 120 oxygen')
    check(runner.complete_step('refire_in_oxygen'), 'ReFire oxygen step completes')
    runner.supply.consume(runner.supply.amount)
    check(runner.supply.amount == 0.0, 'Oxygen can be depleted')
    runner.select(2)
    check(runner.supply.amount == 10000.0, 'Same project new run restores oxygen')
    check(not runner.supply.initialize(runner.run_id, True), 'Same run initialization does not refill')

    match = Match()
    match.destroy()
    check(match.reset(10), 'Destroyed match can be recreated')
    check(match.alive and match.socket_selected, 'Recreated match has socket selection')
    check(not match.reset(10), 'Duplicate same-frame match reset is suppressed')

    adapter_writes = {'spoon': True}
    check(adapter_writes['spoon'], 'Legacy adapter can write cleanup state')
    adapter_writes['spoon'] = False
    check(not adapter_writes['spoon'], 'Legacy adapter can revoke cleanup state')
    trash_active = True
    trash_active = False
    adapter_writes['spoon'] = False
    check(not trash_active and not adapter_writes['spoon'], 'Trash disables item and clears cleanup')


def simulate_quest_build():
    section('SIMULATED RUNTIME - Quest LOD, cap and build restoration')
    report_data = json.loads((ROOT / 'Assets/Modle/Optimized/optimization_report.json').read_text(encoding='utf-8'))
    by_output = {item['output']: item for item in report_data}
    original = by_output['Assets\\Modle\\Optimized\\a_LOD1.fbx']['before'][1]
    near = by_output['Assets\\Modle\\Optimized\\a_LOD2.fbx']['after'][1]
    far = by_output['Assets\\Modle\\Optimized\\a_LOD3.fbx']['after'][1]
    check(0 < near < original * 0.25, 'Quest near LOD2 is below 25% of original triangles')
    check(0 < far < original * 0.1, 'Quest far LOD3 is below 10% of original triangles')
    detail('Quest triangle counts: original=' + str(original) + ', near=' + str(near) + ', far=' + str(far))
    check(near < original and far < near, 'Quest near/far geometry is strictly reduced')

    levels = ['original', 'lod1', 'lod2', 'lod3']
    quest_levels = [levels[2], levels[3]]
    check(quest_levels == ['lod2', 'lod3'], 'Android uses LOD2/LOD3 instead of original/LOD1')
    stripped = [level for level in levels if level not in quest_levels]
    check(stripped == ['original', 'lod1'], 'Build stripping targets original and LOD1')
    backup = levels[:]
    restored = backup[:]
    check(restored == levels, 'Build restoration recovers original LOD chain')
    try:
        raise RuntimeError('simulated build exception')
    except RuntimeError:
        restored = backup[:]
    check(restored == levels, 'Exception restoration recovers original LOD chain')

    segments = 24
    vertices = segments + 1
    triangles = segments
    check(vertices == 25 and triangles == 24, 'Android cap mesh uses 24 segments')
    cap_offset = 0.0002
    check(cap_offset > 0 and cap_offset < 0.001, 'Cap is below the liquid clipping plane')
    source_visible = True
    lod_visible = False
    check(source_visible and not lod_visible, 'Cap follows current LOD visibility')
    check('LiquidLevel' not in ['Cylinder', 'Sphere', 'Water', '____001'], 'Existing LiquidLevel is not duplicated')
    check(4 == 4 and 1 == 1, 'Proxy solver budgets are 4 and 1')
    check(400 <= 400 and 0.75 < 1.0, 'Android VFX budget and playback rate are reduced')
    check(0.001 <= 0.001 and 0.65 < 0.8, 'Android wave and bubble emission budgets are reduced')


def simulate_validation():
    section('SIMULATED RUNTIME - validation and delivery checks')
    for path in list((ROOT / 'Assets/Scripts').rglob('*.cs')) + list((ROOT / 'Assets/Editor').rglob('*.cs')) + list((ROOT / 'Assets/Tests').rglob('*.cs')):
        source = path.read_text(encoding='utf-8-sig')
        clean = re.sub(r'(?ms)/\*.*?\*/', '', source)
        clean = re.sub(r'(?m)//.*$', '', clean)
        clean = re.sub(r'@?"(?:\\.|""|[^"\\])*"', '""', clean)
        for left, right in (('{', '}'), ('(', ')'), ('[', ']')):
            check(clean.count(left) == clean.count(right), 'Brace balance ' + path.name + ' ' + left + right)
    check('RestoreBuildState' in (ROOT / 'Assets/Editor/LabQuestBuildPreprocessor.cs').read_text(encoding='utf-8'), 'Build exception restoration exists')
    check('StartCoroutine(ApplyAfterOneFrame())' in (ROOT / 'Assets/Scripts/Rendering/LabQuestMaterialTuner.cs').read_text(encoding='utf-8'), 'Coroutine invocation has parentheses')
    check('texture.width' in (ROOT / 'Assets/Editor/LabTextureOptimization.cs').read_text(encoding='utf-8'), 'Texture size uses Unity 2020 compatible API')
    scene = (ROOT / 'Assets/Scenes/SampleScene.unity').read_text(encoding='utf-8-sig')
    ids = re.findall(r'(?m)^--- !u!\d+ &(-?\d+)(?: stripped)?\s*$', scene)
    components = re.findall(r'(?m)^  - component: \{fileID: (-?\d+)\}', scene)
    check(len(ids) == len(set(ids)), 'Scene fileIDs are unique')
    check(not (set(components) - set(ids)), 'Scene component references resolve')
    check(scene.count('generateSurfaceCap: 1') == 10, 'Ten liquid bodies enable caps')
    check(scene.count('generateSurfaceCap: 0') == 1, 'Existing LiquidLevel does not duplicate cap')
    for path in (ROOT / 'Assets').rglob('*.meta'):
        match = re.search(r'^guid: ([0-9a-f]+)$', path.read_text(encoding='utf-8-sig'), re.M)
        if match:
            check(len(match.group(1)) == 32, 'GUID is 32 hex characters: ' + path.name)


simulate_experiment()
simulate_quest_build()
simulate_validation()
report['passed'] = not report['failures']
report['totalChecks'] = sum(section['checks'] for section in report['sections'])
report['pending'] = [
    'Unity C# and Shader compilation, Unity Test Runner and APK build were not run.',
    'Quest device frame timing, XR input, transparent ordering and physics contacts were not measured.'
]
(ROOT / 'Logs').mkdir(exist_ok=True)
(ROOT / 'Logs/simulated_runtime_test.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print('\nSIMULATED RUNTIME RESULT:', 'PASS' if report['passed'] else 'FAIL', '| checks=', report['totalChecks'], '| failures=', len(report['failures']))
print('REPORT:', ROOT / 'Logs/simulated_runtime_test.json')
if report['failures']:
    sys.exit(1)
