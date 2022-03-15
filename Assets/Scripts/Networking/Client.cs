using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Networking
{
    public class Client : MonoBehaviour
    {
        public static Client instance;
        private static int dataBufferSize = 4096;

        public string ip = "192.168.20.193";
        public int port = 26950;
        public int myId = 0;
        public Tcp tcp;
        public Udp udp;

        private bool _isConnected = false;

        private delegate void PacketHandler(Packet _packet);
        private static Dictionary<int, PacketHandler> packetHandlers;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
            }
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            tcp = new Tcp();
            udp = new Udp();
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        public void ConnectToServer(Action<string> _callback, string _ip, int _port)
        {
            Debug.Log("Ip: "+ _ip);
            Debug.Log("Port: "+ _port);
            instance.ip = _ip;
            instance.port = _port;
            _callback("Initializing Client Data...");
            InitializeClientData();

            _isConnected = true;

            tcp.Connect(_callback);
        }

        public class Tcp
        {
            public TcpClient socket;
            private Packet _receivedData;
            private NetworkStream _stream;
            private byte[] _receiveBuffer;

            public void Connect(Action<string> _callback)
            {
                _callback("Connecting...");
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                _receiveBuffer = new byte[dataBufferSize];
                IAsyncResult _result = socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
                
                Thread _timeoutThread = new Thread(() =>
                {
                    var _wait = _result.AsyncWaitHandle.WaitOne(3000, true);
                    if (_wait)
                    {
                        if (!socket.Connected)
                        {
                            Debug.Log("Connection Timeout.");
                            _callback("Connection Timeout.");
                        }
                        else
                        {
                            Debug.Log("Connected successfully.");
                            _callback("Connected successfully");
                        }
                    }
                });
                
                _timeoutThread.Start();
            }

            private void ConnectCallback(IAsyncResult _result)
            {
                socket.EndConnect(_result);

                if (!socket.Connected)
                {
                    return;
                }

                _stream = socket.GetStream();

                _receivedData = new Packet();

                _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        _stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Debug.Log($"Error sending data to server via TCP: {_ex}");
                    throw;
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = _stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        instance.Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(_receiveBuffer, _data, _byteLength);

                    _receivedData.Reset(HandleData(_data));
                    _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch
                {
                    Disconnect();
                }
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                _receivedData.SetBytes(_data);

                if (_receivedData.UnreadLength() >= 4)
                {
                    _packetLength = _receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength())
                {
                    byte[] _packetBytes = _receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using Packet _packet = new Packet(_packetBytes);
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    });

                    _packetLength = 0;
                    if (_receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = _receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            private void Disconnect()
            {
                instance.Disconnect();

                _stream = null;
                _receivedData = null;
                _receiveBuffer = null;
                socket = null;
            }
        }

        public class Udp
        {
            public UdpClient socket;
            private IPEndPoint _endPoint;

            public void Connect(int _localPort)
            {
                _endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
                
                socket = new UdpClient(_localPort);

                socket.Connect(_endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using Packet _packet = new Packet();
                SendData(_packet);
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    _packet.InsertInt(instance.myId);
                    if (socket != null)
                    {
                        socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Debug.LogError($"Error sending data to server via UDP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    byte[] _data = socket.EndReceive(_result, ref _endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    // This check maybe should be removed 
                    if (_data.Length < 4)
                    {
                        print("Data length is less than 4");
                        instance.Disconnect();
                        return;
                    }

                    HandleData(_data);
                }
                catch
                {
                    Disconnect();
                }
            }

            private void HandleData(byte[] _data)
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetLength = _packet.ReadInt();
                    _data = _packet.ReadBytes(_packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using Packet _packet = new Packet(_data);
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                });
            }

            private void Disconnect()
            {
                instance.Disconnect();

                _endPoint = null;
                socket = null;
            }
        }
    
        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.Welcome, ClientHandle.Welcome },
                { (int)ServerPackets.GameLoadReceived, ClientHandle.GameLoadReceived },
                { (int)ServerPackets.UDPPingTestReceived, ClientHandle.UDPPingTestReceived },
                { (int)ServerPackets.VoxelMapResponse, ClientHandle.VoxelMapReceive },
                { (int)ServerPackets.ModifyChunkValidation, ClientHandle.ModifyChunkValidation },
                { (int)ServerPackets.ModifyChunk, ClientHandle.ModifyChunk },
                { (int)ServerPackets.SpawnPlayer, ClientHandle.SpawnPlayer },
                { (int)ServerPackets.DespawnPlayer, ClientHandle.DespawnPlayer },
                { (int)ServerPackets.PlayerMovement, ClientHandle.PlayerMovement }
            };
            Debug.Log("Initialized packets.");
        }

        public void Disconnect()
        {
            if (_isConnected)
            {
                _isConnected = false;

                tcp.socket.Close();
                udp.socket.Close();

                Debug.Log("Disconnected from server.");
            }
        }
    }
}
