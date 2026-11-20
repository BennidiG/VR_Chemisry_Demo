import bpy
from pathlib import Path
root = Path(r'C:\Users\Cherry\Desktop\VR_Chemisry_Demo')
for relative in ['Assets/Modle/a.fbx', 'Assets/Modle/Optimized/a_LOD1.fbx']:
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=str(root / relative))
    print('FILE', relative, flush=True)
    for obj in [o for o in bpy.context.scene.objects if o.type == 'MESH'][:8]:
        print('OBJECT', obj.name, 'DATA', obj.data.name, flush=True)
