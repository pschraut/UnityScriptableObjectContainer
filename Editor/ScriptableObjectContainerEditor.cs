﻿//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0002 // Name can be simplified
#pragma warning disable IDE0040 // Add accessibility modifiers
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Oddworm.Framework;
using System.Reflection;

namespace Oddworm.EditorFramework
{
    [CustomEditor(typeof(ScriptableObjectContainer), editorForChildClasses: true, isFallback = false)]
    public class ScriptableObjectContainerEditor : Editor
    {
        List<Editor> m_Editors = new List<Editor>();
        Script m_MissingScriptObject = default; // If a sub-object is null, use the m_MissingScriptObject as object to draw the titlebar
        string m_SearchText = "";
        UnityEditor.IMGUI.Controls.SearchField m_SearchField = default;
        Rect m_AddObjectButtonRect; // the layout rect of the "Add Object" button. We use it to display the popup menu at the proper position

        class Script : ScriptableObject { }

        protected virtual void OnEnable()
        {
            m_SearchText = "";
            m_MissingScriptObject = ScriptableObject.CreateInstance<Script>();
        }

        protected virtual void OnDisable()
        {
            DestroyImmediate(m_MissingScriptObject);
            m_MissingScriptObject = null;

            for (var n = 0; n < m_Editors.Count; ++n)
            {
                Editor.DestroyImmediate(m_Editors[n]);
            }

            m_Editors.Clear();
        }

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.Separator();
            DrawSearchField();

            EditorGUI.indentLevel++;
            DrawContainerGUI();
            EditorGUI.indentLevel--;

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            DrawSubObjectsGUI();
            var hasChanged = EditorGUI.EndChangeCheck();

            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();

            DrawAddSubObjectButton();

            // If a subobject has changed, run the container OnValidate method too.
            if (hasChanged)
            {
                foreach(var t in targets)
                {
                    if (t == null)
                        continue;

                    var m = t.GetType().GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (m != null)
                        m.Invoke(t, null);
                }
            }
        }

