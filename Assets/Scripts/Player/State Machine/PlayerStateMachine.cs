using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    // Declare references variables
    private CharacterController _characterController;
    private Animator _animator;
    private PlayerControls _playerInput;

    // Variables to store optimized setter/getter parameter IDs
    private int _isWalkingHash;
    private int _isRunningHash;
    private int _isFallingHash;

    // Variables to store player input values
    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement;
    private Vector3 _appliedMovement;
    private bool _isMovementPressed;
    private bool _isRunPressed;

    // constants
    float _rotationFactorPerFrame = 15.0f;
    float _runMultiplier = 4.0f;
    int _zero = 0;

    // gravity variables
    float _gravity = -9.81f;
    float _groundedGravity = -.05f;

    // jumping variables
    bool _isJumpPressed = false;
    float _initialJumpVelocity;
    float _maxJumpHeight = 4.0f;
    float _maxJumpTime = .75f;
    bool _isJumping = false;
    int _isJumpingHash;
    int _jumpCountHash;
    bool _requireNewJumpPress = false;
    int _jumpCount = 0;
    Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();
    Coroutine _currentJumpResetRoutine = null;

    // State variables
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    #region Getter and setters
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public CharacterController CharacterController { get { return _characterController; } }
    public Animator Animator { get { return _animator; } }
    public Coroutine CurrentJumpResetRoutine { get { return _currentJumpResetRoutine; } set { _currentJumpResetRoutine = value; } }
    public Dictionary<int, float> InitialJumpVelocities { get { return _initialJumpVelocities; } }
    public Dictionary<int, float> JumpGravities { get { return _jumpGravities; } }
    public int JumpCount { get { return _jumpCount; } set { _jumpCount = value; } }
    public int IsWalkingHash { get { return _isWalkingHash; } }
    public int IsRunningHash { get { return _isRunningHash; } }
    public int IsJumpingHash { get { return _isJumpingHash; } }
    public int IsFallingHash { get { return _isFallingHash; } }
    public int JumpCountHash { get { return _jumpCountHash; } }
    public bool IsMovementPressed { get { return _isMovementPressed; } }
    public bool IsRunPressed { get { return _isRunPressed; } }
    public bool RequireNewJumpPress { get { return _requireNewJumpPress; } set { _requireNewJumpPress = value; } }
    public bool IsJumping { set { _isJumping = value; } }
    public bool IsJumpPressed { get { return _isJumpPressed; } }
    public float GroundedGravity { get { return _groundedGravity; } set { _groundedGravity = value; } }
    public float Gravity { get { return _gravity; } }
    public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; } }
    public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }
    public float AppliedMovementX { get { return _appliedMovement.x; } set { _appliedMovement.x = value; } }
    public float AppliedMovementZ { get { return _appliedMovement.z; } set { _appliedMovement.z = value; } }
    public float RunMultiplier { get { return _runMultiplier; } }
    public Vector2 CurrentMovementInput { get { return _currentMovementInput; } }
    #endregion

    private void Awake()
    {
        // initially set reference variables
        _playerInput = new PlayerControls();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        // Setup state
        _states = new PlayerStateFactory(this);
        Debug.Log($"States var: {_states}");
        _currentState = _states.Grounded();
        _currentState.EnterState();
        Debug.Log($"Current state: {_currentState}");

        // set the parameter hash references
        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _jumpCountHash = Animator.StringToHash("jumpCount");
        _isFallingHash = Animator.StringToHash("isFalling");

        // set the player input callbacks
        _playerInput.MyPlayer.Move.started += OnMovementInput;
        _playerInput.MyPlayer.Move.canceled += OnMovementInput;
        _playerInput.MyPlayer.Move.performed += OnMovementInput;
        _playerInput.MyPlayer.Sprint.started += OnRun;
        _playerInput.MyPlayer.Sprint.canceled += OnRun;
        _playerInput.MyPlayer.Jump.started += OnJump;
        _playerInput.MyPlayer.Jump.canceled += OnJump;

        SetupJumpVariables();
    }

    private void Update()
    {
        HandleRotation();
        _currentState.UpdateStates();
        _characterController.Move(_appliedMovement * Time.deltaTime);
    }

    // set the initial velocity and gravity using jump heights and durations
    private void SetupJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * (_maxJumpHeight + 2)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (_maxJumpHeight + 2)) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * (_maxJumpHeight + 4)) / Mathf.Pow((timeToApex * 1.5f), 2);
        float thirdJumpInitialVelocity = (2 * (_maxJumpHeight + 4)) / (timeToApex * 1.5f);

        _initialJumpVelocities.Add(1, _initialJumpVelocity);
        _initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        _initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        _jumpGravities.Add(0, _gravity);
        _jumpGravities.Add(1, _gravity);
        _jumpGravities.Add(2, secondJumpGravity);
        _jumpGravities.Add(3, thirdJumpGravity);
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt;

        // The change in position our character should point to
        positionToLookAt.x = _currentMovementInput.x;
        positionToLookAt.y = _zero;
        positionToLookAt.z = _currentMovementInput.y;

        // The current rotation of our character
        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            // Creates a new rotation based on where the player is currently pressing
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            // Rotate the character to face the positionToLookAt
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }
    }

    // callback handler function to set the player input values
    void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _isMovementPressed = _currentMovementInput.x != _zero || _currentMovementInput.y != _zero;
        Debug.Log($"Is movement pressed: {_isMovementPressed}");
    }


    // callback handler function for jump buttons
    void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
        _requireNewJumpPress = false;
    }

    // callback handler function for run buttons
    void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    void OnEnable()
    {
        // enable the character controls action map
        _playerInput.MyPlayer.Enable();
    }

    void OnDisable()
    {
        // disable the character controls action map
        _playerInput.MyPlayer.Disable();
    }
}
