using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Networking;
using UnityEngine;

namespace Terrain
{
    public class Chunk
    {
        public bool IsActive
        {
            get { return isActive; }
            set 
            {
                isActive = value;
                if (chunkObject != null)
                    chunkObject.SetActive(value);
            }
        }

        public Vector3 Position { get { return chunkObject.transform.position; } }
        public ChunkCoord coord;
        
        public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

        public bool isInitialized = false;
        public bool isVoxelMapPopulated = false;

        private readonly World world;
        private GameObject chunkObject;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        private int vertexIndex = 0;
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private bool isActive = true;

        public Chunk(ChunkCoord _coord, World _world)
        {
            coord = _coord;
            this.world = _world;
            ClientSend.RequestVoxelMap(coord);
        }

        public void Init()
        {
            chunkObject = new GameObject();
            chunkObject.SetActive(isActive);
            meshFilter = chunkObject.AddComponent<MeshFilter>();
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();

            meshRenderer.material = world.material;
            chunkObject.transform.SetParent(world.transform);
            chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0, coord.z * VoxelData.ChunkWidth);
            chunkObject.name = "Chunk " + coord.x + ", " + coord.z;
            chunkObject.layer = LayerMask.NameToLayer("Ground");

            UpdateChunk();

            isInitialized = true;
        }

        #region Checks
        bool IsVoxelInChunk(int _x, int _y, int _z)
        {
            if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
                return false;
            return true;
        }

        bool CheckVoxel(Vector3 _pos)
        {
            int _x = Mathf.FloorToInt(_pos.x);
            int _y = Mathf.FloorToInt(_pos.y);
            int _z = Mathf.FloorToInt(_pos.z);
            if (_y < 0)
                return false;
            if (!IsVoxelInChunk(_x, _y, _z))
            {
                ChunkCoord _coord = coord.Copy();
                if (_x < 0)
                {
                    _coord.x--;
                    _x = 15;                
                }
                if (_x > 15)
                {
                    _coord.x++;
                    _x = 0;
                }
                if (_z < 0)
                {
                    _coord.z--;
                    _z = 15;
                }
                if (_z > 15)
                {
                    _coord.z++;
                     _z = 0;
                }
                if (world.chunks.TryGetValue(_coord, out Chunk _chunk) && _chunk.isVoxelMapPopulated)
                    return world.blockTypes[_chunk.voxelMap[_x, _y, _z]].isSolid;
                
                //TODO: fix
                return false;
            }
            
            return world.blockTypes[voxelMap[_x, _y, _z]].isSolid;
        }
        #endregion

        #region VoxelMap
        public void LoadVoxelMap(byte[,,] _voxelMap)
        {
            voxelMap = _voxelMap;
            isVoxelMapPopulated = true;
        }

        public byte[] ToCompressedByteArray()
        {
            BinaryFormatter _bf = new BinaryFormatter();
            MemoryStream _ms = new MemoryStream();
            _bf.Serialize(_ms, voxelMap);
            return Compressor.CompressBytes(_ms.ToArray());
        }

        public byte[] ToByteArray()
        {
            BinaryFormatter _bf = new BinaryFormatter();
            MemoryStream _ms = new MemoryStream();
            _bf.Serialize(_ms, voxelMap);
            _ms.Close();
            return _ms.ToArray();
        }

        public string VoxelMapToHash()
        {
            byte[] _voxelMapBytes = ToByteArray();
            using SHA256 _sha256Hash = SHA256.Create();
            return Hasher.GetHash(_sha256Hash, _voxelMapBytes);
        }

        #endregion

        #region Modification
        public (bool, byte, Vector3) EditVoxel(Vector3 _pos, byte _newID)
        {
            int _xCheck = Mathf.FloorToInt(_pos.x);
            int _yCheck = Mathf.FloorToInt(_pos.y);
            int _zCheck = Mathf.FloorToInt(_pos.z);

            _xCheck -= Mathf.FloorToInt(Position.x);
            _zCheck -= Mathf.FloorToInt(Position.z);

            byte _oldId = voxelMap[_xCheck, _yCheck, _zCheck];
            
            if (_oldId == 1)
                return (false, _oldId, Vector3.zero);
            
            voxelMap[_xCheck, _yCheck, _zCheck] = _newID;

            UpdateChunk();
            UpdateSurroundingVoxels(_xCheck, _yCheck, _zCheck);
            return (true, _oldId,  new Vector3(_xCheck, _yCheck, _zCheck));
        }

