using System;
using System.Collections.Generic;
using Assets.BulletDecals.Scripts.Extensions;
using UnityEngine;

namespace Assets.BulletDecals.Scripts.Bullets
{
    /// <summary>
    /// Class to define prefabs by Tag
    /// </summary>
    [Serializable]
    public class PrefabsLayerData
    {               
        public string Tag = "Untagged";
        
        public List<Transform> Prefabs;

        public Transform GetRandomPrefab()
        {
            return Prefabs.RandomItem();
        }        
    }
}