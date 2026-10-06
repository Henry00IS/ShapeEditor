The Editor tests load byte-identical copies of the four projects attached to issue 17. They verify both decomposition modes and chopped partitions with counts 2, 3, and 7, then create actual Unity extrusion meshes and compare cap area and side surface area against independent reference values.

The fixtures have a .txt suffix so Unity imports them as text assets and the tests control their execution. GUIDs in the TestCaseData entries match the fixture metadata.

GenerateConvexPolygons and GenerateChoppedPolygons use additive triangles for contact regions in both useHoles modes. Holes remain empty space between those triangles. The ordinary decomposition path retains the documented subtractive-hole representation when useHoles is false. CSG consumers should account for the additional convex brush count in contact regions.

These tests are prepared for Unity EditMode execution. Portable geometry checks use production source with Unity API stand-ins and establish a separate validation scope. Native Unity execution and live CSG verification remain required acceptance steps.

The thin-contact regression constructs a square touching a triangle at a boundary interval about 0.000001 units long. It checks the preserved area and actual extrusion brush planes for unit normals and consistent half spaces. Plane construction normalizes its cross product in double precision so small valid faces retain a usable normal.

Contact detection sorts endpoints along the longer coordinate axis and scans only nearby candidates using PolyBool's original epsilon predicates. Its cost is O(N log N + K) with a quadratic worst case for dense axis-aligned candidates. Triangle output deliberately avoids merged polygons that become invalid after float conversion. A contact can therefore increase brush count outside the touching lobes. Portable preprocessing measurements exclude native scene rebuilding and live CSG cost, which require separate profiling.
