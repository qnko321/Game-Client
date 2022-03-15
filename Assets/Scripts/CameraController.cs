using Networking;
using StateManagers;
using Terrain;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public World world;
    
    [SerializeField] private Player player;
    [SerializeField] private InputActionReference lookInput;
    [SerializeField] private InputActionReference placeBlockInput;
    [SerializeField] private InputActionReference breakBlockInput;
    [SerializeField] private Transform playerBody;
    [SerializeField] private GameObject breakBlockPrefab;
    [SerializeField] private GameObject placeBlockPrefab;
    [SerializeField] private PlaceBlock placeBlockScript;
    [SerializeField] private float verticalMouseSensitivity = 3.5f;
    [SerializeField] private float horizontalMouseSensitivity = 3.5f;
    [SerializeField][Range(0.0f, 0.1f)] private float mouseSmoothTime = 0.03f;
    [SerializeField] private float checkIncrement = 0.1f;
    [SerializeField] private float reach = 8f;
    [SerializeField] private bool lockCursor = true;

    private Transform placeBlock;
    private Transform breakBlock;
    
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;
    private Vector2 mouseDelta;        
    private float cameraPitch;
    public bool allowBlockModification;

    private void OnEnable()
    {
        placeBlockInput.ToInputAction().started += PlaceBlock;
        breakBlockInput.ToInputAction().started += BreakBlock;
    }

    private void OnDisable()
    {
        placeBlockInput.ToInputAction().started -= PlaceBlock;
        breakBlockInput.ToInputAction().started -= BreakBlock;
    }

    private void PlaceBlock(InputAction.CallbackContext _ctx)
    {
        if (!allowBlockModification) return;
        if (!breakBlock.gameObject.activeSelf) return;
        if (!placeBlockScript.canPlace) return;
        if (!player.inventory.IsBlockSelected()) return;
        
        var _position = placeBlock.position;
        Chunk _chunk = world.GetChunkFromVector3(_position);
        byte _newBlockId = player.inventory.GetBlockId();
        (bool _valid, byte _oldBlockId, Vector3 _voxelPos) = _chunk.EditVoxel(_position, _newBlockId);
        if (_valid)
        {
            int _commandId = StateManager.instance.AddBlockInteractionCommand(new ModifyChunkCommand(world, player.inventory, _chunk.coord, _voxelPos, _oldBlockId,_newBlockId));
            ClientSend.ModifyChunk(_commandId, _chunk.coord, _voxelPos, _oldBlockId, _newBlockId);
        }
        player.collisionHandler.UpdateColliders();
    }

    private void BreakBlock(InputAction.CallbackContext _ctx)
    {
        if (!allowBlockModification) return;
        if (!breakBlock.gameObject.activeSelf) return;
        
        var _position = breakBlock.position;
        Chunk _chunk = world.GetChunkFromVector3(_position);
        (bool _valid, byte _oldBlockId, Vector3 _voxelPos) = _chunk.EditVoxel(_position, 0);
        if (_valid)
        {
            int _commandId = StateManager.instance.AddBlockInteractionCommand(new ModifyChunkCommand(world, player.inventory, _chunk.coord, _voxelPos, _oldBlockId,0));
            ClientSend.ModifyChunk(_commandId, _chunk.coord, _voxelPos, _oldBlockId, 0);
            player.inventory.PickUpItemByBlockId(_oldBlockId, 1);
        }
        
        player.collisionHandler.UpdateColliders();
    }

    private void Start()
    {
        breakBlock = Instantiate(breakBlockPrefab, null).transform;
        placeBlock = Instantiate(placeBlockPrefab, null).transform;
        placeBlockScript = placeBlock.GetComponentInChildren<PlaceBlock>();
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        UpdateMouseLook();
        PlaceCursorBlocks();
    }

    private void UpdateMouseLook()
    {
        mouseDelta = lookInput.ToInputAction().ReadValue<Vector2>();
        
        if (Cursor.lockState == CursorLockMode.None) return;

        currentMouseDelta =
            Vector2.SmoothDamp(currentMouseDelta, mouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraPitch -= currentMouseDelta.y * verticalMouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);
        
        transform.localEulerAngles = Vector3.right * cameraPitch;
        playerBody.Rotate(Vector3.up, currentMouseDelta.x * horizontalMouseSensitivity);
    }

    private void PlaceCursorBlocks() 
    {
        float _step = checkIncrement;
        Vector3 _lastPos = new Vector3();

        while (_step < reach) 
        {
            var _transform = transform;
            Vector3 _pos = _transform.position + _transform.forward * _step;

            if (world.CheckForVoxel(_pos)) 
            {
                breakBlock.position = new Vector3(Mathf.FloorToInt(_pos.x), Mathf.FloorToInt(_pos.y), Mathf.FloorToInt(_pos.z));
                placeBlock.position = _lastPos;

                breakBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            _lastPos = new Vector3(Mathf.FloorToInt(_pos.x), Mathf.FloorToInt(_pos.y), Mathf.FloorToInt(_pos.z));

            _step += checkIncrement;
        }

        breakBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }
}
