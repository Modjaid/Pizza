using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// Base class for units. Contains all things that are common between player unit and bots.
/// </summary>
public abstract class BasicUnit : MonoBehaviour
{
    protected Animator animator;

    public bool isCaught { get; protected set; }

    public List<Transform> childs = new List<Transform>();
    private Quaternion lookAtCurrentSpeed;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        FindEveryChild(gameObject.transform);
        for (int i = 0; i < childs.Count; i++)
        {
            FindEveryChild(childs[i]);
        }
    }

    protected virtual void Start()
    {

    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("TrafficAICar"))
        {
            TrafficAICar car = other.gameObject.GetComponent<TrafficAICar>();
            
            // Car hierarchy: 
            // Car > Model > Collider
            if(car == null) car = other.transform.parent.parent.GetComponent<TrafficAICar>();

            if (car != null && !car.stopped)
            {
                HitByACar();
            }
        }
    }

    /// <summary>
    /// Catch this unit.
    /// </summary>
    public virtual void HitByACar()
    {
        animator.SetTrigger("Catched");
        StopMovement();

        isCaught = true;
    }

    public void SetRendererActive(bool active)
    {
        foreach (Transform child in childs)
        {
            Renderer renderer = child.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.enabled = active;
            }
        }
    }

    public abstract float GetNormalizedSpeed();

    public abstract bool IsMoving();

    /// <summary>
    /// Animates unit.
    /// </summary>
    /// <param name="speedNormalized"></param>
    protected void Animate(float speedNormalized)
    {
        animator.SetFloat("Speed", speedNormalized, 0.1f, Time.deltaTime);
    }

    /// <summary>
    /// Rotates unit on Y axis to match direction.
    /// </summary>
    /// <param name="direction"></param>
    protected void LookAt(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        Quaternion newRotation = Utils.QuaternionSmoothDamp(transform.rotation, targetRotation, ref lookAtCurrentSpeed, 0.03f);

        transform.rotation = newRotation;
    }

    /// <summary>
    /// Creates directional vector from Y euler angle.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns>Direction vector</returns>
    protected Vector3 DirectionFromAngle(float angle)
    {
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    public abstract void StopMovement();

    private void FindEveryChild(Transform parent)
    {
        int count = parent.childCount;
        for (int i = 0; i < count; i++)
        {
            childs.Add(parent.GetChild(i));
        }
    }
}
