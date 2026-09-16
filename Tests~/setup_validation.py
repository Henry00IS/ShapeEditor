#!/usr/bin/env python3
import argparse
import json
from pathlib import Path
import shutil
import subprocess

parser = argparse.ArgumentParser(description='Create an isolated Unity validation project without modifying an existing project.')
parser.add_argument('--project', type=Path, required=True, help='New, empty project directory')
parser.add_argument('--unity', type=Path, required=True, help='Unity executable (2022.3.62f2 was tested)')
parser.add_argument('--realtimecsg', type=Path, help='Existing RealtimeCSG source directory')
parser.add_argument('--without-realtimecsg', action='store_true')
args = parser.parse_args()
project = args.project.resolve()
if project.exists() and any(project.iterdir()):
    parser.error('The project directory must be empty; existing projects are never overwritten.')
package = Path(__file__).resolve().parent.parent
unity = args.unity.resolve()
if not unity.is_file():
    parser.error('Unity executable does not exist.')
for folder in ('Assets/Editor', 'Packages', 'ProjectSettings'):
    (project / folder).mkdir(parents=True, exist_ok=True)
dependencies = {
    'com.aeternumgames.shapeeditor': 'file:' + str(package),
    'com.unity.test-framework': '1.1.33',
}
modules = ('ai', 'animation', 'audio', 'cloth', 'director', 'imageconversion', 'imgui',
           'jsonserialize', 'particlesystem', 'physics', 'physics2d', 'screencapture',
           'terrain', 'terrainphysics', 'tilemap', 'ui', 'uielements', 'umbra',
           'unityanalytics', 'unitywebrequest', 'unitywebrequestassetbundle',
           'unitywebrequestaudio', 'unitywebrequesttexture', 'unitywebrequestwww',
           'vehicles', 'video', 'vr', 'wind', 'xr')
for module in modules:
    dependencies['com.unity.modules.' + module] = '1.0.0'
if args.without_realtimecsg:
    shutil.copy2(package / 'Tests~/WithoutRealtimeCSG/TopologySmoke.cs', project / 'Assets/Editor')
else:
    dependency = args.realtimecsg.resolve() if args.realtimecsg else project.parent / (project.name + '-RealtimeCSG')
    if not args.realtimecsg:
        subprocess.run(['git', 'clone', 'https://github.com/LogicalError/realtime-CSG-for-unity.git', str(dependency)], check=True)
        subprocess.run(['git', '-C', str(dependency), 'checkout', '8ea1d81e917c538d8f4563327c2c657b50a355f0'], check=True)
    if not (dependency / 'package.json').is_file():
        parser.error('RealtimeCSG package.json is missing.')
    dependencies['com.prenominal.realtimecsg'] = 'file:' + str(dependency)
    tests = project / 'Assets/Tests'
    tests.mkdir()
    for source in (package / 'Tests~').iterdir():
        if source.suffix in ('.cs', '.asmdef'):
            shutil.copy2(source, tests)
(project / 'Packages/manifest.json').write_text(json.dumps({'dependencies': dependencies}, indent=2) + '\n')
(project / 'ProjectSettings/ProjectVersion.txt').write_text('m_EditorVersion: 2022.3.62f2\nm_EditorVersionWithRevision: 2022.3.62f2 (7670c08855a9)\n')
(project.parent / 'evidence').mkdir(exist_ok=True)
print('Created', project)
print('Run the commands in Tests~/README.md using your Unity and project paths.')
