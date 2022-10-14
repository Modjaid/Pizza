using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public Transform playerPointer;
    public Transform finishPointer;
    public Transform player;
    public Transform cam;
    private IEnumerator routine;
    private DeliveryJob job;

    private void Start()
    {
        DeliveryJobManager.Instance.OnJobComplete += OnJobCompleted;
        DeliveryJobManager.Instance.OnJobStarted += OnJobStarted;

        // In case we already have job on start, and event was not called.
        if (DeliveryJobManager.Instance.HasJob()) OnJobStarted();
    }

    private void OnEnable()
    {
        if(job != null)
        {
            StartCoroutine(Rotate());
        }
    }

    private void OnJobCompleted()
    {
        if (routine != null)
            StopCoroutine(routine);
        ChangeVisibility(false);
    }

    private void OnJobStarted()
    {
        job = DeliveryJobManager.Instance.CurrentJob;

        if (routine != null)
            StopCoroutine(routine);
        routine = Rotate();
        StartCoroutine(routine);
        ChangeVisibility(true);
    }

    private IEnumerator Rotate()
    {
        while (true)
        {
            var targetPosLocal = cam.transform.InverseTransformPoint(job.destination.position);
            var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg;
            finishPointer.eulerAngles = new Vector3(0, 0, targetAngle);
            
            yield return null;
        }
    }

    private void ChangeVisibility(bool visible)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(visible);
        }
    }
}
