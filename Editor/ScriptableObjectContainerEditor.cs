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

        void ShowScriptableObjectDropdown()
        {
            var menu = new GenericMenu();
            var list = new List<System.Type>();
            foreach(var type in TypeCache.GetTypesWithAttribute<CreateAssetMenuAttribute>())
            {
                if (!type.IsSubclassOf(typeof(ScriptableObject)))
                    continue;

                if (type == typeof(ScriptableObjectContainer) || type.IsSubclassOf(typeof(ScriptableObjectContainer)))
                    continue;

                list.Add(type);

            }

            foreach(var t in targets)
            {
                var container = t as ScriptableObjectContainer;
                container.FilterTypes(list);
            }

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

                    var so = ScriptableObject.CreateInstance(type);
                    so.name = type.Name;
                    Undo.RegisterCreatedObjectUndo(so, "Create");

                    Undo.RecordObject(parent, "Add");
                    AssetDatabase.AddObjectToAsset(so, parent);

                    ScriptableObjectContainer.Editor.Bake(parent);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}
