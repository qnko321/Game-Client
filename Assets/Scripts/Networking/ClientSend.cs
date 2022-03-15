using System;
using Managers;
using Terrain;
using UnityEngine;

namespace Networking
{
    public class ClientSend : MonoBehaviour
    {
        private static void SendTcpData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.tcp.SendData(_packet);
        }

        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.udp.SendData(_packet);
        }

        #region Packets
        public static void WelcomeReceived()
        {
            using Packet _packet = new Packet((int)ClientPackets.WelcomeReceived);
            _packet.Write(Client.instance.myId);
            _packet.Write(MenuUIManager.instance.usernameInputField.text);

            SendTcpData(_packet);
        }

        public static void UDPPingTest()
        {
            using Packet _packet = new Packet((int)ClientPackets.UDPPingTest);
            _packet.Write(DateTime.UtcNow);

            SendUDPData(_packet);
        }

        public static void MovementInput(Vector2 _movementDirection, bool _jump, Quaternion _rotation)
        {
            using Packet _packet = new Packet((int)ClientPackets.PlayerMovementInput);
            _packet.Write(_movementDirection);
            _packet.Write(_jump);
            _packet.Write(_rotation);

            SendUDPData(_packet);
        }

        public static void PlayerLook(float _xRotationOfCamera, Vector3 _bodyRotation)
        {
            using Packet _packet = new Packet((int)ClientPackets.PlayerLook);
            _packet.Write(_xRotationOfCamera);
            _packet.Write(_bodyRotation);

            SendUDPData(_packet);
        }

        public static void RequestVoxelMap(ChunkCoord _coord)
        {
            using Packet _packet = new Packet((int)ClientPackets.VoxelMapRequest);
            _packet.Write(_coord);

            SendTcpData(_packet);
        }

        public static void ModifyChunk(int _commandId, ChunkCoord _coord, Vector3 _voxelPos, byte _oldId, byte _newId)
        {
            using Packet _packet = new Packet((int) ClientPackets.ModifyChunk);
            _packet.Write(_commandId);
            _packet.Write(_coord);
            _packet.Write(_voxelPos);
            _packet.Write(_oldId);
            _packet.Write(_newId);
            
            SendTcpData(_packet);
        }
        #endregion

        public static void GameLoad()
        {
            using Packet _packet = new Packet((int) ClientPackets.GameLoad);
            SendTcpData(_packet);
        }
    }
}
