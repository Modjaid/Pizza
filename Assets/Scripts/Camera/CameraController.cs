using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Written by Oleg "tarsss" Scheglov.
// https://github.com/tarsss

public enum CameraMode
{
    FollowPlayer,
    SafeSpace,
    FlyBy,
    HitByCar,
    InVehicle,
}

/// <summary>
/// Simple general camera controller.
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Follow player mode settings")]

    public GameObject player;
    [SerializeField, Range(1, 20)]
    private float moveSpeed = 1;
    [SerializeField, Range(1, 20)]
    private float rotateSpeed = 1;
    [SerializeField, Range(5, 20)]
    private float distance = 5;
    [SerializeField, Range(2, 20)]
    private float height = 2;
    [SerializeField, Range(20, 60)]
    private float maxRotation = 20;

    private float rotation = 1;

    [Header("Safespace mode settings")]

    public float safeSpaceRotationSpeed;

    [Header("Fly-by mode settings")]

    public Vector3 startingAngle;
    public float waitAtTargetTime;
    public float startingFollowDistance;
    public float smoothTime;
    public float maxVelocity;

    [Header("Hit by car mode settings")]

    public float initialTargetDistance;
    public float targetTargetDistance;
    public float smoothTime1;
    public float maxVelocity1;

    [Header("ETC")]

    [SerializeField] private CameraMode mode = CameraMode.FlyBy;
    public float sprintEffectFOVAngle = 75f;

    public delegate void CameraModeChanged(CameraMode newMode);
    public event CameraModeChanged onCameraModeChanged;

    private DeliveryJob job;
    private Vector3 secondPoint;
    private Vector3 firstPoint;
    private IEnumerator flyByRoutine;
    private IEnumerator hitByCarRoutine;
    private IEnumerator sprintEffectRoutine;
    private float defaultFOV;
    private CameraMode prevMode;
    new private Camera camera;
    private PlayerAxisController playerController;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        camera = GetComponent<Camera>();
        playerController = player.GetComponent<PlayerAxisController>();

        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Confined;

        if (!GameManager.Instance.jumpStartGame)
        {
            job = DeliveryJobManager.Instance.CurrentJob;

            if (mode == CameraMode.FlyBy)
            {
                ChangeMode(CameraMode.FlyBy);
                UIManager.Instance.SetSkipFlyByButtonActive(true);
                UIManager.Instance.HideDeliveryUI();
                UIManager.Instance.SetBlockClicksPanelActive(true);
            }
        }
        else
        {
            ChangeMode(CameraMode.FollowPlayer);
            UIManager.Instance.SetSkipFlyByButtonActive(false);
            UIManager.Instance.ShowDeliveryUI();
        }

        defaultFOV = camera.fieldOfView;
    }

    private void LateUpdate()
    {
        switch (mode)
        {
            case CameraMode.FollowPlayer:
                FollowPlayer();
                break;

            case CameraMode.SafeSpace:
                RotateInSafeSpace();
                break;

            case CameraMode.InVehicle:
                FollowVehicle();
                break;
        }
    }

    public void Rotate(float xAxis)
    {
        xAxis *= -0.10f;
        rotation = Mathf.Clamp(rotation + xAxis, 0, maxRotation);
    }

    public void ChangeMode(CameraMode newMode)
    {
        prevMode = mode;
        mode = newMode;
        job = DeliveryJobManager.Instance.CurrentJob;
        onCameraModeChanged?.Invoke(newMode);

        if (flyByRoutine != null) StopCoroutine(flyByRoutine);
        if (hitByCarRoutine != null) StopCoroutine(hitByCarRoutine);
        
        switch(newMode)
        {
            case CameraMode.FlyBy:
            StartFlyByRoutine();
            UIManager.Instance.SetPlayerControlsActive(false);
            break;

            case CameraMode.HitByCar:
            StartHitByCarRoutine();
            UIManager.Instance.SetPlayerControlsActive(false);
            break;

            case CameraMode.SafeSpace:
            UIManager.Instance.SetPlayerControlsActive(false);
            break;

            default:
            UIManager.Instance.SetPlayerControlsActive(true);
            break;
        }
    }

    public void SkipFlyBy()
    {
        if (flyByRoutine != null)
        {
            StopCoroutine(flyByRoutine);
        }

        if (prevMode != CameraMode.FlyBy)
        {
            ChangeMode(prevMode);
        }
        else
        {
            ChangeMode(CameraMode.FollowPlayer);
        }

        Quaternion targetRotation = Quaternion.Euler(rotation, player.transform.eulerAngles.y, 0f);
        Vector3 dir = Quaternion.AngleAxis(rotation, player.transform.right) * player.transform.forward;
        var targetPosition = player.transform.position - dir * distance;
        targetPosition.y += height;
        transform.position = targetPosition;
        transform.rotation = targetRotation;

        GameManager.Instance.StartGame();
        UIManager.Instance.SetSkipFlyByButtonActive(false);
        UIManager.Instance.ShowDeliveryUI();
        UIManager.Instance.SetBlockClicksPanelActive(false);
        if (playerController.InVehicle) UIManager.Instance.ShowInVehicleUI();
        //angle.y = player.transform.eulerAngles.y;
    }

    public void StopSprintEffectImmediately()
    {
        StopCoroutine(sprintEffectRoutine);
        camera.fieldOfView = defaultFOV;
    }

    public void SetSprintEffectEnabled(bool enabled)
    {
        if (sprintEffectRoutine != null)
            StopCoroutine(sprintEffectRoutine);
        sprintEffectRoutine = SprintEffectRoutine(enabled);
        StartCoroutine(sprintEffectRoutine);
    }

    private void MoveCamera()
    {
        var target = player.transform.position + (player.transform.rotation * new Vector3(0, height, -distance));
        transform.position = Vector3.Lerp(transform.position, target, moveSpeed * Time.deltaTime);
    }

    private void RotateCamera()
    {
        var target = player.transform.position - transform.position + Vector3.up * rotation;
        var euler = Quaternion.LookRotation(target, Vector3.up).eulerAngles;
        euler.x = rotation;
        var lerp = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler), rotateSpeed * Time.deltaTime);
        transform.rotation = lerp;
    }

    private void FollowPlayer()
    {
        MoveCamera();
        RotateCamera();
    }

    private void FollowVehicle()
    {
        /*
        if (target == null) return;

        if (rotate && player != null)
        {
            float newAngle = angle.y;

            float speed = rotationSpeed;
            Quaternion yAngleTarget = Quaternion.Euler(0, player.transform.eulerAngles.y, 0);
            newAngle = Quaternion.RotateTowards(transform.rotation, yAngleTarget, speed * Time.deltaTime).eulerAngles.y;

            angle.y = newAngle;
        }
        else if (fixYRotation && player != null)
        {
            angle.y = target.eulerAngles.y;
        }

        transform.rotation = Quaternion.Euler(angle.x, angle.y, angle.z);
        transform.position = target.transform.position + targetOffset - transform.forward * followDistance;
        */
    }

    private void RotateInSafeSpace()
    {
        float oldAngle = transform.eulerAngles.y;
        float deltaAngle = 0f;

        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                firstPoint = Input.GetTouch(0).position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                secondPoint = Input.GetTouch(0).position;
                deltaAngle = (secondPoint.x - firstPoint.x) * 180 / Screen.width;
            }
        }

        #if UNITY_EDITOR
        deltaAngle = Input.GetAxis("Mouse X") * 1.5f;
        #endif

        //if (Mathf.Abs(oldAngle - newAngle) > 0.2f) UIManager.Instance.SetSafespaceHintActive(false);
        
        //var target = player.transform.position + new Vector3(0, height, -distance);
        //transform.position = Vector3.Lerp(transform.position, target, moveSpeed * Time.deltaTime);

        transform.RotateAround(player.transform.position, Vector3.up, deltaAngle);
        var dir = (player.transform.position - transform.position).normalized;
        var rotation = Quaternion.LookRotation(dir).eulerAngles;
        transform.eulerAngles = new Vector3(0, rotation.y, 0);
        if (Input.touchCount > 0)
        {
            firstPoint = Input.GetTouch(0).position;
        }
    }

    private void StartFlyByRoutine()
    {
        if (flyByRoutine != null)
            StopCoroutine(flyByRoutine);
        flyByRoutine = FlyByRoutine();
        StartCoroutine(flyByRoutine);
    }

    private void StartHitByCarRoutine()
    {
        if (hitByCarRoutine != null)
            StopCoroutine(hitByCarRoutine);
        hitByCarRoutine = HitByCarRoutine();
        StartCoroutine(hitByCarRoutine);
    }

    private IEnumerator FlyByRoutine()
    {
        // Set starting position.
        transform.rotation = Quaternion.Euler(startingAngle.x, startingAngle.y, startingAngle.z);
        transform.position = job.destination.position - transform.forward * startingFollowDistance;

        // Smoothly move to player position.
        Vector3 velocity = Vector3.zero;
        Vector3 targetPosition = player.transform.position - transform.forward * startingFollowDistance;

        UIManager.Instance.HideDeliveryUI();
        UIManager.Instance.HideInVehicleUI();

        yield return new WaitForSecondsRealtime(waitAtTargetTime);

        while ((transform.position - targetPosition).magnitude > 6f)
        {
            Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, maxVelocity, Time.unscaledDeltaTime);
            transform.position = newPosition;
            yield return null;
        }

        Quaternion deriv = Quaternion.identity;
        Quaternion targetRotation = Quaternion.Euler(rotation, player.transform.eulerAngles.y, 0f);
        Vector3 dir = Quaternion.AngleAxis(rotation, player.transform.right) * player.transform.forward;
        targetPosition = player.transform.position - dir * distance;
        targetPosition.y += height;

        // Rotate to player Y angle and move to camera FollowPlayer position.
        while ((transform.position - targetPosition).magnitude > 0.15f)
        {
            Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.6f, maxVelocity, Time.unscaledDeltaTime);
            Quaternion newRotation = Utils.QuaternionSmoothDamp(transform.rotation, targetRotation, ref deriv, 1f);

            transform.position = newPosition;
            transform.rotation = newRotation;

            yield return null;
        }

        if(prevMode != CameraMode.FlyBy)
        {
            ChangeMode(prevMode);
        }
        else
        {
            ChangeMode(CameraMode.FollowPlayer);
        }

        GameManager.Instance.StartGame();
        UIManager.Instance.SetSkipFlyByButtonActive(false);
        UIManager.Instance.ShowDeliveryUI();
        UIManager.Instance.SetBlockClicksPanelActive(false);
        if(playerController.InVehicle) UIManager.Instance.ShowInVehicleUI();

        yield return null;
    }

    private IEnumerator SprintEffectRoutine(bool enabled)
    {
        float targetFov = enabled ? sprintEffectFOVAngle : defaultFOV;
        float velocity = 0f;

        while (Mathf.Abs(camera.fieldOfView - targetFov) > 0.01f)
        {
            camera.fieldOfView = Mathf.SmoothDamp(camera.fieldOfView, targetFov, ref velocity, 0.4f);

            yield return null;
        }
    }

    private IEnumerator HitByCarRoutine()
    {
        Vector3 velocity = Vector3.zero;
        Vector3 targetPosition = player.transform.position + Vector3.up * targetTargetDistance;

        transform.position = player.transform.position + Vector3.up * initialTargetDistance;
        transform.rotation = Quaternion.LookRotation(Vector3.down);

        while (true)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime1, maxVelocity1);

            yield return null;
        }
    }
}

