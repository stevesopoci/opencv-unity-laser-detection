using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.BulletDecals.Scripts.Bullets.Editor
{
    [CustomEditor(typeof (BulletMarksSettings))]
    public class BulletMarksSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (BulletMarksSettings) target;
            if (settings.BulletMarkDatas == null || settings.BulletMarkDatas.Count == 0)
            {
                settings.BulletMarkDatas = new List<PrefabsLayerData> {new PrefabsLayerData()};
            }
            if (settings.ImpactEffectDatas == null || settings.ImpactEffectDatas.Count == 0)
            {
                settings.ImpactEffectDatas = new List<PrefabsLayerData> {new PrefabsLayerData()};
            }
            
            EditorGUILayout.LabelField("BULLET MARKS", EditorStyles.boldLabel);

            settings.EnableStaticObjects =
                EditorGUILayout.Toggle(
                    new GUIContent("Static Batching", "Check this if Static Batching is enabled in Player settings"),
                    settings.EnableStaticObjects);

            DrawDataList(settings.BulletMarkDatas);

            EditorGUILayout.LabelField("IMPACT EFFECTS", EditorStyles.boldLabel);
            DrawDataList(settings.ImpactEffectDatas);            

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawDataList(List<PrefabsLayerData> dataList)
        {            
            int dataCount = dataList.Count;

            var foldoutButton = SettingFoldoutButton.None;
            int actionIndex = -1;

            for (int i = 0; i < dataCount; i++)
            {
                PrefabsLayerData data = dataList[i];

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                           
                EditorGUILayout.BeginHorizontal();

                data.Tag = EditorGUILayout.TagField("Tag", data.Tag);

                //data.Layer.value = EditorGUILayout.MaskField("Layer", data.Layer.value, data.Layer.GetLayerNames());

                var enableRemoveButton = (data.Prefabs != null && data.Prefabs.Count > 0 && data.Prefabs[0] != null) ||
                                         dataCount > 1;
                
                SettingFoldoutButton button = AddSettingFoldoutButtons(enableRemoveButton);
                if (button != SettingFoldoutButton.None)
                {
                    actionIndex = i;
                    foldoutButton = button;
                }

                EditorGUILayout.EndHorizontal();

                if (data.Prefabs == null || data.Prefabs.Count == 0)
                {
                    data.Prefabs = new List<Transform> {null};
                }

                EditorGUILayout.Space();
                DrawPrefabs(data);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (foldoutButton != SettingFoldoutButton.None)
            {
                HandleFoldoutButtons(foldoutButton, actionIndex, dataList);
            }
        }

        private void DrawPrefabs(PrefabsLayerData data)
        {
            var foldoutButton = SettingFoldoutButton.None;
            int actionIndex = -1;
            int count = data.Prefabs.Count;

            EditorGUIUtility.labelWidth = 50;

            for (int j = 0; j < count; j++)
            {
                EditorGUILayout.BeginHorizontal();

                data.Prefabs[j] =
                    EditorGUILayout.ObjectField("Prefab", data.Prefabs[j], typeof (Transform), false) as Transform;

                var enableRemoveButton = data.Prefabs[0] != null || count > 1;
                SettingFoldoutButton button = AddSettingFoldoutButtons(enableRemoveButton);
                if (button != SettingFoldoutButton.None)
                {
                    actionIndex = j;
                    foldoutButton = button;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = 0;

            if (foldoutButton != SettingFoldoutButton.None)
            {
                HandleFoldoutButtons(foldoutButton, actionIndex, data.Prefabs);
            }
        }

        private SettingFoldoutButton AddSettingFoldoutButtons(bool enableRemoveButton = true)
        {            
            bool removePressed = false;
            if (enableRemoveButton)
            {
                removePressed = GUILayout.Button(new GUIContent("x", "Click to remove"), EditorStyles.miniButton,
                    GUILayout.Width(20));    
            }

            bool addPressed = GUILayout.Button(new GUIContent("+", "Click to add item"), EditorStyles.miniButton,
                GUILayout.Width(20));
            
            if (removePressed) return SettingFoldoutButton.Remove;
            if (addPressed) return SettingFoldoutButton.Add;

            return SettingFoldoutButton.None;
        }

        private void HandleFoldoutButtons<T>(SettingFoldoutButton foldoutButton, int actionIndex, List<T> list)
            where T : class
        {
            if (actionIndex != -1)
            {
                switch (foldoutButton)
                {
                    case SettingFoldoutButton.Add:
                        list.Insert(actionIndex + 1, null);
                        break;
                    case SettingFoldoutButton.Remove:
                        list.RemoveAt(actionIndex);
                        break;
                }
            }
        }

        private enum SettingFoldoutButton
        {
            None,
            Remove,
            Add
        }
    }
}