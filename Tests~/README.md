# Point-contact region regressions

The standalone Unity Editor harness exercises the production `Project.GenerateConvexPolygons` and `SegmentListToConvexPolygonMesh` paths. It checks positive convex output, repeated vertices, finite coordinates, signed filled area for synthetic cases, Unity error logs, and an independent 21 × 21 point grid against the input contours. Owner fixtures use their original shape contours and boolean operators as the grid oracle.

The four `.s2d` fixtures are the owner's original [issue #17 examples](https://github.com/Henry00IS/ShapeEditor/files/9568260/S2D.Projects.Bad.zip). They are initialized with `Project.Validate()`, matching the production importer, and tested with both `useHoles` settings. Fixtures remain unmodified. The repository's MIT license is preserved.

To prepare a new project outside this repository:

```text
python Tests~/prepare_validation.py --project <new-project-directory>
```

Run the installed Unity Editor:

```text
Unity -batchmode -nographics -quit -projectPath <new-project-directory> -executeMethod ShapeEditorSingularityTests.Run -logFile <log-path>
```

The harness writes `results.json` in the project root and exits with code 0 when every case passes, or 2 on a test failure. `-resultPath <file>` can select a different report file. The project includes the package's Mathematics dependency and required built-in modules; no RealtimeCSG installation is needed for this region-conversion regression suite.

The supplied final receipt records 27/27 passing cases on Windows Unity 2022.3.22f1, with 11,907 filled-point comparisons. `baseline-initialized-results.json` records 16 passing and 11 failing cases on original master `695aa8931981610a77f068a7d68a1c05e91654d5`, after fixing fixture initialization. Baseline results predate the added point-grid checks; those checks strengthen the final suite and are not represented as baseline executions.

Coverage includes vertex-on-edge contacts, two tips on one edge, multiple lobes, ordinary concavity, separate regions, ordinary and small holes, touching holes, an exterior touching a hole, and tiny touching squares with total area `2e-12`. This validates the tested geometry and APIs; it does not claim exhaustive geometry coverage, older Unity compatibility, or CSG/editor UI interaction testing.
