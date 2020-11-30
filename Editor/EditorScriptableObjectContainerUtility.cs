//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Oddworm.Framework;

namespace Oddworm.EditorFramework
{
    public static class EditorScriptableObjectContainerUtility
    {
        public static void MoveObject(ScriptableObjectContainer container, ScriptableObject moveObject, ScriptableObject targetObject)
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

        public static void AssignContainerProperty(ScriptableObjectContainer container, ScriptableObject subObject)
        {
            var serObj = new SerializedObject(subObject);
            var serProp = serObj.FindProperty("m_ScriptableObjectContainer");
            if (serProp != null)
                serProp.objectReferenceValue = container;
            serObj.ApplyModifiedProperties();
            //serObj.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void ClearContainerProperty(ScriptableObject subObject)
        {
            var serObj = new SerializedObject(subObject);
            var serProp = serObj.FindProperty("m_ScriptableObjectContainer");
            if (serProp != null)
                serProp.objectReferenceValue = null;
            serObj.ApplyModifiedProperties();
            //serObj.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AddObject(ScriptableObjectContainer container, ScriptableObject subObject)
        {
            if (subObject is ScriptableObjectContainer)
            {
                Debug.LogError($"You cannot add a {nameof(ScriptableObjectContainer)} into another {nameof(ScriptableObjectContainer)}");
                return;
            }

            var serObj = new SerializedObject(subObject);
            var serProp = serObj.FindProperty("m_ScriptableObjectContainer");
            if (serProp != null)
                serProp.objectReferenceValue = container;
            serObj.ApplyModifiedProperties();
            //serObj.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.AddObjectToAsset(subObject, container);
            Sync(container);
        }

        public static void RemoveObject(ScriptableObjectContainer container, ScriptableObject subObject)
        {
            Undo.DestroyObjectImmediate(subObject);
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
        }
    }
}
