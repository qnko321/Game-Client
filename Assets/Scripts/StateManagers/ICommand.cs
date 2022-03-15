namespace StateManagers
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}