using System;
using UnityEngine;

namespace Assets.BulletDecals.Scripts.Pool
{
    /// <summary>
    /// Mutable Key Value Pair used in ObjectsPool
    /// </summary>
    [Serializable]
    public class MutableKeyValuePair
    {
        public Transform Key;
        public int Value;

        public MutableKeyValuePair()
        {
        }

        public MutableKeyValuePair(Transform key, int value)
        {
            Key = key;
            Value = value;
        }

        public MutableKeyValuePair Clone()
        {
            return new MutableKeyValuePair(Key, Value);
        }
    }
}
