using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FishingMinigameController : MonoBehaviour
{
    [Header("Background Fish")]
    public Transform backgroundFish;
    public float backgroundSpeed = 2f;
    public float backStep = 1f;
    private float direction = 1f;

    [Header("Bar UI")]
    public RectTransform barFish;
    public RectTransform playerBar;
    public RectTransform barContainer;
    public float barSpeed = 200f;
    public float barMinY = 0f;
    public float barMaxY = 1f;

    [Header("Hook")]
    public Transform hookPoint;
    public LineRenderer lineRenderer;

    [Header("Controls")]
    public InputAction moveLeft;
    public InputAction moveRight;

    private Vector3 backgroundStartPos;

    void OnEnable()
    {
        moveLeft.Enable();
        moveRight.Enable();
    }

    void OnDisable()
    {
        moveLeft.Disable();
        moveRight.Disable();
    }

    void Start()
    {
        backgroundStartPos = backgroundFish.position;
    }

    void Update()
    {
        UpdateBackgroundFish();
        UpdatePlayerBar();
        UpdateLineRenderer();
    }

    void UpdateBackgroundFish()
    {
        // Само движение рыбы
        float moveAmount = backgroundSpeed * Time.deltaTime * direction;

        // Игрок нажал Q или E
        bool q = moveLeft.IsPressed();
        bool e = moveRight.IsPressed();

        if (q && direction > 0)
            direction = -direction;
        else if (e && direction < 0)
            direction = -direction;

        // Медленный откат если не нажато ничего
        if (!q && !e)
            moveAmount = -backStep * Time.deltaTime * direction;

        backgroundFish.position += Vector3.right * moveAmount;
    }

    void UpdatePlayerBar()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 pos = playerBar.anchoredPosition;
            pos.y += barSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, barMinY, barMaxY);
            playerBar.anchoredPosition = pos;
        }
        else
        {
            Vector2 pos = playerBar.anchoredPosition;
            pos.y -= barSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, barMinY, barMaxY);
            playerBar.anchoredPosition = pos;
        }
    }

    void UpdateLineRenderer()
    {
        if (lineRenderer && hookPoint && backgroundFish)
        {
            lineRenderer.SetPosition(0, hookPoint.position);
            lineRenderer.SetPosition(1, backgroundFish.position);
        }
    }
}
