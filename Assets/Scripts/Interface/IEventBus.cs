using System;

namespace SweetSpin
{
    /// <summary>
    /// Event bus for decoupled communication
    /// </summary>
    public interface IEventBus
    {
        void Publish<T>(T gameEvent) where T : IGameEvent;
        void Subscribe<T>(Action<T> handler) where T : IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
    }
}