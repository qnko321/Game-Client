using System;
using System.Collections.Generic;
using Enums;
using LookUps;
using UnityEngine;

namespace Terrain
{
    public class CollisionHandler : MonoBehaviour
    {
        [Header("References")]
        public GameObject colliderPrefab;

        public bool Enabled { set; get; } = true;

        private readonly Dictionary<Direction, GameObject> _colliders = new Dictionary<Direction, GameObject>();
        private Rigidbody _rb;

        // ReSharper disable once InconsistentNaming
        private Transform _transform;
        private Transform _colliderParent;
        private Vector3 _lastPos;

        private Vector3 Position => _transform.position;
        private bool _startPhysics;

        private void Awake()
        {
            // Assign the transform reference so it doesnt have to loop thought all behaviours every time we reference the transform
            _transform = transform;
            _lastPos = Position;
        }

        private void Start()
        {
            _colliderParent = new GameObject(name + " Collider Parent").transform;
            _startPhysics = true;
        }

        private void Update()
        {
            if (_startPhysics)
            {
                CreateColliders();
                UpdateColliders();
                _startPhysics = false;
            }

            // Check if the players position have changed
            if (_lastPos != Position)
                UpdateColliders();
        }

        /// <summary>
        /// Update the positions and scales of all the colliders assigned to this object
        /// </summary>
        public void UpdateColliders()
        {
            _lastPos = Position;

            foreach (Direction _dir in Enum.GetValues(typeof(Direction)))
            {
                Vector3 _voxelPos = new Vector3(.5f, .5f, .5f);
                
                _colliders[_dir].transform.localPosition = _voxelPos + Offsets.GivenHeightDirectionOffset(_dir,
                    World.instance.GetHighestVoxelY(Position + Offsets.DirectionOffsets[_dir],
                        Mathf.FloorToInt(Position.y) + 2));
            }

            _colliderParent.position = World.Vector3ToVoxelCoord(Position, _defY: 0);
        }

        /// <summary>
        /// Creates all the needed colliders to handle the collision of this game object
        /// </summary>
        private void CreateColliders()
        {
            foreach (Direction _dir in Enum.GetValues(typeof(Direction)))
            {
                GameObject _obj = Instantiate(colliderPrefab, _colliderParent);
                _obj.name = _dir + " Collider";
                _colliders.Add(_dir, _obj);
            }
        }
        
        private void OnDestroy()
        {
            if (_colliderParent != null) Destroy(_colliderParent.gameObject);
        }
    }
}
