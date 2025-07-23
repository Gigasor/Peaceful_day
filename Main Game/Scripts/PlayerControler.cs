using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private GameObject exclamationPrefab; // Префаб восклицательного знака

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator;

    private bool canMove = true;

    private FishingController fishingController;
    private GameObject exclamationInstance;

    private Vector3 lastKnownBobberPosition = Vector3.zero; // будем хранить позицию, чтобы не дергать метод

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        fishingController = GetComponent<FishingController>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void Update()
    {
        PlayerInput();
        UpdateAnimation();
        HandleFishingState();
        UpdateExclamationPosition();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void PlayerInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();
    }

    private void Move()
    {
        if (canMove)
        {
            rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
        }
    }

    private void UpdateAnimation()
    {
        bool isMoving = canMove && movement.magnitude > 0;
        animator.SetBool("Move", isMoving);
        animator.SetFloat("moveX", movement.x);
        animator.SetFloat("moveY", movement.y);
    }

    private void HandleFishingState()
    {
        if (movement.magnitude > 0.01f && fishingController != null)
        {
            fishingController.StopFishing();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            canMove = false;
            Debug.Log("Entered Water Trigger");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            canMove = true;
            Debug.Log("Exited Water Trigger");
        }
    }

    // Вызов из FishingController, когда рыба поймалась, с передачей позиции
    public void ShowExclamation(Vector3 bobberPos)
    {
        lastKnownBobberPosition = bobberPos;

        if (exclamationInstance == null)
        {
            exclamationInstance = Instantiate(exclamationPrefab);
        }

        exclamationInstance.SetActive(true);
        exclamationInstance.transform.position = lastKnownBobberPosition + Vector3.up * 0.5f;
    }

    public void HideExclamation()
    {
        if (exclamationInstance != null)
        {
            exclamationInstance.SetActive(false);
        }
    }

    private void UpdateExclamationPosition()
    {
        if (exclamationInstance != null && exclamationInstance.activeSelf)
        {
            exclamationInstance.transform.position = lastKnownBobberPosition + Vector3.up * 0.5f;
        }
    }
}
