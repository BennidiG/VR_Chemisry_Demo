import bpy
import os
from pathlib import Path

ROOT = Path(os.environ.get('VR_PROJECT_ROOT', r'C:\Users\Cherry\Desktop\VR_Chemisry_Demo'))


def clear():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def load(path):
    clear()
    bpy.ops.import_scene.fbx(filepath=str(path))
    return [obj.data.name for obj in bpy.context.scene.objects if obj.type == 'MESH']


def normalize(value):
    return value.replace(' ', '').replace('_', '').replace('.', '')


for source, lod1, lod2 in [
    ('a.fbx', 'a_LOD1.fbx', 'a_LOD2.fbx', 'a_LOD3.fbx'),
    ('18中式陶瓷洗手盆.fbx', 'basin_LOD1.fbx', 'basin_LOD2.fbx', 'basin_LOD3.fbx'),
    ('TestTubeRack.fbx', 'rack_LOD1.fbx', 'rack_LOD2.fbx', 'rack_LOD3.fbx'),
]:
    original = load(ROOT / 'Assets' / 'Modle' / source)
    for lod_name in (lod1, lod2, lod3):
        optimized = load(ROOT / 'Assets' / 'Modle' / 'Optimized' / lod_name)
        missing = []
        ambiguous = []
        for name in original:
            exact = name in optimized
            matches = [candidate for candidate in optimized if normalize(candidate) == normalize(name)]
            if not exact and not matches:
                missing.append(name)
            elif len(matches) > 1 and name not in optimized:
                ambiguous.append(name)
        print(source, '->', lod_name, 'original=', len(original), 'optimized=', len(optimized), 'missing=', missing, 'ambiguous=', ambiguous, flush=True)
