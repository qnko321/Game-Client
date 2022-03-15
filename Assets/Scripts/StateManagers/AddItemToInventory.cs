namespace StateManagers
{
    public class AddItemToInventory : ICommand
    {
        public int itemId;
        public int itemAmount;
        public int[] slots;
        
        public void Execute()
        {
            
        }

        public void Undo()
        {
            
        }
    }
}