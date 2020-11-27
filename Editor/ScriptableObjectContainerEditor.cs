//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0040 // Add accessibility modifiers
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
        Script m_MissingScriptObject = default; // If a sub-object is null, use the m_MissingScriptObject as object to draw the titlebar

        class Script : ScriptableObject { }

        protected virtual void OnEnable()
        {
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

            EditorGUI.indentLevel++;
            DrawContainerGUI();
            EditorGUI.indentLevel--;

            EditorGUILayout.Separator();

            DrawSubObjectsGUI();

            EditorGUILayout.Separator();

            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();

            DrawAddSubObjectButton();
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
                isExpanded = DrawSubObjectTitlebar(m_MissingScriptObject, isExpanded);
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

                    // Unity displays a enable/disable checkbox like you find
                    // for Components in the Inspector, for ScriptableObjects too. However, this has no
                    // affect whether the ScriptableObject OnEnable method is called. Therewore I implemented
                    // the following lines that make sure the value is set to true always.
                    var serObj = editor.serializedObject;
                    if (serObj != null && !serObj.isEditingMultipleObjects)
                    {
                        serObj.UpdateIfRequiredOrScript();
                        var isEnabled = serObj.FindProperty("m_Enabled");
                        if (isEnabled != null && !isEnabled.boolValue)
                        {
                            isEnabled.boolValue = true;
                            serObj.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }
                }
            }

            return isExpanded;
        }

        protected void DrawSubObjectsGUI()
        {
            var subObjectsProperty = EditorScriptableObjectContainerUtility.FindObjectsProperty(serializedObject);

            DestroyUnusedEditors(subObjectsProperty);

            for (var n = 0; n < subObjectsProperty.arraySize; ++n)
            {
                var subObjProperty = subObjectsProperty.GetArrayElementAtIndex(n);
                if (subObjProperty.hasMultipleDifferentValues)
                    continue;

                var subObject = (ScriptableObject)subObjProperty.objectReferenceValue;
                subObjProperty.isExpanded = DrawSubObjectGUI(subObject, subObjProperty.isExpanded);
            }
        }

        protected void DrawAddSubObjectButton()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Object", "AC Button", GUILayout.Width(250)))
                {
                    ShowAddSubObjectPopup();
                }
                GUILayout.FlexibleSpace();
            }
        }

        protected bool DrawSubObjectTitlebar(Object subObject, bool foldout)
        {
            var isMissing = m_MissingScriptObject == subObject;
            var titlebarRect = GUILayoutUtility.GetRect(10, 24, GUILayout.ExpandWidth(true));

            var buttonRect = titlebarRect;
            buttonRect.x += titlebarRect.width - 80;
            buttonRect.width = 20;
            buttonRect.y += 3;

            // Handle "button" input befpre EditorGUI.InspectorTitlebar, otherwise the titlebar swallows the input
            var e = Event.current;
            if (!isMissing && buttonRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                e.Use();

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove Object"), false, delegate (object o)
                {
                    RemoveSubObject((ScriptableObject)o);
                }, subObject);

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Rename Object"), false, delegate (object o)
                {
                    var wnd = EditorWindow.GetWindow<RenameDialog>();
                    wnd.Show((Object)o);
                }, subObject);

                menu.ShowAsContext();
            }

            var value = EditorGUI.InspectorTitlebar(titlebarRect, foldout, subObject, true);

            // Draw the button, this is only for its visual appearance
            if (!isMissing)
                GUI.Button(buttonRect, EditorGUIUtility.IconContent("d__Popup"), "IconButton");

            return value;
        }

        Editor GetOrCreateEditor(Object t)
        {
            for (var n = 0; n < m_Editors.Count; ++n)
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

        class PopupMenuItem
        {
            public string title;
            public System.Type type;
        }

        protected void ShowAddSubObjectPopup()
        {
            var typeList = new List<System.Type>();
            var itemList = new List<PopupMenuItem>();

            GetScriptableObjectTypes(typeList);

            foreach (var t in targets)
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
            foreach (var item in itemList)
            {
                menu.AddItem(new GUIContent(item.title), false, OnClick, item.type);
            }
            menu.ShowAsContext();

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

            Undo.IncrementCurrentGroup();
            var subObject = ScriptableObject.CreateInstance(type);
            subObject.name = type.Name;
            Undo.RegisterCreatedObjectUndo(subObject, "Create");

            var serObj = new SerializedObject(subObject);
            var serProp = serObj.FindProperty("m_ScriptableObjectContainer");
            if (serProp != null)
                serProp.objectReferenceValue = parent;
            serObj.ApplyModifiedPropertiesWithoutUndo();

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
    }
}