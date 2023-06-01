using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.BulletDecals.Scripts.Pool
{
    public class ObjectsPool : MonoBehaviour
    {
        public bool Prewarm;
        public int DefaultCountForNewObject = 10;
        public List<MutableKeyValuePair> PoolDataList;

        private readonly Dictionary<Transform, List<Transform>> _dictionary = new Dictionary<Transform, List<Transform>>();

        private void Start()
        {
            if (Prewarm)
            {
                //TODO create all instances at start
            }
        }

        /// <summary>
        /// Take transform instance of provided prefab
        /// </summary>
        /// <param name="prefabTransform">source prefab</param>
        /// <returns></returns>
        public Transform Take(Transform prefabTransform)
        {
            return Take(prefabTransform, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Take transform instance of provided prefab
        /// </summary>
        /// <param name="prefabTransform"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public Transform Take(Transform prefabTransform, Vector3 position, Quaternion rotation)
        {
            Transform instance = null;

            var item = PoolDataList.FirstOrDefault(p => p.Key == prefabTransform);
            
            if (item != null)
            {
                var maxCount = item.Value;

                List<Transform> list;
                if (_dictionary.TryGetValue(prefabTransform, out list))
                {
                    int currentCount = list.Count;
                    if (currentCount < maxCount)
                    {
                        instance = Instantiate(prefabTransform, position, rotation) as Transform;
                        list.Add(instance);
                    }
                    else
                    {
                        instance = list[0];

                        //reset instance
                        instance.parent = null;

                        //apply new position, scale and rotation                    
                        instance.localScale = prefabTransform.localScale;
                        instance.position = position;
                        instance.rotation = rotation;

                        //move to list end
                        list.RemoveAt(0);
                        list.Add(instance);
                    }
                }
                else if (maxCount > 0)
                {
                    instance = Instantiate(prefabTransform, position, rotation) as Transform;

                    list = new List<Transform> { instance };

                    _dictionary[prefabTransform] = list;
                }
            }

            //if prefab is not found on pool settings then just create instance from provided prefab
            return instance ?? Instantiate(prefabTransform, position, rotation) as Transform;
        }       

        
    }
}