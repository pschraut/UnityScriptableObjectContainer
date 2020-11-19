//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oddworm.Framework
{
    public sealed class ScriptableObjectContainerTypeFilterAttribute : System.Attribute
    { }

    [CreateAssetMenu(menuName = "ScriptableObject Container", order = 310)]
    public class ScriptableObjectContainer : ScriptableObject
    {
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
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var objs = new List<Object>(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath));
            for (var n= objs.Count-1; n>=0; --n)
            {
                var obj = objs[n];

                if (obj is ScriptableObjectContainer)
                {
                    objs.RemoveAt(n);
                    continue;
                }

                if (!(obj is ScriptableObject))
                {
                    objs.RemoveAt(n);
                    continue;
                }
            }

            m_SubObjects = new ScriptableObject[objs.Count];
            for(var n=0; n< objs.Count; ++n)
            {
                m_SubObjects[n] = objs[n] as ScriptableObject;
            }
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
