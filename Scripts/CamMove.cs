using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMove : MonoBehaviour
{
    public Transform player; // ������ �� ������
    public float smoothSpeed = 0.125f; // �������� �����������
    public Vector3 offset; // �������� ������ ������������ ������
    public float zoomSpeed = 2.0f; // �������� ����������� ������
    public float minZoom = 5f; // ����������� ������ (�������)
    public float maxZoom = 10f; // ������������ ������ (�������)

    private Camera mainCamera;

    // ���� ����� ����������� �� �����������
    public bool useBounds = false;
    public Vector2 minCameraPos;
    public Vector2 maxCameraPos;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("CamMove: Player �� ��������!");
            enabled = false;
            return;
        }

        float initialDistance = Vector3.Distance(transform.position, player.position);
        mainCamera.orthographicSize = Mathf.Clamp(initialDistance, minZoom, maxZoom);
    }

    void LateUpdate()
    {
        if (player == null) return;

        // ������ �������� ������� ������
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // ���� ������ ������� � ��������� ��
        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minCameraPos.x, maxCameraPos.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minCameraPos.y, maxCameraPos.y);
        }

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);

        // ������� ��� � ����������� �� ���������� �� ������
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float targetZoom = Mathf.Clamp(distanceToPlayer, minZoom, maxZoom);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
    }
}
