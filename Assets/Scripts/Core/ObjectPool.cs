/*
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street - Fifth Floor, Boston, MA 02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Capstone
{
    /// <summary>
    /// A single prefab to instance in the pool
    /// </summary>
    [Serializable]
    public class ObjectPoolItem
    {
        public int amount;
        public GameObject prefab = null;
        public bool expandable;
    }

    /// <summary>
    /// Instantiates and manages a pool of objects that may often be reused. (ie. bullets, enemies...)
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        static ObjectPool instance = null;

        [SerializeField] List<ObjectPoolItem> items = null;
        List<GameObject> pool = null;

        /// <summary>
        /// Ensure only one pool exists, and set it up.
        /// </summary>
        private void Awake()
        {
            if (instance != null)
                Destroy(gameObject);
            instance = this;

            pool = new List<GameObject>();
        }

        /// <summary>
        /// Instantiate the prefabs specified in the pool settings.
        /// </summary>
        private void Start()
        {
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = 0; j < items[i].amount; j++)
                {
                    var obj = Instantiate(items[i].prefab);
                    obj.name = $"{obj.name} {(char)(j + 65)}";
                    obj.SetActive(false);
                    pool.Add(obj);
                }
            }
        }

        /// <summary>
        /// Retrieve an item from the pool, allocating a new one if expandable.
        /// </summary>
        /// <param name="tag">One of the tags associated with the prefab</param>
        /// <returns>An appropriate item from the pool, or nothing otherwise</returns>
        public static GameObject Get(string tag)
        {
            for (int i = 0; i < instance.pool.Count; i++)
            {
                if (!instance.pool[i].activeInHierarchy && instance.pool[i].TryGetComponent(out TagCollection tags))
                {
                    if (tags.tags.Contains(tag))
                        return instance.pool[i];
                }
            }

            for (int i = 0; i < instance.items.Count; i++)
            {
                if (instance.items[i].prefab.TryGetComponent(out TagCollection tags))
                {
                    if (instance.items[i].expandable && tags.tags.Contains(tag))
                    {
                        var obj = Instantiate(instance.items[i].prefab);
                        obj.SetActive(false);
                        instance.pool.Add(obj);
                        return obj;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Delete every item from the pool.
        /// </summary>
        private void OnDestroy()
        {
            for (int i = 0; i < pool.Count; i++)
                Destroy(pool[i]);
        }
    }
}
