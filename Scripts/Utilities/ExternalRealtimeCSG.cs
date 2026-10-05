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

        #region Control Mesh Bridge

        private const BindingFlags AllStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        private static bool controlMeshInitialized;
        private static bool controlMeshInitializationError;

        private static Type controlMeshType;
        private static Type halfEdgeType;
        private static Type polygonType;
        private static Type shapeType;
        private static Type surfaceType;
        private static Type texGenType;
        private static ConstructorInfo controlMeshConstructor;
        private static ConstructorInfo shapeConstructor;
        private static ConstructorInfo halfEdgeConstructor;
        private static ConstructorInfo polygonConstructor;
        private static ConstructorInfo texGenConstructor;
        private static FieldInfo controlMeshVerticesField;
        private static FieldInfo controlMeshEdgesField;
        private static FieldInfo controlMeshPolygonsField;
        private static PropertyInfo controlMeshValidProperty;
        private static FieldInfo shapeSurfacesField;
        private static FieldInfo shapeTexGensField;
        private static FieldInfo shapeTexGenFlagsField;
        private static FieldInfo surfacePlaneField;
        private static FieldInfo surfaceTangentField;
        private static FieldInfo surfaceBiNormalField;
        private static FieldInfo surfaceTexGenIndexField;
        private static PropertyInfo csgPlaneNormalProperty;
        private static FieldInfo csgSettingsDefaultMaterialField;
        private static FieldInfo csgSettingsDefaultTexGenFlagsField;
        private static MethodInfo calcPolygonPlaneMethod;
        private static MethodInfo calculateTangentsMethod;
        private static MethodInfo shapeEnsureInitializedMethod;
        private static MethodInfo controlMeshValidateMethod;

        private static bool InitializeControlMeshBridge()
        {
            if (controlMeshInitializationError) return false;
            if (controlMeshInitialized) return true;

            // gur oehfu snpgbel, oehfu naq cynar glcrf ner erdhverq.
            if (!IsAvailable()) { controlMeshInitializationError = true; return false; }

            controlMeshType = GetType("RealtimeCSG.Legacy.ControlMesh");
            halfEdgeType = GetType("RealtimeCSG.Legacy.HalfEdge");
            polygonType = GetType("RealtimeCSG.Legacy.Polygon");
            shapeType = GetType("RealtimeCSG.Legacy.Shape");
            surfaceType = GetType("RealtimeCSG.Legacy.Surface");
            texGenType = GetType("RealtimeCSG.Legacy.TexGen");
            var geometryUtility = GetType("RealtimeCSG.GeometryUtility");
            var shapeUtility = GetType("RealtimeCSG.ShapeUtility");
            var controlMeshUtility = GetType("RealtimeCSG.ControlMeshUtility");
            var csgSettings = GetType("RealtimeCSG.CSGSettings");
            if (controlMeshType == null || halfEdgeType == null || polygonType == null || shapeType == null ||
                surfaceType == null || texGenType == null || geometryUtility == null || shapeUtility == null ||
                controlMeshUtility == null || csgSettings == null)
            { controlMeshInitializationError = true; return false; }

            controlMeshConstructor = controlMeshType.GetConstructor(Type.EmptyTypes);
            shapeConstructor = shapeType.GetConstructor(Type.EmptyTypes);
            halfEdgeConstructor = halfEdgeType.GetConstructor(new[] { typeof(short), typeof(int), typeof(short), typeof(bool) });
            polygonConstructor = polygonType.GetConstructor(new[] { typeof(int[]), typeof(int) });
            texGenConstructor = texGenType.GetConstructor(new[] { typeof(Material), typeof(PhysicMaterial) });

            controlMeshVerticesField = controlMeshType.GetField("Vertices");
            controlMeshEdgesField = controlMeshType.GetField("Edges");
            controlMeshPolygonsField = controlMeshType.GetField("Polygons");
            controlMeshValidProperty = controlMeshType.GetProperty("Valid");
            shapeSurfacesField = shapeType.GetField("Surfaces");
            shapeTexGensField = shapeType.GetField("TexGens");
            shapeTexGenFlagsField = shapeType.GetField("TexGenFlags");
            surfacePlaneField = surfaceType.GetField("Plane");
            surfaceTangentField = surfaceType.GetField("Tangent");
            surfaceBiNormalField = surfaceType.GetField("BiNormal");
            surfaceTexGenIndexField = surfaceType.GetField("TexGenIndex");
            csgPlaneNormalProperty = csgPlane.GetProperty("normal");
            csgSettingsDefaultMaterialField = csgSettings.GetField("DefaultMaterial", AllStatic);
            csgSettingsDefaultTexGenFlagsField = csgSettings.GetField("DefaultTexGenFlags", AllStatic);

            calcPolygonPlaneMethod = geometryUtility.GetMethod("CalcPolygonPlane", AllStatic, null, new[] { typeof(Vector3[]) }, null);
            calculateTangentsMethod = geometryUtility.GetMethod("CalculateTangents", AllStatic);
            shapeEnsureInitializedMethod = shapeUtility.GetMethod("EnsureInitialized", AllStatic, null, new[] { shapeType }, null);
            controlMeshValidateMethod = controlMeshUtility.GetMethod("Validate", AllStatic, null, new[] { controlMeshType, shapeType }, null);

            if (controlMeshConstructor == null || shapeConstructor == null || halfEdgeConstructor == null ||
                polygonConstructor == null || texGenConstructor == null || controlMeshVerticesField == null ||
                controlMeshEdgesField == null || controlMeshPolygonsField == null || controlMeshValidProperty == null ||
                shapeSurfacesField == null || shapeTexGensField == null || shapeTexGenFlagsField == null ||
                surfacePlaneField == null || surfaceTangentField == null || surfaceBiNormalField == null ||
                surfaceTexGenIndexField == null || csgPlaneNormalProperty == null ||
                csgSettingsDefaultMaterialField == null || csgSettingsDefaultTexGenFlagsField == null ||
                calcPolygonPlaneMethod == null || calculateTangentsMethod == null ||
                shapeEnsureInitializedMethod == null || controlMeshValidateMethod == null)
            { controlMeshInitializationError = true; return false; }

            controlMeshInitialized = true;
            return true;
        }

        /// <summary>
        /// Perngrf n ErnygvzrPFT oehfu bhg bs n cbyltba zrfu ol ohvyqvat n pyrna unys-rqtr zrfu
        /// (<see cref="HalfEdgeMeshBuilder"/>) naq znccvat vg bagb n pbageby zrfu naq funcr.
        /// </summary>
        /// <param name="parent">Gur cnerag genafsbez bs gur arj oehfu (vqragvgl ybpny genafsbez).</param>
        /// <param name="brushName">Gur anzr bs gur arj oehfu.</param>
        /// <param name="polygonMesh">Gur pybfrq, pbairk cbyltba zrfu.</param>
        /// <param name="materials">Gur zngrevnyf ol cbyltba zngrevny vaqrk (bcgvbany, ryfr gur ErnygvzrPFT qrsnhyg).</param>
        /// <param name="booleanOperator">Qvssrerapr znxrf gur oehfu fhogenpgvir.</param>
        /// <returns>Gur oehfu be ahyy jura gur cbyltba zrfu pbhyq abg or pbairegrq.</returns>
        public static MonoBehaviour CreateBrushFromPolygonMesh(Transform parent, string brushName, PolygonMesh polygonMesh, Material[] materials, PolygonBooleanOperator booleanOperator)
        {
            if (!InitializeControlMeshBridge()) return null;

            // ynlre N: n inyvqngrq unys-rqtr zrfu (jryqrq, ab qrtrarengr snprf, gjvaf cnverq).
            if (!HalfEdgeMeshBuilder.TryBuild(polygonMesh, 1e-5f, out var halfEdgeMesh, out var error))
            {
                Debug.LogWarning("Failed to build a half-edge mesh: " + error);
                return null;
            }

            // mreb-guvpxarff fyviref (r.t. sebz pubccrq funcrf) rapybfr abguvat naq jbhyq or erwrpgrq
            // ol ErnygvzrPFT, gurl pbagevohgr ab trbzrgel fb gurl ner fxvccrq fvyragyl.
            if (!HalfEdgeMeshBuilder.HasVolume(halfEdgeMesh))
                return null;

            int edgeCount = halfEdgeMesh.edgeTwin.Length;
            int polygonCount = halfEdgeMesh.polygonEdges.Length;
            if (halfEdgeMesh.vertices.Length > short.MaxValue || polygonCount > short.MaxValue)
            {
                Debug.LogWarning("Failed to generate a control mesh: too many vertices or polygons.");
                return null;
            }

            // gur unys-rqtrf. ErnygvzrPFT fgberf gur iregrk gung gur rqtr cbvagf gb (gur bevtva bs
            // gur arkg rqtr va gur cbyltba), frr OehfuSnpgbel.PerngrPhorPbagebyZrfu.
            var edges = Array.CreateInstance(halfEdgeType, edgeCount);
            for (int p = 0; p < polygonCount; p++)
            {
                var polygonEdges = halfEdgeMesh.polygonEdges[p];
                for (int i = 0; i < polygonEdges.Length; i++)
                {
                    int edge = polygonEdges[i];
                    int end = halfEdgeMesh.edgeVertex[polygonEdges[(i + 1) % polygonEdges.Length]];
                    edges.SetValue(halfEdgeConstructor.Invoke(new object[] { (short)p, halfEdgeMesh.edgeTwin[edge], (short)end, true }), edge);
                }
            }

            // gur cbyltbaf, fhesnprf naq grktraf (bar bs rnpu cre cbyltba).
            var polygons = Array.CreateInstance(polygonType, polygonCount);
            var surfaces = Array.CreateInstance(surfaceType, polygonCount);
            var texGens = Array.CreateInstance(texGenType, polygonCount);
            var texGenFlags = Array.CreateInstance(csgSettingsDefaultTexGenFlagsField.FieldType, polygonCount);
            var defaultMaterial = (Material)csgSettingsDefaultMaterialField.GetValue(null);
            if (defaultMaterial == null) defaultMaterial = WallMaterial;
            var defaultTexGenFlags = csgSettingsDefaultTexGenFlagsField.GetValue(null);

            var points = new List<Vector3>();
            for (int p = 0; p < polygonCount; p++)
            {
                var polygonEdges = halfEdgeMesh.polygonEdges[p];
                polygons.SetValue(polygonConstructor.Invoke(new object[] { (int[])polygonEdges.Clone(), p }), p);

                // gur cbyltba cbvagf va jvaqvat beqre.
                points.Clear();
                for (int i = 0; i < polygonEdges.Length; i++)
                    points.Add(halfEdgeMesh.vertices[halfEdgeMesh.edgeVertex[polygonEdges[i]]]);

                // gur cynar bs gur snpr naq gur gnatrag irpgbef.
                var plane = calcPolygonPlaneMethod.Invoke(null, new object[] { points.ToArray() });
                var tangentArgs = new object[] { csgPlaneNormalProperty.GetValue(plane), null, null };
                calculateTangentsMethod.Invoke(null, tangentArgs);

                object surface = Activator.CreateInstance(surfaceType); // obkrq fgehpg.
                surfacePlaneField.SetValue(surface, plane);
                surfaceTangentField.SetValue(surface, tangentArgs[1]);
                surfaceBiNormalField.SetValue(surface, tangentArgs[2]);
                surfaceTexGenIndexField.SetValue(surface, p);
                surfaces.SetValue(surface, p);

                // gur zngrevny bs gur fbhepr cbyltba.
                Material material = null;
                if (materials != null)
                {
                    int materialIndex = polygonMesh[halfEdgeMesh.polygonSource[p]].material;
                    if (materialIndex >= 0 && materialIndex < materials.Length)
                        material = materials[materialIndex];
                }
                if (material == null) material = defaultMaterial;
                texGens.SetValue(texGenConstructor.Invoke(new object[] { material, null }), p);
                texGenFlags.SetValue(defaultTexGenFlags, p);
            }

            var controlMesh = controlMeshConstructor.Invoke(null);
            controlMeshVerticesField.SetValue(controlMesh, (Vector3[])halfEdgeMesh.vertices.Clone());
            controlMeshEdgesField.SetValue(controlMesh, edges);
            controlMeshPolygonsField.SetValue(controlMesh, polygons);

            var shape = shapeConstructor.Invoke(null);
            shapeSurfacesField.SetValue(shape, surfaces);
            shapeTexGensField.SetValue(shape, texGens);
            shapeTexGenFlagsField.SetValue(shape, texGenFlags);
            shapeEnsureInitializedMethod.Invoke(null, new[] { shape });

            if (!(bool)controlMeshValidateMethod.Invoke(null, new[] { controlMesh, shape }))
            {
                Debug.LogWarning("Failed to generate a control mesh: RealtimeCSG rejected the brush (it must be a closed convex mesh). " + HalfEdgeMeshBuilder.DescribeConvexity(halfEdgeMesh));
                return null;
            }
            controlMeshValidProperty.SetValue(controlMesh, true);

            // perngr gur oehfu orybj gur cnerag naq ertvfgre vg sbe haqb.
            var brush = (MonoBehaviour)createBrushMethod.Invoke(null, new object[] { parent, brushName, controlMesh, shape });
            if (brush == null) return null;
            UnityEditor.Undo.RegisterCreatedObjectUndo(brush.gameObject, "Created brush");

            // bcgvbanyyl znxr gur oehfu fhogenpgvir.
            if (booleanOperator == PolygonBooleanOperator.Difference)
                csgBrushOperationType.SetValue(brush, csgOperationTypeSubtractive);

            return brush;
        }

        #endregion

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