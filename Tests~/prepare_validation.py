from pathlib import Path
import argparse
import json
import os
import shutil

parser = argparse.ArgumentParser()
parser.add_argument('--project', type=Path)
parser.add_argument('--unity-version', default='2022.3.22f1')
args = parser.parse_args()
tests = Path(__file__).resolve().parent
repo = tests.parent
project = (args.project or repo.parent / (repo.name + '-singularity-validation')).resolve()
if project == repo or repo in project.parents:
    raise SystemExit('Use a new validation directory outside the source repository')
relative_source = os.path.relpath(repo, project / 'Packages').replace('\\', '/')
project.mkdir(parents=True, exist_ok=False)
for sub in ('Assets/Editor', 'Assets/OwnerFixtures', 'Packages', 'ProjectSettings'):
    (project / sub).mkdir(parents=True, exist_ok=True)
(project / 'Packages/manifest.json').write_text(json.dumps({'dependencies': {
    'com.aeternumgames.shapeeditor': 'file:' + relative_source,
    'com.unity.mathematics': '1.1.0',
    'com.unity.modules.imgui': '1.0.0',
    'com.unity.modules.jsonserialize': '1.0.0',
    'com.unity.modules.physics': '1.0.0',
    'com.unity.modules.imageconversion': '1.0.0'
}}, indent=2), encoding='utf-8')
(project / 'ProjectSettings/ProjectVersion.txt').write_text('m_EditorVersion: ' + args.unity_version + '\n', encoding='utf-8')
shutil.copyfile(tests / 'ShapeEditorSingularityTests.cs', project / 'Assets/Editor/ShapeEditorSingularityTests.cs')
for fixture in (tests / 'Fixtures').glob('*.s2d'):
    shutil.copyfile(fixture, project / 'Assets/OwnerFixtures' / fixture.name)
print(project)
