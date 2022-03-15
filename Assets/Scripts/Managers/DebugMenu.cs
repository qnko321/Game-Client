using Terrain;
using TMPro;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    public static DebugMenu instance;
    
    [SerializeField] private TMP_Text chunkCoordText;

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

    public void UpdateChunkCoord(ChunkCoord _coord)
    {
        chunkCoordText.text = $"Chunk Coord: {_coord.x} / {_coord.z}";
    }
}
