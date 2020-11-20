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

        List<Editor> m_Editors = new List<Editor>();

        void OnEnable()
        {
            m_SubObjects = serializedObject.FindProperty("m_SubObjects");
        }

        void OnDisable()
        {
            for (var n=0; n< m_Editors.Count; ++n)
            {
                Editor.DestroyImmediate(m_Editors[n]);
            }

            m_Editors.Clear();
        }

        Editor GetOrCreateEditor(Object t)
        {
            for (var n=0; n< m_Editors.Count; ++n)
            {
                if (m_Editors[n].target == t)
                    return m_Editors[n];
            }

            var editor = Editor.CreateEditor(t);
            m_Editors.Add(editor);
            return editor;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Separator();

            for (var n=0; n< m_SubObjects.arraySize; ++n)
            {
                var subObjProperty = m_SubObjects.GetArrayElementAtIndex(n);
                subObjProperty.isExpanded = DrawTitlebar(new[] { subObjProperty.objectReferenceValue }, subObjProperty.isExpanded);
                if (!subObjProperty.isExpanded)
                    continue;

                var editor = GetOrCreateEditor(subObjProperty.objectReferenceValue);
                editor.OnInspectorGUI();
                EditorGUILayout.Separator();
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button("Add"))
            {
                ShowScriptableObjectDropdown();
            }
        }

        bool DrawTitlebar(Object[] objs, bool foldout)
        {
            var r = GUILayoutUtility.GetRect(10, 24, GUILayout.ExpandWidth(true));
            r.x -= 18; r.width += 22; // for some reason the titlebar doesn't cover the full width, so we expand the rect outself

            return EditorGUI.InspectorTitlebar(r, foldout, objs, true);
        }

        void GetScriptableObjectTypes(List<System.Type> result)
        {
            foreach (var type in TypeCache.GetTypesWithAttribute<CreateSubAssetMenuAttribute>())
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
            foreach (var method in TypeCache.GetMethodsWithAttribute<ScriptableObjectContainer.TypeFilterAttribute>())
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

            var serObj = new SerializedObject(so);
            var serProp = serObj.FindProperty("m_ScriptableObjectContainer");
            if (serProp != null)
                serProp.objectReferenceValue = parent;
            serObj.ApplyModifiedPropertiesWithoutUndo();

            Undo.RecordObject(parent, "Add");
            AssetDatabase.AddObjectToAsset(so, parent);

            ScriptableObjectContainer.Editor.Bake(parent);
        }
    }
}
