// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxonGenesis
{

    public struct KeyValueItem<K, V>
    {
        public K Key;
        public V Value;

    }

    public struct SDictionaryJsonWrapper<K, V>
    {
        public K[] Keys;
        public V[] Values;

    }

    /// <summary>
    /// A serializable dictionary class for storing key-value pair information.
    /// </summary>
    /// <typeparam name="K">The key type</typeparam>
    /// <typeparam name="V">The value type</typeparam>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "Serializable")]
    public class SDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
    {

        private class Enumerator : IEnumerator<KeyValuePair<K, V>>
        {
            SDictionary<K, V> Dictionary;
            int current = -1;

            public Enumerator(SDictionary<K, V> dictionary)
            {
                Dictionary = dictionary;
            }

            public KeyValuePair<K, V> Current {
                get {
                    return Dictionary.GetAt(current);
                }
            }

            object System.Collections.IEnumerator.Current {
                get {
                    return Dictionary.GetAt(current);
                }
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                ++current;

                return current < Dictionary.Count;
            }

            public void Reset()
            {
                current = -1;
            }
        }

        [SerializeField]
        public List<K> KeysList = new List<K>();

        [SerializeField]
        public List<V> ValuesList = new List<V>();

        [NonSerialized]
        private bool dictionaryRestored;

        [NonSerialized]
        private Dictionary<K, int> Dictionary = new Dictionary<K, int>();

        public V this[K key] {
            get {
                if (!dictionaryRestored)
                    RestoreDictionary();

                return ValuesList[Dictionary[key]];
            }
            set {
                Add(key, value);
            }
        }

        public void Add(K key, V value)
        {
            if (!dictionaryRestored)
                RestoreDictionary();

            int index;
            if (Dictionary.TryGetValue(key, out index)) {
                ValuesList[index] = value;
            }
            else {
                Dictionary[key] = ValuesList.Count;
                KeysList.Add(key);
                ValuesList.Add(value);
            }
        }

        public V Get(K key, V defaultValue)
        {
            if (!dictionaryRestored)
                RestoreDictionary();

            int index;
            if (Dictionary.TryGetValue(key, out index))
                return ValuesList[index];
            else
                return defaultValue;
        }

        public bool GetValue(K key, out V value)
        {
            if (!dictionaryRestored)
                RestoreDictionary();

            int index;
            if (Dictionary.TryGetValue(key, out index)) {
                if (index >= 0 && index < ValuesList.Count) {
                    value = ValuesList[index];
                    return true;
                }
                else {
                    value = default(V);
                    return false;
                }
            }
            else {
                value = default(V);
                return false;
            }
        }

        public bool Remove(K key)
        {
            if (!dictionaryRestored)
                RestoreDictionary();

            int index;
            if (Dictionary.TryGetValue(key, out index)) {
                if (index >= 0 && index < ValuesList.Count) {
                    Dictionary.Remove(key);
                    KeysList.RemoveAt(index);
                    ValuesList.RemoveAt(index);
                }
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (!dictionaryRestored)
                RestoreDictionary();

            if (index >= 0 && index < ValuesList.Count && index < KeysList.Count) {
                K key = KeysList[index];

                Dictionary.Remove(key);
                KeysList.RemoveAt(index);
                ValuesList.RemoveAt(index);
            }
        }

        public KeyValuePair<K, V> GetAt(int index)
        {
            return new KeyValuePair<K, V>(KeysList[index], ValuesList[index]);
        }

        public V GetValueAt(int index)
        {
            return ValuesList[index];
        }

        public int Count {
            get {
                return ValuesList.Count;
            }
        }

        public bool ContainsKey(K key)
        {
            if (key == null) return false;
            if (!dictionaryRestored)
                RestoreDictionary();

            return Dictionary.ContainsKey(key);
        }

        public void Clear()
        {
            Dictionary.Clear();
            KeysList.Clear();
            ValuesList.Clear();
        }

        private void RestoreDictionary()
        {
            for (int i = 0; i < KeysList.Count; ++i) {
                Dictionary[KeysList[i]] = i;
            }

            dictionaryRestored = true;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void Sort()
        {
            List<K> sortedKeys = new List<K>(KeysList);
            sortedKeys.Sort();

            int i = 0;
            List<V> sortedValues = new List<V>();
            foreach (K k in sortedKeys) {
                V value;
                GetValue(k, out value);
                sortedValues.Add(value);
                Dictionary[k] = i;
                i++;
            }

            KeysList = sortedKeys;
            ValuesList = sortedValues;
        }

        public string ToJson(bool prettyPrint = true)
        {
            if (KeysList == null || KeysList.Count == 0) return "";
            SDictionaryJsonWrapper<K, V> wrapper = new SDictionaryJsonWrapper<K, V>();
            wrapper.Keys = KeysList.ToArray();
            wrapper.Values = ValuesList.ToArray();
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        public void FromJson(string json)
        {
            SDictionaryJsonWrapper<K, V> wrapper = JsonUtility.FromJson<SDictionaryJsonWrapper<K, V>>(json);
            if (wrapper.Keys != null) KeysList = wrapper.Keys.ToList<K>();
            if (wrapper.Values != null) ValuesList = wrapper.Values.ToList<V>();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }

    [Serializable]
    public class SDictionaryString : SDictionary<string, string> { }

    [Serializable]
    public class SDictionaryBool : SDictionary<string, bool> { }

    [Serializable]
    public class SDictionaryInt : SDictionary<string, int> { }

    [Serializable]
    public class SDictionaryFloat : SDictionary<string, float> { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "SDictionaryFloatFloat")]
    public class SDictionaryFloatFloat : SDictionary<float, float> { }

    [Serializable]
    public class SDictionaryVector2 : SDictionary<string, Vector2> { }

    [Serializable]
    public class SDictionaryVector3 : SDictionary<string, Vector3> { }

    [Serializable]
    public class SDictionaryVector4 : SDictionary<string, Vector4> { }

    [Serializable]
    public class SDictionaryColor : SDictionary<string, Color> { }

    [Serializable]
    public class SDictionaryRect : SDictionary<string, Rect> { }

    [Serializable]
    public class SDictionaryQuaternion : SDictionary<string, Quaternion> { }

    [Serializable]
    public class SDictionaryEnum : SDictionary<string, System.Enum> { }

    [Serializable]
    public class SDictionaryObject : SDictionary<string, UnityEngine.Object> { }

    [Serializable]
    public class SDictionaryObjectList : SDictionary<string, List<UnityEngine.Object>> { }

    [Serializable]
    public class SDictionarySerializableObject : SDictionary<string, SerializableObject> { }

}//AxonGenesis