using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class LerpCameraController : MonoBehaviour, IRotatable
{
	public GameObject player;
	[SerializeField, Range(1,20)]
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

    public void Start()
    {
		Application.targetFrameRate = 60;
	}

    private void LateUpdate()
    {
		MoveCamera();
		RotateCamera();
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

    public void Rotate(float xAxis)
	{
		xAxis *= -0.10f;
		rotation = Mathf.Clamp(rotation + xAxis, 0, maxRotation);
	}
}
