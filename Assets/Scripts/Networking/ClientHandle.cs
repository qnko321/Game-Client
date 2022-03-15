using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using StateManagers;
using Terrain;
using UnityEngine;

namespace Networking
{
    public class ClientHandle : MonoBehaviour
    {
        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();

            Debug.Log($"Message from server: {_msg}");
            Client.instance.myId = _myId;
            ClientSend.WelcomeReceived();

            Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void GameLoadReceived(Packet _packet)
        {
            Vector3 _pos = _packet.ReadVector3();
            
            //TODO: Fix Username Logic
            GameManager.instance.SpawnLocalPlayer(_pos);
        }

        public static void UDPPingTestReceived(Packet _packet)
        {
            DateTime _startTime = _packet.ReadDateTime();
            DateTime _now = DateTime.UtcNow;
            Debug.Log("Ping: " + (_now - _startTime).Milliseconds);
        }

        public static void VoxelMapReceive(Packet _packet)
        {
            ChunkCoord _coord = _packet.ReadChunkCoord();
            int _length = _packet.ReadInt();
            byte[] _voxelMapBytes = _packet.ReadBytes(_length);
            byte[] _decompressedVoxelMapBytes = Compressor.DecompressBytes(_voxelMapBytes);
            BinaryFormatter _bf = new BinaryFormatter();
            byte[,,] _voxelMap = (byte[,,]) _bf.Deserialize(new MemoryStream(_decompressedVoxelMapBytes));
            World.instance.LoadChunkVoxelMap(_coord, _voxelMap);
        }

        public static void ModifyChunkValidation(Packet _packet)
        {
            int _commandId = _packet.ReadInt();
            bool _isValid = _packet.ReadBool();
            
            StateManager.instance.ValidateCommand(_commandId, _isValid);
        }

        public static void ModifyChunk(Packet _packet)
        {
            ChunkCoord _coord = _packet.ReadChunkCoord();
            Vector3 _voxelPos = _packet.ReadVector3();
            byte _newId = _packet.ReadByte();
            
            World.instance.ModifyChunk(_coord, _voxelPos, _newId);
        }

        public static void SpawnPlayer(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _username = _packet.ReadString();
            Vector3 _pos = _packet.ReadVector3();
            
            GameManager.instance.SpawnPlayer(_id, _username, _pos);
        }

        public static void DespawnPlayer(Packet _packet)
        {
            int _id = _packet.ReadInt();
            
            GameManager.instance.DespawnPlayer(_id);
        }

        public static void PlayerMovement(Packet _packet)
        {
            int _id = _packet.ReadInt();
            Vector3 _pos = _packet.ReadVector3();

            GameManager.instance.MovePlayer(_id, _pos);
        }
    }
}
