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
    /// Mark a <see cref="UnityEngine.ScriptableObject"/>-derived type to be automatically listed in the "Add Object" submenu,
    /// so that instances of the type can be easily created and added to the particular <see cref="ScriptableObjectContainer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CreateSubAssetMenuAttribute : Attribute
    {
        /// <summary>
        /// The display name for this type shown in the "Add Object" menu.
        /// If <c>menuName</c> is empty, it uses the type name instead.
        /// </summary>
        /// <remarks>
        /// As with other menu item code, use a forward-slash ("/") to group items into submenus.
        /// For example, specifying a <c>menuName</c> of "Gameplay/Objective" will cause the menu item for a type to be
        /// 'Objective' inside a 'Gameplay' submenu of the "Add Object" menu.
        /// </remarks>
        public string menuName
        {
            get;
            set;
        }
    }
}
