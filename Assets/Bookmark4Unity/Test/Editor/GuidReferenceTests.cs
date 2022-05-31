﻿namespace Bookmark4Unity.Test.Editor
{
    using System.Collections;
    using Bookmark4Unity.Guid;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;

    public class GuidReferenceTests
    {
        // Tests - make a new GUID
        // duplicate it
        // make it a prefab
        // delete it
        // reference it
        // dereference it

        string prefabPath;
        GuidComponent guidBase;
        GameObject prefab;
        GuidComponent guidPrefab;

        [OneTimeSetUp]
        public void Setup()
        {
            prefabPath = "Assets/TemporaryTestGuid.prefab";

            guidBase = CreateNewGuid();
            prefab = PrefabUtility.SaveAsPrefabAsset(guidBase.gameObject, prefabPath);

            guidPrefab = prefab.GetComponent<GuidComponent>();
        }

        public GuidComponent CreateNewGuid()
        {
            var newGO = new GameObject("GuidTestGO");
            return newGO.AddComponent<GuidComponent>();
        }

        [UnityTest]
        public IEnumerator GuidCreation()
        {
            GuidComponent guid1 = guidBase;
            GuidComponent guid2 = CreateNewGuid();

            Assert.AreNotEqual(guid1.GetGuid(), guid2.GetGuid());

            yield return null;
        }

        [UnityTest]
        public IEnumerator GuidDuplication()
        {
            LogAssert.Expect(LogType.Warning, "Guid Collision Detected while creating GuidTestGO(Clone).\nAssigning new Guid.");

            GuidComponent clone = GameObject.Instantiate<GuidComponent>(guidBase);

            Assert.AreNotEqual(guidBase.GetGuid(), clone.GetGuid());

            yield return null;
        }

        [UnityTest]
        public IEnumerator GuidPrefab()
        {
            Assert.AreNotEqual(guidBase.GetGuid(), guidPrefab.GetGuid());
            Assert.AreEqual(guidPrefab.GetGuid(), System.Guid.Empty);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GuidPrefabInstance()
        {
            var instance = GameObject.Instantiate<GuidComponent>(guidPrefab);
            Assert.AreNotEqual(guidBase.GetGuid(), instance.GetGuid());
            Assert.AreNotEqual(instance.GetGuid(), guidPrefab.GetGuid());

            yield return null;
        }

        [UnityTest]
        public IEnumerator GuidValidReference()
        {
            var reference = new GuidReference(guidBase);
            Assert.AreEqual(reference.gameObject, guidBase.gameObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GuidInvalidReference()
        {
            var newGuid = CreateNewGuid();
            var reference = new GuidReference(newGuid);
            Object.DestroyImmediate(newGuid);

            Assert.IsNull(reference.gameObject);

            yield return null;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
    }
}
