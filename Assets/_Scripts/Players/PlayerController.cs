using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("PLAYER MOVEMENT")]
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Vector3 _groundCheckOffset;

    [Header("PLAYER LOOK")]
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private Transform _playerCam;
    [SerializeField] private float _upperLimit = -40f;
    [SerializeField] private float _bottomLimit = 70f;
    [SerializeField] private float _mouseSensitivity = 20f;

    [Header("PLAYER ANIMATION")]
    [SerializeField] private float _animBlendSpeed = 8.9f;

    private Rigidbody _rb;
    private Animator _animator;
    private bool _hasAnimator;
    private int _xVelHash;
    private int _yVelHash;
    private float _xRotation;
    private Camera _cam;

    private Vector2 _currentVelocity;
    private bool _isGrounded = false;

    private GameInput _input;

    private GameInput Input {
        get {
            if (_input != null) return _input;
            return _input = new GameInput();
        }
    }

    private void OnEnable() => Input.Enable();

    private void OnDisable() => Input.Disable();

    private void Start() {
        _hasAnimator = TryGetComponent<Animator>(out _animator);
        _rb = GetComponent<Rigidbody>();
        _cam = _playerCam.GetComponent<Camera>();

        _xVelHash = Animator.StringToHash("XVel");
        _yVelHash = Animator.StringToHash("YVel");
    }

    #region UPDATE
    private void Update() {
        HideCursor();
    }

    private void HideCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }
    #endregion

    #region FIXED_UPDATE

    void FixedUpdate() {
        DetectGround();
        Move();
    }

    private void DetectGround() {
        _isGrounded = Physics.CheckSphere(transform.position + _groundCheckOffset, _groundDistance, _groundMask);
    }

    private void Move() {
        if (!_hasAnimator) return;

        float targetSpeed = _walkSpeed;
        if (Input.Player.Move.ReadValue<Vector2>() == Vector2.zero) targetSpeed = 0.1f;

        _currentVelocity.x = Mathf.Lerp(_currentVelocity.x, Input.Player.Move.ReadValue<Vector2>().x * targetSpeed, _animBlendSpeed * Time.fixedDeltaTime);
        _currentVelocity.y = Mathf.Lerp(_currentVelocity.y, Input.Player.Move.ReadValue<Vector2>().y * targetSpeed, _animBlendSpeed * Time.fixedDeltaTime);

        var xVelDifference = _currentVelocity.x - _rb.velocity.x;
        var zVelDifference = _currentVelocity.y - _rb.velocity.y;

        _rb.AddForce(transform.TransformVector(new Vector3(xVelDifference, 0f, zVelDifference)), ForceMode.VelocityChange);

        _animator.SetFloat(_xVelHash, _currentVelocity.x);
        _animator.SetFloat(_yVelHash, _currentVelocity.y);
    }

    #endregion

    #region LATE_UPDATE

    private void LateUpdate() {
        CamMovement();
    }

    private void CamMovement() {
        if (!_hasAnimator) return;

        float mouseX = Input.Player.Look.ReadValue<Vector2>().x;
        float mouseY = Input.Player.Look.ReadValue<Vector2>().y;
        _playerCam.position = _cameraRoot.position;

        _xRotation -= mouseY * _mouseSensitivity * Time.deltaTime;
        _xRotation = Mathf.Clamp(_xRotation, _upperLimit, _bottomLimit);

        _playerCam.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up, mouseX * _mouseSensitivity * Time.deltaTime);
    }

    #endregion
}
