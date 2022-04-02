//
// ScriptableObject Container for Unity. Copyright (c) 2020-2022 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles
using UnityEngine;
using UnityEditor;

namespace Oddworm.EditorFramework
{
    internal class RenameDialog : EditorWindow
    {
        string m_NewText;
        Object m_ObjectToRename;
        bool m_FirstUpdate;

        public void Show(Object objectToRename)
        {
            m_ObjectToRename = objectToRename;
            m_NewText = m_ObjectToRename.name;
            m_FirstUpdate = true;

            ShowModalUtility();
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Rename");
            minSize = new Vector2(300, 120);
            maxSize = new Vector2(300, 120);
        }

        void OnGUI()
        {
            var cancel = false;
            var rename = false;

            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
                cancel = true;

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
                rename = !string.IsNullOrEmpty(m_NewText);

            EditorGUILayout.Space();
            GUI.SetNextControlName("RenameField");
            m_NewText = EditorGUILayout.TextField(m_NewText);
            if (m_FirstUpdate && e.type == EventType.Layout)
            {
                m_FirstUpdate = false;
                GUI.FocusControl("RenameField");
                EditorGUI.FocusTextInControl("RenameField");
            }
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Press Return to rename or Escape to cancel.", MessageType.Info, true);
            EditorGUILayout.Space();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_NewText));
            if (GUILayout.Button(new GUIContent("Rename", "Return")) || rename)
            {
                if (m_ObjectToRename != null && m_ObjectToRename.name != m_NewText)
                {
                    Undo.RecordObject(m_ObjectToRename, "Rename");
                    m_ObjectToRename.name = m_NewText;
                }
                Close();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(new GUIContent("Cancel", "Escape")) || cancel)
                Close();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
    }
}
