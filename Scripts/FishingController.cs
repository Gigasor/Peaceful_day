using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class FishingController : MonoBehaviour
{
    [Header("Объекты")]
    public GameObject idleRod;
    public Transform rodTip;
    public Transform rodSocketCenter;
    public Transform rodSocketLeft;
    public Transform rodSocketRight;
    public LineRenderer lineRenderer;

    [Header("Tilemap")]
    public Tilemap waterTilemap;
    public AnimatedTile fishingAnimationTile;
    public TileBase defaultWaterTile;

    [Header("Рыбы")]
    public GameObject fishPrefabSmall;
    public GameObject fishPrefabMedium;
    public GameObject fishPrefabLarge;

    [Header("Настройки")]
    public float fishApproachDuration = 3f;
    public float fishMoveRadius = 0.2f;

    // 👇 Добавлены разные таймеры
    [Header("Таймер мини-игры (секунды)")]
    public float miniGameTimeoutSmall = 2f;
    public float miniGameTimeoutMedium = 3f;
    public float miniGameTimeoutLarge = 4f;

    private float currentMiniGameTimeout = 3f; // 👈 Используется внутри

    private Animator animator;
    private bool isHoldingRod = false;
    private bool isFishing = false;

    private PlayerControls inputActions;
    private Vector3 fishingTarget;
    private Transform currentSocket;

    private Vector3Int lastFishingTilePos;

    private GameObject currentFish;
    private bool miniGameActive = false;
    private bool miniGameSuccess = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        inputActions = new PlayerControls();

        idleRod?.SetActive(false);

        if (lineRenderer != null)
        {
            lineRenderer.gameObject.SetActive(false);
            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 10;

            if (lineRenderer.material == null)
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            lineRenderer.widthMultiplier = 0.03f;
            lineRenderer.numCapVertices = 5;
            lineRenderer.useWorldSpace = true;
        }

        if (waterTilemap == null)
            Debug.LogError("❌ Water Tilemap не назначен!");
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Movement.Fish.performed += OnFishPressed;
        inputActions.Movement.Fish.canceled += OnFishReleased;
    }

    private void OnDisable()
    {
        inputActions.Movement.Fish.performed -= OnFishPressed;
        inputActions.Movement.Fish.canceled -= OnFishReleased;
        inputActions.Disable();
    }

    private void Update()
    {
        if (isFishing || !isHoldingRod)
            return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        if (idleRod != null)
        {
            Vector3 dir = mouseWorld - idleRod.transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            idleRod.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (lineRenderer != null && rodTip != null)
        {
            if (!lineRenderer.gameObject.activeSelf)
                lineRenderer.gameObject.SetActive(true);

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, rodTip.position);
            lineRenderer.SetPosition(1, mouseWorld);
        }
    }

    private void OnFishPressed(InputAction.CallbackContext ctx)
    {
        if (isFishing) return;

        isHoldingRod = true;
        idleRod?.SetActive(true);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        if (lineRenderer != null && rodTip != null)
        {
            lineRenderer.gameObject.SetActive(true);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, rodTip.position);
            lineRenderer.SetPosition(1, mouseWorld);
        }
    }

    private void OnFishReleased(InputAction.CallbackContext ctx)
    {
        if (!isHoldingRod || isFishing) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        isHoldingRod = false;

        if (IsWaterTileAtPosition(mouseWorld))
        {
            fishingTarget = mouseWorld;
            lastFishingTilePos = waterTilemap.WorldToCell(fishingTarget);
            SelectRodSocketByDirection(mouseWorld);
            StartCoroutine(DoFishing());
        }
        else
        {
            idleRod?.SetActive(false);
            lineRenderer?.gameObject.SetActive(false);
        }
    }

    private void SelectRodSocketByDirection(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        currentSocket = Mathf.Abs(toTarget.x) < 0.1f
            ? rodSocketCenter
            : (toTarget.x > 0 ? rodSocketRight : rodSocketLeft);
    }

    private bool IsWaterTileAtPosition(Vector3 worldPos)
    {
        if (waterTilemap == null) return false;

        Vector3Int tilePos = waterTilemap.WorldToCell(worldPos);
        return waterTilemap.HasTile(tilePos);
    }

    private IEnumerator DoFishing()
    {
        isFishing = true;
        animator.SetBool("isFishing", true);

        yield return null;

        idleRod?.SetActive(false);

        if (waterTilemap != null && fishingAnimationTile != null)
        {
            waterTilemap.SetTile(lastFishingTilePos, fishingAnimationTile);
        }

        if (lineRenderer != null && currentSocket != null)
        {
            lineRenderer.gameObject.SetActive(true);
            lineRenderer.positionCount = 2;
        }

        SpawnRandomFishAtPosition(fishingTarget);
        yield return StartCoroutine(FishApproachCoroutine());
        yield return StartCoroutine(MiniGameCoroutine());

        CleanupFishing();

        yield return new WaitForSeconds(0.5f);

        animator.SetBool("isFishing", false);
        lineRenderer?.gameObject.SetActive(false);

        if (waterTilemap != null && defaultWaterTile != null)
        {
            waterTilemap.SetTile(lastFishingTilePos, defaultWaterTile);
        }

        isFishing = false;
    }

    private void SpawnRandomFishAtPosition(Vector3 pos)
    {
        if (currentFish != null)
        {
            Destroy(currentFish);
            currentFish = null;
        }

        GameObject[] fishPrefabs = { fishPrefabSmall, fishPrefabMedium, fishPrefabLarge };
        int index = Random.Range(0, fishPrefabs.Length);
        GameObject prefab = fishPrefabs[index];

        // 👇 Устанавливаем таймер для мини-игры в зависимости от рыбы
        switch (index)
        {
            case 0: currentMiniGameTimeout = miniGameTimeoutSmall; break;
            case 1: currentMiniGameTimeout = miniGameTimeoutMedium; break;
            case 2: currentMiniGameTimeout = miniGameTimeoutLarge; break;
            default: currentMiniGameTimeout = 3f; break;
        }

        currentFish = Instantiate(prefab, pos, Quaternion.identity);
    }

    private IEnumerator FishApproachCoroutine()
    {
        if (currentFish == null) yield break;

        Vector3 startPos = currentFish.transform.position;
        Vector3 targetPos = fishingTarget;

        float elapsed = 0f;

        while (elapsed < fishApproachDuration)
        {
            float t = elapsed / fishApproachDuration;
            Vector2 offset = Random.insideUnitCircle * fishMoveRadius * 0.3f;
            currentFish.transform.position = Vector3.Lerp(startPos, targetPos, t) + (Vector3)offset;

            if (lineRenderer != null && currentSocket != null)
                lineRenderer.SetPosition(0, currentSocket.position);

            lineRenderer?.SetPosition(1, fishingTarget);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (currentFish != null)
            currentFish.transform.position = fishingTarget;
    }

    private IEnumerator MiniGameCoroutine()
    {
        miniGameActive = true;
        miniGameSuccess = false;

        float elapsed = 0f;

        while (elapsed < currentMiniGameTimeout)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                miniGameSuccess = true;
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        miniGameActive = false;

        if (miniGameSuccess)
        {
            Debug.Log("🎣 Рыба поймана!");
        }
        else
        {
            Debug.Log("🐟 Рыба уплыла...");
        }
    }

    private void CleanupFishing()
    {
        if (currentFish != null)
        {
            Destroy(currentFish);
            currentFish = null;
        }
    }

    public void StopFishing()
    {
        if (!isFishing) return;

        StopAllCoroutines();
        isFishing = false;

        animator.SetBool("isFishing", false);
        idleRod?.SetActive(false);
        lineRenderer?.gameObject.SetActive(false);

        if (waterTilemap != null && defaultWaterTile != null)
        {
            waterTilemap.SetTile(lastFishingTilePos, defaultWaterTile);
        }

        CleanupFishing();
    }

    public Vector3 GetBobberPosition()
    {
        return fishingTarget;
    }
}
