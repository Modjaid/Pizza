using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAxisController : BasicUnit, IMoveable, IRotatable
{
    private CharacterController controller;
    [SerializeField, Range(1, 40)]
    protected float forwardSpeed;
    [SerializeField, Range(1, 20)]
    protected float sideSpeed;
    [SerializeField, Range(1, 20)]
    protected float backSpeed;
    [SerializeField, Range(1, 10)]
    protected float acceleration = 1;
    [SerializeField, Range(1, 10)]
    protected float deacceleration = 1;
    [SerializeField, Range(1, 10)]
    protected float division = 1;
    [SerializeField, Range(1,3)]
    protected float sprintMultiplier = 2;

    private Vector3 prevDirection;

    protected override void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        // Добавленная часть
        LookAtDestination();

        StartCoroutine(SendStealthTriggers());
        StartCoroutine(LookForSafeSpace());
        StartCoroutine(LookForVehicle());
        // Конец добавленной части
    }

    public void Move(Vector2 direction)
    {
        if (isCaught || frozen)
            return;
        direction.y = isSprinting ? direction.y * sprintMultiplier : direction.y;
        if (currentVehicle != null)
            currentVehicle.UpdateInput(direction);
        else
            ControllerMove(direction);
    }

    private void ControllerMove(Vector2 direction)
    {
        if (isCaught || frozen)
            return;
        direction.y *= direction.y > 0 ? forwardSpeed : backSpeed;
        direction.x *= sideSpeed;
        Vector3 moveDirection = transform.rotation * new Vector3(direction.x, 0, direction.y);
        prevDirection = Vector3.Distance(moveDirection, prevDirection) > division ? Vector3.LerpUnclamped(prevDirection, moveDirection, acceleration * Time.deltaTime) : Vector3.MoveTowards(prevDirection, moveDirection, deacceleration * Time.deltaTime);
        controller.SimpleMove(prevDirection);
        moveDirection = Quaternion.Inverse(transform.rotation) * prevDirection;
        moveDirection.z /= moveDirection.z > 0 ? forwardSpeed : backSpeed;
        moveDirection.x /= sideSpeed;
        animator.SetFloat("X", moveDirection.x);
        animator.SetFloat("Y", moveDirection.z);
    }

    public void Rotate(float y)
    {
        transform.rotation *= Quaternion.Euler(0, y, 0);
    }

    private IEnumerator sprintRoutine;
    private bool frozen;
    private Vector3 positionBeforeHiding;
    private Renderer[] renderers;
    private bool inVehicle;
    private BaseVehicle currentVehicle;
    private Collider[] colliders;
