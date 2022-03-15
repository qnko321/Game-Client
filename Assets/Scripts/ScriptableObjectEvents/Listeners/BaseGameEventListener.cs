using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    public abstract class BaseGameEventListener<T, E, UER> : MonoBehaviour, IGameEventListener<T> where E : BaseGameEvent<T> where UER : UnityEvent<T>
    {
        [SerializeField] private E gameEvent;
        [SerializeField] private UER unityEventResponse;

        private void OnEnable()
        {
            if (gameEvent == null) return;

            gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (gameEvent != null) gameEvent.UnregisterListener(this);
        }

        public void OnEventInvoked(T _item)
        {
            unityEventResponse?.Invoke(_item);
        }
    }
}
