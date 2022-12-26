namespace Bookmark4Unity.Editor
{
    using System.Collections.Generic;
    using Bookmark4Unity.Guid;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class SceneObjectBookmarkGroup
    {
        public VisualElement Element { get; private set; }
        public Foldout Root { get; private set; }
        public List<GuidReference> Data { get; private set; }
        public ListView DataListView { get; private set; }
        public const int ItemHeight = 15;
        public bool IsEmpty => Data.Count < 1;
        public Dictionary<int, EventCallback<ClickEvent>> pingActions = new();
        public Dictionary<int, EventCallback<ClickEvent>> focusActions = new();
        public Dictionary<int, EventCallback<ClickEvent>> delActions = new();
        public Dictionary<int, EventCallback<PointerLeaveEvent>> dragActions = new();


        public SceneObjectBookmarkGroup(string groupName, Color borderColor, List<GuidReference> data, VisualTreeAsset groupAsset, VisualTreeAsset btnAsset)
        {
            Element = groupAsset.Instantiate();
            Root = Element.Q<Foldout>("Root");
            Root.style.borderTopColor = borderColor;
            Root.style.borderBottomColor = borderColor;
            Root.style.borderLeftColor = borderColor;
            Root.style.borderRightColor = borderColor;
            Root.text = groupName;
            Data = data;

            DataListView = new(Data, ItemHeight, () =>
            {
                return btnAsset.Instantiate();
            },
            (item, i) =>
            {
                var icon = item.Q<Button>("Icon");
                var btn = item.Q<Button>("Btn");
                var focus = item.Q<Button>("Focus");
                var del = item.Q<Button>("Del");
                var data = Data[i];
                var index = i; // save value for lambda functions
                icon.style.backgroundImage = Background.FromTexture2D(
                    data.gameObject is null ?
                    EditorGUIUtility.IconContent("console.warnicon").image as Texture2D :
                    PrefabUtility.GetIconForGameObject(data.gameObject));
                focus.style.backgroundImage = Background.FromTexture2D(SceneViewBookmarkManager.SceneViewBookmarkIcon);
                del.style.backgroundImage = Background.FromTexture2D(SceneViewBookmarkManager.SceneViewEmptyIcon);
                btn.text = data.CachedName;

                // ping
                if (pingActions.ContainsKey(i)) btn.UnregisterCallback<ClickEvent>(pingActions[i]);
                pingActions[i] = evt => Ping(index);
                btn.RegisterCallback<ClickEvent>(pingActions[i]);
                btn.tooltip = $"Select \"{data.CachedName}\"";

                // focus
                if (focusActions.ContainsKey(i)) focus.UnregisterCallback<ClickEvent>(focusActions[i]);
                focusActions[i] = evt => Focus(index);
                focus.RegisterCallback<ClickEvent>(focusActions[i]);
                focus.tooltip = $"Focus on \"{data.CachedName}\"";

                // del
                if (delActions.ContainsKey(i)) del.UnregisterCallback<ClickEvent>(delActions[i]);
                delActions[i] = evt => Remove(index);
                del.RegisterCallback<ClickEvent>(delActions[i]);
                del.tooltip = $"Unpin \"{data.CachedName}\"";

                // drag
                if (dragActions.ContainsKey(i)) btn.UnregisterCallback<PointerLeaveEvent>(dragActions[i]);
                dragActions[i] = evt => OnDrag(index);
                btn.RegisterCallback<PointerLeaveEvent>(dragActions[i]);
            })
            {
                reorderable = true,
                showBorder = false
            };

            DataListView.style.flexGrow = 1f; // Fills the window
            Root.Add(DataListView);
        }

        public bool Add(GuidReference data)
        {
            if (Data.Contains(data)) return false;
            Data.Add(data);
            DataListView.Rebuild();
            Element.RemoveFromClassList(Bookmark4UnityWindow.HiddenContentClassName);
            return true;
        }

        public void Ping(int index)
        {
            if (Data[index].gameObject != null)
            {
                Selection.activeGameObject = Data[index].gameObject;
            }
            else
            {
                if (EditorUtility.DisplayDialog(Data[index].CachedName, "Selected game object dows not exist on current scene, remove it from list?", "Yes", "No"))
                {
                    Remove(index);
                }
            }
        }

        public void Focus(int index)
        {
            if (Data[index].gameObject != null)
            {
                Selection.activeGameObject = Data[index].gameObject;
                SceneView.lastActiveSceneView.FrameSelected();
            }
            else
            {
                if (EditorUtility.DisplayDialog(Data[index].CachedName, "Selected game object dows not exist on current scene, remove it from list?", "Yes", "No"))
                {
                    Remove(index);
                }
            }
        }

        public void Remove(int index)
        {
            if (Data[index].gameObject is not null)
            {
                UnityEngine.Object.DestroyImmediate(Data[index].gameObject.GetComponent<GuidComponent>());
            }

            Data.RemoveAt(index);
            DataListView.Rebuild();
            if (IsEmpty) Element.AddToClassList(Bookmark4UnityWindow.HiddenContentClassName);
            Bookmark4UnityWindow.UpdateSavedData();
        }

        public void RemoveAll()
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].gameObject is not null)
                {
                    UnityEngine.Object.DestroyImmediate(Data[i].gameObject.GetComponent<GuidComponent>());
                }
            }

            Data.Clear();
            DataListView.Rebuild();
            Element.AddToClassList(Bookmark4UnityWindow.HiddenContentClassName);
            Bookmark4UnityWindow.UpdateSavedData();
        }

        private void OnDrag(int index)
        {
            if (Event.current.type != EventType.MouseDrag || Data[index].gameObject is null) return;
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new UnityEngine.Object[] { Data[index].gameObject };
            DragAndDrop.StartDrag(Data[index].CachedName);
            Event.current.Use();
        }

        public void SortDesc()
        {
            Data.Sort((a, b) => a.CachedName.CompareTo(b.CachedName));
            DataListView.RefreshItems();
        }

        public void SortAsc()
        {
            Data.Sort((a, b) => b.CachedName.CompareTo(a.CachedName));
            DataListView.RefreshItems();
        }
    }
}