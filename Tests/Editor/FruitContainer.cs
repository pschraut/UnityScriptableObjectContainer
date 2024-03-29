﻿//
// ScriptableObject Container for Unity. Copyright (c) 2020-2023 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable IDE0051 // Remove unused private members
using UnityEngine;
using Oddworm.Framework;

namespace Oddworm.EditorFramework.Tests.ScriptableObjectContainerTest
{
#if SCRIPTABLEOBJECTCONTAINER_ENABLE_TESTS
    [CreateAssetMenu(menuName = "ScriptableObjectContainer Tests/Fruit Container")]
#endif
    internal class FruitContainer : ScriptableObjectContainer
    {
    }
}
