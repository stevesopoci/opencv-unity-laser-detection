using System.Collections.Generic;
using UnityEngine;

namespace Assets.BulletDecals.Scripts.Extensions
{
    /// <summary>
    /// Layer Mask Extension Methods
    /// </summary>
    public static class LayerMaskExtension
    {
        public static bool IsInLayerMask(this LayerMask mask, GameObject obj)
        {
            return IsInLayerMask(mask, obj.layer);
        }

        public static bool IsInLayerMask(this LayerMask mask, int layer)
        {
            return ((mask.value & (1 << layer)) > 0);
        }

        public static string[] GetLayerNames(this LayerMask mask)
        {
            var names = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                var layerName = LayerMask.LayerToName(i);
                names.Add(layerName);                                
            }
            return names.ToArray();
        }
        
    }
}