//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE1006 // Naming Styles
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oddworm.Framework
{
    [CreateAssetMenu(menuName = "ScriptableObject Container", order = 310)]
    public class ScriptableObjectContainer : ScriptableObject
    {
        public sealed class FilterTypesMethodAttribute : System.Attribute
        { }

        [HideInInspector]
        [SerializeField] ScriptableObject[] m_SubObjects = new ScriptableObject[0];

        /// <summary>
        /// Gets a reference to the internal array that contains the sub-objects.
        /// </summary>
        public ScriptableObject[] subObjects
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_SubObjects; }
        }

        /// <summary>
        /// Gets the sub-object of the specified type.
        /// </summary>
        /// <param name="type">The type of sub-object to retrieve. The type must derive from ScriptableObject or must be an interface.</param>
        /// <returns>The sub-object on success, null otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="System.ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public ScriptableObject GetSubObject(System.Type type)
        {
            ThrowIfInvalidType(type, nameof(GetSubObject));

            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                if (m_SubObjects[n] != null && m_SubObjects[n].GetType() == type)
                    return m_SubObjects[n];
            }
            return null;
        }

        /// <summary>
        /// Gets the sub-object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of sub-object to retrieve. The type must derive from ScriptableObject or must be an interface.</typeparam>
        /// <returns>The sub-object on success, null otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="System.ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public T GetSubObject<T>() where T : class
        {
            ThrowIfInvalidType(typeof(T), nameof(GetSubObject));

            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                var subobj = m_SubObjects[n] as T;
                if (subobj != null)
                    return subobj;
            }
            return null;
        }

        /// <summary>
        /// Gets all sub-objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of sub-object to retrieve. The type must derive from ScriptableObject or must be an interface.</typeparam>
        /// <param name="result">The list where all sub-objects are added to.</param>
        /// <exception cref="System.ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="System.ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public void GetSubObjects<T>(List<T> result) where T : class
        {
            ThrowIfInvalidType(typeof(T), nameof(GetSubObjects));

            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                var subobj = m_SubObjects[n] as T;
                if (subobj != null)
                    result.Add(subobj);
            }
        }

        /// <summary>
        /// Gets all sub-objects of the specified type.
        /// </summary>
        /// <param name="type">The type of sub-object to retrieve. The type must derive from ScriptableObject or must be an interface.</param>
        /// <param name="result">The list where all sub-objects are added to.</param>
        /// <exception cref="System.ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="System.ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public void GetSubObjects(List<ScriptableObject> result, System.Type type)
        {
            ThrowIfInvalidType(type, nameof(GetSubObjects));

            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                if (m_SubObjects[n] != null && m_SubObjects[n].GetType() == type)
                    result.Add(m_SubObjects[n]);
            }
        }

        /// <summary>
        /// Throws an exception if the specified type is invalid to be used with any GetSubObject method.
        /// </summary>
        /// <param name="type">The type to verify.</param>
        /// <param name="methodName">The method name of the caller.</param>
        void ThrowIfInvalidType(System.Type type, string methodName)
        {
            if (type == null)
                throw new System.ArgumentNullException($"Type must not be null.");

            var isValidType = type.IsSubclassOf(typeof(ScriptableObject));
            if (type.IsInterface)
                isValidType = true;

            if (!isValidType)
                throw new System.ArgumentException($"{methodName} requires that the requested component '{type.Name}' derives from {nameof(ScriptableObject)} or is an interface.");
        }

#if UNITY_EDITOR
        void EditorBake()
        {
            var objs = new List<ScriptableObject>();

            // load all objects in the container asset
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (!(obj is ScriptableObject))
                    continue;
                if (obj is ScriptableObjectContainer)
                    continue;

                objs.Add(obj as ScriptableObject);
            }

            var temp = new List<ScriptableObject>(m_SubObjects);

            // add all objects that are currently not in the m_SubObjects array
            foreach (var so in objs)
            {
                if (temp.IndexOf(so) == -1)
                    temp.Add(so);
            }

            // remove all objects that in the m_SubObjects array, but not in the asset anymore
            for (var n = temp.Count - 1; n >= 0; --n)
            {
                if (objs.IndexOf(temp[n]) == -1)
                    temp.RemoveAt(n);
            }

            // and we have our new array
            m_SubObjects = temp.ToArray();
        }
#endif

#if UNITY_EDITOR
        public static class Editor
        {
            public static void Bake(ScriptableObjectContainer container)
            {
                container.EditorBake();
            }
        }
#endif
    }
}
