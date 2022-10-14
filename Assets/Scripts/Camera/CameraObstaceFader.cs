using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraObstaceFader : MonoBehaviour
{
    public Transform cameraTarget;
    public LayerMask obstacleMask;
    public Vector3 targetOffset;
    public float checkDelay = 0.1f;
    Camera cam;

    private List<TransparencyChanger> blockingObjects = new List<TransparencyChanger>();

    private void Start()
    {
        cam = Camera.main;

        StartCoroutine(CheckForBlockingObjects());
    }

    private IEnumerator CheckForBlockingObjects()
    {
        while (true)
        {
            Ray ray = cam.ScreenPointToRay(cam.WorldToScreenPoint(cameraTarget.position + targetOffset));
            Ray ray1 = new Ray(cameraTarget.position + targetOffset, -ray.direction);
            float distanceToTarget = Vector3.Distance(transform.position, cameraTarget.position + targetOffset);
            RaycastHit[] hits = Physics.SphereCastAll(ray, 0.1f, distanceToTarget - 0.05f, obstacleMask);
            RaycastHit[] hits1 = Physics.SphereCastAll(ray1, 0.1f, distanceToTarget - 0.05f, obstacleMask);

            List<RaycastHit> hitObjects = new List<RaycastHit>(hits);

            foreach (RaycastHit hit in hits1)
            {
                if (!hitObjects.Contains(hit))
                {
                    hitObjects.Add(hit);
                }
            }

            List<TransparencyChanger> newBlockingObjects = new List<TransparencyChanger>();

            if (hitObjects.Count > 0)
            {
                foreach (RaycastHit hit in hitObjects)
                {
                    GameObject hitObstacle;
                    if (hit.collider.CompareTag("Building Child Collider"))
                    {
                        hitObstacle = hit.collider.transform.parent.gameObject;
                    }
                    else
                    {
                        hitObstacle = hit.collider.gameObject;
                    }

                    TransparencyChanger obstacle = hitObstacle.GetComponent<TransparencyChanger>();
                    if (obstacle == null)
                    {
                        obstacle = hitObstacle.AddComponent<TransparencyChanger>();
                    }

                    newBlockingObjects.Add(obstacle);
                    if (!blockingObjects.Contains(obstacle))
                        obstacle.ChangeTransparency(true);
                }
            }

            foreach (TransparencyChanger obj in blockingObjects)
            {
                if (!newBlockingObjects.Contains(obj))
                {
                    obj.ChangeTransparency(false);
                }
            }

            blockingObjects = new List<TransparencyChanger>(newBlockingObjects);

            yield return new WaitForSecondsRealtime(checkDelay);
        }
    }
}