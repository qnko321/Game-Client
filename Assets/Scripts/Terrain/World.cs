using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class World : MonoBehaviour
    {
        public static World instance;
        public PlayerController player;
        public Vector3 spawnLocation;

        public Material material;
        public BlockType[] blockTypes;

        public readonly Dictionary<ChunkCoord, Chunk> chunks = new Dictionary<ChunkCoord, Chunk>();
        private readonly List<ChunkCoord> _activeChunks = new List<ChunkCoord>();
        private ChunkCoord _playerLastChunkCoord;
        private ChunkCoord _playerChunkCoord;

        private readonly List<ChunkCoord> _chunksToLoad = new List<ChunkCoord>();

        private bool _spawnedPlayer;

        private void Clear()
        {
            foreach (Chunk _chunk in chunks.Values)
            {
                _chunk.Destroy();
            }
            chunks.Clear();
            _activeChunks.Clear();
            _chunksToLoad.Clear();
            
            _playerLastChunkCoord = new ChunkCoord();
            _playerChunkCoord = new ChunkCoord();

            _spawnedPlayer = false;
        }
        
        #region UnityEvents

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
        }

        private void Start()
        {
            DebugMenu.instance.UpdateChunkCoord(_playerChunkCoord);
            LoadSpawn();
            Debug.Log(JsonUtility.ToJson(blockTypes[0]));
        }

        private void Update()
        {
            _playerChunkCoord = new ChunkCoord(player.Position);

            // Check fi the player has moved from the last coord he was standing on
            if (!_playerChunkCoord.Equals(_playerLastChunkCoord))
            {
                DebugMenu.instance.UpdateChunkCoord(_playerChunkCoord);
                CheckViewDistance();
            }
            
            // Check if there are any chunks that wait to be loaded
            // If true Initialize 1 chunk
            if (_chunksToLoad.Count > 0)
                InitializeChunk();

            // Check if the player is spawned
            // if false get the players spawn location and teleport them there
            if (!_spawnedPlayer)
            {
                if (chunks[new ChunkCoord()].isInitialized)
                {
                    spawnLocation = new Vector3(0.5f, GetHighestVoxelY(Vector3.zero) + 4f, 0.5f);
                    player.Teleport(spawnLocation);
                    _spawnedPlayer = true;
                }
            }
        }

        private void OnDisable()
        {
            Clear();
        }

        #endregion

        #region Checks
        
        /// <summary>
        /// Gets chunk coord and loads chunks in view distance
        /// </summary>
        private void LoadSpawn()
        {
            // Get the coordinates of the chunk that the player stands on
            ChunkCoord _playerChunk = new ChunkCoord(player.Position);

            // Load the chunks that are in the specified view distance
            for (int _x = _playerChunk.x - VoxelData.ViewDistanceInChunks; _x <= _playerChunk.x + VoxelData.ViewDistanceInChunks; _x++)
            {
                for (int _z = _playerChunk.z - VoxelData.ViewDistanceInChunks; _z <= _playerChunk.z + VoxelData.ViewDistanceInChunks; _z++)
                {
                    CreateChunk(new ChunkCoord(_x, _z));
                }
            }
        }

        /// <summary>
        /// Finds the new chunks that should be loaded and adds them to the queue
        /// </summary>
        private void CheckViewDistance()
        {
            ChunkCoord _playerChunkCoord = new ChunkCoord(player.Position);
            _playerLastChunkCoord = this._playerChunkCoord;

            List<ChunkCoord> _previouslyActiveChunks = new List<ChunkCoord>(_activeChunks);

            // Looping through all chunks that are in view distance
            for (int _x = _playerChunkCoord.x - VoxelData.ViewDistanceInChunks; _x < _playerChunkCoord.x + VoxelData.ViewDistanceInChunks; _x++)
            {
                for (int _z = _playerChunkCoord.z - VoxelData.ViewDistanceInChunks; _z < _playerChunkCoord.z + VoxelData.ViewDistanceInChunks; _z++)
                {
                    ChunkCoord _thisChunk = new ChunkCoord(_x, _z);
                    
                    // Check if the chunk class is created
                    if (!chunks.ContainsKey(_thisChunk))
                    {
                        // If not add it to queue
                        CreateChunk(_thisChunk);
                    }
                    // Check if the chunk is active
                    else if (!chunks[_thisChunk].IsActive)
                    {
                        // If not activate it and add it to the array of active chunks
                        chunks[_thisChunk].IsActive = true;
                        _activeChunks.Add(_thisChunk);
                    }

                    // Loop through the previously active chunks
                    foreach (ChunkCoord _coord in _previouslyActiveChunks.ToArray())
                        // If the current chunk was previously active
                        if (_coord.Equals(_thisChunk))
                            // If yes remove it from previously active chunks
                            _previouslyActiveChunks.Remove(_coord);
                }
            }

            // Loop through the previously active chunks and deactivate them
            foreach (ChunkCoord _coord in _previouslyActiveChunks)
                chunks[_coord].IsActive = false;
            
            //TODO: Update active chunks on server
        }
        #endregion
        
        #region Voxels
        
        /// <summary>
        /// Checks if theres a voxel on the specified position
        /// </summary>
        /// <param name="_pos">The position of the potential voxel</param>
        /// <returns>True if theres a voxel</returns>
        public bool CheckForVoxel (Vector3 _pos) 
        {
            int _xCheck = Mathf.FloorToInt(_pos.x);
            int _yCheck = Mathf.FloorToInt(_pos.y);
            int _zCheck = Mathf.FloorToInt(_pos.z);

            int _xChunk = Mathf.FloorToInt(_xCheck / (float)VoxelData.ChunkWidth);
            int _zChunk = Mathf.FloorToInt(_zCheck / (float)VoxelData.ChunkWidth);

            _xCheck -= _xChunk * VoxelData.ChunkWidth;
            _zCheck -= _zChunk * VoxelData.ChunkWidth;

            ChunkCoord _coord = new ChunkCoord(_xChunk, _zChunk);
            if (!chunks.ContainsKey(_coord)) return false;
            Chunk _chunk = chunks[_coord];
            if (_chunk.isVoxelMapPopulated)
            {
                if (_yCheck < 0)
                    return true;
                byte _voxelValue = _chunk.voxelMap[_xCheck, _yCheck, _zCheck];
                bool _isSolid = blockTypes[_voxelValue].isSolid;
                return _isSolid;
            }

            //TODO: fix
            return true;
        }
        
        /// <summary>
        /// Gets the y-coord of the voxel that's highest on that position
        /// </summary>
        /// <param name="_pos">The position to check for hightest voxel</param>
        /// <param name="_max">The height it should start searching from</param>
        /// <returns></returns>
        public float GetHighestVoxelY(Vector3 _pos, int _max = 128)
        {
            for (int _y = _max - 1; _y >= 0; _y--)
            {
                if (CheckForVoxel(new Vector3(_pos.x, _y, _pos.z)))
                {
                    return _y;
                }
            }
            return 0f;
        }

        #endregion
     
        #region Chunks

        /// <summary>
        /// Calls ModifyChunk method on the chunks that should be updated
        /// </summary>
        /// <param name="_coord">The coordinate of the chunk that should be modified</param>
        /// <param name="_voxelPos">The position of the modified voxel</param>
        /// <param name="_newId">The new id of the modified voxel</param>
        public void ModifyChunk(ChunkCoord _coord, Vector3 _voxelPos, byte _newId)
        {
            chunks[_coord].ModifyChunk(_voxelPos, _newId);
        }
        
        /// <summary>
        /// Adds chunk to chunks to load queue
        /// </summary>
        /// <param name="_coord">The coordinates of the chunk</param>
        private void CreateChunk(ChunkCoord _coord)
        {
            if (chunks.ContainsKey(_coord)) return;

            chunks.Add(_coord, new Chunk(_coord, this));
            _chunksToLoad.Add(_coord);
            _activeChunks.Add(_coord);
            
            //TODO: Update active chunks on server
        }

        /// <summary>
        /// Initialize the first chunk added to the queue if it's voxel map is populated
        /// </summary>
        private void InitializeChunk()
        {
            for (int _i = 0; _i < _chunksToLoad.Count; _i++)
            {
                ChunkCoord _coord = _chunksToLoad[_i];
                if (!chunks[_coord].isVoxelMapPopulated) continue;
                
                _chunksToLoad.RemoveAt(0);
                chunks[_coord].Init();
                break;
            }
        }
        
        /// <summary>
        /// Load a specified chunk from bytes[,,]
        /// </summary>
        /// <param name="_coord">The coordinates of the chunk</param>
        /// <param name="_voxelMap">the new voxel map</param>
        public void LoadChunkVoxelMap(ChunkCoord _coord, byte[,,] _voxelMap)
        {
            chunks[_coord].LoadVoxelMap(_voxelMap);
        }
        #endregion

        #region DataTranslation

        /// <summary>
        /// Get chunk reference from Vector3 world position
        /// </summary>
        /// <param name="_position">The world position used to find the chunk</param>
        /// <returns>Reference to the requested chunk</returns>
        public Chunk GetChunkFromVector3(Vector3 _position)
        {
            return chunks[new ChunkCoord(_position)];
        }

        /// <summary>
        /// Floors the axis and creates a new Vector3
        /// </summary>
        /// <param name="_pos">The position that should be converted to VoxelCoord</param>
        /// <param name="_defX">Default value for X-axis</param>
        /// <param name="_defY">Default value for Y-axis</param>
        /// <param name="_defZ">Default value for Z-axis</param>
        /// <returns>Return the new floored to int Vector3</returns>
        public static Vector3 Vector3ToVoxelCoord(Vector3 _pos, int _defX = -100, int _defY = -100, int _defZ = -100)
        {
            var _x = _defX != -100 ? _defX : Mathf.FloorToInt(_pos.x);
            var _y = _defY != -100 ? _defY : Mathf.FloorToInt(_pos.y);
            var _z = _defZ != -100 ? _defZ : Mathf.FloorToInt(_pos.z); 

            return new Vector3(_x, _y, _z);
        }
        
        #endregion
    }

    [Serializable]
    public class BlockType
    {
        public string name;
        public bool isSolid;

        [Header("Textures")]
        public int backFaceTexture;
        public int frontFaceTexture;
        public int topFaceTexture;
        public int bottomFaceTexture;
        public int leftFaceTexture;
        public int rightFaceTexture;

        public int GetTextureID(int _faceIndex)
        {
            switch (_faceIndex)
            {
                case 0:
                    return backFaceTexture;
                case 1:
                    return frontFaceTexture;
                case 2:
                    return topFaceTexture;
                case 3:
                    return bottomFaceTexture;
                case 4:
                    return leftFaceTexture;
                case 5:
                    return rightFaceTexture;
                default:
                    Debug.LogError("Error in GetTextureID; invalid face index");
                    return 0;
            }
        } 
    }

    public struct ChunkCoord
    {
        public int x;
        public int z;

        public ChunkCoord(int _x = 0, int _z = 0)
        {
            x = _x;
            z = _z;
        }

        public ChunkCoord(Vector3 _pos)
        {
            x = Mathf.FloorToInt(_pos.x / VoxelData.ChunkWidth);
            z = Mathf.FloorToInt(_pos.z / VoxelData.ChunkWidth);
        }

        public ChunkCoord Copy()
        {
            return new ChunkCoord(x, z);
        }
        
        public override int GetHashCode()
        {
            var _hashCode = 43270662;
            _hashCode = _hashCode * -1521134295 + x.GetHashCode();
            _hashCode = _hashCode * -1521134295 + z.GetHashCode();
            return _hashCode;
        }

        public bool Equals(ChunkCoord _other) => _other.x == x && _other.z == z;
    }
}