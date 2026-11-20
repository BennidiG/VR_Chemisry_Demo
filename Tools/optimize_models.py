import bpy
import json
import os
import sys
from pathlib import Path

ROOT = Path(os.environ.get('VR_PROJECT_ROOT', r'C:\Users\Cherry\Desktop\VR_Chemisry_Demo'))
OUT = ROOT / 'Assets' / 'Modle' / 'Optimized'
OUT.mkdir(parents=True, exist_ok=True)

REPORT = []


def mesh_stats(objects):
    triangles = 0
    vertices = 0
    for obj in objects:
        if obj.type != 'MESH':
            continue
        mesh = obj.data
        vertices += len(mesh.vertices)
        mesh.calc_loop_triangles()
        triangles += len(mesh.loop_triangles)
    return vertices, triangles


def clear_scene():
    # Reset datablocks as well as objects; deleting only objects leaves Mesh
    # datablocks alive and causes suffixes on the next FBX import.
    bpy.ops.wm.read_factory_settings(use_empty=True)


def import_model(source):
    bpy.ops.import_scene.fbx(filepath=str(source))
    return [obj for obj in bpy.context.scene.objects]


def export_model(objects, destination):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=str(destination),
        use_selection=True,
        object_types={'MESH', 'EMPTY', 'ARMATURE'},
        bake_anim=False,
        use_mesh_modifiers=True,
        add_leaf_bones=False,
    )


def decimate(source_name, output_name, ratio):
    source = ROOT / 'Assets' / 'Modle' / source_name
    destination = OUT / output_name
    clear_scene()
    objects = import_model(source)
    before = mesh_stats(objects)
    for obj in objects:
        if obj.type != 'MESH':
            continue
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new('VR Chemistry LOD', 'DECIMATE')
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.mesh.remove_doubles(threshold=0.000001)
        bpy.ops.mesh.dissolve_degenerate()
        bpy.ops.object.mode_set(mode='OBJECT')
        obj.select_set(False)
    after = mesh_stats(objects)
    export_model(objects, destination)
    REPORT.append({'source': source_name, 'output': str(destination.relative_to(ROOT)), 'ratio': ratio, 'before': before, 'after': after})
    print('LOD', source_name, output_name, before, '->', after, flush=True)


def convex_proxy(source_name, output_name):
    source = ROOT / 'Assets' / 'Modle' / source_name
    destination = OUT / output_name
    clear_scene()
    objects = import_model(source)
    before = mesh_stats(objects)
    for obj in objects:
        if obj.type != 'MESH':
            continue
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.mesh.convex_hull()
        bpy.ops.object.mode_set(mode='OBJECT')
        modifier = obj.modifiers.new('VR Chemistry Collision LOD', 'DECIMATE')
        modifier.ratio = 0.08 if source_name == 'a.fbx' else 0.2
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.mesh.remove_doubles(threshold=0.000001)
        bpy.ops.mesh.dissolve_degenerate()
        bpy.ops.object.mode_set(mode='OBJECT')
        obj.select_set(False)
    after = mesh_stats(objects)
    export_model(objects, destination)
    REPORT.append({'source': source_name, 'output': str(destination.relative_to(ROOT)), 'proxy': 'convex_hull', 'before': before, 'after': after})
    print('PROXY', source_name, output_name, before, '->', after, flush=True)


for source, prefix, first, second, third in [
    ('a.fbx', 'a', 0.45, 0.20, 0.08),
    ('18中式陶瓷洗手盆.fbx', 'basin', 0.50, 0.25, 0.12),
    ('TestTubeRack.fbx', 'rack', 0.50, 0.25, 0.12),
]:
    decimate(source, prefix + '_LOD1.fbx', first)
    decimate(source, prefix + '_LOD2.fbx', second)
    decimate(source, prefix + '_LOD3.fbx', third)

convex_proxy('a.fbx', 'a_PROXY.fbx')
convex_proxy('TestTubeRack.fbx', 'rack_PROXY.fbx')

with open(OUT / 'optimization_report.json', 'w', encoding='utf-8') as stream:
    json.dump(REPORT, stream, ensure_ascii=False, indent=2)
print('MODEL_OPTIMIZATION_COMPLETE', OUT, flush=True)
