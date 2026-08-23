using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [SerializeField] private float speed = 3f;
    [SerializeField] private float dodgePower = 10f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float cooldown = 5f;

    private Rigidbody2D _rigidbody;
    private SpriteRenderer _sprite;
    private InputSystem_Actions _inputActions;
    private PlayerStats _stats;
    private bool _isDodging;
    private bool _canDodge = true;
    private Vector2 _lastMovementDirection = Vector2.right;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        Instance = this;
        _stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();
        _inputActions.Player.Dodge.performed += OnDodgeInput;
    }

    private void Update()
    {
        if (_isDodging)
        {
            return;
        }

        Move();
        UpdateSpriteDirection();
    }

    private void Move()
    {
        Vector2 input = GetMovementVector();
        if (input != Vector2.zero)
        {
            _lastMovementDirection = input;
        }

        _rigidbody.linearVelocity = input * speed;
    }

    private void UpdateSpriteDirection()
    {
        if (_rigidbody.linearVelocity.x < -0.1f)
        {
            _sprite.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (_rigidbody.linearVelocity.x > 0.1f)
        {
            _sprite.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private Vector2 GetMovementVector()
    {
        return _inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void OnDodgeInput(InputAction.CallbackContext context)
    {
        if (_canDodge && !_isDodging)
        {
            StartCoroutine(PerformDodge());
        }
    }

    private IEnumerator PerformDodge()
    {
        _canDodge = false;
        _isDodging = true;
        if (_stats != null)
        {
            _stats.invulnerability = true;
        }

        Vector2 dodgeDirection = GetMovementVector();
        if (dodgeDirection == Vector2.zero)
        {
            dodgeDirection = _lastMovementDirection;
        }

        _rigidbody.linearVelocity = dodgeDirection * dodgePower;
        yield return new WaitForSeconds(dodgeDuration);
        _isDodging = false;
        if (_stats != null)
        {
            _stats.invulnerability = false;
        }

        yield return new WaitForSeconds(cooldown);
        _canDodge = true;
    }

    private void OnDisable()
    {
        if (_inputActions == null)
        {
            return;
        }

        _inputActions.Player.Dodge.performed -= OnDodgeInput;
        _inputActions.Disable();
    }
}
