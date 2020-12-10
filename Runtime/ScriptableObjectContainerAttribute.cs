//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
using System;

namespace Oddworm.Framework
{
    /// <summary>
    /// Automatically assign a reference to the ScriptableObjectContainer that owns this subasset.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ScriptableObjectContainerAttribute : Attribute
    {
    }
}
