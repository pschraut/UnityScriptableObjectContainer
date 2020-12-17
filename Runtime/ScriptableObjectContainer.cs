//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
#endif

namespace Oddworm.Framework
{
    [CreateAssetMenu(menuName = "ScriptableObject Container", order = 310)]
    public class ScriptableObjectContainer : ScriptableObject
    {
        [Tooltip("The array holds references to the added objects.")]
        [HideInInspector]
        [SerializeField] ScriptableObject[] m_SubObjects = new ScriptableObject[0];

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
                        Debug.LogError($"Loopguard kicked in, detected more than 64 levels of inheritence?");
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
                            Debug.LogError($"The field '{fieldInfo.Name}' in class '{so.GetType().FullName}' uses the [{nameof(SubAssetOwnerAttribute)}]. The attribute can be used for fields of type '{nameof(ScriptableObject)}' only, but it uses '{fieldInfo.FieldType.FullName}' which does not inherit from '{nameof(ScriptableObject)}'.");
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
                            Debug.LogError($"The field '{fieldInfo.Name}' in class '{so.GetType().FullName}' uses the [{nameof(SubAssetOwnerAttribute)}], but the container and field type are incompatible. The field is of type '{fieldInfo.FieldType.FullName}', but the container is of type '{GetType().FullName}', which is not a sub-class of '{fieldInfo.FieldType.FullName}'.");
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
