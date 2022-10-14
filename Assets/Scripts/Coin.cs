using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoCached
{
    public float rotationSpeed;
    public float floatMagnitude;
    public float floatSpeed;

    private Transform _transform;
    private Vector3 initialPos;
    private float randomCycleOffset;

    private void Awake()
    {
        _transform = transform;
        initialPos = transform.position;
        randomCycleOffset = Random.Range(-100f, 100);
        transform.Rotate(Vector3.up, Random.Range(-180f, 180f));
    }
    
    public override void Tick()
    {
        // Rotate.
         _transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        // Float.
        Vector3 newPosition = initialPos;
        newPosition.y = newPosition.y + Mathf.Sin((Time.timeSinceLevelLoad + randomCycleOffset) * floatSpeed) * floatMagnitude;
        _transform.position = newPosition;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Player")
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        Destroy(gameObject);
    }
}
