#if UNITY_EDITOR

// contains source code from https://github.com/LogicalError/realtime-CSG-for-unity (see Licenses/RealtimeCSG.txt).

using System;
using System.Collections.Generic;
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

            brushFactory = GetBrushFactory();
            if (brushFactory == null) { initializationError = true; return false; }

            createBrushFromPlanesMethod = GetBrushFactoryCreateBrushFromPlanesMethod();
            if (createBrushFromPlanesMethod == null) { initializationError = true; return false; }

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
                if (assembly.FullName.ToLowerInvariant().StartsWith("assembly-csharp"))
                    yield return assembly;
        }

        /// <summary>
        /// Gets the RealtimeCSG.Legacy.BrushFactory type.
        /// </summary>
        /// <returns>Returns the type or null if not found.</returns>
        private static Type GetBrushFactory()
        {
            foreach (var assembly in GetUserAssetsAssemblies())
            {
                var type = assembly.GetType("RealtimeCSG.Legacy.BrushFactory");
                if (type != null)
                    return type;
            }
            return null;
        }

        // public static CSGBrush CreateBrushFromPlanes(string brushName, UnityEngine.Plane[]
        // planes, UnityEngine.Vector3[] tangents = null, UnityEngine.Vector3[] binormals = null,
        // UnityEngine.Material[] materials = null, UnityEngine.Matrix4x4[] textureMatrices = null,
        // TextureMatrixSpace textureMatrixSpace = TextureMatrixSpace.WorldSpace)
        private static MethodInfo GetBrushFactoryCreateBrushFromPlanesMethod()
        {
            foreach (var method in brushFactory.GetMethods())
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 7
                    && parameters[0].Name == "brushName"
                    && parameters[1].Name == "planes"
                    && parameters[2].Name == "tangents"
                    && parameters[3].Name == "binormals"
                    && parameters[4].Name == "materials"
                    && parameters[5].Name == "textureMatrices"
                    && parameters[6].Name == "textureMatrixSpace")
                    return method;
            }

            return null;
        }

        public static MonoBehaviour CreateBrushFromPlanes(string brushName, Plane[] planes)
        {
            if (!IsAvailable()) return null;

            var brush = createBrushFromPlanesMethod.Invoke(null, new object[] { brushName, planes, null, null, null, null, 0 });
            if (brush == null) return null;
            return (MonoBehaviour)brush;
        }
    }
}

#endif