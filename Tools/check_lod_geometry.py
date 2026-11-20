import bpy
import json
import math
import os
from pathlib import Path

ROOT = Path(os.environ.get('VR_PROJECT_ROOT', r'C:\Users\Cherry\Desktop\VR_Chemisry_Demo'))
REPORT = []


def clear():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def normalize(value):
    return value.replace(' ', '').replace('_', '').replace('.', '')


def load(path):
    clear()
    bpy.ops.import_scene.fbx(filepath=str(path))
    return [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']


def geometry_stats(objects):
    result = {'vertices': 0, 'triangles': 0, 'uvMeshes': 0, 'invalidValues': 0, 'degenerateTriangles': 0}
    for obj in objects:
        mesh = obj.data
        mesh.calc_loop_triangles()
        result['vertices'] += len(mesh.vertices)
        result['triangles'] += len(mesh.loop_triangles)
        if len(mesh.uv_layers) > 0:
            result['uvMeshes'] += 1
        for vertex in mesh.vertices:
            if not all(math.isfinite(value) for value in vertex.co):
                result['invalidValues'] += 1
        for triangle in mesh.loop_triangles:
            if triangle.area < 1e-14:
                result['degenerateTriangles'] += 1
    return result


def inspect(source, optimized):
    original_objects = load(ROOT / 'Assets' / 'Modle' / source)
    original_names = [obj.data.name for obj in original_objects]
    original_stats = geometry_stats(original_objects)
    optimized_objects = load(ROOT / 'Assets' / 'Modle' / 'Optimized' / optimized)
    optimized_names = [obj.data.name for obj in optimized_objects]
    optimized_stats = geometry_stats(optimized_objects)

    missing = []
    ambiguous = []
    for name in original_names:
        exact = name in optimized_names
        matches = [candidate for candidate in optimized_names if normalize(candidate) == normalize(name)]
        if not exact and not matches:
            missing.append(name)
        elif len(matches) > 1 and not exact:
            ambiguous.append(name)

    degenerate_delta = optimized_stats['degenerateTriangles'] - original_stats['degenerateTriangles']
    uv_preserved = optimized_stats['uvMeshes'] >= original_stats['uvMeshes']
    result = {
        'source': source,
        'optimized': optimized,
        'originalMeshCount': len(original_objects),
        'optimizedMeshCount': len(optimized_objects),
        'original': original_stats,
        'optimized': optimized_stats,
        'uvPreserved': uv_preserved,
        'degenerateDelta': degenerate_delta,
        'missingNames': missing,
        'ambiguousNames': ambiguous,
    }
    REPORT.append(result)
    print(source, '->', optimized, 'triangles', original_stats['triangles'], '->', optimized_stats['triangles'], 'uv', original_stats['uvMeshes'], '->', optimized_stats['uvMeshes'], 'degenerate', original_stats['degenerateTriangles'], '->', optimized_stats['degenerateTriangles'], 'missing=', missing, 'ambiguous=', ambiguous, flush=True)


for source, optimized in [
    ('a.fbx', 'a_LOD1.fbx'),
    ('a.fbx', 'a_LOD2.fbx'),
    ('a.fbx', 'a_LOD3.fbx'),
    ('18中式陶瓷洗手盆.fbx', 'basin_LOD1.fbx'),
    ('18中式陶瓷洗手盆.fbx', 'basin_LOD2.fbx'),
    ('18中式陶瓷洗手盆.fbx', 'basin_LOD3.fbx'),
    ('TestTubeRack.fbx', 'rack_LOD1.fbx'),
    ('TestTubeRack.fbx', 'rack_LOD2.fbx'),
    ('TestTubeRack.fbx', 'rack_LOD3.fbx'),
    ('a.fbx', 'a_PROXY.fbx'),
    ('TestTubeRack.fbx', 'rack_PROXY.fbx'),
]:
    inspect(source, optimized)

failed = [item for item in REPORT if item['missingNames'] or item['ambiguousNames'] or item['optimized']['invalidValues'] > 0 or (not item['uvPreserved'] and '_PROXY' not in item['optimized']) or item['degenerateDelta'] > 0 or item['optimized']['triangles'] == 0 or item['optimizedMeshCount'] == 0]
output = ROOT / 'Logs' / 'lod_geometry_report.json'
output.parent.mkdir(exist_ok=True)
output.write_text(json.dumps({'passed': not failed, 'models': REPORT, 'failed': failed}, ensure_ascii=False, indent=2), encoding='utf-8')
print('GEOMETRY_SIMULATION_RESULT', 'PASS' if not failed else 'FAIL', 'REPORT', output, flush=True)
if failed:
    raise SystemExit(1)