        protected void DrawSearchField()
        {
            if (m_SearchField == null)
                m_SearchField = new UnityEditor.IMGUI.Controls.SearchField();

            EditorGUI.indentLevel++;
            m_SearchText = m_SearchField.OnToolbarGUI(m_SearchText);
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Override this method to draw custom GUI for the ScriptableObjectContainer itself.
        /// </summary>
        protected virtual void DrawContainerGUI()
        {
            base.OnInspectorGUI();
        }

        protected bool DrawSubObjectGUI(ScriptableObject subObject, bool isExpanded)
        {
            if (subObject == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                isExpanded = DrawSubObjectTitlebar(m_MissingScriptObject, isExpanded);
                EditorGUI.EndDisabledGroup();
                if (isExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox("The associated script could not be loaded.\nPlease fix any compile errors and assign a valid script.", MessageType.Warning);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Separator();
                }
            }
            else
            {
                if ((subObject.hideFlags & HideFlags.HideInInspector) == 0)
                {
                    var editor = GetOrCreateEditor(subObject);

                    isExpanded = DrawSubObjectTitlebar(subObject, isExpanded);
                    if (isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        editor.OnInspectorGUI();
                        EditorGUI.indentLevel--;

                        EditorGUILayout.Separator();
                    }
                }
            }

            return isExpanded;
        }

        protected void DrawSubObjectsGUI()
        {
            var subObjectsProperty = EditorScriptableObjectContainerUtility.FindObjectsProperty(serializedObject);

            DestroyUnusedEditors(subObjectsProperty);
            var searchSplits = m_SearchText.Split(new[] { ' ', '\t', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            for (var n = 0; n < subObjectsProperty.arraySize; ++n)
            {
                var subObjProperty = subObjectsProperty.GetArrayElementAtIndex(n);
                if (subObjProperty.hasMultipleDifferentValues)
                    continue;

                var subObject = (ScriptableObject)subObjProperty.objectReferenceValue;

                if (searchSplits.Length > 0)
                {
                    var isVisible = false;
                    var subObjectName = subObject.name;
                    foreach (var sp in searchSplits)
                    {
                        if (subObjectName.IndexOf(sp, System.StringComparison.InvariantCultureIgnoreCase) != -1)
                        {
                            isVisible = true;
                            break;
                        }
                    }
                    if (!isVisible)
                        continue;
                }

                subObjProperty.isExpanded = DrawSubObjectGUI(subObject, subObjProperty.isExpanded);
            }

            var dropRect = GUILayoutUtility.GetRect(1, 3, GUILayout.ExpandWidth(true));
            DrawSeparator(dropRect);
            HandleDragAndDrop(dropRect, null);

            void DrawSeparator(Rect d)
            {
                d.height = 1;
                var personalColor = new Color(0.5f, 0.5f, 0.5f, 1);
                var proColor = new Color(0.1f, 0.1f, 0.1f, 1);
                var color = EditorGUIUtility.isProSkin ? proColor : personalColor;
                EditorGUI.DrawRect(d, color);
            }
        }

        protected void DrawAddSubObjectButton()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add Object", "AC Button", GUILayout.Width(250)))
                {
                    ShowAddSubObjectPopup(m_AddObjectButtonRect);
                }
                if (Event.current.type == EventType.Repaint)
                    m_AddObjectButtonRect = GUILayoutUtility.GetLastRect();

                GUILayout.FlexibleSpace();
            }
        }

        protected bool DrawSubObjectTitlebar(Object subObject, bool foldout)
        {
            var e = Event.current;
            var isMissing = m_MissingScriptObject == subObject;
            var titlebarRect = GUILayoutUtility.GetRect(10, 24, GUILayout.ExpandWidth(true));

            var buttonRect = titlebarRect;
            buttonRect.x += titlebarRect.width - 80;
            buttonRect.width = 20;
            buttonRect.y += 3;

            // Unity adds an "enabled checkbox" for ScriptableObjects, but even if you disable them,
            // they still get their OnEnable call, so it doesn't work and is probably just an UI oversight.
            // Therefore we swallow all clicks for that checkbox
            var enabledHit = false;
            var enabledRect = titlebarRect;
            enabledRect.x += 37;
            enabledRect.y += 1;
            enabledRect.width = 21;
            enabledRect.height -= 4;
            if (enabledRect.Contains(e.mousePosition) && (e.type != EventType.Layout && e.type != EventType.Repaint))
            {
                enabledHit = e.type == EventType.MouseUp;
                e.Use();
            }


            // Handle "button" input befpre EditorGUI.InspectorTitlebar, otherwise the titlebar swallows the input
            if (!isMissing && buttonRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                e.Use();

                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Rename..."), false, delegate (object o)
                {
                    var wnd = EditorWindow.GetWindow<RenameDialog>();
                    wnd.Show((Object)o);
                }, subObject);

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Remove"), false, delegate (object o)
                {
                    RemoveSubObject((ScriptableObject)o);
                }, subObject);

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Extract..."), false, delegate (object o)
                {
                    ExtractSubObject((ScriptableObject)o);
                }, subObject);

