using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class GameplayCameraController : MonoBehaviour
{
    [Header("Referências")]
    public PlayerControlManager controlManager;
    public Transform tamer;
    public Transform playerMon;
    public Transform enemyMon;

    [Header("Modo Tamer - visão estratégica")]
    public float tamerCenterY = 1.35f;
    public float tamerOrthographicSize = 4.8f;
    public float tamerFollowStrengthX = 0.45f;

    [Header("Modo Mon - câmera de luta")]
    public float monCenterY = 0.25f;
    public float monOrthographicSize = 2.35f;
    public bool useSmallDistanceZoom = true;
    public float distanceZoomMultiplier = 0.10f;
    public float monMinOrthographicSize = 2.25f;
    public float monMaxOrthographicSize = 2.75f;

    [Header("Limites horizontais da arena")]
    public bool useBounds = true;
    public float minX = -4.5f;
    public float maxX = 4.5f;

    [Header("Fim da batalha")]
    public bool keepLastGameplayCameraOnEnd = true;

    [Header("Suavização")]
    public float positionSmoothTime = 0.10f;
    public float zoomSmoothTime = 0.10f;

    private Camera cameraComponent;
    private Vector3 positionVelocity;
    private float zoomVelocity;

    private PlayerControlMode lastGameplayMode = PlayerControlMode.Tamer;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        cameraComponent.orthographic = true;
    }

    private void LateUpdate()
    {
        PlayerControlMode cameraMode = GetCameraMode();

        Vector3 targetPosition = GetTargetPosition(cameraMode);
        float targetSize = GetTargetOrthographicSize(cameraMode);

        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        cameraComponent.orthographicSize = Mathf.SmoothDamp(
            cameraComponent.orthographicSize,
            targetSize,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    private PlayerControlMode GetCameraMode()
    {
        if (controlManager == null)
        {
            return lastGameplayMode;
        }

        if (controlManager.GameplayEnabled)
        {
            lastGameplayMode = controlManager.CurrentMode;
            return controlManager.CurrentMode;
        }

        if (keepLastGameplayCameraOnEnd)
        {
            return lastGameplayMode;
        }

        return PlayerControlMode.Tamer;
    }

    private Vector3 GetTargetPosition(PlayerControlMode cameraMode)
    {
        if (cameraMode == PlayerControlMode.Mon)
        {
            return GetFightCameraPosition();
        }

        return GetTamerCameraPosition();
    }

    private Vector3 GetTamerCameraPosition()
    {
        float targetX = 0f;

        if (tamer != null)
        {
            targetX = tamer.position.x * tamerFollowStrengthX;
        }

        return new Vector3(targetX, tamerCenterY, -10f);
    }

    private Vector3 GetFightCameraPosition()
    {
        float targetX = 0f;

        if (playerMon != null && enemyMon != null)
        {
            targetX = (playerMon.position.x + enemyMon.position.x) * 0.5f;
        }
        else if (playerMon != null)
        {
            targetX = playerMon.position.x;
        }

        return new Vector3(targetX, monCenterY, -10f);
    }

    private float GetTargetOrthographicSize(PlayerControlMode cameraMode)
    {
        if (cameraMode == PlayerControlMode.Mon)
        {
            return GetFightCameraSize();
        }

        return tamerOrthographicSize;
    }

    private float GetFightCameraSize()
    {
        if (!useSmallDistanceZoom) return monOrthographicSize;
        if (playerMon == null || enemyMon == null) return monOrthographicSize;

        float distance = Mathf.Abs(playerMon.position.x - enemyMon.position.x);
        float targetSize = monOrthographicSize + distance * distanceZoomMultiplier;

        return Mathf.Clamp(targetSize, monMinOrthographicSize, monMaxOrthographicSize);
    }
}