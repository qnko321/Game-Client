using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace LookUps
{
    public static class Offsets
    {
        public static readonly Dictionary<Direction, Vector3> DirectionOffsets = new Dictionary<Direction, Vector3>() { 
            { Direction.Center, new Vector3(0f, 0f, 0f) },
            { Direction.North, new Vector3(0f, 0f, 1f) },
            { Direction.NorthEast, new Vector3(1f, 0f, 1f) },
            { Direction.East, new Vector3(1f, 0f, 0f) },
            { Direction.SouthEast, new Vector3(1f, 0f, -1f) },
            { Direction.South, new Vector3(0f, 0f, -1f) },
            { Direction.SouthWest, new Vector3(-1f, 0f, -1f) },
            { Direction.West, new Vector3(-1f, 0f, 0f) },
            { Direction.NorthWest, new Vector3(-1f, 0f, 1f) }
        };

        public static Vector3 GivenHeightDirectionOffset(Direction _dir, float _y)
        {
            Vector3 _rVal = DirectionOffsets[_dir];
            _rVal.y = _y;
            return _rVal;
        }
    }
}