using System;
using System.Collections.Generic;
using UnityEngine;

namespace SweetSpin.Core
{
    /// <summary>
    /// Service Locator for managing game services in the slot machine
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        public static ServiceLocator Instance { get; private set; }

        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private readonly object servicesLock = new object();

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            RegisterServices();
        }

        private void RegisterServices()
        {
            // Register core services
            Register<IEventBus>(new EventBus());
            Register<IAudioService>(new AudioService());
            Register<ISaveService>(new SaveService());
            Register<IRandomService>(new RandomService());
            Register<IPaylineService>(new PaylineService());
            Register<ISymbolService>(new SymbolService());
            Register<IAutoPlayService>(new AutoPlayService());
        }

        public void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            lock (servicesLock)
            {
                if (services.ContainsKey(type))
                {
                    Debug.LogWarning($"Service {type.Name} is being overwritten");
                }
                services[type] = service;
            }
        }

        public T Get<T>() where T : class
        {
            var type = typeof(T);
            lock (servicesLock)
            {
                if (services.TryGetValue(type, out var service))
                {
                    return (T)service;
                }

                Debug.LogError($"Service {type.Name} not found!");
                return null;
            }
        }

        public void Unregister<T>() where T : class
        {
            var type = typeof(T);
            lock (servicesLock)
            {
                services.Remove(type);
            }
        }

        private void OnDestroy()
        {
            // Clean up services that need disposal
            foreach (var service in services.Values)
            {
                if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}