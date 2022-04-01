using System;
using Networking;
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
    [SerializeField] private int blockPlaceIntervalInMs;
    [SerializeField] private bool lockCursor = true;

    private Transform placeBlock;
    private Transform breakBlock;
    
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;
    private Vector2 mouseDelta;        
    private float cameraPitch;
    public bool allowBlockModification;
    private double prevBlockPlaceTime;

    private bool isLeftMouseClicked;
    private bool isRightMouseClicked;

    private void OnEnable()
    {
        placeBlockInput.ToInputAction().started += OnRightMouseDown;
        placeBlockInput.ToInputAction().canceled += OnRightMouseUp;
        
        breakBlockInput.ToInputAction().started += OnLeftMouseDown;
        breakBlockInput.ToInputAction().canceled += OnLeftMouseUp;
    }

    private void OnDisable()
    {
        placeBlockInput.ToInputAction().started -= OnRightMouseDown;
        placeBlockInput.ToInputAction().canceled -= OnRightMouseUp;
        
        breakBlockInput.ToInputAction().started -= OnLeftMouseDown;
        breakBlockInput.ToInputAction().canceled -= OnLeftMouseUp;
    }

    private void OnLeftMouseUp(InputAction.CallbackContext _ctx) => isLeftMouseClicked = false;

    private void OnLeftMouseDown(InputAction.CallbackContext _ctx) => isLeftMouseClicked = true;

    private void OnRightMouseUp(InputAction.CallbackContext _ctx) => isRightMouseClicked = false;

    private void OnRightMouseDown(InputAction.CallbackContext _ctx)
    {
        isRightMouseClicked = true;
        PlaceBlock(true);
    }

    private void PlaceBlock(bool clickedNow = false)
    {
        if (!allowBlockModification) return;
        if (!breakBlock.gameObject.activeSelf) return;
        if (!placeBlockScript.canPlace) return;
        if (!player.inventory.IsBlockSelected()) return;
        if (!clickedNow)
            if ((Time.unscaledTimeAsDouble - prevBlockPlaceTime) * 1000 < blockPlaceIntervalInMs) 
                return;
        
        Debug.Log((int) ((Time.unscaledTimeAsDouble - prevBlockPlaceTime) * 1000));
        var _position = placeBlock.position;
        Chunk _chunk = world.GetChunkFromVector3(_position);
        byte _newBlockId = player.inventory.GetBlockId();
        (bool _valid, byte _oldBlockId, Vector3 _voxelPos) = _chunk.EditVoxel(_position, _newBlockId);
        if (_valid)
        {
            ClientSend.ModifyChunk(_chunk.coord, _voxelPos, _oldBlockId, _newBlockId);
            player.inventory.PlaceBlock();
            prevBlockPlaceTime = Time.unscaledTimeAsDouble;
            player.collisionHandler.UpdateColliders();
        }
    }

    private void BreakBlock()
    {
        if (!allowBlockModification) return;
        if (!breakBlock.gameObject.activeSelf) return;
        
        var _position = breakBlock.position;
        Chunk _chunk = world.GetChunkFromVector3(_position);
        (bool _valid, byte _oldBlockId, Vector3 _voxelPos) = _chunk.EditVoxel(_position, 0);
        if (_valid)
        {
            ClientSend.ModifyChunk(_chunk.coord, _voxelPos, _oldBlockId, 0);
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
        BreakPlaceBlock();
    }

    private void BreakPlaceBlock()
    {
        if (isRightMouseClicked)
            PlaceBlock();
        else if (isLeftMouseClicked)
            BreakBlock();
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
