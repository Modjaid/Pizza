using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2AxisController : MonoBehaviour, IMoveable, IRotatable
{
    private CharacterController controller;
    private Animator animator;
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

    private Vector3 prevDirection;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    public void Move(Vector2 direction)
    {
        direction.y *= direction.y > 0 ? forwardSpeed : backSpeed;
        direction.x *= sideSpeed;
        Vector3 moveDirection = transform.rotation * new Vector3(direction.x, 0, direction.y);
        prevDirection = Vector3.Distance(moveDirection, prevDirection) > division ? Vector3.LerpUnclamped(prevDirection, moveDirection, acceleration * Time.deltaTime): Vector3.MoveTowards(prevDirection, moveDirection, deacceleration * Time.deltaTime);
        controller.SimpleMove(prevDirection);
        moveDirection = Quaternion.Inverse(transform.rotation) * prevDirection;
        moveDirection.z /= moveDirection.z > 0 ? forwardSpeed : backSpeed;
        moveDirection.x /= sideSpeed;
        Debug.Log(moveDirection);
        animator.SetFloat("X", moveDirection.x);
        animator.SetFloat("Y", moveDirection.z);
    }

    public void Rotate(float y)
    {
        transform.rotation *= Quaternion.Euler(0, y, 0);
    }
}
