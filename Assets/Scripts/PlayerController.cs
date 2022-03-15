using Networking;
using ScriptableObjectEvents;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector3 Position => transform.position;
    
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private InputActionReference jumpInput;
    [SerializeField] Transform groundCheck;
    [SerializeField] float walkSpeed = 6.0f;
    [SerializeField] float jumpHeight = 3.0f;
    [SerializeField] float gravity = -13.0f;
    [SerializeField] private float groundDistance;
    [SerializeField] private LayerMask groundMask;
    [SerializeField][Range(0.0f, 0.1f)] float moveSmoothTime = 0.008f;
    
    private CharacterController _controller;
    
    private  Vector3 _velocity;
    private Vector2 _currentDir = Vector2.zero;
    private Vector2 _currentDirVelocity = Vector2.zero;
    private float _velocityY;

    private Vector2 _movementDirection;
    private bool _isGrounded;
    private bool _jump;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        GetInput();
        UpdateMovement();
    }

    private void GetInput()
    {
        _movementDirection = moveInput.ToInputAction().ReadValue<Vector2>();
        _jump = jumpInput.ToInputAction().IsPressed();
        ClientSend.MovementInput(_movementDirection, _jump, _transform.rotation);
    }

    /// <summary>
    /// Calculate the movement of the player
    /// </summary>
    private void UpdateMovement()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_isGrounded)
        {
            _controller.stepOffset = 0.3f;
            if (_velocity.y < 0f)
            {
                _velocity.y = -1f;
            }

            if (_jump)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            _controller.stepOffset = 0f;
        }

        _currentDir = Vector2.SmoothDamp(_currentDir, _movementDirection, ref _currentDirVelocity, moveSmoothTime);

        Vector3 _move = _transform.right * _currentDir.x + _transform.forward * _currentDir.y;
        _controller.Move(_move * (walkSpeed * Time.deltaTime));

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    /// <summary>
    /// Change the position of the player to the specified one
    /// </summary>
    /// <param name="_pos">The new position of the player</param>
    public void Teleport(Vector3 _pos)
    {
        _controller.enabled = false;
        _transform.position = _pos;
        _controller.enabled = true;
    }
}