#if UNITY_EDITOR

// contains source code from https://github.com/LogicalError/realtime-CSG-for-unity (see Licenses/RealtimeCSG.txt).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Provides communication with RealtimeCSG through reflection.
    /// </summary>
    public class ExternalRealtimeCSG
    {
        /// <summary>
        /// The cached RealtimeCSG.Legacy.BrushFactory type after initialization.
        /// </summary>
        private static Type brushFactory = null;

        /// <summary>
        /// The cached create brush from planes method after initialization.
        /// </summary>
        private static MethodInfo createBrushFromPlanesMethod = null;

        /// <summary>
        /// The cached create brush method after initialization.
        /// </summary>
        private static MethodInfo createBrushMethod = null;

        /// <summary>
        /// The cached RealtimeCSG.ShapePolygonUtility type after initialization.
        /// </summary>
        private static Type shapePolygonUtility = null;

        /// <summary>
        /// The cached create clean sub polygons from vertices method after initialization.
        /// </summary>
        private static MethodInfo createCleanSubPolygonsFromVerticesMethod = null;

        /// <summary>
        /// The cached generate control mesh from vertices method after initialization.
        /// </summary>
        private static MethodInfo generateControlMeshFromVerticesMethod = null;

        /// <summary>
        /// The cached RealtimeCSG.Components.CSGBrush type after initialization.
        /// </summary>
        private static Type csgBrush = null;

        /// <summary>
        /// The cached CSGBrush OperationType field after initialization.
        /// </summary>
        private static FieldInfo csgBrushOperationType = null;

        /// <summary>
        /// The cached RealtimeCSG.Foundation.CSGOperationType type after initialization.
        /// </summary>
        private static Type csgOperationType = null;

        /// <summary>
        /// The cached CSGOperationType.Subtractive enum value after initialization.
        /// </summary>
        private static object csgOperationTypeSubtractive = null;

        /// <summary>
        /// The cached RealtimeCSG.Components.CSGNode type after initialization.
        /// </summary>
        private static Type csgNode = null;

        /// <summary>
        /// The cached RealtimeCSG.Legacy.CSGPlane type after initialization.
        /// </summary>
        private static Type csgPlane = null;

        /// <summary>
        /// The cached CSGPlane constructor method after initialization.
        /// </summary>
        private static ConstructorInfo csgPlaneConstructorMethod = null;

        /// <summary>
        /// The cached CSGOperation component type after initialization.
        /// </summary>
        private static Type csgOperation = null;

        /// <summary>
        /// The cached CSGOperation HandleAsOne field after initialization.
        /// </summary>
        private static FieldInfo csgOperationHandleAsOneField = null;

        /// <summary>
        /// The cached EditModeManager type after initialization.
        /// </summary>
        private static Type editModeManager = null;

        /// <summary>
        /// The cached update selection method after initialization.
        /// </summary>
        private static MethodInfo updateSelectionMethod = null;

        /// <summary>
        /// The cached MaterialUtility type after initialization.
        /// </summary>
        private static Type materialUtility = null;

        /// <summary>
        /// The cached MaterialUtility WallMaterial property after initialization.
        /// </summary>
        private static PropertyInfo materialUtilityWallMaterialProperty = null;

        private static Type legacyControlMesh = null;
        private static Type legacyPolygon = null;
        private static Type legacyHalfEdge = null;
        private static Type legacyShape = null;
        private static Type legacySurface = null;
        private static Type legacyTexGen = null;
        private static Type geometryUtility = null;
        private static Type controlMeshUtility = null;
        private static Type shapeUtility = null;
        private static Type csgSettings = null;
        private static ConstructorInfo legacyPolygonConstructor = null;
        private static ConstructorInfo legacyHalfEdgeConstructor = null;
        private static ConstructorInfo legacyTexGenConstructor = null;
        private static MethodInfo calcPolygonPlaneMethod = null;
        private static MethodInfo calculateTangentsMethod = null;
        private static MethodInfo validateControlMeshMethod = null;
        private static MethodInfo ensureInitializedShapeMethod = null;
        private static FieldInfo csgSettingsDefaultTexGenFlagsField = null;

        /// <summary>
        /// Used to store whether an initialization error occured.
        /// </summary>
        private static bool initializationError;

        /// <summary>
        /// Used to store whether RealtimeCSG is available.
        /// </summary>
        private static bool initializationSuccess;

        /// <summary>
        /// Ensures that RealtimeCSG exists and is available. It remembers the error and success
        /// states, further calls to this function are extremely fast.
        /// </summary>
        /// <returns>True when RealtimeCSG is ready or else false.</returns>
        public static bool IsAvailable()
        {
            // if an initialization error occured we stop here.
            if (initializationError) return false;
            // if already available we also stop here.
            if (initializationSuccess) return true;

            brushFactory = GetType("RealtimeCSG.Legacy.BrushFactory");
            if (brushFactory == null) { initializationError = true; return false; }

            createBrushFromPlanesMethod = brushFactory.GetMethodByName("CreateBrushFromPlanes", "brushName", "planes", "tangents", "binormals", "materials", "textureMatrices", "textureMatrixSpace");
            if (createBrushFromPlanesMethod == null) { initializationError = true; return false; }

            createBrushMethod = brushFactory.GetMethodByName("CreateBrush", "parent", "brushName", "controlMesh", "shape");
            if (createBrushMethod == null) { initializationError = true; return false; }

            shapePolygonUtility = GetType("RealtimeCSG.ShapePolygonUtility");
            if (shapePolygonUtility == null) { initializationError = true; return false; }

            createCleanSubPolygonsFromVerticesMethod = shapePolygonUtility.GetMethodByName("CreateCleanSubPolygonsFromVertices", "vertices2d", "buildPlane");
            if (createCleanSubPolygonsFromVerticesMethod == null) { initializationError = true; return false; }

            generateControlMeshFromVerticesMethod = shapePolygonUtility.GetMethodByName("GenerateControlMeshFromVertices", "shape2DPolygon", "localToWorld", "direction", "height", "capTexgen", "smooth", "singleSurfaceEnds", "controlMesh", "shape");
            if (generateControlMeshFromVerticesMethod == null) { initializationError = true; return false; }

            csgBrush = GetType("RealtimeCSG.Components.CSGBrush");
            if (csgBrush == null) { initializationError = true; return false; }

            csgBrushOperationType = csgBrush.GetField("OperationType");
            if (csgBrushOperationType == null) { initializationError = true; return false; }

            csgOperationType = GetType("RealtimeCSG.Foundation.CSGOperationType");
            if (csgOperationType == null) { initializationError = true; return false; }

            csgOperationTypeSubtractive = Enum.Parse(csgOperationType, "Subtractive");
            if (csgOperationTypeSubtractive == null) { initializationError = true; return false; }

            csgPlane = GetType("RealtimeCSG.Legacy.CSGPlane");
            if (csgPlane == null) { initializationError = true; return false; }

            csgNode = GetType("RealtimeCSG.Components.CSGNode");
            if (csgNode == null) { initializationError = true; return false; }

            csgPlaneConstructorMethod = csgPlane.GetConstructor(new Type[] { typeof(Plane) });
            if (csgPlaneConstructorMethod == null) { initializationError = true; return false; }

            csgOperation = GetType("RealtimeCSG.Components.CSGOperation");
            if (csgOperation == null) { initializationError = true; return false; }

            csgOperationHandleAsOneField = csgOperation.GetField("HandleAsOne");
            if (csgOperationHandleAsOneField == null) { initializationError = true; return false; }

            editModeManager = GetType("RealtimeCSG.EditModeManager");
            if (editModeManager == null) { initializationError = true; return false; }

            updateSelectionMethod = editModeManager.GetMethodByName("UpdateSelection", "forceUpdate");
            if (updateSelectionMethod == null) { initializationError = true; return false; }

            materialUtility = GetType("RealtimeCSG.MaterialUtility");
            if (materialUtility == null) { initializationError = true; return false; }

            materialUtilityWallMaterialProperty = materialUtility.GetProperty("WallMaterial");
            if (materialUtilityWallMaterialProperty == null) { initializationError = true; return false; }

            legacyControlMesh = GetType("RealtimeCSG.Legacy.ControlMesh");
            if (legacyControlMesh == null) { initializationError = true; return false; }

            legacyPolygon = GetType("RealtimeCSG.Legacy.Polygon");
            if (legacyPolygon == null) { initializationError = true; return false; }

            legacyHalfEdge = GetType("RealtimeCSG.Legacy.HalfEdge");
            if (legacyHalfEdge == null) { initializationError = true; return false; }

            legacyShape = GetType("RealtimeCSG.Legacy.Shape");
            if (legacyShape == null) { initializationError = true; return false; }

            legacySurface = GetType("RealtimeCSG.Legacy.Surface");
            if (legacySurface == null) { initializationError = true; return false; }

            legacyTexGen = GetType("RealtimeCSG.Legacy.TexGen");
            if (legacyTexGen == null) { initializationError = true; return false; }

            geometryUtility = GetType("RealtimeCSG.GeometryUtility");
            if (geometryUtility == null) { initializationError = true; return false; }

            controlMeshUtility = GetType("RealtimeCSG.ControlMeshUtility");
            if (controlMeshUtility == null) { initializationError = true; return false; }

            shapeUtility = GetType("RealtimeCSG.ShapeUtility");
            if (shapeUtility == null) { initializationError = true; return false; }

            csgSettings = GetType("RealtimeCSG.CSGSettings");
            if (csgSettings == null) { initializationError = true; return false; }

            legacyPolygonConstructor = legacyPolygon.GetConstructor(new Type[] { typeof(int[]), typeof(int) });
            if (legacyPolygonConstructor == null) { initializationError = true; return false; }

            legacyHalfEdgeConstructor = legacyHalfEdge.GetConstructor(new Type[] { typeof(short), typeof(int), typeof(short), typeof(bool) });
            if (legacyHalfEdgeConstructor == null) { initializationError = true; return false; }

            legacyTexGenConstructor = legacyTexGen.GetConstructor(new Type[] { typeof(Material), typeof(PhysicMaterial) });
            if (legacyTexGenConstructor == null) { initializationError = true; return false; }

            calcPolygonPlaneMethod = geometryUtility.GetMethod("CalcPolygonPlane", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { legacyControlMesh, typeof(short) }, null);
            if (calcPolygonPlaneMethod == null) { initializationError = true; return false; }

            calculateTangentsMethod = geometryUtility.GetMethod("CalculateTangents", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(Vector3), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType() }, null);
            if (calculateTangentsMethod == null) { initializationError = true; return false; }

            validateControlMeshMethod = controlMeshUtility.GetMethod("Validate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { legacyControlMesh, legacyShape }, null);
            if (validateControlMeshMethod == null) { initializationError = true; return false; }

            ensureInitializedShapeMethod = shapeUtility.GetMethod("EnsureInitialized", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { legacyShape }, null);
            if (ensureInitializedShapeMethod == null) { initializationError = true; return false; }

            csgSettingsDefaultTexGenFlagsField = csgSettings.GetField("DefaultTexGenFlags");
            if (csgSettingsDefaultTexGenFlagsField == null) { initializationError = true; return false; }

            initializationSuccess = true;
            return true;
        }

        /// <summary>
        /// Gets the csharp assemblies created by user scripts in the assets directory.
        /// </summary>
        /// <returns>The Assembly-CSharp Assemblies.</returns>
        private static IEnumerable<Assembly> GetUserAssetsAssemblies()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyName = assembly.FullName.ToLowerInvariant();
                if (assemblyName.StartsWith("realtimecsg") || assemblyName.StartsWith("assembly-csharp"))
                    yield return assembly;
            }
        }

        /// <summary>
        /// Gets the type of a RealtimeCSG type by full name.
        /// </summary>
        /// <param name="name">The name to find in the assets assemblies.</param>
        /// <returns>Returns the type or null if not found.</returns>
        private static Type GetType(string name)
        {
            foreach (var assembly in GetUserAssetsAssemblies())
            {
                var type = assembly.GetType(name);
                if (type != null)
                    return type;
            }
            return null;
        }

        private static object NewCSGPlane(Plane plane)
        {
            return csgPlaneConstructorMethod.Invoke(new object[] { plane });
        }

        public static Material WallMaterial
        {
            get
            {
                if (!IsAvailable()) return null;
                return (Material)materialUtilityWallMaterialProperty.GetValue(materialUtility);
            }
        }

        public static void CreateExtrudedBrushesFromPolygon(Transform parent, string brushName, Vector2[] vertices, float distance, PolygonBooleanOperator booleanOperator)
        {
            if (!IsAvailable()) return;

            // create a list of RealtimeCSG.ShapePolygon out of the input vertices.
            var listShapePolygon = (IEnumerable<object>)createCleanSubPolygonsFromVerticesMethod.Invoke(null, new object[] { vertices, NewCSGPlane(new Plane(Vector3.forward, 0f)) });

            // for every shape polygon:
            foreach (var shapePolygon in listShapePolygon)
            {
                // generate a control mesh and shape.
                var m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(270f, 180f, 180f)), Vector3.one);
                var args = new object[] { shapePolygon, m, Vector3.up, -distance, null, false, true, null, null };
                if (!(bool)generateControlMeshFromVerticesMethod.Invoke(null, args))
                {
                    Debug.LogWarning("Failed to generate a control mesh.");
                    continue;
                }

                // get the out parameters.
                var controlMesh = args[7];
                var shape = args[8];

                // create a brush.
                var brush = (MonoBehaviour)createBrushMethod.Invoke(null, new object[] { parent, brushName, controlMesh, shape });

                // optionally make the brush subtractive.
                if (booleanOperator == PolygonBooleanOperator.Difference)
                    csgBrushOperationType.SetValue(brush, csgOperationTypeSubtractive);
            }
        }

        public static MonoBehaviour CreateBrushFromPlanes(string brushName, Plane[] planes, Material[] materials, PolygonBooleanOperator booleanOperator)
        {
            if (!IsAvailable()) return null;

            // this fixes some of the errors, but I don't like it:
            planes = planes.Distinct().ToArray();

            var brush = createBrushFromPlanesMethod.Invoke(null, new object[] { brushName, planes, null, null, materials, null, 0 });
            if (brush == null) return null;

            // optionally make the brush subtractive.
            if (booleanOperator == PolygonBooleanOperator.Difference)
                csgBrushOperationType.SetValue(brush, csgOperationTypeSubtractive);

            return (MonoBehaviour)brush;
        }

        public static MonoBehaviour CreateBrushFromPolygonMesh(Transform parent, string brushName, PolygonMesh mesh, Material[] materials, PolygonBooleanOperator booleanOperator)
        {
            if (!IsAvailable()) return null;
            if (!TryBuildPolygonMeshBrush(mesh, materials, out var controlMesh, out var shape)) return null;

            ensureInitializedShapeMethod.Invoke(null, new object[] { shape });
            var valid = (bool)validateControlMeshMethod.Invoke(null, new object[] { controlMesh, shape });
            legacyControlMesh.GetProperty("Valid").SetValue(controlMesh, valid, null);
            if (!valid) return null;

            var brush = createBrushMethod.Invoke(null, new object[] { parent, brushName, controlMesh, shape });
            if (brush == null) return null;

            if (booleanOperator == PolygonBooleanOperator.Difference)
                csgBrushOperationType.SetValue(brush, csgOperationTypeSubtractive);

            return (MonoBehaviour)brush;
        }

        private static bool TryBuildPolygonMeshBrush(PolygonMesh mesh, Material[] materials, out object controlMesh, out object shape)
        {
            controlMesh = null;
            shape = null;

            if (mesh == null || mesh.Count < 4) return false;

            var uniqueVertices = new List<Vector3>();
            var polygonVertexIndices = new int[mesh.Count][];
            var polygonEdgeIndices = new int[mesh.Count][];
            var edgeCount = 0;

            for (int polygonIndex = 0; polygonIndex < mesh.Count; polygonIndex++)
            {
                var polygon = mesh[polygonIndex];
                if (polygon == null || polygon.Count < 3) return false;

                polygonVertexIndices[polygonIndex] = new int[polygon.Count];
                polygonEdgeIndices[polygonIndex] = new int[polygon.Count];

                for (int vertexIndex = 0; vertexIndex < polygon.Count; vertexIndex++)
                {
                    var reversedVertex = polygon[polygon.Count - 1 - vertexIndex];
                    var uniqueVertexIndex = FindOrAddVertex(uniqueVertices, reversedVertex.position);
                    if (uniqueVertexIndex > short.MaxValue) return false;

                    polygonVertexIndices[polygonIndex][vertexIndex] = uniqueVertexIndex;
                    polygonEdgeIndices[polygonIndex][vertexIndex] = edgeCount++;
                }
            }

            if (mesh.Count > short.MaxValue) return false;

            var polygons = Array.CreateInstance(legacyPolygon, mesh.Count);
            for (int polygonIndex = 0; polygonIndex < mesh.Count; polygonIndex++)
                polygons.SetValue(legacyPolygonConstructor.Invoke(new object[] { polygonEdgeIndices[polygonIndex], polygonIndex }), polygonIndex);

            var halfEdges = Array.CreateInstance(legacyHalfEdge, edgeCount);
            for (int polygonIndex = 0; polygonIndex < mesh.Count; polygonIndex++)
            {
                for (int vertexIndex = 0; vertexIndex < polygonVertexIndices[polygonIndex].Length; vertexIndex++)
                {
                    var edgeIndex = polygonEdgeIndices[polygonIndex][vertexIndex];
                    halfEdges.SetValue(legacyHalfEdgeConstructor.Invoke(new object[] { (short)polygonIndex, -1, (short)polygonVertexIndices[polygonIndex][vertexIndex], true }), edgeIndex);
                }
            }

            for (int polygonIndex = 0; polygonIndex < polygonVertexIndices.Length; polygonIndex++)
            {
                for (int vertexIndex = 0; vertexIndex < polygonVertexIndices[polygonIndex].Length; vertexIndex++)
                {
                    var twinIndex = FindTwinEdgeIndex(polygonVertexIndices, polygonEdgeIndices, polygonIndex, vertexIndex);
                    if (twinIndex < 0) return false;
                    SetHalfEdgeTwinIndex(halfEdges, polygonEdgeIndices[polygonIndex][vertexIndex], twinIndex);
                }
            }

            controlMesh = Activator.CreateInstance(legacyControlMesh);
            legacyControlMesh.GetField("Vertices").SetValue(controlMesh, uniqueVertices.ToArray());
            legacyControlMesh.GetField("Edges").SetValue(controlMesh, halfEdges);
            legacyControlMesh.GetField("Polygons").SetValue(controlMesh, polygons);

            shape = Activator.CreateInstance(legacyShape);
            var surfaces = Array.CreateInstance(legacySurface, mesh.Count);
            var texGens = Array.CreateInstance(legacyTexGen, mesh.Count);
            var texGenFlags = Array.CreateInstance(csgSettingsDefaultTexGenFlagsField.FieldType, mesh.Count);
            var defaultTexGenFlags = csgSettingsDefaultTexGenFlagsField.GetValue(null);

            for (int polygonIndex = 0; polygonIndex < mesh.Count; polygonIndex++)
            {
                var surface = Activator.CreateInstance(legacySurface);
                var plane = calcPolygonPlaneMethod.Invoke(null, new object[] { controlMesh, (short)polygonIndex });
                var normal = (Vector3)plane.GetType().GetProperty("normal").GetValue(plane, null);
                var tangentArgs = new object[] { normal, null, null };
                calculateTangentsMethod.Invoke(null, tangentArgs);

                legacySurface.GetField("Plane").SetValue(surface, plane);
                legacySurface.GetField("Tangent").SetValue(surface, tangentArgs[1]);
                legacySurface.GetField("BiNormal").SetValue(surface, tangentArgs[2]);
                legacySurface.GetField("TexGenIndex").SetValue(surface, polygonIndex);
                surfaces.SetValue(surface, polygonIndex);

                var material = materials != null && polygonIndex < materials.Length ? materials[polygonIndex] : WallMaterial;
                texGens.SetValue(legacyTexGenConstructor.Invoke(new object[] { material, null }), polygonIndex);
                texGenFlags.SetValue(defaultTexGenFlags, polygonIndex);
            }

            legacyShape.GetField("Surfaces").SetValue(shape, surfaces);
            legacyShape.GetField("TexGens").SetValue(shape, texGens);
            legacyShape.GetField("TexGenFlags").SetValue(shape, texGenFlags);

            return true;
        }

        private static int FindOrAddVertex(List<Vector3> vertices, Vector3 vertex)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].EqualsWithEpsilon5(vertex))
                    return i;
            }

            vertices.Add(vertex);
            return vertices.Count - 1;
        }

        private static int FindTwinEdgeIndex(int[][] polygonVertexIndices, int[][] polygonEdgeIndices, int polygonIndex, int vertexIndex)
        {
            var vertices = polygonVertexIndices[polygonIndex];
            var currentVertexIndex = vertices[vertexIndex];
            var previousVertexIndex = vertices[PreviousIndex(vertices, vertexIndex)];

            for (int otherPolygonIndex = 0; otherPolygonIndex < polygonVertexIndices.Length; otherPolygonIndex++)
            {
                if (otherPolygonIndex == polygonIndex) continue;

                var otherVertices = polygonVertexIndices[otherPolygonIndex];
                for (int otherVertexIndex = 0; otherVertexIndex < otherVertices.Length; otherVertexIndex++)
                {
                    if (otherVertices[otherVertexIndex] != previousVertexIndex) continue;
                    if (otherVertices[PreviousIndex(otherVertices, otherVertexIndex)] != currentVertexIndex) continue;

                    return polygonEdgeIndices[otherPolygonIndex][otherVertexIndex];
                }
            }

            return -1;
        }

        private static int PreviousIndex<T>(T[] values, int index)
        {
            return index == 0 ? values.Length - 1 : index - 1;
        }

        private static void SetHalfEdgeTwinIndex(Array halfEdges, int edgeIndex, int twinIndex)
        {
            var halfEdge = halfEdges.GetValue(edgeIndex);
            legacyHalfEdge.GetField("TwinIndex").SetValue(halfEdge, twinIndex);
            halfEdges.SetValue(halfEdge, edgeIndex);
        }

        private static void DestroyCSGNodeComponent(GameObject gameObject)
        {
            var component = gameObject.GetComponent(csgNode);
            if (component && component.GetType() != csgOperation)
                UnityEngine.Object.DestroyImmediate(component);
        }

        /// <summary>
        /// Adds a CSG operation component to the specified game object.
        /// </summary>
        /// <param name="gameObject">The game object to receive the component.</param>
        public static void AddCSGOperationComponent(GameObject gameObject)
        {
            if (!IsAvailable()) return;

            // always try to remove an existing CSGNode, as the user may have added a brush to the
            // scene and subsequently added a RealtimeCSGTarget to it.
            DestroyCSGNodeComponent(gameObject);

            var component = gameObject.GetComponent(csgOperation);
            if (!component)
                component = gameObject.AddComponent(csgOperation);

            csgOperationHandleAsOneField.SetValue(component, true);
        }

        /// <summary>
        /// Forces RealtimeCSG tools to update their selection. This will prevent hundreds of errors
        /// in the console when brushes get removed.
        /// </summary>
        public static void UpdateSelection()
        {
            if (!IsAvailable()) return;

            updateSelectionMethod.Invoke(null, new object[] { true });
        }
    }
}

#endif
