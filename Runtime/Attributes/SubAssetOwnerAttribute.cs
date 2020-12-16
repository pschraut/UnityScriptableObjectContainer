//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
using System;

namespace Oddworm.Framework
{
    /// <summary>
    /// Use the [<see cref="SubAssetOwnerAttribute"/>] on a field in your sub-asset to let the editor
    /// automatically assign a reference to its owner, the <see cref="ScriptableObjectContainer"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// using UnityEngine;
    /// using Oddworm.Framework;
    ///
    /// [CreateSubAssetMenu(menuName = "Example")]
    /// public class Example : ScriptableObject
    /// {
    ///     [SubAssetOwner]
    ///     [SerializeField] ScriptableObjectContainer m_Container;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class SubAssetOwnerAttribute : Attribute
    {
    }
}
