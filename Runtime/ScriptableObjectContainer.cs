//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oddworm.Framework
{
    [CreateAssetMenu(menuName = "ScriptableObject Container", order = 310)]
    public class ScriptableObjectContainer : ScriptableObject
    {
        public sealed class FilterTypesMethodAttribute : System.Attribute
        { }

        [HideInInspector]
        [SerializeField] ScriptableObject[] m_SubObjects = new ScriptableObject[0];

        public T GetSubObject<T>() where T : ScriptableObject
        {
            for (var n=0; n< m_SubObjects.Length; ++n)
            {
                var subobj = m_SubObjects[n] as T;
                if (subobj != null)
                    return subobj;
            }
            return null;
        }

        public void GetSubObjects<T>(List<T> result) where T : ScriptableObject
        {
            for (var n = 0; n < m_SubObjects.Length; ++n)
            {
                var subobj = m_SubObjects[n] as T;
                if (subobj != null)
                    result.Add(subobj);
            }
        }

#if UNITY_EDITOR
        void EditorBake()
        {
            var objs = new List<ScriptableObject>();

            // load all objects in the container asset
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            foreach(var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (!(obj is ScriptableObject))
                    continue;
                if (obj is ScriptableObjectContainer)
                    continue;

                objs.Add(obj as ScriptableObject);
            }

            var temp = new List<ScriptableObject>(m_SubObjects);

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
            m_SubObjects = temp.ToArray();
        }
#endif

#if UNITY_EDITOR
        public static class Editor
        {
            public static void Bake(ScriptableObjectContainer container)
            {
                container.EditorBake();
            }
        }
#endif
    }
}
