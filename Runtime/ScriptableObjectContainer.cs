//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oddworm.Framework
{
    [CreateAssetMenu(menuName = "ScriptableObject Container", order = 310)]
    public class ScriptableObjectContainer : ScriptableObject
    {
        [Tooltip("The array holds references to the added objects.")]
        [HideInInspector]
        [SerializeField] ScriptableObject[] m_SubObjects = new ScriptableObject[0];

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class FilterTypesMethodAttribute : Attribute
        { }

        /// <summary>
        /// Gets a reference to the internal array that contains the objects.
        /// </summary>
        public ScriptableObject[] objects
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_SubObjects; }
        }

        /// <summary>
        /// Gets the object of the specified type.
        /// </summary>
        /// <param name="type">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</param>
        /// <returns>The object on success, null otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="System.ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public ScriptableObject GetObject(System.Type type)
        {
            ThrowIfInvalidType(type, nameof(GetObject));

            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                var subObject = m_SubObjects[n];
                if (subObject == null)
                    continue;

                var subObjectType = subObject.GetType();
                if (subObjectType == type || subObjectType.IsSubclassOf(type))
                    return subObject;
            }

            return null;
        }

        /// <summary>
        /// Gets the object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</typeparam>
        /// <returns>The object on success, null otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="System.ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public T GetObject<T>() where T : class
        {
            ThrowIfInvalidType(typeof(T), nameof(GetObject));

            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                var subobj = m_SubObjects[n] as T;
                if (subobj != null)
                    return subobj;
            }

            return null;
        }

        /// <summary>
        /// Gets all objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</typeparam>
        /// <param name="results">The list where all objects are added to.</param>
        /// <exception cref="System.ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="System.ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public void GetObjects<T>(List<T> results) where T : class
        {
            ThrowIfInvalidType(typeof(T), nameof(GetObjects));

            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                var subobj = m_SubObjects[n] as T;
                if (subobj != null)
                    results.Add(subobj);
            }
        }

        /// <summary>
        /// Gets all objects of the specified type.
        /// </summary>
        /// <param name="type">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</param>
        /// <param name="results">The list where all objects are added to.</param>
        /// <exception cref="System.ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="System.ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public void GetObjects(List<ScriptableObject> results, System.Type type)
        {
            ThrowIfInvalidType(type, nameof(GetObjects));

            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                var subObject = m_SubObjects[n];
                if (subObject == null)
                    continue;

                var subObjectType = subObject.GetType();
                if (subObjectType == type || subObjectType.IsSubclassOf(type))
                    results.Add(subObject);
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

            var isValidType = type.IsSubclassOf(typeof(ScriptableObject)) || type == typeof(ScriptableObject);
            if (type.IsInterface)
                isValidType = true;

            if (!isValidType)
                throw new System.ArgumentException($"{methodName} requires that the requested component '{type.Name}' derives from {nameof(ScriptableObject)} or is an interface.");
        }
    }
}
