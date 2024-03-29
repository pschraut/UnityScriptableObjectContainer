﻿//
// ScriptableObject Container for Unity. Copyright (c) 2020-2023 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0062 // Make local function 'static'
#pragma warning disable IDE0074 // Use compound assignment
using UnityEngine;
using NUnit.Framework;
using UnityEditor;
using Oddworm.Framework;

namespace Oddworm.EditorFramework.Tests.ScriptableObjectContainerTest
{
#if SCRIPTABLEOBJECTCONTAINER_ENABLE_TESTS
    class ScriptableObjectContainerTests
    {
        const string k_PackageTestsFolderAssetGUID = "f07a2a99328f0c74fa791bb5d36d1960";

        [Test]
        public void Test_FindContainerAssets()
        {
            var folder = AssetDatabase.GUIDToAssetPath(k_PackageTestsFolderAssetGUID);
            var assets = FindAllPipelineAssets(new[] { folder });
            Assert.IsTrue(assets.Length > 0);
        }

        [Test]
        public void Test_FindContainerAsset()
        {
            Assert.IsNotNull(FindContainerAsset("Test_001"));
            Assert.IsNotNull(FindContainerAsset("Test_002"));
            Assert.IsNotNull(FindContainerAsset("Test_003"));
            Assert.IsNotNull(FindContainerAsset("Test_004"));
            Assert.IsNotNull(FindContainerAsset("Test_005"));
            Assert.IsNotNull(FindContainerAsset("Test_006"));
            Assert.IsNotNull(FindContainerAsset("Test_007"));
            Assert.IsNotNull(FindContainerAsset("Test_008"));
        }

        [Test]
        public void Test_001_GetObject()
        {
            var container = FindContainerAsset("Test_001");
            Assert.IsNotNull(container.GetObject(typeof(Fruit)));
            Assert.IsNotNull(container.GetObject<Fruit>());

            Assert.IsNotNull(container.GetObject(typeof(ScriptableObject)));
            Assert.IsNotNull(container.GetObject<ScriptableObject>());
        }

        [Test]
        public void Test_001_GetObjects()
        {
            var container = FindContainerAsset("Test_001");
            Assert.IsNotNull(container.GetObjects(typeof(Fruit)));
            Assert.IsNotNull(container.GetObjects<Fruit>());

            Assert.AreEqual(1, container.GetObjects(typeof(Fruit)).Length);
            Assert.AreEqual(1, container.GetObjects<Fruit>().Length);


            Assert.IsNotNull(container.GetObjects(typeof(ScriptableObject)));
            Assert.IsNotNull(container.GetObjects<ScriptableObject>());

            Assert.AreEqual(1, container.GetObjects(typeof(ScriptableObject)).Length);
            Assert.AreEqual(1, container.GetObjects<ScriptableObject>().Length);
        }

        [Test]
        public void Test_002_GetObject()
        {
            var container = FindContainerAsset("Test_002");
            Assert.IsNotNull(container.GetObject(typeof(Fruit)));
            Assert.IsNotNull(container.GetObject<Fruit>());

            Assert.IsNotNull(container.GetObject(typeof(ScriptableObject)));
            Assert.IsNotNull(container.GetObject<ScriptableObject>());
        }

        [Test]
        public void Test_002_GetObjects()
        {
            var container = FindContainerAsset("Test_002");

            Test(container.GetObjects(typeof(Fruit)));
            Test(container.GetObjects<Fruit>());

            Test(container.GetObjects(typeof(ScriptableObject)));
            Test(container.GetObjects<ScriptableObject>());

            void Test<T>(System.Collections.Generic.IEnumerable<T> objs)
            {
                Assert.IsNotNull(objs);
                if (objs == null)
                    return;

                // Tests if "GetObjects" retrieves objects in the expected order
                var n = 0;
                foreach(var o in objs)
                {
                    var obj = o as Fruit;
                    Assert.NotNull(obj);
                    if (obj == null)
                        return;

                    Assert.AreEqual(n, obj.number);
                    n++;
                }

                Assert.AreEqual(3, n);
            }
        }

        [Test]
        public void Test_003_GetObjects()
        {
            var container = FindContainerAsset("Test_003");

            Test(container.GetObjects(typeof(Fruit)), 3, 0);
            Test(container.GetObjects<Fruit>(), 3, 0);

            Test(container.GetObjects(typeof(Meat)), 0, 3);
            Test(container.GetObjects<Meat>(), 0, 3);

            Test(container.GetObjects(typeof(ScriptableObject)), 3, 3);
            Test(container.GetObjects<ScriptableObject>(), 3, 3);

            void Test<T>(System.Collections.Generic.IEnumerable<T> objs, int expectedFruitNumber, int expectedMeatNumber)
            {
                Assert.IsNotNull(objs);
                if (objs == null)
                    return;

                // Tests if "GetObjects" retrieves objects in the expected order
                var fruitNumber = 0;
                var meatNumber = 0;
                foreach (var o in objs)
                {
                    var fruit = o as Fruit;
                    var meat = o as Meat;
                    Assert.IsTrue(meat != null || fruit != null);

                    if (fruit != null)
                    {
                        Assert.AreEqual(fruitNumber, fruit.number);
                        fruitNumber++;
                    }

                    if (meat != null)
                    {
                        Assert.AreEqual(meatNumber, meat.number);
                        meatNumber++;
                    }
                }

                Assert.AreEqual(expectedFruitNumber, fruitNumber);
                Assert.AreEqual(expectedMeatNumber, meatNumber);
            }
        }

