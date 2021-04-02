//
// ScriptableObject Container for Unity. Copyright (c) 2020-2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityScriptableObjectContainer
//
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable IDE0051 // Remove unused private members
using UnityEngine;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using Oddworm.Framework;

namespace Oddworm.EditorFramework.Tests.ScriptableObjectContainerTest
{

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
            Assert.IsNotNull(FindContainerAsset("Test_FruitContainer_1"));
            Assert.IsNotNull(FindContainerAsset("Test_FruitContainer_2"));
            Assert.IsNotNull(FindContainerAsset("Test_FruitContainer_3"));
        }

        [Test]
        public void Test_FruitContainer_1_GetObject()
        {
            var container = FindContainerAsset("Test_FruitContainer_1");
            Assert.IsNotNull(container.GetObject(typeof(Fruit)));
            Assert.IsNotNull(container.GetObject<Fruit>());

            Assert.IsNotNull(container.GetObject(typeof(ScriptableObject)));
            Assert.IsNotNull(container.GetObject<ScriptableObject>());
        }

        [Test]
        public void Test_FruitContainer_1_GetObjects()
        {
            var container = FindContainerAsset("Test_FruitContainer_1");
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
        public void Test_FruitContainer_2_GetObject()
        {
            var container = FindContainerAsset("Test_FruitContainer_2");
            Assert.IsNotNull(container.GetObject(typeof(Fruit)));
            Assert.IsNotNull(container.GetObject<Fruit>());

            Assert.IsNotNull(container.GetObject(typeof(ScriptableObject)));
            Assert.IsNotNull(container.GetObject<ScriptableObject>());
        }

        [Test]
        public void Test_FruitContainer_2_GetObjects()
        {
            var container = FindContainerAsset("Test_FruitContainer_2");

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
        public void Test_FruitContainer_3_GetObjects()
        {
            var container = FindContainerAsset("Test_FruitContainer_3");

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
}
