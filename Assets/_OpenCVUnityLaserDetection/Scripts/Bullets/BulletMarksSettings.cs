using System.Collections.Generic;
using System.Linq;
using Assets.BulletDecals.Scripts.Extensions;
using Assets.BulletDecals.Scripts.Pool;
using UnityEngine;

namespace Assets.BulletDecals.Scripts.Bullets
{
    /// <summary>
    /// BulletMarksSettings provides settings for bullet marks and impact effects
    /// To increase performance it uses pool of objects    
    /// </summary>
    [RequireComponent(typeof(ObjectsPool))]
    public class BulletMarksSettings : MonoBehaviour
    {
        public bool EnableStaticObjects;
        /// <summary>
        /// Data for bullet marks
        /// </summary>
        public List<PrefabsLayerData> BulletMarkDatas;
        /// <summary>
        /// Data for impact effects
        /// </summary>
        public List<PrefabsLayerData> ImpactEffectDatas;

        /// <summary>
        /// Pool of objects
        /// </summary>
        private ObjectsPool _objectsPool;

        public void Awake()
        {            
            if (EnableStaticObjects)
            {
                CacheStaticObjectsMeshData();
            }            
        }              

        public void Start()
        {
            _objectsPool = GetComponent<ObjectsPool>();
        }

        private void CacheStaticObjectsMeshData()
        {
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var singleObject in allObjects)
            {
                if (singleObject.activeInHierarchy)
                {
                    if (singleObject.isStatic)
                    {
                        var staticCollider = singleObject.GetComponent<Collider>();
                        if (staticCollider != null)
                        {
                            if (!(staticCollider is TerrainCollider))
                            {
                                singleObject.AddComponent<MeshData>().Initialize();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get bullet mark by tag
        /// </summary>
        /// <param name="objectTag">tag</param>
        /// <param name="position">bullet mark position</param>
        /// <param name="rotation">bullet mark rotation</param>
        /// <returns></returns>
        public Transform GetBulletMarkByTag(string objectTag, Vector3 position, Quaternion rotation)
        {
            var layerData = GetBulletMarkDataByTag(objectTag);
            return TakeFromLayerData(layerData, position, rotation);
        }

       
        /// <summary>
        /// Get impact effect by tag
        /// </summary>
        /// <param name="objectTag">tag</param>
        /// <param name="position">impact effect position</param>
        /// <param name="rotation">impact effect rotation</param>
        /// <returns></returns>
        public Transform GetImpactEffectByTag(string objectTag, Vector3 position, Quaternion rotation)
        {
            var layerData = GetImpactEffectDataByTag(objectTag);
            return TakeFromLayerData(layerData, position, rotation);
        }

        /// <summary>
        /// Get random prefab from PrefabsLayerData
        /// </summary>
        /// <param name="layerData"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private Transform TakeFromLayerData(PrefabsLayerData layerData, Vector3 position, Quaternion rotation)
        {
            if (layerData != null)
            {
                var prefab = layerData.GetRandomPrefab();
                if (prefab != null)
                {
                    return _objectsPool.Take(prefab, position, rotation);
                }
            }
            return null;
        }       

        private PrefabsLayerData GetBulletMarkDataByTag(string objectTag)
        {
            return GetLayerData(BulletMarkDatas, objectTag);
        }        

        private PrefabsLayerData GetImpactEffectDataByTag(string objectTag)
        {            
            return GetLayerData(ImpactEffectDatas, objectTag);
        }

       

        private PrefabsLayerData GetLayerData(List<PrefabsLayerData> layerDatas, string objectTag)
        {
            if (layerDatas != null && layerDatas.Count > 0)
            {
                List<PrefabsLayerData> dataByLayer = layerDatas.Where(o => o.Tag == objectTag).ToList();
                return dataByLayer.RandomItem();
            }

            return null;
        }

        
    }
}