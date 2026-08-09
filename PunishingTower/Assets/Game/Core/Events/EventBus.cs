using System;
using System.Collections.Generic;

namespace PunishingTower.Core.Events
{
    /// <summary>
    /// Central event hub for gameplay communication.
    /// Systems subscribe to events instead of referencing each other directly.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> TypedHandlers = new Dictionary<Type, List<Delegate>>();
        private static readonly List<Action<IGameEvent>> WildcardHandlers = new List<Action<IGameEvent>>();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!TypedHandlers.TryGetValue(typeof(T), out List<Delegate> list))
            {
                list = new List<Delegate>();
                TypedHandlers[typeof(T)] = list;
            }

            list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null)
            {
                return;
            }

            if (TypedHandlers.TryGetValue(typeof(T), out List<Delegate> list))
            {
                list.Remove(handler);
            }
        }

        /// <summary>Subscribes a listener that receives every event. Useful for relics, passives and boss rules.</summary>
        public static void SubscribeAll(Action<IGameEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            WildcardHandlers.Add(handler);
        }

        public static void UnsubscribeAll(Action<IGameEvent> handler)
        {
            if (handler == null)
            {
                return;
            }

            WildcardHandlers.Remove(handler);
        }

        public static void Publish<T>(T evt) where T : IGameEvent
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt));
            }

            if (TypedHandlers.TryGetValue(typeof(T), out List<Delegate> list))
            {
                foreach (Delegate handler in list.ToArray())
                {
                    ((Action<T>)handler).Invoke(evt);
                }
            }

            foreach (Action<IGameEvent> handler in WildcardHandlers.ToArray())
            {
                handler.Invoke(evt);
            }
        }

        /// <summary>Removes every registered handler. Call between battles or in test teardown.</summary>
        public static void Clear()
        {
            TypedHandlers.Clear();
            WildcardHandlers.Clear();
        }
    }
}