        [Test]
        public void Test_004_CanAddObjectOfType()
        {
            var container = FindContainerAsset("Test_004");

            var canAdd = EditorScriptableObjectContainerUtility.CanAddObjectOfType(container, typeof(SingleFruit), false);
            Assert.IsFalse(canAdd);
        }

        [Test]
        public void Test_005_CanAddObjectOfType()
        {
            var container = FindContainerAsset("Test_005");

            var canAdd = EditorScriptableObjectContainerUtility.CanAddObjectOfType(container, typeof(SingleFruit), false);
            Assert.IsTrue(canAdd);
        }

        [Test]
        public void Test_006()
        {
            var container = FindContainerAsset("Test_006");
            Assert.NotNull(container);

            var subObj = container.GetObject<SubAssetWithToggle>();
            Assert.NotNull(subObj);

            var fields = EditorScriptableObjectContainerUtility.GetObjectToggleFields(subObj);
            Assert.NotNull(fields);
            Assert.IsTrue(fields.Count == 1);

            var value = EditorScriptableObjectContainerUtility.GetObjectToggleValue(subObj, fields);
            Assert.IsTrue(value);
        }

        [Test]
        public void Test_007()
        {
            var container = FindContainerAsset("Test_007");
            Assert.NotNull(container);

            var subObj = container.GetObject<SubAssetWithToggle>();
            Assert.NotNull(subObj);

            var fields = EditorScriptableObjectContainerUtility.GetObjectToggleFields(subObj);
            Assert.NotNull(fields);
            Assert.IsTrue(fields.Count == 1);

            var value = EditorScriptableObjectContainerUtility.GetObjectToggleValue(subObj, fields);
            Assert.IsFalse(value);
        }

        [Test]
        public void Test_008_SetObjectToggleValue()
        {
            var container = FindContainerAsset("Test_008");
            Assert.NotNull(container);

            var subObj = container.GetObject<SubAssetWithToggle>();
            Assert.NotNull(subObj);

            var fields = EditorScriptableObjectContainerUtility.GetObjectToggleFields(subObj);
            Assert.NotNull(fields);
            Assert.IsTrue(fields.Count == 1);

            Assert.IsFalse(EditorScriptableObjectContainerUtility.GetObjectToggleValue(subObj, fields));
            try
            {
                EditorScriptableObjectContainerUtility.SetObjectToggleValue(subObj, fields, true);
                Assert.IsTrue(EditorScriptableObjectContainerUtility.GetObjectToggleValue(subObj, fields));
            }
            finally
            {
                EditorScriptableObjectContainerUtility.SetObjectToggleValue(subObj, fields, false);
            }
        }

        ScriptableObjectContainer FindContainerAsset(string assetName)
        {
            var folder = AssetDatabase.GUIDToAssetPath(k_PackageTestsFolderAssetGUID);
            return FindPipelineAsset(assetName, new[] { folder });
        }

        /// <summary>
        /// Finds the pipeline asset with the specified name.
        /// </summary>
        /// <param name="pipelineAssetName">The pipeline asset filename</param>
        /// <param name="searchInFolders">Search for pipeline assets in the specified folders only. Pass null to search entire Assets directory.</param>
        /// <returns>The asset on success, null otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="pipelineAssetName"/> is null.</exception>
        static ScriptableObjectContainer FindPipelineAsset(string pipelineAssetName, string[] searchInFolders = null)
        {
            if (string.IsNullOrEmpty(pipelineAssetName))
                return null;

            foreach (var asset in FindAllPipelineAssets(searchInFolders))
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);
                var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

                if (string.Equals(assetName, pipelineAssetName, System.StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<ScriptableObjectContainer>(assetPath);
            }

            return null;
        }

        /// <summary>
        /// Finds all pipeline asset in the project.
        /// </summary>
        /// <param name="searchInFolders">Search for pipeline assets in the specified folders only. Pass null to search entire Assets directory.</param>
        /// <returns>The pipeline assets that were found in the project or an empty array if no pipeline asset was found.</returns>
        static ScriptableObjectContainer[] FindAllPipelineAssets(string[] searchInFolders = null)
        {
            if (searchInFolders == null)
                searchInFolders = new[] { "Assets" };

            var guids = AssetDatabase.FindAssets($"t: {nameof(ScriptableObjectContainer)}", searchInFolders);
            var pipelineAssets = new ScriptableObjectContainer[guids.Length];

            for (var n = 0; n < guids.Length; ++n)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[n]);
                var pipelineAsset = AssetDatabase.LoadAssetAtPath<ScriptableObjectContainer>(assetPath);

                pipelineAssets[n] = pipelineAsset;
            }

            return pipelineAssets;
        }
    }
#endif // SCRIPTABLEOBJECTCONTAINER_ENABLE_TESTS
}
