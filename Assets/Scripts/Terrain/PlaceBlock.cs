using UnityEngine;
using UnityEngine.Serialization;

namespace Terrain
{
    public class PlaceBlock : MonoBehaviour
    {
        [FormerlySerializedAs("CanPlace")] public bool canPlace = true;

        void OnTriggerEnter(Collider _col)
        {
            if (_col.gameObject.layer == LayerMask.NameToLayer("Ground"))
                return;
            canPlace = false;
        }

        void OnTriggerExit(Collider _col)
        {
            if (_col.gameObject.layer == LayerMask.NameToLayer("Ground"))
                return;
            canPlace = true;
        }
    }
}