//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
using System;

namespace Oddworm.Framework
{
    /// <summary>
    /// Prevents ScriptableObjects of same type (or subtype) to be added more than once to a ScriptableObjectContainer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DisallowMultipleSubAssetAttribute : Attribute
    {
    }
}