                menu.ShowAsContext();
            }

            var value = EditorGUI.InspectorTitlebar(titlebarRect, foldout, subObject, true);

            if (!isMissing)
            {
                // Draw the button, this is only for its visual appearance
                GUI.Button(buttonRect, EditorGUIUtility.IconContent("d__Popup"), "IconButton");


                var fields = EditorScriptableObjectContainerUtility.GetObjectToggleFields((ScriptableObject)subObject);
                var isEnabledPresent = fields.Count > 0;

                enabledRect.x += 3;
                if (isEnabledPresent)
                {
                    var isEnabled = EditorScriptableObjectContainerUtility.GetObjectToggleValue((ScriptableObject)subObject, fields);
                    EditorGUI.Toggle(enabledRect, GUIContent.none, isEnabled);

                    var newIsEnabled = isEnabled;
                    if (enabledHit)
                        newIsEnabled = !newIsEnabled;

                    if (newIsEnabled != isEnabled)
                    {
                        Undo.RecordObject(subObject, "Inspector");
                        EditorScriptableObjectContainerUtility.SetObjectToggleValue((ScriptableObject)subObject, fields, newIsEnabled);
                        EditorUtility.SetDirty(subObject);
                        Undo.FlushUndoRecordObjects();
                    }
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.Toggle(enabledRect, new GUIContent("", $"Decorate a bool field with [{nameof(SubAssetToggleAttribute)}] to toggle the enabled state."), true);
                    EditorGUI.EndDisabledGroup();
                }
            }

            var dropRect = titlebarRect;
            dropRect.y -= 3;
            dropRect.height = 3;
            HandleDragAndDrop(dropRect, subObject);

            return value;
        }

        void HandleDragAndDrop(Rect targetRect, Object targetObject)
        {
            var e = Event.current;
            if (!targetRect.Contains(e.mousePosition))
                return;

            // Support drag&drop of a single object only
            var objectReferences = DragAndDrop.objectReferences;
            if (objectReferences.Length != 1)
                return;

            // Do not support to drag&drop ScriptableObjects, but NOT a container inside another container
            var dragObj = objectReferences[0] as ScriptableObject;
            if (dragObj == null || dragObj is ScriptableObjectContainer)
                return;

            // Do not support to drag&drop missing script references, as this caused issues in my tests
            if (targetObject is Script || objectReferences[0] is Script)
                return;

            // Support drag/drop for a single container only
            if (targets.Length != 1)
                return;

            // Support drag&drop inside the same conainer only
            //var dropEditor = FindEditor(targetObject);
            var fromSelf = FindEditor(dragObj) != null;
            DragAndDrop.visualMode = fromSelf ? DragAndDropVisualMode.Move : DragAndDropVisualMode.Copy;
            GUI.Box(targetRect, GUIContent.none, "InsertionMarker");

            if (e.type == EventType.DragPerform)
            {
                e.Use();

                var container = (ScriptableObjectContainer)serializedObject.targetObject;
                Undo.IncrementCurrentGroup();
                if (fromSelf)
                {
                    Undo.RegisterCompleteObjectUndo(this.target, "Move Object");
                    EditorScriptableObjectContainerUtility.MoveObject(container, (ScriptableObject)objectReferences[0], (ScriptableObject)targetObject);
                }
                else
                {
                    if (EditorScriptableObjectContainerUtility.CanAddObjectOfType(container, objectReferences[0].GetType(), true))
                    {
                        Undo.RegisterCompleteObjectUndo(this.target, "Add Object");
                        var newObj = ScriptableObject.CreateInstance(objectReferences[0].GetType());
                        EditorScriptableObjectContainerUtility.AddObject(container, newObj);
                        EditorUtility.CopySerialized(objectReferences[0], newObj);
                        EditorScriptableObjectContainerUtility.MoveObject(container, newObj, (ScriptableObject)targetObject);
                    }
                }
                Undo.FlushUndoRecordObjects();
                EditorUtility.SetDirty(serializedObject.targetObject);
                serializedObject.UpdateIfRequiredOrScript();
                GUIUtility.ExitGUI();
            }
        }

        Editor GetOrCreateEditor(Object t)
        {
            var editor = FindEditor(t);
            if (editor != null)
                return editor;

            editor = Editor.CreateEditor(t);
            m_Editors.Add(editor);
            return editor;
        }

        Editor FindEditor(Object t)
        {
            for (var n = 0; n < m_Editors.Count; ++n)
            {
                if (m_Editors[n].target == t)
                    return m_Editors[n];
            }
            return null;
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

        class PopupMenuItem
        {
            public string title;
            public System.Type type;
        }

        protected void ShowAddSubObjectPopup(Rect popupMenuRect)
        {
            var typeList = new List<System.Type>();
            var itemList = new List<PopupMenuItem>();

            GetScriptableObjectTypes(typeList);

            foreach (var t in targets)
                FilterScriptableObjectTypes(t.GetType(), typeList);

            foreach (var type in typeList)
            {
                var itemName = ObjectNames.NicifyVariableName(type.Name);

                var attributes = type.GetCustomAttributes(typeof(CreateSubAssetMenuAttribute), true) as CreateSubAssetMenuAttribute[];
                if (attributes != null && attributes.Length > 0 && !string.IsNullOrEmpty(attributes[0].menuName))
                    itemName = attributes[0].menuName;

                var item = new PopupMenuItem();
                item.title = itemName;
                item.type = type;
                itemList.Add(item);
            }

            // Sort menu items alphabetically
            itemList.Sort(delegate (PopupMenuItem a, PopupMenuItem b)
            {
                return string.Compare(a.title, b.title, true);
            });

            // Create and show the actual menu
            var menu = new GenericMenu();
            foreach (var item in itemList)
            {
                menu.AddItem(new GUIContent(item.title), false, OnClick, item.type);
            }
            menu.DropDown(popupMenuRect);

            // Callback when a menu item is clicked
            void OnClick(object userData)
            {
                var type = (System.Type)userData;

                foreach (var t in targets)
                {
                    var parent = t as ScriptableObjectContainer;
                    if (parent == null)
                        continue;

                    CreateSubObject(parent, type);
                }
            }

            // Get all types that are decorated with the CreateSubAssetMenu attribute.
            void GetScriptableObjectTypes(List<System.Type> result)
            {
                foreach (var type in TypeCache.GetTypesWithAttribute<CreateSubAssetMenuAttribute>())
                {
                    if (!type.IsSubclassOf(typeof(ScriptableObject)))
                        continue;

                    if (type.IsAbstract)
                        continue;

                    if (type.IsGenericType)
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
                    if (method.DeclaringType != containerType)
                        continue;

                    if (!VerifyFilterTypesMethod(method))
                    {
                        Debug.LogError($"The method '{method.Name}' in type '{method.DeclaringType.FullName}' is decorated with the '{typeof(ScriptableObjectContainer.FilterTypesMethodAttribute).FullName}' attribute, but the method signature is incorrect. The method signature must be 'static void {method.Name}(System.Collections.Generic.List<System.Type> types)' instead.");
                        continue;
                    }

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

            bool VerifyFilterTypesMethod(System.Reflection.MethodInfo method)
            {
                if (!method.IsStatic)
                    return false;

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

        protected void CreateSubObject(ScriptableObjectContainer parent, System.Type type)
        {
            if (parent == null)
                return;

            if (!EditorScriptableObjectContainerUtility.CanAddObjectOfType(parent, type, true))
                return;

            Undo.IncrementCurrentGroup();
            var subObject = ScriptableObject.CreateInstance(type);
            subObject.name = type.Name;
            Undo.RegisterCreatedObjectUndo(subObject, "Create");

            Undo.RegisterCompleteObjectUndo(parent, "Create");
            EditorScriptableObjectContainerUtility.AddObject(parent, subObject);
            Undo.FlushUndoRecordObjects();
            EditorUtility.SetDirty(parent);
        }

        protected void RemoveSubObject(ScriptableObject subObject)
        {
            if (subObject == null)
                return;

            var parent = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(subObject)) as ScriptableObjectContainer;
            if (parent == null)
                return;

            Undo.IncrementCurrentGroup();
            Undo.RegisterCompleteObjectUndo(parent, "Delete");
            EditorScriptableObjectContainerUtility.RemoveObject(parent, subObject);
            Undo.FlushUndoRecordObjects();
            EditorUtility.SetDirty(parent);
        }

        protected void ExtractSubObject(ScriptableObject subObject)
        {
            if (subObject == null)
                return;

            var defaultName = ObjectNames.NicifyVariableName($"{subObject.name} ({subObject.GetType().Name})").Trim();
            var assetPath = EditorUtility.SaveFilePanelInProject($"Extract {subObject.name} ({subObject.GetType().Name})...", defaultName, "asset", "Please select an extraction path.");
            if (string.IsNullOrEmpty(assetPath))
                return;

            AssetDatabase.StartAssetEditing();
            try
            {
                string metaContent = "";
                if (System.IO.File.Exists(assetPath))
                {
                    var metaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath);
                    metaContent = System.IO.File.ReadAllText(metaPath);
                    System.IO.File.Delete(assetPath);
                    System.IO.File.Delete(metaPath);
                }

                var newObj = ScriptableObject.CreateInstance(subObject.GetType());
                EditorUtility.CopySerialized(subObject, newObj);
                AssetDatabase.CreateAsset(newObj, assetPath);

                if (!string.IsNullOrEmpty(metaContent))
                    System.IO.File.WriteAllText(AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath), metaContent);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }
    }
}