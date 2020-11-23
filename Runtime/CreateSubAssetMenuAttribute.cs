//
// ScriptableObject Container for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oddworm.Framework
{
    /// <summary>
    /// Mark a ScriptableObject-derived type to be automatically listed in the "Add Object" submenu,
    /// so that instances of the type can be easily created and added to the particular ScriptableObjectContainer.
    /// </summary>
    public sealed class CreateSubAssetMenuAttribute : System.Attribute
    {
        /// <summary>
        /// The display name for this type shown in the "Add Object" menu.
        /// </summary>
        /// <remarks>
        /// As with other menu item code, use a forward-slash ("/") path separator to group items into submenus.
        /// For example, specifying a menuName of "Gameplay/Objective" will cause the menu item for a type to be
        /// 'Objective' inside a 'Gameplay' submenu of the "Add Object" submenu.
        /// </remarks>
        public string menuName
        {
            get;
            set;
        }
    }
}
