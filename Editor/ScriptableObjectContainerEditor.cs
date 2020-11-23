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
        List<Editor> m_Editors = new List<Editor>();

        void OnEnable()
        {
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

        void DestroyUnusedEditors(SerializedProperty subObjectsProperty)
        {
            for (var j = m_Editors.Count - 1; j >= 0; --j)
            {
                var isUsed = false;
                for (var n = 0; n < subObjectsProperty.arraySize; ++n)
                {
                    var subObjProperty = subObjectsProperty.GetArrayElementAtIndex(n);
                    if (subObjProperty.objectReferenceValue != null && m_Editors[j].target == subObjProperty.objectReferenceValue)
                    {
                        isUsed = true;
                        break;
                    }
                }

                if (!isUsed)
                {
                    Editor.DestroyImmediate(m_Editors[j]);
                    m_Editors.RemoveAt(j);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Separator();

            serializedObject.Update();
            var subObjectsProperty = serializedObject.FindProperty("m_SubObjects");

            DestroyUnusedEditors(subObjectsProperty);


            for (var n=0; n< subObjectsProperty.arraySize; ++n)
            {
                var subObjProperty = subObjectsProperty.GetArrayElementAtIndex(n);

                var subObject = subObjProperty.objectReferenceValue;
                if (subObject == null)
                {
                    EditorGUILayout.HelpBox("The associated script could not be loaded.\nPlease fix any compile errors and assign a valid script.", MessageType.Warning);
                    EditorGUILayout.Separator();
                    continue;
                }

                if (subObjProperty.hasMultipleDifferentValues)
                    continue;

                if ((subObject.hideFlags & HideFlags.HideInInspector) != 0)
                    continue;

                subObjProperty.isExpanded = DrawTitlebar(subObject, subObjProperty.isExpanded);
                if (!subObjProperty.isExpanded)
                    continue;

                var editor = GetOrCreateEditor(subObject);
                editor.OnInspectorGUI();
                EditorGUILayout.Separator();
            }

            EditorGUILayout.Separator();
            serializedObject.ApplyModifiedProperties();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Object", "AC Button", GUILayout.Width(250)))
                {
                    ShowScriptableObjectDropdown();
                }
                GUILayout.FlexibleSpace();
            }
        }

        bool DrawTitlebar(Object subObject, bool foldout)
        {
            var titlebarRect = GUILayoutUtility.GetRect(10, 24, GUILayout.ExpandWidth(true));
            titlebarRect.x -= 18; titlebarRect.width += 22; // for some reason the titlebar doesn't cover the full width, so we expand the rect outself

            var buttonRect = titlebarRect;
            buttonRect.x += titlebarRect.width - 80;
            buttonRect.width = 20;
            buttonRect.y += 3;

            // Handle "button" input befpre EditorGUI.InspectorTitlebar, otherwise the titlebar swallows the input
            var e = Event.current;
            if (buttonRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                e.Use();

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Delete"), false, delegate(object o)
                {
                    DeleteSubObject((Object)o);
                }, subObject);
                menu.ShowAsContext();
            }

            // Draw the titlebar
            var value = EditorGUI.InspectorTitlebar(titlebarRect, foldout, subObject, true);

            // Draw the button, only for its visual appearance
            GUI.Button(buttonRect, EditorGUIUtility.IconContent("d__Popup"), "IconButton");

            return value;
        }

        class PopupMenuItem
        {
            public string title;
            public System.Type type;
        }

        void ShowScriptableObjectDropdown()
        {
            var typeList = new List<System.Type>();
            var itemList = new List<PopupMenuItem>();

            GetScriptableObjectTypes(typeList);

            foreach(var t in targets)
                FilterScriptableObjectTypes(t.GetType(), typeList);

            foreach (var type in typeList)
            {
                var itemName = type.Name;

                var attributes = type.GetCustomAttributes(typeof(CreateSubAssetMenuAttribute), true) as CreateSubAssetMenuAttribute[];
                if (attributes != null && attributes.Length > 0 && !string.IsNullOrEmpty(attributes[0].menuName))
                    itemName = attributes[0].menuName;

                var item = new PopupMenuItem();
                item.title = itemName;
                item.type = type;
                itemList.Add(item);
            }

            itemList.Sort(delegate (PopupMenuItem a, PopupMenuItem b)
            {
                return string.Compare(a.title, b.title, true);
            });

            var menu = new GenericMenu();
            foreach(var item in itemList)
            {
                menu.AddItem(new GUIContent(item.title), false, OnClick, item.type);
            }
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
                foreach (var method in TypeCache.GetMethodsWithAttribute<ScriptableObjectContainer.FilterTypesMethodAttribute>())
                {
                    if (!method.IsStatic)
                        continue;
                    if (method.DeclaringType != containerType)
                        continue;

                    method.Invoke(null, new[] { list });
                }

                // Remove all non ScriptableObjects that might have been added during the filter process
                for (var n = list.Count - 1; n >= 0; --n)
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
        }

        void CreateSubObject(ScriptableObjectContainer parent, System.Type type)
        {
            if (parent == null)
                return;

            Undo.IncrementCurrentGroup();
            var so = ScriptableObject.CreateInstance(type);
            so.name = type.Name;
            Undo.RegisterCreatedObjectUndo(so, "Create");

            var serObj = new SerializedObject(so);
            var serProp = serObj.FindProperty("m_ScriptableObjectContainer");
            if (serProp != null)
                serProp.objectReferenceValue = parent;
            serObj.ApplyModifiedPropertiesWithoutUndo();

            Undo.RegisterCompleteObjectUndo(parent, "Create");
            AssetDatabase.AddObjectToAsset(so, parent);
            ScriptableObjectContainer.Editor.Bake(parent);
            Undo.FlushUndoRecordObjects();
            EditorUtility.SetDirty(parent);
        }

        static void DeleteSubObject(Object subObject)
        {
            if (subObject == null)
                return;

            var parent = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(subObject)) as ScriptableObjectContainer;
            if (parent == null)
                return;

            Undo.IncrementCurrentGroup();
            Undo.RegisterCompleteObjectUndo(parent, "Delete");
            Undo.DestroyObjectImmediate(subObject);
            ScriptableObjectContainer.Editor.Bake(parent);
            Undo.FlushUndoRecordObjects();
            EditorUtility.SetDirty(parent);
        }
    }
}