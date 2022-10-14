using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Bike. 
/// </summary>
public class Bike : BaseVehicle
{
    public float maxSpeed;
    public float acceleration;
    public float forwardDrag;
    public float steeringSpeed;
    public float forwardWheelSteeringRange;
    public Transform forwardWheelPivot;
    public Material outlinedMaterial;

    private Vector3 input;
    new private Rigidbody rigidbody;
    new private Renderer renderer;
    private Material defaultMaterial;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        defaultMaterial = new Material(renderer.material);

        SetOutlineActive(!occupied);
    }

    private void FixedUpdate()
    {
        if (occupied)
        {
            Move(input);
        }
        else
        {
            rigidbody.rotation = Quaternion.Euler(new Vector3(0, rigidbody.rotation.y, 0));
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(!occupied) return;

        TrafficAICar car = other.gameObject.GetComponent<TrafficAICar>();

        if (car != null && !car.stopped)
        {
            player.HitByACar();
        }
    }

    /// <summary>
    /// Move in direction.
    /// </summary>
    /// <param name="direction">Direction of movement relative to camera.</param>
    public override void Move(Vector3 direction)
    {
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            // Rotate to match direction.
            float rotationSpeed = steeringSpeed * Time.fixedDeltaTime;
            Quaternion newRotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(direction), rotationSpeed);
            rigidbody.rotation = newRotation;

            // Calculate forward acceleration.
            float forwardAcceleration = acceleration * direction.magnitude * Time.fixedDeltaTime;

            rigidbody.AddForce(transform.forward * forwardAcceleration);
        }

        // Rotate velocity of rigidbody to match rotation of the bike.
        // probably need to normalize the direction idk.
        Vector3 newVelocity = transform.forward * rigidbody.velocity.magnitude;

        rigidbody.velocity = newVelocity;

        // Clamp the speed.
        if(rigidbody.velocity.magnitude > maxSpeed)
        {
            rigidbody.velocity *= maxSpeed / rigidbody.velocity.magnitude;
        }
    }

    public override void UpdateInput(Vector3 movement)
    {
        input = movement;
    }

    /// <summary>
    /// This method is called when unit gets in the vehicle.
    /// </summary>
    public override void GetIn()
    {
        occupied = true;

        SetOutlineActive(false);
    }

    /// <summary>
    /// This method is called when unit gets out of the vehicle.
    /// </summary>
    public override void GetOut()
    {
        occupied = false;
        input = Vector3.zero;

        SetOutlineActive(true);
    }

    private void SetOutlineActive(bool active)
    {
        if(active)
        {
            renderer.material = outlinedMaterial;
        }
        else
        {
            renderer.material = defaultMaterial;
        }
    }
}