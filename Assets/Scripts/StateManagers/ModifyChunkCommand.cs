using Inventory;
using Terrain;
using UnityEngine;

namespace StateManagers
{
    public class ModifyChunkCommand// : ICommand
    {
        private readonly PlayerInventory inventory;
        private readonly World world;
        private readonly ChunkCoord chunkCoord;
        private readonly Vector3 blockPosition;
        private readonly byte oldBlockId;
        private readonly byte newBlockId;

        public ModifyChunkCommand(World _world, PlayerInventory _inventory, ChunkCoord _chunkCoord, Vector3 _blockPosition, byte _oldBlockId, byte _newBlockId)
        {
            inventory = _inventory;
            world = _world;
            chunkCoord = _chunkCoord;
            blockPosition = _blockPosition;
            oldBlockId = _oldBlockId;
            newBlockId = _newBlockId;
            
            Execute();
        }

        public void Execute()
        {
            world.chunks[chunkCoord].ModifyChunk(blockPosition, newBlockId);
            inventory.PlaceBlock();
        }

        public void Undo()
        {
            world.chunks[chunkCoord].ModifyChunk(blockPosition, oldBlockId);
            inventory.UndoPlaceBlock();
        }
    }
}