//    private float beganSprintTime = -1;
    private float beganSprintCooldownTime = -1;
    private bool godmode;

    public float noiseRadius = 6f;
    public CameraController cameraController;
    public float sprintCooldown;
    public float sprintSpeed;
    public float sprintDuration;
    [HideInInspector]
    public bool canSprint = true;
    [HideInInspector]
    public bool isSprinting;
    public bool IsHidden { get; set; }
    public float safeSpaceUsageRadius;
    public float vehicleUsageRadius;
    public LayerMask safeSpaceMask;
    public LayerMask vehicleMask;
    public LayerMask obstacleMask;

    public bool InVehicle
    {
        get { return inVehicle; }
    }

    public float MaximalForwardSpeed
    {
        get { return forwardSpeed; }
    }

    protected override void Awake()
    {
        base.Awake();

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponents<Collider>();
    }

    private IEnumerator SendStealthTriggers()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.4f);

            if (!IsHidden)
            {
                //float velocityNormalized = controller.velocity.magnitude / speed;
                float velocityNormalized = controller.velocity.magnitude / 1;
                float triggerRadius = velocityNormalized * noiseRadius;

                if (velocityNormalized > 0.25f)
                    StealthManager.Instance.SendTriggerToEnemies(transform.position, triggerRadius);
            }
        }
    }

    private void SetRendererEnabled(bool enabled)
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = enabled;
        }
    }

    public void Hide(Vector3 safeSpacePosition)
    {
        controller.enabled = false;
        positionBeforeHiding = transform.position;

        frozen = true;
        IsHidden = true;
        SetRendererEnabled(false);

        transform.position = safeSpacePosition;

        cameraController.ChangeMode(CameraMode.SafeSpace);
        UIManager.Instance.ShowPlayerHiddenHereIcon(transform.position);
        UIManager.Instance.ShowPlayerHiddenUI();
        UIManager.Instance.SetPlayerControlsActive(false);
        UIManager.Instance.SetSafespaceHintActive(true);
    }

    public void UnHide()
    {
        transform.position = positionBeforeHiding;

        frozen = false;
        IsHidden = false;
        SetRendererEnabled(true);

        cameraController.ChangeMode(CameraMode.FollowPlayer);
        UIManager.Instance.HidePlayerHiddenHereIcon();
        UIManager.Instance.HidePlayerHiddenUI();
        UIManager.Instance.SetPlayerControlsActive(true);
        UIManager.Instance.SetSafespaceHintActive(false);

        controller.enabled = true;
    }

    public void Sprint()
    {
        if (!canSprint) return;
        if (godmode) return;
        if (inVehicle) return;

        if (sprintRoutine != null)
            StopCoroutine(sprintRoutine);
        sprintRoutine = SprintRoutine();
        StartCoroutine(sprintRoutine);
    }

    private IEnumerator SprintRoutine()
    {
        VFXManager.Instance.PlaySprintEffect();
        canSprint = false;
        isSprinting = true;
        cameraController.SetSprintEffectEnabled(true);

        yield return new WaitForSeconds(sprintDuration);

        VFXManager.Instance.StopSprintEffect();
        isSprinting = false;
        cameraController.SetSprintEffectEnabled(false);

        // Cooldown.
        yield return new WaitForSeconds(sprintCooldown);

        canSprint = true;
    }

    private IEnumerator LookForSafeSpace()
    {
        int lastFrameCollidersCount = 0;

        while (true)
        {
            yield return null;

            if (!IsHidden && !inVehicle)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, safeSpaceUsageRadius, safeSpaceMask);

                if (colliders.Length > lastFrameCollidersCount)
                {
                    Hide(colliders[0].transform.position);
                }

                lastFrameCollidersCount = colliders.Length;
            }
        }
    }

    private IEnumerator LookForVehicle()
    {
        int lastFrameCollidersCount = 0;

        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (GameManager.Instance.GameStatus == GameStatus.Started)
            {
                if (!inVehicle && !IsHidden)
                {
                    Collider[] colliders = Physics.OverlapSphere(transform.position, vehicleUsageRadius, vehicleMask);

                    if (colliders.Length > lastFrameCollidersCount && lastFrameCollidersCount == 0)
                    {
                        UIManager.Instance.ShowTakeVehicleUI();
                    }
                    else if (colliders.Length == 0)
                    {
                        UIManager.Instance.HideTakeVehicleUI();
                    }

                    lastFrameCollidersCount = colliders.Length;
                }
                else
                {
                    lastFrameCollidersCount = 0;
                }
            }
            else
            {
                UIManager.Instance.HideTakeVehicleUI();
                lastFrameCollidersCount = 0;
            }
        }
    }

    public void GetInVehicle()
    {
        BaseVehicle vehicle = FindVehicle();

        currentVehicle = vehicle;
        inVehicle = true;
        vehicle.GetIn();
        //vehicle.player = this;
        vehicle.player = null;
        transform.SetParent(vehicle.transform);
        transform.rotation = vehicle.transform.rotation;
        transform.position = vehicle.transform.position;

        controller.enabled = false;
        StopSprint();
        UIManager.Instance.HideTakeVehicleUI();
        UIManager.Instance.ShowInVehicleUI();
        cameraController.ChangeMode(CameraMode.InVehicle);
    }

    public void GetOutOfVehicle()
    {
        currentVehicle.GetOut();
        inVehicle = false;
        currentVehicle = null;
        transform.SetParent(null);

        controller.enabled = true;
        transform.rotation = Quaternion.identity;
        transform.position = FindSafeSpaceForGetOut();
        UIManager.Instance.HideInVehicleUI();
        cameraController.ChangeMode(CameraMode.FollowPlayer);
    }

    public void Catched(Enemy enemy)
    {
        if (godmode) return;

        animator.SetTrigger("Catched");
        StopMovement();
        // note: dirty hack.
        isCaught = true;
        IsHidden = true;

        GameManager.Instance.OnPlayerCatched();
        UIManager.Instance.OnPlayerCatched();
        VFXManager.Instance.StopSprintEffect();
        
        //VFXManager.Instance.CreateFightEffect(this, enemy);
        VFXManager.Instance.CreateFightEffect(null, enemy);
        cameraController.SetSprintEffectEnabled(false);
    }

    public void StopSprint()
    {
        VFXManager.Instance.StopSprintEffect();
        beganSprintCooldownTime = Time.time + sprintCooldown;
        if (sprintRoutine != null)
            StopCoroutine(sprintRoutine);
        //UpdateSprintButton();

        isSprinting = false;
        cameraController.SetSprintEffectEnabled(false);
        canSprint = true;
    }

    public void SetGodmodeActive(bool active)
    {
        godmode = active;
        if (active) StopSprint();
        forwardSpeed = active ? 50f : 20;
        IsHidden = active;
    }

    public override void HitByACar()
    {
        if (godmode) return;

        cameraController.ChangeMode(CameraMode.HitByCar);
        UIManager.Instance.SetHitByACarScreenActive(true);
        UIManager.Instance.SetPlayerControlsActive(false);
        VFXManager.Instance.StopSprintEffect();
        cameraController.SetSprintEffectEnabled(false);
        animator.SetTrigger("HitByCar");
        transform.position = new Vector3(transform.position.x, 0.4f, transform.position.z);
        IsHidden = true;

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    public override float GetNormalizedSpeed()
    {
        return 1;
    }

    public override bool IsMoving()
    {
        if (godmode) return true;
        return IsHidden || inVehicle ? false : controller.velocity.magnitude > 0.1f;
    }

    public override void StopMovement()
    {
        controller.SimpleMove(Vector3.zero);
    }

    private BaseVehicle FindVehicle()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position,
        vehicleUsageRadius, vehicleMask);

        foreach (Collider collider in colliders)
        {
            BaseVehicle foundVehicle = collider.GetComponent<BaseVehicle>();

            if (foundVehicle != null)
            {
                return foundVehicle;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds position not occupied by obstacles.
    /// </summary>
    /// <returns> Vector3 postiion. </returns>
    private Vector3 FindSafeSpaceForGetOut()
    {
        Vector3 tempPosition = transform.position;
        var deltaRadius = vehicleUsageRadius - controller.radius;
        for (int i = 0; i < 360; i += 5)
        {
            var direction = Quaternion.AngleAxis(i, Vector3.up) * transform.right;
            tempPosition = transform.position + direction * deltaRadius;
            if (!Physics.CheckSphere(tempPosition, controller.radius, obstacleMask))
            {
                return new Vector3(tempPosition.x, tempPosition.y + 0.2f, tempPosition.z);
            }
        }
        return new Vector3(tempPosition.x, tempPosition.y + 0.2f, tempPosition.z);
    }
/*
    private void UpdateSprintButton()
    {
        float value = -1;

        if (isSprinting && beganSprintTime != -1)
        {
            value = (beganSprintTime + sprintDuration - Time.time) / sprintDuration;
        }
        else if (beganSprintCooldownTime != -1)
        {
            value = (Time.time - beganSprintCooldownTime) / sprintCooldown;
        }

        if (value != -1)
        {
            UIManager.Instance.sprintButton.UpdateBarValue(!canSprint, value);
        }
    }
*/
    private void LookAtDestination()
    {
        transform.LookAt(DeliveryJobManager.Instance.CurrentJob.destination.position);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, noiseRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, safeSpaceUsageRadius);
    }
}
