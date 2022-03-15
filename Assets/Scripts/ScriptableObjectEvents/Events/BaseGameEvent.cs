using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectEvents
{
    public class BaseGameEvent<T> : ScriptableObject
    {
        private readonly List<IGameEventListener<T>> eventListeners = new List<IGameEventListener<T>>();

        public void Invoke(T _item)
        {
            for (int _i = eventListeners.Count - 1; _i >= 0; _i--)
            {
                eventListeners[_i].OnEventInvoked(_item);
            }
        }

        public void RegisterListener(IGameEventListener<T> _listener)
        {
            if (!eventListeners.Contains(_listener))
            {
                eventListeners.Add(_listener);
            }
        }
        
        public void UnregisterListener(IGameEventListener<T> _listener)
        {
            if (eventListeners.Contains(_listener))
            {
                eventListeners.Remove(_listener);
            }
        }
    }
}