        public void ModifyChunk(Vector3 _voxelPos, byte _newId)
        {
            int _x = (int)_voxelPos.x;
            int _y = (int)_voxelPos.y;
            int _z = (int)_voxelPos.z;
            
            if (voxelMap[_x, _y, _z] == 1)
                return;
            
            voxelMap[_x, _y, _z] = _newId;

            UpdateChunk();
            UpdateSurroundingVoxels(_x, _y, _z);
        }

        public void UpdateSurroundingVoxels (int _x, int _y, int _z) 
        {
            Vector3 _thisVoxel = new Vector3(_x, _y, _z);

            for (int _p = 0; _p < 6; _p++) {

                Vector3 _currentVoxel = _thisVoxel + VoxelData.FaceChecks[_p];

                if (!IsVoxelInChunk((int)_currentVoxel.x, (int)_currentVoxel.y, (int)_currentVoxel.z)) 
                {
                    world.GetChunkFromVector3(_currentVoxel + Position).UpdateChunk();
                }
            }
        }
        #endregion

        #region Mesh

        private void UpdateChunk() 
        {
            ClearMeshData();

            for (int _y = 0; _y < VoxelData.ChunkHeight; _y++)
            for (int _x = 0; _x < VoxelData.ChunkWidth; _x++)
            for (int _z = 0; _z < VoxelData.ChunkWidth; _z++)
                if (world.blockTypes[voxelMap[_x,_y,_z]].isSolid)
                    UpdateMeshData(new Vector3(_x, _y, _z));

            CreateMesh();
        }

        void UpdateMeshData (Vector3 _pos) {

            for (int _p = 0; _p < 6; _p++) { 

                if (!CheckVoxel(_pos + VoxelData.FaceChecks[_p])) 
                {
                    byte _blockID = voxelMap[(int)_pos.x, (int)_pos.y, (int)_pos.z];

                    vertices.Add (_pos + VoxelData.VoxelVerts [VoxelData.VoxelTris [_p, 0]]);
                    vertices.Add (_pos + VoxelData.VoxelVerts [VoxelData.VoxelTris [_p, 1]]);
                    vertices.Add (_pos + VoxelData.VoxelVerts [VoxelData.VoxelTris [_p, 2]]);
                    vertices.Add (_pos + VoxelData.VoxelVerts [VoxelData.VoxelTris [_p, 3]]);

                    AddTexture(world.blockTypes[_blockID].GetTextureID(_p));

                    triangles.Add (vertexIndex);
                    triangles.Add (vertexIndex + 1);
                    triangles.Add (vertexIndex + 2);
                    triangles.Add (vertexIndex + 2);
                    triangles.Add (vertexIndex + 1);
                    triangles.Add (vertexIndex + 3);
                    vertexIndex += 4;
                }
            }
        }

        void CreateMesh()
        {
            Mesh _mesh = new Mesh();
            _mesh.vertices = vertices.ToArray();
            _mesh.triangles = triangles.ToArray();
            _mesh.uv = uvs.ToArray();

            _mesh.RecalculateNormals();
            meshFilter.mesh = _mesh;
        }

        void AddTexture(int _textureID)
        {
            float _y = _textureID / VoxelData.TextureAtlasSizeInBlocks;
            float _x = _textureID - (_y * VoxelData.TextureAtlasSizeInBlocks);

            _x *= VoxelData.NormalizedBlockTextureSize;
            _y *= VoxelData.NormalizedBlockTextureSize;

            _y = 1 - _y - VoxelData.NormalizedBlockTextureSize;

            uvs.Add(new Vector2(_x, _y));
            uvs.Add(new Vector2(_x, _y + VoxelData.NormalizedBlockTextureSize));
            uvs.Add(new Vector2(_x + VoxelData.NormalizedBlockTextureSize, _y));
            uvs.Add(new Vector2(_x + VoxelData.NormalizedBlockTextureSize, _y + VoxelData.NormalizedBlockTextureSize));
        }

        void ClearMeshData () 
        {
            vertexIndex = 0;
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();
        }
        #endregion

        public void Destroy()
        {
            ClearMeshData();
            voxelMap = null;
            
            Object.Destroy(chunkObject);
        }
    }
}
