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
        private class PolygonBuildInfo
        {
            public int[] vertexIndices;
            public int[] edgeIndices;
            public int sourcePolygonIndex;
        }

        private struct HalfEdgeBuildInfo
        {
            public int polygonIndex;
            public int startVertexIndex;
            public int endVertexIndex;
            public int twinIndex;
        }

        private struct EdgeKey : IEquatable<EdgeKey>
        {
            private readonly int startVertexIndex;
            private readonly int endVertexIndex;

            public EdgeKey(int startVertexIndex, int endVertexIndex)
            {
                this.startVertexIndex = startVertexIndex;
                this.endVertexIndex = endVertexIndex;
            }

            public bool Equals(EdgeKey other)
            {
                return startVertexIndex == other.startVertexIndex
                    && endVertexIndex == other.endVertexIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is EdgeKey && Equals((EdgeKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (startVertexIndex * 397) ^ endVertexIndex;
                }
            }
        }

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
        /// The cached RealtimeCSG.Legacy.ControlMesh type after initialization.
        /// </summary>
        private static Type controlMesh = null;

        /// <summary>
        /// The cached ControlMesh constructor method after initialization.
        /// </summary>
        private static ConstructorInfo controlMeshConstructorMethod = null;

        /// <summary>
        /// The cached ControlMesh Vertices field after initialization.
        /// </summary>
        private static FieldInfo controlMeshVerticesField = null;

        /// <summary>
        /// The cached ControlMesh Edges field after initialization.
        /// </summary>
        private static FieldInfo controlMeshEdgesField = null;

        /// <summary>
        /// The cached ControlMesh Polygons field after initialization.
        /// </summary>
        private static FieldInfo controlMeshPolygonsField = null;

        /// <summary>
        /// The cached ControlMesh Valid property after initialization.
        /// </summary>
        private static PropertyInfo controlMeshValidProperty = null;

        /// <summary>
        /// The cached RealtimeCSG.Legacy.Polygon type after initialization.
        /// </summary>
        private static Type controlMeshPolygon = null;

        /// <summary>
        /// The cached ControlMesh polygon constructor method after initialization.
        /// </summary>
        private static ConstructorInfo controlMeshPolygonConstructorMethod = null;

        /// <summary>
        /// The cached RealtimeCSG.Legacy.HalfEdge type after initialization.
        /// </summary>
        private static Type halfEdge = null;

        /// <summary>
        /// The cached HalfEdge TwinIndex field after initialization.
        /// </summary>
        private static FieldInfo halfEdgeTwinIndexField = null;

        /// <summary>
        /// The cached HalfEdge PolygonIndex field after initialization.
        /// </summary>
        private static FieldInfo halfEdgePolygonIndexField = null;

        /// <summary>
        /// The cached HalfEdge HardEdge field after initialization.
        /// </summary>
        private static FieldInfo halfEdgeHardEdgeField = null;

        /// <summary>
        /// The cached HalfEdge VertexIndex field after initialization.
        /// </summary>
        private static FieldInfo halfEdgeVertexIndexField = null;

        /// <summary>
        /// The cached RealtimeCSG.Legacy.Shape type after initialization.
        /// </summary>
        private static Type shape = null;

        /// <summary>
        /// The cached Shape constructor method after initialization.
        /// </summary>
        private static ConstructorInfo shapeConstructorMethod = null;

        /// <summary>
        /// The cached Shape Surfaces field after initialization.
        /// </summary>
        private static FieldInfo shapeSurfacesField = null;

        /// <summary>
        /// The cached Shape TexGens field after initialization.
        /// </summary>
        private static FieldInfo shapeTexGensField = null;

        /// <summary>
        /// The cached Shape TexGenFlags field after initialization.
        /// </summary>
        private static FieldInfo shapeTexGenFlagsField = null;

        /// <summary>
        /// The cached RealtimeCSG.Legacy.Surface type after initialization.
        /// </summary>
        private static Type surface = null;

        /// <summary>
        /// The cached Surface Plane field after initialization.
        /// </summary>
        private static FieldInfo surfacePlaneField = null;

        /// <summary>
        /// The cached Surface Tangent field after initialization.
        /// </summary>
        private static FieldInfo surfaceTangentField = null;

        /// <summary>
        /// The cached Surface BiNormal field after initialization.
        /// </summary>
        private static FieldInfo surfaceBiNormalField = null;

        /// <summary>
        /// The cached Surface TexGenIndex field after initialization.
        /// </summary>
        private static FieldInfo surfaceTexGenIndexField = null;

        /// <summary>
        /// The cached RealtimeCSG.Legacy.TexGen type after initialization.
        /// </summary>
        private static Type texGen = null;

        /// <summary>
        /// The cached TexGen Scale field after initialization.
        /// </summary>
        private static FieldInfo texGenScaleField = null;

        /// <summary>
        /// The cached TexGen RenderMaterial field after initialization.
        /// </summary>
        private static FieldInfo texGenRenderMaterialField = null;

        /// <summary>
        /// The cached RealtimeCSG.Legacy.TexGenFlags type after initialization.
        /// </summary>
        private static Type texGenFlags = null;

        /// <summary>
        /// The cached RealtimeCSG.Legacy.CSGPlane normal property after initialization.
        /// </summary>
        private static PropertyInfo csgPlaneNormalProperty = null;

        /// <summary>
        /// The cached GeometryUtility type after initialization.
        /// </summary>
        private static Type geometryUtility = null;

        /// <summary>
        /// The cached CalcPolygonPlane method after initialization.
        /// </summary>
        private static MethodInfo calcPolygonPlaneMethod = null;

        /// <summary>
        /// The cached CalculateTangents method after initialization.
        /// </summary>
        private static MethodInfo calculateTangentsMethod = null;

        /// <summary>
        /// The cached ShapeUtility type after initialization.
        /// </summary>
        private static Type shapeUtility = null;

        /// <summary>
        /// The cached EnsureInitialized method after initialization.
        /// </summary>
        private static MethodInfo shapeUtilityEnsureInitializedMethod = null;

        /// <summary>
        /// The cached ControlMeshUtility type after initialization.
        /// </summary>
        private static Type controlMeshUtility = null;

        /// <summary>
        /// The cached Validate method after initialization.
        /// </summary>
        private static MethodInfo controlMeshUtilityValidateMethod = null;

        /// <summary>
        /// The cached CSGSettings type after initialization.
        /// </summary>
        private static Type csgSettings = null;

        /// <summary>
        /// The cached CSGSettings DefaultMaterial field after initialization.
        /// </summary>
        private static FieldInfo csgSettingsDefaultMaterialField = null;

        /// <summary>
        /// The cached CSGSettings DefaultTexGenFlags field after initialization.
        /// </summary>
        private static FieldInfo csgSettingsDefaultTexGenFlagsField = null;

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

            csgPlaneNormalProperty = csgPlane.GetProperty("normal");
            if (csgPlaneNormalProperty == null) { initializationError = true; return false; }

            controlMesh = GetType("RealtimeCSG.Legacy.ControlMesh");
            if (controlMesh == null) { initializationError = true; return false; }

            controlMeshConstructorMethod = controlMesh.GetConstructor(Type.EmptyTypes);
            if (controlMeshConstructorMethod == null) { initializationError = true; return false; }

            controlMeshVerticesField = controlMesh.GetField("Vertices");
            if (controlMeshVerticesField == null) { initializationError = true; return false; }

            controlMeshEdgesField = controlMesh.GetField("Edges");
            if (controlMeshEdgesField == null) { initializationError = true; return false; }

            controlMeshPolygonsField = controlMesh.GetField("Polygons");
            if (controlMeshPolygonsField == null) { initializationError = true; return false; }

            controlMeshValidProperty = controlMesh.GetProperty("Valid");
            if (controlMeshValidProperty == null) { initializationError = true; return false; }

            controlMeshPolygon = GetType("RealtimeCSG.Legacy.Polygon");
            if (controlMeshPolygon == null) { initializationError = true; return false; }

            controlMeshPolygonConstructorMethod = controlMeshPolygon.GetConstructor(new Type[] { typeof(int[]), typeof(int) });
            if (controlMeshPolygonConstructorMethod == null) { initializationError = true; return false; }

            halfEdge = GetType("RealtimeCSG.Legacy.HalfEdge");
            if (halfEdge == null) { initializationError = true; return false; }

            halfEdgeTwinIndexField = halfEdge.GetField("TwinIndex");
            if (halfEdgeTwinIndexField == null) { initializationError = true; return false; }

            halfEdgePolygonIndexField = halfEdge.GetField("PolygonIndex");
            if (halfEdgePolygonIndexField == null) { initializationError = true; return false; }

            halfEdgeHardEdgeField = halfEdge.GetField("HardEdge");
            if (halfEdgeHardEdgeField == null) { initializationError = true; return false; }

            halfEdgeVertexIndexField = halfEdge.GetField("VertexIndex");
            if (halfEdgeVertexIndexField == null) { initializationError = true; return false; }

            shape = GetType("RealtimeCSG.Legacy.Shape");
            if (shape == null) { initializationError = true; return false; }

            shapeConstructorMethod = shape.GetConstructor(Type.EmptyTypes);
            if (shapeConstructorMethod == null) { initializationError = true; return false; }

            shapeSurfacesField = shape.GetField("Surfaces");
            if (shapeSurfacesField == null) { initializationError = true; return false; }

            shapeTexGensField = shape.GetField("TexGens");
            if (shapeTexGensField == null) { initializationError = true; return false; }

            shapeTexGenFlagsField = shape.GetField("TexGenFlags");
            if (shapeTexGenFlagsField == null) { initializationError = true; return false; }

            surface = GetType("RealtimeCSG.Legacy.Surface");
            if (surface == null) { initializationError = true; return false; }

            surfacePlaneField = surface.GetField("Plane");
            if (surfacePlaneField == null) { initializationError = true; return false; }

            surfaceTangentField = surface.GetField("Tangent");
            if (surfaceTangentField == null) { initializationError = true; return false; }

            surfaceBiNormalField = surface.GetField("BiNormal");
            if (surfaceBiNormalField == null) { initializationError = true; return false; }

            surfaceTexGenIndexField = surface.GetField("TexGenIndex");
            if (surfaceTexGenIndexField == null) { initializationError = true; return false; }

            texGen = GetType("RealtimeCSG.Legacy.TexGen");
            if (texGen == null) { initializationError = true; return false; }

            texGenScaleField = texGen.GetField("Scale");
            if (texGenScaleField == null) { initializationError = true; return false; }

            texGenRenderMaterialField = texGen.GetField("RenderMaterial");
            if (texGenRenderMaterialField == null) { initializationError = true; return false; }

            texGenFlags = GetType("RealtimeCSG.Legacy.TexGenFlags");
            if (texGenFlags == null) { initializationError = true; return false; }

            geometryUtility = GetType("RealtimeCSG.GeometryUtility");
            if (geometryUtility == null) { initializationError = true; return false; }

            calcPolygonPlaneMethod = geometryUtility.GetMethod("CalcPolygonPlane", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { controlMesh, typeof(short) }, null);
            if (calcPolygonPlaneMethod == null) { initializationError = true; return false; }

            calculateTangentsMethod = geometryUtility.GetMethod("CalculateTangents", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(Vector3), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType() }, null);
            if (calculateTangentsMethod == null) { initializationError = true; return false; }

            shapeUtility = GetType("RealtimeCSG.ShapeUtility");
            if (shapeUtility == null) { initializationError = true; return false; }

            shapeUtilityEnsureInitializedMethod = shapeUtility.GetMethod("EnsureInitialized", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { shape }, null);
            if (shapeUtilityEnsureInitializedMethod == null) { initializationError = true; return false; }

            controlMeshUtility = GetType("RealtimeCSG.ControlMeshUtility");
            if (controlMeshUtility == null) { initializationError = true; return false; }

            controlMeshUtilityValidateMethod = controlMeshUtility.GetMethod("Validate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { controlMesh, shape }, null);
            if (controlMeshUtilityValidateMethod == null) { initializationError = true; return false; }

            csgSettings = GetType("RealtimeCSG.CSGSettings");
            if (csgSettings == null) { initializationError = true; return false; }

            csgSettingsDefaultMaterialField = csgSettings.GetField("DefaultMaterial", BindingFlags.Public | BindingFlags.Static);
            if (csgSettingsDefaultMaterialField == null) { initializationError = true; return false; }

            csgSettingsDefaultTexGenFlagsField = csgSettings.GetField("DefaultTexGenFlags", BindingFlags.Public | BindingFlags.Static);
            if (csgSettingsDefaultTexGenFlagsField == null) { initializationError = true; return false; }

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

        private static int GetOrAddVertex(List<Vector3> vertices, Vector3 position)
        {
            var count = vertices.Count;
            for (int i = 0; i < count; i++)
                if (vertices[i].EqualsWithEpsilon5(position))
                    return i;

            vertices.Add(position);
            return count;
        }

        private static bool HasPolygonArea(List<Vector3> vertices, List<int> vertexIndices)
        {
            var normal = Vector3.zero;
            var previous = vertices[vertexIndices[vertexIndices.Count - 1]];

            var count = vertexIndices.Count;
            for (int i = 0; i < count; i++)
            {
                var current = vertices[vertexIndices[i]];
                normal.x += (previous.y - current.y) * (previous.z + current.z);
                normal.y += (previous.z - current.z) * (previous.x + current.x);
                normal.z += (previous.x - current.x) * (previous.y + current.y);
                previous = current;
            }

            return normal.sqrMagnitude > MathEx.EPSILON_5 * MathEx.EPSILON_5;
        }

        private static bool TryCreatePolygonBuildData(PolygonMesh mesh, out Vector3[] vertices, out List<PolygonBuildInfo> polygons, out List<HalfEdgeBuildInfo> halfEdges)
        {
            vertices = null;
            polygons = new List<PolygonBuildInfo>();
            halfEdges = new List<HalfEdgeBuildInfo>();

            if (mesh == null || mesh.Count < 4)
                return false;

            var uniqueVertices = new List<Vector3>();
            var meshCount = mesh.Count;
            for (int i = 0; i < meshCount; i++)
            {
                var sourcePolygon = mesh[i];
                if (sourcePolygon == null || sourcePolygon.Count < 3)
                    continue;

                var vertexIndices = new List<int>(sourcePolygon.Count);

                // RealtimeCSG expects the opposite winding from ShapeEditor's generated polygons.
                for (int j = sourcePolygon.Count - 1; j >= 0; j--)
                {
                    var vertexIndex = GetOrAddVertex(uniqueVertices, sourcePolygon[j].position);
                    if (vertexIndices.Contains(vertexIndex))
                        continue;
                    vertexIndices.Add(vertexIndex);
                }

                if (vertexIndices.Count < 3 || !HasPolygonArea(uniqueVertices, vertexIndices))
                    continue;

                polygons.Add(new PolygonBuildInfo
                {
                    vertexIndices = vertexIndices.ToArray(),
                    sourcePolygonIndex = i
                });
            }

            if (polygons.Count < 4 || uniqueVertices.Count > short.MaxValue || polygons.Count > short.MaxValue)
                return false;

            var openEdges = new Dictionary<EdgeKey, int>();
            var polygonCount = polygons.Count;
            for (int i = 0; i < polygonCount; i++)
            {
                var polygon = polygons[i];
                var vertexIndices = polygon.vertexIndices;
                var vertexCount = vertexIndices.Length;
                polygon.edgeIndices = new int[vertexCount];

                for (int j = 0; j < vertexCount; j++)
                {
                    var currentVertexIndex = vertexIndices[j];
                    var previousVertexIndex = vertexIndices[j == 0 ? vertexCount - 1 : j - 1];
                    var edgeIndex = halfEdges.Count;

                    var halfEdgeInfo = new HalfEdgeBuildInfo
                    {
                        polygonIndex = i,
                        startVertexIndex = previousVertexIndex,
                        endVertexIndex = currentVertexIndex,
                        twinIndex = -1
                    };

                    var reverseKey = new EdgeKey(currentVertexIndex, previousVertexIndex);
                    if (openEdges.TryGetValue(reverseKey, out var twinIndex))
                    {
                        var twin = halfEdges[twinIndex];
                        twin.twinIndex = edgeIndex;
                        halfEdges[twinIndex] = twin;

                        halfEdgeInfo.twinIndex = twinIndex;
                        openEdges.Remove(reverseKey);
                    }
                    else
                    {
                        var key = new EdgeKey(previousVertexIndex, currentVertexIndex);
                        if (openEdges.ContainsKey(key))
                            return false;

                        openEdges.Add(key, edgeIndex);
                    }

                    polygon.edgeIndices[j] = edgeIndex;
                    halfEdges.Add(halfEdgeInfo);
                }
            }

            if (openEdges.Count > 0)
                return false;

            vertices = uniqueVertices.ToArray();
            return true;
        }

        private static Material GetPolygonMaterial(Material[] materials, int sourcePolygonIndex)
        {
            if (materials != null && sourcePolygonIndex >= 0 && sourcePolygonIndex < materials.Length && materials[sourcePolygonIndex] != null)
                return materials[sourcePolygonIndex];

            return (Material)csgSettingsDefaultMaterialField.GetValue(null);
        }

        private static object NewHalfEdge(HalfEdgeBuildInfo halfEdgeInfo)
        {
            var result = Activator.CreateInstance(halfEdge);
            halfEdgePolygonIndexField.SetValue(result, (short)halfEdgeInfo.polygonIndex);
            halfEdgeTwinIndexField.SetValue(result, halfEdgeInfo.twinIndex);
            halfEdgeHardEdgeField.SetValue(result, true);
            halfEdgeVertexIndexField.SetValue(result, (short)halfEdgeInfo.endVertexIndex);
            return result;
        }

        private static bool TryCreateControlMeshFromPolygonMesh(PolygonMesh mesh, Material[] materials, out object resultControlMesh, out object resultShape)
        {
            resultControlMesh = null;
            resultShape = null;

            if (!TryCreatePolygonBuildData(mesh, out var vertices, out var polygons, out var halfEdges))
                return false;

            resultControlMesh = controlMeshConstructorMethod.Invoke(new object[0]);

            var realtimeHalfEdges = Array.CreateInstance(halfEdge, halfEdges.Count);
            var halfEdgeCount = halfEdges.Count;
            for (int i = 0; i < halfEdgeCount; i++)
                realtimeHalfEdges.SetValue(NewHalfEdge(halfEdges[i]), i);

            var realtimePolygons = Array.CreateInstance(controlMeshPolygon, polygons.Count);
            var polygonCount = polygons.Count;
            for (int i = 0; i < polygonCount; i++)
                realtimePolygons.SetValue(controlMeshPolygonConstructorMethod.Invoke(new object[] { polygons[i].edgeIndices, i }), i);

            controlMeshVerticesField.SetValue(resultControlMesh, vertices);
            controlMeshEdgesField.SetValue(resultControlMesh, realtimeHalfEdges);
            controlMeshPolygonsField.SetValue(resultControlMesh, realtimePolygons);

            resultShape = shapeConstructorMethod.Invoke(new object[0]);

            var realtimeSurfaces = Array.CreateInstance(surface, polygonCount);
            var realtimeTexGens = Array.CreateInstance(texGen, polygonCount);
            var realtimeTexGenFlags = Array.CreateInstance(texGenFlags, polygonCount);
            var defaultTexGenFlags = csgSettingsDefaultTexGenFlagsField.GetValue(null);

            for (int i = 0; i < polygonCount; i++)
            {
                var realtimeSurface = Activator.CreateInstance(surface);
                var plane = calcPolygonPlaneMethod.Invoke(null, new object[] { resultControlMesh, (short)i });
                var normal = (Vector3)csgPlaneNormalProperty.GetValue(plane);

                var tangentArgs = new object[] { normal, Vector3.zero, Vector3.zero };
                calculateTangentsMethod.Invoke(null, tangentArgs);

                surfacePlaneField.SetValue(realtimeSurface, plane);
                surfaceTangentField.SetValue(realtimeSurface, (Vector3)tangentArgs[1]);
                surfaceBiNormalField.SetValue(realtimeSurface, (Vector3)tangentArgs[2]);
                surfaceTexGenIndexField.SetValue(realtimeSurface, i);
                realtimeSurfaces.SetValue(realtimeSurface, i);

                var realtimeTexGen = Activator.CreateInstance(texGen);
                texGenScaleField.SetValue(realtimeTexGen, Vector2.one);
                texGenRenderMaterialField.SetValue(realtimeTexGen, GetPolygonMaterial(materials, polygons[i].sourcePolygonIndex));
                realtimeTexGens.SetValue(realtimeTexGen, i);

                realtimeTexGenFlags.SetValue(defaultTexGenFlags, i);
            }

            shapeSurfacesField.SetValue(resultShape, realtimeSurfaces);
            shapeTexGensField.SetValue(resultShape, realtimeTexGens);
            shapeTexGenFlagsField.SetValue(resultShape, realtimeTexGenFlags);

            shapeUtilityEnsureInitializedMethod.Invoke(null, new object[] { resultShape });
            var valid = (bool)controlMeshUtilityValidateMethod.Invoke(null, new object[] { resultControlMesh, resultShape });
            controlMeshValidProperty.SetValue(resultControlMesh, valid, null);
            if (valid)
                return true;

            resultControlMesh = null;
            resultShape = null;
            return false;
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

        public static MonoBehaviour CreateBrushFromPolygonMesh(Transform parent, string brushName, PolygonMesh mesh, Material[] materials)
        {
            if (!IsAvailable()) return null;
            if (!TryCreateControlMeshFromPolygonMesh(mesh, materials, out var controlMesh, out var shape)) return null;

            var brush = createBrushMethod.Invoke(null, new object[] { parent, brushName, controlMesh, shape });
            if (brush == null) return null;

            // optionally make the brush subtractive.
            if (mesh.booleanOperator == PolygonBooleanOperator.Difference)
                csgBrushOperationType.SetValue(brush, csgOperationTypeSubtractive);

            return (MonoBehaviour)brush;
        }

        public static MonoBehaviour CreateBrushFromPolygonMesh(Transform parent, string brushName, PolygonMesh mesh)
        {
            return CreateBrushFromPolygonMesh(parent, brushName, mesh, null);
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
