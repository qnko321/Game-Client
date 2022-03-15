using System.Collections.Generic;
using Helpers;
using Networking;
using UnityEngine;

namespace StateManagers
{
    public class StateManager : MonoBehaviour
    {
        public static StateManager instance;
        
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(this);
        }
        
        private int commandIndex = 1;
        private readonly Dictionary<int, ICommand> commands = new Dictionary<int, ICommand>();

        public int AddBlockInteractionCommand(ICommand _command)
        {
            int _commandId = Client.instance.myId.SetLength(4, 0) + commandIndex;
            commands.Add(_commandId, _command);
            commandIndex++;
            return _commandId;
        }

        public void ValidateCommand(int _commandId, bool _isValid)
        {
            if (_isValid)
            {
                commands.Remove(_commandId);
            }
            else
            {
                commands[_commandId].Undo();
                commands.Remove(_commandId);
            }
        }
    }
}