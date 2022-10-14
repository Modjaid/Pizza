using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryDestination : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) 
    {
        if(other.tag == "Player")
        {
            DeliveryJobManager.Instance.OnArrivedInDestination();

            Disappear();
        }
    }

    private void Disappear()
    {
        Destroy(gameObject);
    }
}
