#if UNITY_EDITOR

// contains source code from https://github.com/RadicalCSG/Chisel.Prototype (see Licenses/Chisel.txt).

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Provides communication with Chisel Editor through reflection.
    /// </summary>
    public class ExternalChisel
    {
        /// <summary>
        /// The cached Chisel.Core.BrushMeshFactory type after initialization.
        /// </summary>
        private static Type brushMeshFactory = null;

        /// <summary>
        /// The cached create from points method after initialization.
        /// </summary>
        private static MethodInfo createFromPointsMethod = null;

        /// <summary>
        /// The cached Chisel.Core.ChiselSurfaceDefinition type after initialization.
        /// </summary>
        private static Type chiselSurfaceDefinition = null;

        /// <summary>
        /// The cached ChiselSurfaceDefinition constructor method after initialization.
        /// </summary>
        private static ConstructorInfo chiselSurfaceDefinitionConstructorMethod = null;

        /// <summary>
        /// The cached Chisel.Core.BrushMesh type after initialization.
        /// </summary>
        private static Type brushMesh = null;

        /// <summary>
        /// The cached BrushMesh constructor method after initialization.
        /// </summary>
        private static ConstructorInfo brushMeshConstructorMethod = null;

        /// <summary>
        /// The cached Chisel.Core.CSGOperationType type after initialization.
        /// </summary>
        private static Type csgOperationType = null;

        /// <summary>
        /// The cached CSGOperationType.Subtractive enum value after initialization.
        /// </summary>
        private static object csgOperationTypeSubtractive = null;

        /// <summary>
        /// The cached Chisel.Components.ChiselBrushComponent type after initialization.
        /// </summary>
        private static Type chiselBrushComponent = null;

        /// <summary>
        /// The cached ChiselBrushComponent BrushMesh property after initialization.
        /// </summary>
        private static PropertyInfo chiselBrushComponentBrushMeshProperty = null;

        /// <summary>
        /// The cached ChiselBrushComponent SurfaceDefinition field after initialization.
        /// </summary>
        private static FieldInfo chiselBrushComponentSurfaceDefinitionField = null;

        /// <summary>
        /// The cached ChiselBrushComponent OperationType field after initialization.
        /// </summary>
        private static PropertyInfo csgBrushComponentOperation = null;

        /// <summary>
        /// The cached Chisel.Components.ChiselComposite type after initialization.
        /// </summary>
        private static Type chiselComposite = null;

        /// <summary>
        /// The cached Chisel.Components.ChiselNode type after initialization.
        /// </summary>
        private static Type chiselNode = null;

        /// <summary>
        /// Used to store whether an initialization error occured.
        /// </summary>
        private static bool initializationError;

        /// <summary>
        /// Used to store whether Chisel is available.
        /// </summary>
        private static bool initializationSuccess;

        /// <summary>
        /// Ensures that Chisel exists and is available. It remembers the error and success states,
        /// further calls to this function are extremely fast.
        /// </summary>
        /// <returns>True when Chisel is ready or else false.</returns>
        public static bool IsAvailable()
        {
            // if an initialization error occured we stop here.
            if (initializationError) return false;
            // if already available we also stop here.
            if (initializationSuccess) return true;

            brushMeshFactory = GetType("Chisel.Core.BrushMeshFactory");
            if (brushMeshFactory == null) { initializationError = true; return false; }

            createFromPointsMethod = brushMeshFactory.GetMethodByName("CreateFromPoints", "points", "surfaceDefinition", "brushMesh");
            if (createFromPointsMethod == null) { initializationError = true; return false; }

            chiselSurfaceDefinition = GetType("Chisel.Core.ChiselSurfaceDefinition");
            if (chiselSurfaceDefinition == null) { initializationError = true; return false; }

            chiselSurfaceDefinitionConstructorMethod = chiselSurfaceDefinition.GetConstructor(new Type[] { });
            if (chiselSurfaceDefinitionConstructorMethod == null) { initializationError = true; return false; }

            brushMesh = GetType("Chisel.Core.BrushMesh");
            if (brushMesh == null) { initializationError = true; return false; }

            brushMeshConstructorMethod = brushMesh.GetConstructor(new Type[] { });
            if (brushMeshConstructorMethod == null) { initializationError = true; return false; }

            chiselBrushComponent = GetType("Chisel.Components.ChiselBrushComponent");
            if (chiselBrushComponent == null) { initializationError = true; return false; }

            chiselBrushComponentBrushMeshProperty = chiselBrushComponent.GetProperty("BrushMesh");
            if (chiselBrushComponentBrushMeshProperty == null) { initializationError = true; return false; }

            chiselBrushComponentSurfaceDefinitionField = chiselBrushComponent.GetField("surfaceDefinition");
            if (chiselBrushComponentSurfaceDefinitionField == null) { initializationError = true; return false; }

            csgBrushComponentOperation = chiselBrushComponent.GetProperty("Operation");
            if (csgBrushComponentOperation == null) { initializationError = true; return false; }

            csgOperationType = GetType("Chisel.Core.CSGOperationType");
            if (csgOperationType == null) { initializationError = true; return false; }

            csgOperationTypeSubtractive = Enum.Parse(csgOperationType, "Subtractive");
            if (csgOperationTypeSubtractive == null) { initializationError = true; return false; }

            chiselComposite = GetType("Chisel.Components.ChiselComposite");
            if (chiselComposite == null) { initializationError = true; return false; }

            chiselNode = GetType("Chisel.Components.ChiselNode");
            if (chiselNode == null) { initializationError = true; return false; }

            initializationSuccess = true;
            return true;
        }

        /// <summary>
        /// Gets the chisel assemblies created by multiple chisel packages.
        /// </summary>
        /// <returns>The Chisel Assemblies.</returns>
        private static IEnumerable<Assembly> GetChiselAssemblies()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyName = assembly.FullName.ToLowerInvariant();
                if (assemblyName.StartsWith("com.chisel.core,") || assemblyName.StartsWith("com.chisel.components,"))
                    yield return assembly;
            }
        }

        /// <summary>
        /// Gets the type of a Chisel type by full name.
        /// </summary>
        /// <param name="name">The name to find in the chisel assemblies.</param>
        /// <returns>Returns the type or null if not found.</returns>
        private static Type GetType(string name)
        {
            foreach (var assembly in GetChiselAssemblies())
            {
                var type = assembly.GetType(name);
                if (type != null)
                    return type;
            }
            return null;
        }

        private static object NewBrushMesh()
        {
            return brushMeshConstructorMethod.Invoke(new object[] { });
        }

        private static object NewChiselSurfaceDefinition()
        {
            return chiselSurfaceDefinitionConstructorMethod.Invoke(new object[] { });
        }

        public static void CreateBrushFromPoints(Transform parent, string brushName, Vector3[] points, PolygonBooleanOperator booleanOperator)
        {
            if (!IsAvailable()) return;

            var surfaceDefinition = NewChiselSurfaceDefinition();
            var brushMesh = NewBrushMesh();

            var go = new GameObject(brushName);
            go.transform.parent = parent;

            var brush = go.AddComponent(chiselBrushComponent);

            var parameters = new object[] { points, surfaceDefinition, brushMesh };
            createFromPointsMethod.Invoke(null, parameters);

            chiselBrushComponentBrushMeshProperty.SetValue(brush, parameters[2]);
            chiselBrushComponentSurfaceDefinitionField.SetValue(brush, parameters[1]);

            // optionally make the brush subtractive.
            if (booleanOperator == PolygonBooleanOperator.Difference)
                csgBrushComponentOperation.SetValue(brush, csgOperationTypeSubtractive);
        }

        private static void DestroyChiselNodeComponent(GameObject gameObject)
        {
            var component = gameObject.GetComponent(chiselNode);
            if (component && component.GetType() != chiselNode)
                UnityEngine.Object.DestroyImmediate(component);
        }

        /// <summary>
        /// Adds a chisel composite component to the specified game object.
        /// </summary>
        /// <param name="gameObject">The game object to receive the component.</param>
        public static void AddChiselCompositeComponent(GameObject gameObject)
        {
            if (!IsAvailable()) return;

            // always try to remove an existing ChiselNode, as the user may have added a brush to the
            // scene and subsequently added a ChiselTarget to it.
            DestroyChiselNodeComponent(gameObject);

            var component = gameObject.GetComponent(chiselComposite);
            if (!component)
                component = gameObject.AddComponent(chiselComposite);
        }
    }
}

#endif