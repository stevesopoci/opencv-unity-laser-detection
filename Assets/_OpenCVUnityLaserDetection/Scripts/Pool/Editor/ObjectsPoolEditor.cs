using System.Collections.Generic;
using System.Linq;
using Assets.BulletDecals.Scripts.Bullets;
using Assets.BulletDecals.Scripts.Extensions;
using UnityEditor;
using UnityEngine;

namespace Assets.BulletDecals.Scripts.Pool.Editor
{
    [CustomEditor(typeof(ObjectsPool))]
    public class ObjectsPoolEditor : UnityEditor.Editor 
    {
        private enum SettingFoldoutButton
        {
            None,
            ShiftUp,
            ShiftDown,
            Remove,
            Add
        }

        public override void OnInspectorGUI()
        {
            var pool = (ObjectsPool) target;

            if (pool.PoolDataList == null)
            {
                pool.PoolDataList = new List<MutableKeyValuePair>(); 
            }            

            pool.DefaultCountForNewObject = EditorGUILayout.IntField("Default Count", pool.DefaultCountForNewObject);
            //pool.Prewarm = EditorGUILayout.Toggle("Prewarm", pool.Prewarm);

            var settings = pool.GetComponent<BulletMarksSettings>();
            if (settings != null)
            {
                if (GUILayout.Button("Grab prefabs from " + settings.GetType().Name))
                {
                    pool.PoolDataList.Clear();

                    AddPrefabsFromSettings(settings);
                }
            }

            string addButtonTitle;
            if (pool.PoolDataList.Count == 0)
            {
                addButtonTitle = "Add new object";
            }
            else
            {
                DrawDataList();
                addButtonTitle = "Add next object";
            }

            if (GUILayout.Button(addButtonTitle))
            {
                pool.PoolDataList.Add(new MutableKeyValuePair(null, pool.DefaultCountForNewObject));
            }

            HandleDragAndDrop();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }            
        }

        private void AddPrefabsFromSettings(BulletMarksSettings settings)
        {
            var pool = (ObjectsPool) target;
            foreach (var prefabsLayerData in settings.BulletMarkDatas)
            {
                foreach (var prefab in prefabsLayerData.Prefabs)
                {
                    if (prefab != null)
                    {
                        var prefabAlreadyAdded = pool.PoolDataList.FirstOrDefault(o => o.Key == prefab) != null;
                        if (!prefabAlreadyAdded)
                        {
                            pool.PoolDataList.Add(new MutableKeyValuePair(prefab, pool.DefaultCountForNewObject));
                        }
                    }
                }
            }
        }

        public void HandleDragAndDrop()
        {
            var pool = (ObjectsPool)target;
            
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:                    
                    if (!GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        var count = DragAndDrop.objectReferences.Length;
                        for (var i = 0; i < count; i++)
                        {
                            var gameObject = DragAndDrop.objectReferences[i] as GameObject;
                            if (gameObject != null)
                            {
                                var type = PrefabUtility.GetPrefabType(gameObject);
                                if (type == PrefabType.None)
                                {
                                    continue;                                    
                                }

                                pool.PoolDataList.Add(new MutableKeyValuePair(gameObject.transform, pool.DefaultCountForNewObject));
                            }
                        }                        
                    }
                    break;
            }
        }

        private void DrawDataList()
        {
            var pool = (ObjectsPool)target;
            int dataCount = pool.PoolDataList.Count;

            var foldoutButton = SettingFoldoutButton.None;
            int actionIndex = -1;            
            
            EditorGUILayout.LabelField("Prefab", "Count");

            for (int i = 0; i < dataCount; i++)
            {
                var keyValuePair = pool.PoolDataList[i];

                EditorGUILayout.BeginHorizontal();                                

                keyValuePair.Key = EditorGUILayout.ObjectField(keyValuePair.Key, typeof (Transform), false) as Transform;
                keyValuePair.Value = EditorGUILayout.IntField(keyValuePair.Value);

                SettingFoldoutButton button = AddSettingFoldoutButtons();
                if (button != SettingFoldoutButton.None)
                {
                    actionIndex = i;
                    foldoutButton = button;
                }

                EditorGUILayout.EndHorizontal();                
            }            

            if (foldoutButton != SettingFoldoutButton.None)
            {
                HandleFoldoutButtons(foldoutButton, actionIndex, pool.PoolDataList);
            }
        }

        private SettingFoldoutButton AddSettingFoldoutButtons()
        {
            bool addPressed = GUILayout.Button(new GUIContent("+", "Click to clone"), EditorStyles.miniButton);

            bool shiftUpPressed = GUILayout.Button(new GUIContent("\u25B2", "Click to shift up"), EditorStyles.miniButton);

            bool shiftDownPressed = GUILayout.Button(new GUIContent("\u25BC", "Click to shift down"),
                EditorStyles.miniButton);

            bool removePressed = GUILayout.Button(new GUIContent("x", "Click to remove"), EditorStyles.miniButton);

            if (addPressed) return SettingFoldoutButton.Add;
            if (shiftUpPressed) return SettingFoldoutButton.ShiftUp;
            if (shiftDownPressed) return SettingFoldoutButton.ShiftDown;
            if (removePressed) return SettingFoldoutButton.Remove;

            return SettingFoldoutButton.None;
        }

        private void HandleFoldoutButtons(SettingFoldoutButton foldoutButton, int actionIndex, List<MutableKeyValuePair> list)
        {
            if (actionIndex != -1)
            {
                switch (foldoutButton)
                {
                    case SettingFoldoutButton.Add:
                        list.Insert(actionIndex + 1, list[actionIndex].Clone());
                        break;
                    case SettingFoldoutButton.Remove:
                        list.RemoveAt(actionIndex);
                        break;
                    case SettingFoldoutButton.ShiftDown:
                        list.ShiftItemDown(actionIndex);
                        break;
                    case SettingFoldoutButton.ShiftUp:
                        list.ShiftItemUp(actionIndex);
                        break;
                }
            }
        }

        
    }
}
