//
// ScriptableObject Container for Unity. Copyright (c) 2020-2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles
using System;

namespace Oddworm.Framework
{
    /// <summary>
    /// The <see cref="FilterSubAssetTypesMethodAttribute"/> to allow to filter what sub-asset types a container supports.
    /// </summary>
    /// <example>
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    ///
    /// public class MyContainer : Oddworm.Framework.ScriptableObjectContainer
    /// {
    ///     [Oddworm.Framework.FilterSubAssetTypesMethod]
    ///     static void MyFilterSubAssetTypesMethod(List<Type> types) // Can be static or non-static
    ///     {
    ///         for (var n = types.Count - 1; n >= 0; --n)
    ///         {
    ///             if (!types[n].IsSubclassOf(typeof(MySubAssetBaseClass)))
    ///                 types.RemoveAt(n);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class FilterSubAssetTypesMethodAttribute : Attribute
    {
        /// <summary>
        /// Override the order in which methods with this attribute are called.
        /// Smaller order is called first. The default order is 0.
        /// </summary>
        public int order
        {
            get;
            set;
        }
    }
}
