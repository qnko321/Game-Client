namespace ScriptableObjectEvents
{
    public interface IGameEventListener<T>
    {
        void OnEventInvoked(T _item);
    }
}