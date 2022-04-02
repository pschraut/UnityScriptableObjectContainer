//
// ScriptableObject Container for Unity. Copyright (c) 2020-2022 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0019 // Use Pattern matching
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
#endif

namespace Oddworm.Framework
{
#if !SCRIPTABLEOBJECTCONTAINER_DISABLE_MENUITEM
    [CreateAssetMenu(menuName = "ScriptableObject Container", order = 310)]
#endif
    public class ScriptableObjectContainer : ScriptableObject
    {
        [Tooltip("The array holds references to the added objects.")]
        [HideInInspector]
        [SerializeField] ScriptableObject[] m_SubObjects = new ScriptableObject[0];

        static readonly List<object> s_TempCache = new List<object>();

        /// <summary>
        /// Gets the first object of the specified type.
        /// </summary>
        /// <param name="type">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</param>
        /// <returns>The object on success, <c>null</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public ScriptableObject GetObject(Type type)
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
        /// Gets the first object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</typeparam>
        /// <returns>The object on success,<c>null</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
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
        /// Gets the first object of the specified type.
        /// </summary>
        /// <param name="type">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</param>
        /// <param name="result">The output argument that will contain the object or null.</param>
        /// <returns>true if the object is found, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public bool TryGetObject(Type type, out object result)
        {
            result = GetObject(type);
            return result != null;
        }

        /// <summary>
        /// Gets the first object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</typeparam>
        /// <param name="result">The output argument that will contain the object or null.</param>
        /// <returns>true if the object is found, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public bool TryGetObject<T>(out T result) where T : class
        {
            result = GetObject<T>();
            return result != null;
        }

        /// <summary>
        /// Gets all objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</typeparam>
        /// <param name="results">The list where all objects are added to.</param>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
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
        /// <typeparam name="T">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</typeparam>
        /// <returns>
        /// A newly allocated array that contains references to the objects, or an empty array if no objects could be found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public T[] GetObjects<T>() where T : class
        {
            s_TempCache.Clear();
            GetObjects(typeof(T), s_TempCache);

            var result = new T[s_TempCache.Count];
            for (var n = 0; n < s_TempCache.Count; ++n)
                result[n] = s_TempCache[n] as T;

            s_TempCache.Clear();
            return result;
        }

        /// <summary>
        /// Gets all objects of the specified type.
        /// </summary>
        /// <param name="type">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</param>
        /// <param name="results">The list where all objects are added to.</param>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public void GetObjects(Type type, List<object> results)
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
        /// Gets all objects of the specified type.
        /// </summary>
        /// <param name="type">The type of object to retrieve. The type must derive from ScriptableObject or must be an interface.</param>
        /// <returns>
        /// A newly allocated array that contains references to the objects, or an empty array if no objects could be found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the specified type is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the specified type does not derive from ScriptableObject or isn't an interface.</exception>
        public object[] GetObjects(Type type)
        {
            s_TempCache.Clear();
            GetObjects(type, s_TempCache);

            var result = new object[s_TempCache.Count];
            for (var n = 0; n < s_TempCache.Count; ++n)
                result[n] = s_TempCache[n];

            s_TempCache.Clear();
            return result;
        }

        protected virtual void OnEnable()
        {
            // In case I need to implement something in OnEnable in the future, lets make it virtual.
        }

        protected virtual void OnDisable()
        {
            // In case I need to implement something in OnDisable in the future, lets make it virtual.
        }

        protected virtual void OnValidate()
        {
            ProcessSubAssetOwnerAttribute();
        }

        /// <summary>
        /// Check each sub asset if any of its fields uses the <see cref="SubAssetOwnerAttribute"/> and
        /// assign that field to this ScriptableObjectContainer.
        /// </summary>
        void ProcessSubAssetOwnerAttribute()
        {
#if UNITY_EDITOR
            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                var so = m_SubObjects[n];
                if (so == null)
                    continue;

                var soType = so.GetType();
                var loopguard = 0;

                do
                {
                    if (++loopguard > 64)
                    {
                        Debug.LogError($"Loopguard kicked in, detected more than {loopguard} levels of inheritence?", this);
                        break;
                    }

                    foreach (var fieldInfo in soType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
                    {
                        var attribute = fieldInfo.GetCustomAttribute<SubAssetOwnerAttribute>(true);
                        if (attribute == null)
                            continue;

                        // Make sure the field is a reference to a ScriptableObject or derived type
                        var valid = false;
                        if (fieldInfo.FieldType == typeof(ScriptableObject))
                            valid = true;
                        if (fieldInfo.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                            valid = true;
                        if (!valid)
                        {
                            Debug.LogError($"The field '{fieldInfo.Name}' in class '{so.GetType().FullName}' uses the [{nameof(SubAssetOwnerAttribute)}]. The attribute can be used for fields of type '{nameof(ScriptableObject)}' only, but it uses '{fieldInfo.FieldType.FullName}' which does not inherit from '{nameof(ScriptableObject)}'.", this);
                            continue;
                        }

                        // Make sure the field- and container type are compatible
                        valid = false;
                        if (fieldInfo.FieldType == GetType())
                            valid = true;
                        if (GetType().IsSubclassOf(fieldInfo.FieldType))
                            valid = true;
                        if (!valid)
                        {
                            Debug.LogError($"The field '{fieldInfo.Name}' in class '{so.GetType().FullName}' uses the [{nameof(SubAssetOwnerAttribute)}], but the container and field type are incompatible. The field is of type '{fieldInfo.FieldType.FullName}', but the container is of type '{GetType().FullName}', which is not a sub-class of '{fieldInfo.FieldType.FullName}'.", this);
                            continue;
                        }

                        if ((ScriptableObject)fieldInfo.GetValue(so) != this)
                        {
                            fieldInfo.SetValue(so, this);
                            UnityEditor.EditorUtility.SetDirty(so);
                        }
                    }

                    soType = soType.BaseType;
                } while (soType != null && soType != typeof(ScriptableObject));
            }
#endif
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
