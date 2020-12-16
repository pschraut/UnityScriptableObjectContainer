//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Oddworm.Framework;
using System.Reflection;

namespace Oddworm.EditorFramework
{
    public static class EditorScriptableObjectContainerUtility
    {
        static int s_FilterTypesStackOverflowGuard = 0;

        /// <summary>
        /// Gets all private and public fields in the specified <paramref name="subObject"/> that are decorated with the <c>[SubAssetToggle]</c> attribute.
        /// </summary>
        /// <param name="subObject">The sub-asset.</param>
        /// <returns>A list of fields decorated with [<see cref="SubAssetToggleAttribute"/>].</returns>
        public static List<FieldInfo> GetObjectToggleFields(ScriptableObject subObject)
        {
            var result = new List<FieldInfo>();
            var type = subObject.GetType();
            var loopguard = 0;

            do
            {
                if (++loopguard > 64)
                {
                    Debug.LogError($"Loopguard kicked in, detected more than 64 levels of inheritence?");
                    break;
                }

                foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (fieldInfo.FieldType != typeof(bool))
                        continue;
                    if (fieldInfo.GetCustomAttribute<SubAssetToggleAttribute>(true) == null)
                        continue;

                    result.Add(fieldInfo);
                }

                type = type.BaseType;
            } while (type != null && type != typeof(ScriptableObject));

            return result;
        }

