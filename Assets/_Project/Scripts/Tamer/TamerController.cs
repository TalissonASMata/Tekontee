using UnityEngine;
using UnityEngine.InputSystem;

public class TamerController : MonoBehaviour
{
    [Header("Movimento")]
    public bool canMove = true;
    public float moveSpeed = 4f;
    public float jumpForce = 6f;
    public float gravity = -18f;

    [Header("Limites")]
    public bool clampX = true;
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Chão lógico")]
    public bool useInitialYAsGround = true;
    public float groundY = 2f;

    private float verticalVelocity;
    private bool isGrounded = true;
    private float lastHorizontalInput;

    private void Start()
    {
        if (useInitialYAsGround)
        {
            groundY = transform.position.y;
        }

        SnapToGround();
    }

    private void Update()
    {
        if (!canMove) return;

        float horizontalInput = ReadHorizontalInput();

        MoveHorizontal(horizontalInput);
        HandleJumpInput();
        ApplyVerticalMovement();
        UpdateFacing(horizontalInput);
    }

    private float ReadHorizontalInput()
    {
        if (Keyboard.current == null) return 0f;

        float input = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            input -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            input += 1f;
        }

        return input;
    }

    private void MoveHorizontal(float input)
    {
        Vector3 position = transform.position;

        position.x += input * moveSpeed * Time.deltaTime;

        if (clampX)
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }

        transform.position = position;
    }

    private void HandleJumpInput()
    {
        if (Keyboard.current == null) return;

        bool jumpPressed =
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
        }
    }

    private void ApplyVerticalMovement()
    {
        Vector3 position = transform.position;

        verticalVelocity += gravity * Time.deltaTime;
        position.y += verticalVelocity * Time.deltaTime;

        if (position.y <= groundY)
        {
            position.y = groundY;
            verticalVelocity = 0f;
            isGrounded = true;
        }

        transform.position = position;
    }

    private void UpdateFacing(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) < 0.01f) return;

        lastHorizontalInput = horizontalInput;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(lastHorizontalInput);
        transform.localScale = scale;
    }

    private void SnapToGround()
    {
        Vector3 position = transform.position;
        position.y = groundY;
        transform.position = position;

        verticalVelocity = 0f;
        isGrounded = true;
    }
}