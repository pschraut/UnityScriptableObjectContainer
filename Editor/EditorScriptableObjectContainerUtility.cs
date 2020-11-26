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
        public static SerializedProperty FindObjectsProperty(SerializedObject container)
        {
            return container.FindProperty("m_SubObjects");
        }

        public static void AddObject(ScriptableObjectContainer container, ScriptableObject subObject)
        {
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
            var temp = new List<ScriptableObject>();
            for (var n = 0; n < subObjProperty.arraySize; ++n)
            {
                var element = subObjProperty.GetArrayElementAtIndex(n);
                temp.Add(element.objectReferenceValue as ScriptableObject);
            }

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
            subObjProperty.ClearArray();
            for (var n = 0; n < temp.Count; ++n)
            {
                subObjProperty.InsertArrayElementAtIndex(n);
                var element = subObjProperty.GetArrayElementAtIndex(n);
                element.objectReferenceValue = temp[n];
            }

            serObj.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