        /// <summary>
        /// Gets if any of the specified <paramref name="toggleFields"/> is <c>true</c>.
        /// </summary>
        /// <param name="subObject">The sub-asset.</param>
        /// <param name="toggleFields">The result of <see cref="GetObjectToggleFields(ScriptableObject)"/></param>
        /// <returns><c>true</c> if any field is <c>true</c>, <c>false</c> otherwise.</returns>
        public static bool GetObjectToggleValue(ScriptableObject subObject, List<FieldInfo> toggleFields)
        {
            foreach (var fieldInfo in toggleFields)
            {
                if ((bool)fieldInfo.GetValue(subObject))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Sets all <paramref name="toggleFields"/> to the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="subObject">The sub-asset.</param>
        /// <param name="toggleFields">The result of <see cref="GetObjectToggleFields(ScriptableObject)"/></param>
        /// <param name="value">The value to set all <paramref name="toggleFields"/> to.</param>
        public static void SetObjectToggleValue(ScriptableObject subObject, List<FieldInfo> toggleFields, bool value)
        {
            foreach (var fieldInfo in toggleFields)
            {
                fieldInfo.SetValue(subObject, value);
            }
        }

        /// <summary>
        /// Gets whether a sub-asset of the specied <paramref name="type"/> can be asset to the <paramref name="container"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The type of the sub-asset.</param>
        /// <param name="displayDialog">Whether to display an error dialog if the object can't be added.</param>
        /// <returns><c>true</c> when it can be added, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// For example, if the container contains a sub-asset that uses the [<see cref="DisallowMultipleSubAssetAttribute"/>],
        /// it can't add another sub-asset of the same type.
        /// </remarks>
        public static bool CanAddObjectOfType(ScriptableObjectContainer container, System.Type type, bool displayDialog)
        {
            if (type.IsAbstract)
            {
                if (displayDialog)
                {
                    var title = $"Can't add object!";
                    var message = $"The object of type '{type.Name}' cannot be added, because the type is abstract.";
                    EditorUtility.DisplayDialog(title, message, "OK");
                }
                return false;
            }

            if (type.IsGenericType)
            {
                if (displayDialog)
                {
                    var title = $"Can't add object!";
                    var message = $"The object of type '{type.Name}' cannot be added, because the type is a Generic.";
                    EditorUtility.DisplayDialog(title, message, "OK");
                }
                return false;
            }

            if (!type.IsSubclassOf(typeof(ScriptableObject)))
            {
                if (displayDialog)
                {
                    var title = $"Can't add object!";
                    var message = $"The object of type '{type.Name}' cannot be added, because it doesn't inherit from '{nameof(ScriptableObject)}'.";
                    EditorUtility.DisplayDialog(title, message, "OK");
                }
                return false;
            }

            if (type == typeof(ScriptableObjectContainer) || type.IsSubclassOf(typeof(ScriptableObjectContainer)))
            {
                if (displayDialog)
                {
                    var title = $"Can't add object!";
                    var message = $"The object of type '{type.Name}' cannot be added, because it inherits from '{nameof(ScriptableObjectContainer)}'.\n\nContainer objects can't be nested.";
                    EditorUtility.DisplayDialog(title, message, "OK");
                }
                return false;
            }

            var addedObj = container.GetObject(type);
            if (addedObj != null)
            {
                var disallow = GetCustomAttribute(type, typeof(DisallowMultipleSubAssetAttribute));
                if (disallow != null)
                {
                    if (displayDialog)
                    {
                        var title = $"Can't add the same object multiple times!";
                        var message = $"The object of type '{type.Name}' cannot be added, because '{container.name}' already contains an object of the same type.\n\nRemove the [{nameof(DisallowMultipleSubAssetAttribute)}] from class '{type.Name}' to be able to add multiple objects of the same type.";
                        EditorUtility.DisplayDialog(title, message, "OK");
                    }

                    return false;
                }
            }

            // If CanAddObjectOfType is called from CanAddObjectOfType in its FilterTypes callback,
            // don't run it to avoid stack overflow issues.
            if (s_FilterTypesStackOverflowGuard == 0)
            {
                s_FilterTypesStackOverflowGuard++;
                try
                {
                    var typesList = new List<System.Type>();
                    typesList.Add(type);
                    FilterTypes(container, typesList);
                    var isTypeValid = typesList.Contains(type);
                    if (!isTypeValid)
                    {
                        if (displayDialog)
                        {
                            var title = $"Can't add object!";
                            var message = $"The object of type '{type.Name}' cannot be added, because '{container.name}' does not support this type.\n\nSupport for types can be filtered using the [{nameof(ScriptableObjectContainer.FilterTypesMethodAttribute)}] in that container.";
                            EditorUtility.DisplayDialog(title, message, "OK");
                        }

                        return false;
                    }
                }
                finally
                {
                    s_FilterTypesStackOverflowGuard--;
                }
            }

            return true;
        }

        public static void FilterTypes(ScriptableObjectContainer container, List<System.Type> typesList)
        {
            if (container == null)
            {
                typesList.Clear();
                return;
            }

            var methods = new List<MethodInfo>();
            var containerType = container.GetType();
            foreach (var method in TypeCache.GetMethodsWithAttribute<ScriptableObjectContainer.FilterTypesMethodAttribute>())
            {
                if (method.DeclaringType != containerType)
                    continue;

                if (!VerifyFilterTypesMethod(method))
                {
                    Debug.LogError($"The method '{method.Name}' in type '{method.DeclaringType.FullName}' is decorated with the '{typeof(ScriptableObjectContainer.FilterTypesMethodAttribute).FullName}' attribute, but the method signature is incorrect. The method signature must be 'void {method.Name}(System.Collections.Generic.List<System.Type> types)' instead and can be either static or not.");
                    continue;
                }

                methods.Add(method);
            }

            methods.Sort(delegate (MethodInfo a, MethodInfo b)
            {
                var attr0 = a.GetCustomAttribute<ScriptableObjectContainer.FilterTypesMethodAttribute>(true);
                var attr1 = b.GetCustomAttribute<ScriptableObjectContainer.FilterTypesMethodAttribute>(true);

                // If the order is identical, use the full name for sorting.
                // This is to make execution order stable (as long as name doesn't change)
                if (attr0.order == attr1.order)
                {
                    var name0 = $"{a.DeclaringType.FullName}.{a.Name}";
                    var name1 = $"{b.DeclaringType.FullName}.{b.Name}";

                    return string.Compare(name0, name1, System.StringComparison.Ordinal);
                }

                return attr0.order.CompareTo(attr1.order);
            });

            // Call the FilterTypes method
            for (var n=0; n<methods.Count; ++n)
            {
                var method = methods[n];

                if (method.IsStatic)
                    method.Invoke(null, new[] { typesList });
                else
                    method.Invoke(container, new[] { typesList });
            }

            // Remove all non ScriptableObjects that might have been added during the filter process
            RemoveNonScriptableObjects();

            void RemoveNonScriptableObjects()
            {
                for (var n = typesList.Count - 1; n >= 0; --n)
                {
                    if (typesList[n] == null)
                    {
                        typesList.RemoveAt(n);
                        continue;
                    }

                    if (!typesList[n].IsSubclassOf(typeof(ScriptableObject)))
                    {
                        typesList.RemoveAt(n);
                        continue;
                    }
                }
            }

            bool VerifyFilterTypesMethod(System.Reflection.MethodInfo method)
            {
                if (method.IsAbstract)
                    return false;

                //if (!method.IsStatic)
                //    return false;

                // Accept method with one argument only.
                var parameters = method.GetParameters();
                if (parameters == null || parameters.Length != 1)
                    return false;

                // Accept List<System.Type> as parameter type only.
                if (parameters[0].ParameterType != typeof(List<System.Type>))
                    return false;

                // Accept void return type only.
                if (method.ReturnType != typeof(void))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Gets the specified <paramref name="attributeType"/> in the specified <paramref name="type"/> or any of its base class.
        /// </summary>
        /// <param name="type">The type to look for the attribute.</param>
        /// <param name="attributeType">The type of attribute to search for.</param>
        /// <param name="inherit">true to search the types' inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>The attribute on success, null otherwise.</returns>
        static System.Attribute GetCustomAttribute(System.Type type, System.Type attributeType, bool inherit = true)
        {
            var loopguard = 0;

            do
            {
                if (++loopguard > 64)
                {
                    Debug.LogError($"Loopguard kicked in, detected more than 64 levels of inheritence?");
                    break;
                }

                var attribute = type.GetCustomAttribute(attributeType, inherit);
                if (attribute != null)
                    return attribute;

                type = type.BaseType;
            } while (type != null && type != typeof(UnityEngine.Object));

            return null;
        }

        /// <summary>
        /// Moves the specified moveObject above the specified targetObject.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="moveObject">The sub-asset to move in the Inspector above or below another sub-asset.</param>
        /// <param name="targetObject">The target sub-asset or null. If null then the moveObject is moved to the bottom of the list.</param>
        public static void MoveObject(ScriptableObjectContainer container, ScriptableObject moveObject, ScriptableObject targetObject = null)
        {
            if (moveObject == targetObject)
                return;

            var serContainer = new SerializedObject(container);
            serContainer.UpdateIfRequiredOrScript();

            var subObjProperty = FindObjectsProperty(serContainer);

            // Create a copy of the current m_SubObjects array
            var objects = new List<ScriptableObject>();
            for (var n = 0; n < subObjProperty.arraySize; ++n)
            {
                var element = subObjProperty.GetArrayElementAtIndex(n);
                objects.Add(element.objectReferenceValue as ScriptableObject);
            }

            objects.Remove(moveObject);
            var insertAt = targetObject == null ? -1 : objects.IndexOf(targetObject);
            if (insertAt == -1)
                insertAt = objects.Count;
            objects.Insert(insertAt, moveObject);

            // and we have our new array
            subObjProperty.ClearArray();
            for (var n = 0; n < objects.Count; ++n)
            {
                subObjProperty.InsertArrayElementAtIndex(n);
                var element = subObjProperty.GetArrayElementAtIndex(n);
                element.objectReferenceValue = objects[n];
            }

            serContainer.ApplyModifiedPropertiesWithoutUndo();
        }

        public static SerializedProperty FindObjectsProperty(SerializedObject container)
        {
            return container.FindProperty("m_SubObjects");
        }

        public static bool AddObject(ScriptableObjectContainer container, ScriptableObject subObject)
        {
            if (container == null || subObject == null)
                return false;

            if (!CanAddObjectOfType(container, subObject.GetType(), false))
                return false;

            AssetDatabase.AddObjectToAsset(subObject, container);
            Sync(container);

            return true;
        }

        public static void RemoveObject(ScriptableObjectContainer container, ScriptableObject subObject)
        {
            Undo.DestroyObjectImmediate(subObject); // TODO: this should not use Undo
            Sync(container);
        }

        /// <summary>
        /// Syncronize the internal sub-objects array with the actual content that Unity manages.
        /// </summary>
        /// <param name="container">The container to syncronize.</param>
        /// <remarks>
        /// If you remove a sub-object from a container, the container must update its internal sub-objects array too.
        /// If you use UnityEditor.AssetDatabase.RemoveObject, you have to syncronize the container afterwards. Otherwise
        /// the container holds a reference to a missing sub-object, the one that was removed.
        /// </remarks>
        public static void Sync(ScriptableObjectContainer container)
        {
            var objs = new List<ScriptableObject>();

            // load all objects in the container asset
            var assetPath = AssetDatabase.GetAssetPath(container);
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (!(obj is ScriptableObject))
                    continue;
                if (obj is ScriptableObjectContainer)
                    continue;

                objs.Add(obj as ScriptableObject);
            }

            var serObj = new SerializedObject(container);
            serObj.UpdateIfRequiredOrScript();

            var subObjProperty = FindObjectsProperty(serObj);

            // Create a copy of the current m_SubObjects array
            var objects = new List<ScriptableObject>();
            var added = new List<ScriptableObject>();
            for (var n = 0; n < subObjProperty.arraySize; ++n)
            {
                var element = subObjProperty.GetArrayElementAtIndex(n);
                objects.Add(element.objectReferenceValue as ScriptableObject);
            }

            // add all objects that are currently not in the m_SubObjects array
            foreach (var so in objs)
            {
                if (objects.IndexOf(so) == -1)
                {
                    objects.Add(so);
                    added.Add(so);
                }
            }

            // remove all objects that in the m_SubObjects array, but not in the asset anymore
            for (var n = objects.Count - 1; n >= 0; --n)
            {
                if (objs.IndexOf(objects[n]) == -1)
                    objects.RemoveAt(n);
            }

            // and we have our new array
            subObjProperty.ClearArray();
            for (var n = 0; n < objects.Count; ++n)
            {
                subObjProperty.InsertArrayElementAtIndex(n);
                var element = subObjProperty.GetArrayElementAtIndex(n);
                element.objectReferenceValue = objects[n];

                // If this object has just been added to the container,
                // expand its view in the Inspector
                if (added.IndexOf(objects[n]) != -1)
                    element.isExpanded = true;
            }

            serObj.ApplyModifiedPropertiesWithoutUndo();

            //container.GetType().GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(container, null);
        }
    }
}
