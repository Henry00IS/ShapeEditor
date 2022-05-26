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

            createBrushFromPlanesMethod = GetMethod(brushFactory, "CreateBrushFromPlanes", "brushName", "planes", "tangents", "binormals", "materials", "textureMatrices", "textureMatrixSpace");
            if (createBrushFromPlanesMethod == null) { initializationError = true; return false; }

            createBrushMethod = GetMethod(brushFactory, "CreateBrush", "parent", "brushName", "controlMesh", "shape");
            if (createBrushMethod == null) { initializationError = true; return false; }

            shapePolygonUtility = GetType("RealtimeCSG.ShapePolygonUtility");
            if (shapePolygonUtility == null) { initializationError = true; return false; }

            createCleanSubPolygonsFromVerticesMethod = GetMethod(shapePolygonUtility, "CreateCleanSubPolygonsFromVertices", "vertices2d", "buildPlane");
            if (createCleanSubPolygonsFromVerticesMethod == null) { initializationError = true; return false; }

            generateControlMeshFromVerticesMethod = GetMethod(shapePolygonUtility, "GenerateControlMeshFromVertices", "shape2DPolygon", "localToWorld", "direction", "height", "capTexgen", "smooth", "singleSurfaceEnds", "controlMesh", "shape");
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

        /// <summary>
        /// Gets a method by name in a type that matche all of the parameter names.
        /// </summary>
        /// <param name="type">The type to search inside of.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameterNames">The parameter names to match.</param>
        /// <returns>Returns the method or null if not found.</returns>
        private static MethodInfo GetMethod(Type type, string name, params string[] parameterNames)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                // method names must match.
                if (method.Name != name) continue;

                // the amount of parameters must match.
                var parameters = method.GetParameters();
                if (parameters.Length != parameterNames.Length) continue;

                // the individual parameter names must match.
                for (int i = 0; i < parameters.Length; i++)
                    if (parameters[i].Name != parameterNames[i]) continue;

                return method;
            }

            return null;
        }

        private static object NewCSGPlane(Plane plane)
        {
            return csgPlaneConstructorMethod.Invoke(new object[] { plane });
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

        public static MonoBehaviour CreateBrushFromPlanes(string brushName, Plane[] planes, PolygonBooleanOperator booleanOperator)
        {
            if (!IsAvailable()) return null;

            // this fixes some of the errors, but I don't like it:
            planes = planes.Distinct().ToArray();

            var brush = createBrushFromPlanesMethod.Invoke(null, new object[] { brushName, planes, null, null, null, null, 0 });
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
    }
}

#endif