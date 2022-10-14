using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBonus : MonoBehaviour
{
    public float timeAdded;

    public void OnTriggerEnter(Collider collider)
    {
        PlayerAxisController player = collider.GetComponent<PlayerAxisController>();

        if(player != null)
        {
            PickUp();
        }
    }
    
    private void PickUp()
    {
        DeliveryJobManager.Instance.AddDeliveryTime(timeAdded);

        Destroy(gameObject);
    }
}
