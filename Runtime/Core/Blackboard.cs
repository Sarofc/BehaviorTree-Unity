using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bonsai
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

        private readonly Dictionary<string, object> memory = new Dictionary<string, object>();

        public IReadOnlyDictionary<string, object> Memory => memory;

        [SerializeField]
        private List<string> keys = new List<string>();

        private readonly List<Action<KeyEvent>> observers = new List<Action<KeyEvent>>();

        public void Set(string key)
        {
            if (!memory.ContainsKey(key))
            {
                memory.Add(key, null);
                NotifyObservers(new KeyEvent(EventType.Add, key, null));
            }
        }

        public void Set(string key, object value)
        {
            if (!memory.ContainsKey(key))
            {
                memory.Add(key, value);
                NotifyObservers(new KeyEvent(EventType.Add, key, value));
            }
            else
            {
                var old = memory[key];
                if ((old == null && value != null) || (old != null && !old.Equals(value)))
                {
                    memory[key] = value;
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
                return memory[key];
            }

            return null;
        }

        public void Remove(string key)
        {
            if (memory.Remove(key))
            {
                NotifyObservers(new KeyEvent(EventType.Remove, key, null));
            }
        }

        public void Unset(string key)
        {
            if (Contains(key))
            {
                memory[key] = null;
                NotifyObservers(new KeyEvent(EventType.Change, key, null));
            }
        }

        public void Clear()
        {
            memory.Clear();
        }

        public bool Contains(string key)
        {
            return memory.ContainsKey(key);
        }

        public bool IsSet(string key)
        {
            return Contains(key) && memory[key] != null;
        }

        public bool IsUnset(string key)
        {
            return Contains(key) && memory[key] == null;
        }

        public void AddObserver(Action<KeyEvent> action)
        {
            observers.Add(action);
        }

        public void RemoveObserver(Action<KeyEvent> action)
        {
            observers.Remove(action);
        }

        public int ObserverCount => observers.Count;

        public int Count => memory.Count;

        public void OnAfterDeserialize()
        {
            memory.Clear();
            foreach (var key in keys)
            {
                memory.Add(key, null);
            }
        }

        public void OnBeforeSerialize()
        {
            keys.Clear();
            foreach (var key in memory.Keys)
            {
                keys.Add(key);
            }
        }

        private void NotifyObservers(KeyEvent e)
        {
            foreach (var action in observers)
            {
                action(e);
            }
        }
    }


}