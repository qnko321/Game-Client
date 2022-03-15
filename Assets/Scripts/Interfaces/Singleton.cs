namespace Interfaces
{
    public interface ISingleton<T>
    {
        private static T instance;

        public bool AssignInstance(T _obj)
        {
            if (instance == null)
            {
                instance = _obj;
                return true;
            }

            if (_obj.Equals(instance))
            {
                return false;
            }

            return true;
        }
    }
}