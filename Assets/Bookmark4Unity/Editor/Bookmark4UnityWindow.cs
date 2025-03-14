namespace Bookmark4Unity.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using Bookmark4Unity.Guid;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;
    using Random = UnityEngine.Random;
    using Scene = UnityEngine.SceneManagement.Scene;

    public class Bookmark4UnityWindow : EditorWindow
    {
        [System.Serializable]
        public class DataWrapper
        {
            public List<GuidData> references = new();
            public List<AssetData> assets = new();
            public List<string> closedAssetTypes = new();
            public bool isAssetTabActive;
        }

        public const string Name = "Bookmark4Unity";
        public static string Prefix => Application.productName + "_BOOKMARK4UNITY_";
        public static string PinnedKey => Prefix + "pinned";
    }
}
