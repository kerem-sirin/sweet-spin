using System;
using System.Collections.Generic;

namespace SweetSpin
{
    /// <summary>
    /// Event bus implementation
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> handlers = new Dictionary<Type, List<Delegate>>();

        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var type = typeof(T);
            if (handlers.TryGetValue(type, out var eventHandlers))
            {
                foreach (var handler in eventHandlers.ToArray()) // ToArray to avoid modification during iteration
                {
                    (handler as Action<T>)?.Invoke(gameEvent);
                }
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (!handlers.ContainsKey(type))
            {
                handlers[type] = new List<Delegate>();
            }
            handlers[type].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (handlers.TryGetValue(type, out var eventHandlers))
            {
                eventHandlers.Remove(handler);
            }
        }
    }
}