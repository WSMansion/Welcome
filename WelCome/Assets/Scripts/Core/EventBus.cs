using System;
using System.Collections.Generic;
using UnityEngine;

namespace WelCome.Core
{
    public static class EventBus
    {
        private static Dictionary<Type, List<Delegate>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            if (!list.Contains(handler))
                list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
                list.Remove(handler);
        }

        public static void Publish<T>(T evt)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                try
                {
                    ((Action<T>)list[i])?.Invoke(evt);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public static void Clear()
        {
            _handlers.Clear();
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _handlers = new Dictionary<Type, List<Delegate>>();
        }
#endif
    }
}
