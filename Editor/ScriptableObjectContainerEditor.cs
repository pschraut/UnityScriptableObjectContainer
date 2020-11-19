//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Oddworm.Framework;

namespace Oddworm.EditorFramework
{
    [CustomEditor(typeof(ScriptableObjectContainer), editorForChildClasses: true, isFallback = false)]
    public class ScriptableObjectContainerEditor : Editor
    {
        SerializedProperty m_SubObjects;

        void OnEnable()
        {
            m_SubObjects = serializedObject.FindProperty("m_SubObjects");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Add"))
            {
                ShowScriptableObjectDropdown();
            }
        }

        void GetScriptableObjectTypes(List<System.Type> result)
        {
            foreach (var type in TypeCache.GetTypesWithAttribute<CreateAssetMenuAttribute>())
            {
                if (!type.IsSubclassOf(typeof(ScriptableObject)))
                    continue;

                var isContainer = type == typeof(ScriptableObjectContainer);
                if (type.IsSubclassOf(typeof(ScriptableObjectContainer)))
                    isContainer = true;

                if (!isContainer)
                    result.Add(type);
            }
        }

        void FilterScriptableObjectTypes(System.Type containerType, List<System.Type> list)
        {
            foreach (var method in TypeCache.GetMethodsWithAttribute<ScriptableObjectContainerTypeFilterAttribute>())
            {
                if (!method.IsStatic)
                    continue;
                if (method.DeclaringType != containerType)
                    continue;

                method.Invoke(null, new[] { list });
            }

            // Remove all non ScriptableObjects that might have been added during the filter process
            for (var n= list.Count-1; n>=0; --n)
            {
                if (list[n] == null)
                {
                    list.RemoveAt(n);
                    continue;
                }

                if (!list[n].IsSubclassOf(typeof(ScriptableObject)))
                {
                    list.RemoveAt(n);
                    continue;
                }
            }
        }

        void ShowScriptableObjectDropdown()
        {
            var menu = new GenericMenu();
            var list = new List<System.Type>();

            GetScriptableObjectTypes(list);

            foreach(var t in targets)
                FilterScriptableObjectTypes(t.GetType(), list);

            foreach (var type in list)
                menu.AddItem(new GUIContent(type.Name), false, OnClick, type);

            menu.ShowAsContext();

            void OnClick(object userData)
            {
                var type = (System.Type)userData;

                foreach(var t in targets)
                {
                    var parent = t as ScriptableObjectContainer;
                    if (parent == null)
                        continue;

                    CreateSubObject(parent, type);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        void CreateSubObject(ScriptableObjectContainer parent, System.Type type)
        {
            var so = ScriptableObject.CreateInstance(type);
            so.name = type.Name;
            Undo.RegisterCreatedObjectUndo(so, "Create");

            Undo.RecordObject(parent, "Add");
            AssetDatabase.AddObjectToAsset(so, parent);

            ScriptableObjectContainer.Editor.Bake(parent);
        }
    }
}
