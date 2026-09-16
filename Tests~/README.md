# Polygon mesh / RealtimeCSG validation

Issue: https://github.com/Henry00IS/ShapeEditor/issues/3

The production package keeps RealtimeCSG optional. This directory is excluded from Unity package import. Copy its tests into a separate project with the helper below; do not add a RealtimeCSG reference to the production assembly.

## Reproduce

Use Unity 2022.3.62f2 and RealtimeCSG 1.60.1 at commit `8ea1d81e917c538d8f4563327c2c657b50a355f0`. RealtimeCSG's supplied macOS native plugin is x86_64, so use the **Intel** editor under Rosetta on Apple Silicon. The ARM editor can compile the package but cannot validate this native plugin.

From the package checkout:

```sh
python3 'Tests~/setup_validation.py' --unity '/path/to/Unity' --project /tmp/ShapeEditorValidation
```

The helper creates a new project and clones the pinned dependency beside it. Pass `--realtimecsg /path/to/RealtimeCSG` to reuse an existing checkout. It refuses to overwrite a nonempty project.

```sh
'/path/to/Unity' -batchmode -nographics -projectPath /tmp/ShapeEditorValidation \
  -runTests -testPlatform EditMode -testResults /tmp/evidence/tests.xml \
  -logFile /tmp/evidence/tests.log

'/path/to/Unity' -batchmode -nographics -quit -projectPath /tmp/ShapeEditorValidation \
  -executeMethod PolygonMeshBenchmark.Run -logFile /tmp/evidence/benchmark.log

'/path/to/Unity' -batchmode -quit -projectPath /tmp/ShapeEditorValidation \
  -executeMethod PolygonMeshDemo.Create -logFile /tmp/evidence/demo.log
```

The demo needs graphics enabled. It saves `Assets/PolygonMeshDemo/Demo.unity` plus a rendered `native-demo.png`. Open the scene for interactive inspection. Benchmark and image artifacts default to `../evidence` relative to the project; set `SHAPE_EDITOR_EVIDENCE` to override this path.

To verify optional-dependency behavior, create a second project with `--without-realtimecsg`, then run `-executeMethod TopologySmoke.Run -batchmode -nographics -quit`. This checks package compilation, absence detection, and generated topology without native CSG.

The maintainer prototype reproduction is available via `-executeMethod BountyBaseline.Run`. It exercises the original raw-list lookup against the deduplicated vertex buffer and counts collapsed pyramid edges before native submission.

## Test in the editor

1. Open the generated validation project with Unity 2022.3.62f2. On Apple Silicon, use the Intel editor under Rosetta so RealtimeCSG's native plugin can load.
2. Open **Window > General > Test Runner**, choose **EditMode**, and click **Run All**. Expect 41 passing tests and no skipped tests. Run these in the disposable validation project: the fixtures replace the active scene.
3. Run the demo command above, then open `Assets/PolygonMeshDemo/Demo.unity`. Inspect the prism, both pyramid directions, and the subtraction hole in Scene view. Their surfaces should be closed, with no missing faces or spikes.
4. Open **Window > 2D Shape Editor**, create a closed shape, and choose **RealtimeCSG > Create Bevel** from the window's toolbar. Select the generated target. In its Inspector, set **Front Scale** to `(0, 0)` and **Back Scale** to `(1, 1)`, then swap them. Both should produce a closed pyramid without Console errors. Repeat with triangular and square profiles.
5. Change the depth and face materials, click **Rebuild** repeatedly, and exercise Undo/Redo. For a spline target, move its control-point children and rebuild; the generated `Brushes` container should never become an extra control point.

The automated results are recorded in [VALIDATION.md](VALIDATION.md). Steps 4–5 are the interactive authoring checks to perform before submission; they are not claimed as completed by the automated suite. Use the benchmark command for the repeatable performance comparison, with other Unity instances and heavy workloads closed.

## Coverage and measurement

- Real generated render-mesh volumes for prisms, both zero-scale pyramid directions, frusta, a wedge, an offset pyramid under a transformed parent, and subtraction.
- Half-edge reciprocity, shared vertex indices, RealtimeCSG's own validator, surface normals/tangents, source immutability, materials and current texture defaults.
- Rebuilds through all six target modes; depth Undo/Redo and ten successive rebuilds in both pyramid directions with native-volume and material checks.
- Open, duplicate, mixed-winding, non-finite, nonplanar, concave, flat, missing and oversized inputs must create no brush GameObject.
- Benchmarks compare the existing plane reconstruction with direct conversion on 4/16/32/64/128-sided prisms. Both paths must first pass native volume checks. Each scope uses two warm-up samples followed by seven alternating samples per path; results report medians and preserve raw samples. Destruction is outside the timed regions. Creation-only excludes subsequent native rebuild; creation-and-rebuild includes `CSGModelManager.EnsureBuildFinished`. These are editor workload timings, not player FPS measurements.

## Implementation boundaries

Vertices are welded within `1e-5` per coordinate using neighboring spatial cells. Adjacent duplicate vertices and collapsed faces are removed without changing the source polygons. Face material mappings follow original polygon indices. Closed convex geometry is required: invalid/open meshes and T-junctions are rejected with a warning before creating a scene object. This does not attempt to repair arbitrary concave meshes.

Half-edge pairing uses a dictionary instead of scanning every edge pair. Convexity checking still costs O(faces × vertices), and RealtimeCSG's own legacy validation also has nonlinear work. The entire conversion is therefore not claimed to be linear. RealtimeCSG 1.60.1 rejects tetrahedra at its native-binding minimum-count guards. The converter subdivides one tetrahedron face into three coplanar triangles, preserving its boundary, volume and source material while meeting those limits. The dependency is not patched.

Two generator integration regressions are also covered: collapsed quads no longer assert in `SplitNonPlanar4`, and the spline target excludes its generated `Brushes` child when collecting spline control points.

The existing plane API remains available for other callers; all six Shape Editor target modes use the new path.
