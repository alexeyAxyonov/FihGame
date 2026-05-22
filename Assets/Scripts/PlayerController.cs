using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDataPersistence
{
    public float characterSpeed = 30f;

    public float dashSpeed = 60f;  // dash feels a bit off due to gravity interactions
    public float dashDuration = 0.35f;
    public float dashCooldown = 0.07f;
    private bool _isDashing = false;
    private float _dashTimer = 0f; 
    private float _dashCooldownTimer = 0f;
    private float dashStaminaCost = 1f;
    private Vector3 _dashDirection;

    public float gravity = -9.81f * 2;
    public float jumpHeight = 5f;
    public float stamina = 3f;
    public bool jumpPressed = false;
    public float currentStamina = 3f; // Для двойных прыжков и рывков

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    bool isGrounded;
    bool isMoving;

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private WeaponHolder weaponHolder;

    private Vector2 _move;
    private float _verticalVelocity = 0f;

    private int timesShot;

    public void LoadData(GameData data)
    {
        timesShot = data.timesShot;
        if (data.hasSavedPosition)
        {
            transform.position = data.playerPosition;
        }
    }
    public void SaveData(ref GameData data)
    {
        data.timesShot = timesShot;
        data.playerPosition = transform.position;
        data.hasSavedPosition = true;
    }

    public void OnMove(InputValue val)
    {
        _move = val.Get<Vector2>();
    }
    public void OnJump(InputValue val)
    {
        if (val.isPressed)
        {
            if (isGrounded)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                Debug.Log($" SingleJump: Velocity: {_verticalVelocity}, Grounded: {isGrounded}");
            }
            else if (currentStamina >= 1 && !isGrounded)
            {
                Debug.Log($" DoubleJump: Velocity: {_verticalVelocity}, Grounded: {isGrounded}");
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                currentStamina -= 1;
            }
        }
    }

    public void OnSprint(InputValue val)
    {
        if (!val.isPressed) return;
        if (_isDashing) return;
        if (_dashCooldownTimer > 0f) return;
        if (currentStamina < dashStaminaCost) return;
        if (_move.magnitude < 0.1f) return;

        _isDashing = true;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;
        currentStamina -= dashStaminaCost;

        _dashDirection = (GetForward() * _move.y + GetRight() * _move.x).normalized;
    }

    public void OnAttack(InputValue val)
    {
        if (val.isPressed)
        {
            weaponHolder.Attack();
            timesShot++;
        }
    }
    public void OnSwitchWeaponNext(InputValue val)
    {
        if (val.isPressed)
            weaponHolder.SwitchToNext();
    }

    public void OnSwitchWeaponPrevious(InputValue val)
    {
        if (val.isPressed)
            weaponHolder.SwitchToPrevious();
    }
    public void OnWeaponSlot(InputValue val)
    {
        int slot = val.Get<int>() - 1;
        weaponHolder.EquipWeapon(slot);
    }

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        timesShot = 0;
    }

    public void Update()
    {
        if (!_isDashing)
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
        else
        {
            _verticalVelocity = 0f;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = 0f;
        }

        Vector3 moveDirection = GetForward() * _move.y + GetRight() * _move.x;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
            }

            Vector3 dashMovement = _dashDirection * dashSpeed * Time.deltaTime;
            _characterController.Move(dashMovement);
        }
        else
        {
            Vector3 horizontalMovement = moveDirection * Time.deltaTime * characterSpeed;
            Vector3 verticalMovement = Vector3.up * _verticalVelocity * Time.deltaTime;

            _characterController.Move(horizontalMovement + verticalMovement);
        }

        if (!_isDashing && currentStamina < stamina)
        {
            currentStamina = ((currentStamina + 0.33f * Time.deltaTime) < stamina)? currentStamina + 0.33f * Time.deltaTime : stamina;
        }

        if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
    }

    private Vector3 GetForward()
    {
        Vector3 forward = _camera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    private Vector3 GetRight()
    {
        Vector3 right = _camera.transform.right;
        right.y = 0;
        return right.normalized;
    }
}
