using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saro.BT
{
    public class Blackboard : ScriptableObject, ISerializationCallbackReceiver
    {
        public enum EventType
        {
            Add, Remove, Change
        }

        public struct KeyEvent
        {
            public KeyEvent(EventType type, string key, object value)
            {
                Type = type;
                Key = key;
                Value = value;
            }

            public EventType Type { get; }
            public string Key { get; }
            public object Value { get; }
        }

        private readonly Dictionary<string, object> m_Memory = new Dictionary<string, object>();

        public IReadOnlyDictionary<string, object> Memory => m_Memory;

        [SerializeField]
        private List<string> m_Keys = new List<string>();

        private readonly List<Action<KeyEvent>> m_Observers = new List<Action<KeyEvent>>();

        public void Set(string key)
        {
            if (!m_Memory.ContainsKey(key))
            {
                m_Memory.Add(key, null);
                NotifyObservers(new KeyEvent(EventType.Add, key, null));
            }
        }

        public void Set(string key, object value)
        {
            if (!m_Memory.ContainsKey(key))
            {
                m_Memory.Add(key, value);
                NotifyObservers(new KeyEvent(EventType.Add, key, value));
            }
            else
            {
                var old = m_Memory[key];
                if ((old == null && value != null) || (old != null && !old.Equals(value)))
                {
                    m_Memory[key] = value;
                    NotifyObservers(new KeyEvent(EventType.Change, key, value));
                }
            }
        }

        public T Get<T>(string key)
        {
            var value = (T)Get(key);
            if (value == null) return default;
            return (T)value;
        }

        public object Get(string key)
        {
            if (Contains(key))
            {
                return m_Memory[key];
            }

            return null;
        }

        public void Remove(string key)
        {
            if (m_Memory.Remove(key))
            {
                NotifyObservers(new KeyEvent(EventType.Remove, key, null));
            }
        }

        public void Unset(string key)
        {
            if (Contains(key))
            {
                m_Memory[key] = null;
                NotifyObservers(new KeyEvent(EventType.Change, key, null));
            }
        }

        public void Clear()
        {
            m_Memory.Clear();
        }

        public bool Contains(string key)
        {
            return m_Memory.ContainsKey(key);
        }

        public bool IsSet(string key)
        {
            return Contains(key) && m_Memory[key] != null;
        }

        public bool IsUnset(string key)
        {
            return Contains(key) && m_Memory[key] == null;
        }

        public void AddObserver(Action<KeyEvent> action)
        {
            m_Observers.Add(action);
        }

        public void RemoveObserver(Action<KeyEvent> action)
        {
            m_Observers.Remove(action);
        }

        public int ObserverCount => m_Observers.Count;

        public int Count => m_Memory.Count;

        public void OnAfterDeserialize()
        {
            m_Memory.Clear();
            foreach (var key in m_Keys)
            {
                m_Memory.Add(key, null);
            }
        }

        public void OnBeforeSerialize()
        {
            m_Keys.Clear();
            foreach (var key in m_Memory.Keys)
            {
                m_Keys.Add(key);
            }
        }

        private void NotifyObservers(KeyEvent e)
        {
            foreach (var action in m_Observers)
            {
                action(e);
            }
        }
    }


}