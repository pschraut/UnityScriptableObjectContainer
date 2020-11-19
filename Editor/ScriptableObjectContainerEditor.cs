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
    [CustomEditor(typeof(ScriptableObjectContainer))]
    public class ScriptableObjectContainerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("ScriptableObjectContainerEditor", MessageType.Info);
        }
    }
}
