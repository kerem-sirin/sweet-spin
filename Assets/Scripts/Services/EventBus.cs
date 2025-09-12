using System;
using System.Collections.Generic;

namespace SweetSpin
{
    /// <summary>
    /// Implements a publish-subscribe pattern for decoupled communication between game components.
    /// Allows services and views to react to game events without direct references.
    /// </summary>
    public class EventBus : IEventBus
    {
        /// <summary>Registry of event handlers organized by event type</summary>
        private readonly Dictionary<Type, List<Delegate>> handlers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Broadcasts an event to all registered handlers of that event type
        /// </summary>
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

        /// <summary>
        /// Registers a handler to receive events of a specific type
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (!handlers.ContainsKey(type))
            {
                handlers[type] = new List<Delegate>();
            }
            handlers[type].Add(handler);
        }

        /// <summary>
        /// Removes a handler from receiving events of a specific type
        /// </summary>
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