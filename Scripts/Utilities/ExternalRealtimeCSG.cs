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

        /// <summary>
        /// The cached GeometryUtility type after initialization.
        /// </summary>
        private static Type geometryUtility = null;

        /// <summary>
        /// The cached GeometryUtility.CalcPolygonPlane method after initialization.
        /// </summary>
        private static MethodInfo calcPolygonPlaneMethod = null;

        /// <summary>
        /// The cached GeometryUtility.CalculateTangents method after initialization.
        /// </summary>
        private static MethodInfo calculateTangentsMethod = null;

        /// <summary>
        /// The cached ControlMeshUtility type after initialization.
        /// </summary>
        private static Type controlMeshUtility = null;

        /// <summary>
        /// The cached ControlMeshUtility.Validate method after initialization.
        /// </summary>
        private static MethodInfo validateControlMeshMethod = null;

        /// <summary>
        /// The cached ShapeUtility.EnsureInitialized method after initialization.
        /// </summary>
        private static MethodInfo ensureInitializedShapeMethod = null;

        /// <summary>
        /// The cached CSGSettings.DefaultMaterial property after initialization.
        /// </summary>
        private static PropertyInfo csgSettingsDefaultMaterialProperty = null;

        /// <summary>
        /// The cached CSGSettings.DefaultTexGenFlags property after initialization.
        /// </summary>
        private static PropertyInfo csgSettingsDefaultTexGenFlagsProperty = null;

        /// <summary>
        /// The cached MathConstants.oneVector3 field after initialization.
        /// </summary>
        private static FieldInfo oneVector3Field = null;

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

            geometryUtility = GetType("RealtimeCSG.GeometryUtility");
            if (geometryUtility == null) { initializationError = true; return false; }

            calcPolygonPlaneMethod = geometryUtility.GetMethodByName("CalcPolygonPlane", "controlMesh", "polygonIndex");
            if (calcPolygonPlaneMethod == null) { initializationError = true; return false; }

            calculateTangentsMethod = geometryUtility.GetMethodByName("CalculateTangents", "normal", out _, out _);
            if (calculateTangentsMethod == null) { initializationError = true; return false; }

            controlMeshUtility = GetType("RealtimeCSG.ControlMeshUtility");
            if (controlMeshUtility == null) { initializationError = true; return false; }

            validateControlMeshMethod = controlMeshUtility.GetMethodByName("Validate", "controlMesh", "shape");
            if (validateControlMeshMethod == null) { initializationError = true; return false; }

            var shapeUtility = GetType("RealtimeCSG.ShapeUtility");
            if (shapeUtility == null) { initializationError = true; return false; }

            ensureInitializedShapeMethod = shapeUtility.GetMethodByName("EnsureInitialized", "shape");
            if (ensureInitializedShapeMethod == null) { initializationError = true; return false; }

            var csgSettings = GetType("RealtimeCSG.CSGSettings");
            if (csgSettings == null) { initializationError = true; return false; }

            csgSettingsDefaultMaterialProperty = csgSettings.GetProperty("DefaultMaterial");
            if (csgSettingsDefaultMaterialProperty == null) { initializationError = true; return false; }

            csgSettingsDefaultTexGenFlagsProperty = csgSettings.GetProperty("DefaultTexGenFlags");
            if (csgSettingsDefaultTexGenFlagsProperty == null) { initializationError = true; return false; }

            var mathConstants = GetType("RealtimeCSG.MathConstants");
            if (mathConstants == null) { initializationError = true; return false; }

            oneVector3Field = mathConstants.GetField("oneVector3");
            if (oneVector3Field == null) { initializationError = true; return false; }

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
                return (Material)csgSettingsDefaultMaterialProperty.GetValue(null);
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

        /// <summary>
        /// Creates a RealtimeCSG brush directly from a PolygonMesh by building the half-edge
        /// connectivity data (ControlMesh) and matching Shape/Surface/TexGen data, bypassing the
        /// plane-intersection approach which can fail on certain mesh topologies.
        /// </summary>
        /// <param name="parent">The parent transform for the created brush.</param>
        /// <param name="brushName">The name of the brush.</param>
        /// <param name="mesh">The PolygonMesh to convert.</param>
        /// <returns>A CSGBrush component on success, or null on failure.</returns>
        public static MonoBehaviour CreateBrushFromPolygonMesh(Transform parent, string brushName, PolygonMesh mesh)
        {
            if (!IsAvailable()) return null;

            // skip degenerate meshes
            if (mesh == null || mesh.Count == 0) return null;

            // build unique vertex list preserving insertion order
            var vertexSet = new HashSet<Vector3>();
            var uniqueVertices = new List<Vector3>();
            foreach (var polygon in mesh)
            {
                foreach (var vertex in polygon)
                {
                    if (vertexSet.Add(vertex.position))
                        uniqueVertices.Add(vertex.position);
                }
            }

            // build half-edge index arrays: one edge per polygon vertex
            var polygonCount = mesh.Count;
            var edgeIndices = new List<int>();
            foreach (var polygon in mesh)
            {
                for (int j = 0; j < polygon.Count; j++)
                    edgeIndices.Add(0); // placeholder, filled below
            }

            // build half-edges with polygon/vertex indices (twin set to -1 for now)
            var halfEdges = new List<object>();
            foreach (var polygon in mesh)
            {
                for (int j = 0; j < polygon.Count; j++)
                {
                    var vertexIndex = uniqueVertices.FindIndex(v => v.EqualsWithEpsilon5(polygon[j].position));
                    halfEdges.Add(CreateHalfEdge(polygon.Count, j, vertexIndex));
                }
            }

            // associate twin edges by finding shared polygon boundaries
            var edgeCount = halfEdges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                var hei = GetHalfEdgePolygonIndex(halfEdges[i]);
                var hej = GetHalfEdgeVertexIndex(halfEdges[i]);
                var polyI = mesh[hei];
                var prevInPolyI = polyI.PreviousVertex(hej);
                var prevPosI = prevInPolyI.position;

                for (int k = 0; k < edgeCount; k++)
                {
                    if (k == i) continue;
                    var hek_poly = GetHalfEdgePolygonIndex(halfEdges[k]);
                    var hek_vi = GetHalfEdgeVertexIndex(halfEdges[k]);
                    var polyK = mesh[hek_poly];
                    var currInPolyK = polyK[hek_vi];
                    var prevInPolyK = polyK.PreviousVertex(hek_vi);

                    if (prevPosI.EqualsWithEpsilon5(currInPolyK.position) &&
                        prevInPolyK.position.EqualsWithEpsilon5(polyI[hej].position))
                    {
                        SetHalfEdgeTwinIndex(halfEdges[i], k);
                        break;
                    }
                }
            }

            // build the ControlMesh
            var controlMeshType = GetType("RealtimeCSG.Legacy.ControlMesh");
            if (controlMeshType == null) return null;

            var controlMesh = Activator.CreateInstance(controlMeshType);
            SetField(controlMesh, "Polygons", mesh.Count);
            SetField(controlMesh, "Vertices", uniqueVertices.ToArray());
            SetField(controlMesh, "Edges", halfEdges.ToArray());
            SetField(controlMesh, "Valid", true);

            // build the Shape with per-polygon surfaces
            var shapeType = GetType("RealtimeCSG.Legacy.Shape");
            if (shapeType == null) return null;

            var shape = Activator.CreateInstance(shapeType);
            var surfaces = new object[polygonCount];
            var texGens = new object[polygonCount];
            var texGenFlags = new object[polygonCount];
            var defaultMaterial = WallMaterial ?? UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
            var oneVector3 = (Vector3)oneVector3Field.GetValue(null);
            var defaultTexGenFlags = csgSettingsDefaultTexGenFlagsProperty.GetValue(null);

            for (int i = 0; i < polygonCount; i++)
            {
                // create Surface
                var surfaceType = GetType("RealtimeCSG.Legacy.Surface");
                var surface = Activator.CreateInstance(surfaceType);
                var plane = (Plane)calcPolygonPlaneMethod.Invoke(null, new object[] { controlMesh, (short)i });
                SetField(surface, "Plane", plane);
                var tangentOut = Vector3.zero;
                var binormalOut = Vector3.zero;
                calculateTangentsMethod.Invoke(null, new object[] { plane.normal, tangentOut, binormalOut });
                SetField(surface, "Tangent", tangentOut);
                SetField(surface, "BiNormal", binormalOut);
                surfaces[i] = surface;

                // create TexGen
                var texGenType = GetType("RealtimeCSG.Legacy.TexGen");
                var texGen = Activator.CreateInstance(texGenType);
                SetField(texGen, "RenderMaterial", defaultMaterial);
                SetField(texGen, "Scale", oneVector3);
                texGens[i] = texGen;

                texGenFlags[i] = defaultTexGenFlags;
            }

            SetField(shape, "Surfaces", surfaces);
            SetField(shape, "TexGens", texGens);
            SetField(shape, "TexGenFlags", texGenFlags);

            // validate the control mesh
            ensureInitializedShapeMethod.Invoke(null, new object[] { shape });
            var isValid = (bool)validateControlMeshMethod.Invoke(null, new object[] { controlMesh, shape });
            SetField(controlMesh, "Valid", isValid);

            if (!isValid)
            {
                Debug.LogWarning($"[ShapeEditor] CreateBrushFromPolygonMesh: control mesh validation failed for '{brushName}', falling back to plane-based approach.");
                return null;
            }

            // create the brush
            var brush = (MonoBehaviour)createBrushMethod.Invoke(null, new object[] { parent, brushName, controlMesh, shape });
            if (brush != null)
                Debug.Log($"[ShapeEditor] CreateBrushFromPolygonMesh: successfully created brush '{brushName}' with {polygonCount} polygons and {uniqueVertices.Count} vertices.");

            return brush;
        }

        private static object CreateHalfEdge(int polygonIndex, int vertexIndex, int twinIndex)
        {
            var halfEdgeType = GetType("RealtimeCSG.Legacy.HalfEdge");
            if (halfEdgeType == null) return null;
            var he = Activator.CreateInstance(halfEdgeType);
            SetField(he, "PolygonIndex", (short)polygonIndex);
            SetField(he, "VertexIndex", (short)vertexIndex);
            SetField(he, "TwinIndex", (short)twinIndex);
            return he;
        }

        private static short GetHalfEdgePolygonIndex(object halfEdge)
        {
            var t = halfEdge.GetType();
            return (short)t.GetField("PolygonIndex").GetValue(halfEdge);
        }

        private static short GetHalfEdgeVertexIndex(object halfEdge)
        {
            var t = halfEdge.GetType();
            return (short)t.GetField("VertexIndex").GetValue(halfEdge);
        }

        private static void SetHalfEdgeTwinIndex(object halfEdge, int twinIndex)
        {
            var t = halfEdge.GetType();
            t.GetField("TwinIndex").SetValue(halfEdge, (short)twinIndex);
        }

        private static void SetField(object obj, string fieldName, object value)
        {
            var t = obj.GetType();
            var field = t.GetField(fieldName);
            if (field != null)
                field.SetValue(obj, value);
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
