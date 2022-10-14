using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCloseManager : MonoBehaviour
{
    public static CarCloseManager Instance { get; private set; }
    public Transform player;
    public Transform markersContainer;
    public GameObject markerPrefab;
    // distance to car at which the marker is created
    public float requiredDistance;

    private IEnumerator checkRoutine;
    private List<CarNearbyMarker> markers = new List<CarNearbyMarker>();

    // There shouldn't be more than 4 markers on the screen.
    // 4 - for 4 directions where from the car can possibly go towards player. There is no need to show multiple
    // Markers that point in the same direction.

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += StartCheckRoutine;
        GameManager.Instance.OnGameStopped += StopCheckRoutine;
        GameManager.Instance.OnGameStopped += DeleteAllMarkers;
    }

    private void StartCheckRoutine()
    {
        if (checkRoutine != null)
            StopCoroutine(checkRoutine);
        checkRoutine = CheckRoutine();
        StartCoroutine(checkRoutine);
    }

    private void StopCheckRoutine()
    {
        if (checkRoutine != null)
            StopCoroutine(checkRoutine);
    }

    private void DeleteAllMarkers()
    {
        foreach(var marker in markers)
        {
            if(marker != null)
            Destroy(marker.gameObject);
        }
    }

    private IEnumerator CheckRoutine()
    {
        // Check if there are cars that need markers created to alarm player.
        // Create, if there are any (and if there is no marker for this car already).  
        // And destroy markers when car is not visible.
        // Marker positioning is handled by marker itself.

        // NOTE: Only create marker if car is moving +- towards player!
        // TODO: This script's perfomance is fucking terrible, and it was written in rush. To do something with it.

        while (true)
        {
            // pick closest car from all.
            TrafficAICar closest = null;
            float minDist = float.MaxValue;

            foreach (var car in TrafficManager.Instance.cars)
            {
                CarNearbyMarker marker1 = GetMarkerForCar(car.transform);

                if (IsTargetInCameraView(car.transform) || !CarIsMovingInPlayerDirection(car))
                {
                    var marker = marker1;
                    if (marker != null)
                    {
                        markers.Remove(marker);
                        Destroy(marker.gameObject);
                    }
                    continue;
                }

                float dist = Vector3.Distance(player.position, car.transform.position);

                if (dist > requiredDistance && marker1 != null)
                {
                    markers.Remove(marker1);
                    Destroy(marker1.gameObject);
                }

                if (dist < minDist)
                {
                    closest = car;
                    minDist = dist;
                }
            }

            if (closest != null && minDist <= requiredDistance)
            {
                if (GetMarkerForCar(closest.transform) == null && CarIsMovingInPlayerDirection(closest))
                {
                    var newMarker = CreateMarker(closest.transform);
                    markers.Add(newMarker);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private CarNearbyMarker GetMarkerForCar(Transform car)
    {
        foreach (var marker in markers)
        {
            if (marker.targetCar == car) return marker;
        }
        return null;
    }

    private CarNearbyMarker GetMarkerPointingApproximatelySameDirection(CarNearbyMarker originalMarker)
    {
        Vector3 direction = (originalMarker.targetCar.position - player.position).normalized;

        foreach (var marker in markers)
        {
            Vector3 direction1 = (marker.targetCar.position - player.position).normalized;

            float dot = Vector3.Dot(direction, direction1);

            if (dot >= 0.15) return marker;
        }

        return null;
    }

    private bool CarIsMovingInPlayerDirection(TrafficAICar car)
    {
        Vector3 directionToPlayer = (player.position - car.transform.position).normalized;

        if (Vector3.Dot(car.GetMovementDirection(), directionToPlayer) > 0.8f)
        {
            return true;
        }

        return false;
    }

    private bool IsTargetInCameraView(Transform target)
    {
        Vector3 viewportPoint = cam.WorldToViewportPoint(target.position);
        bool onScreen = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;
        return onScreen;
    }

    private CarNearbyMarker CreateMarker(Transform targetCar)
    {
        Vector3 position = new Vector3(-9999, -9999, 0);
        var marker = Instantiate(markerPrefab, position, Quaternion.identity, markersContainer).GetComponent<CarNearbyMarker>();
        marker.SetTargetCar(targetCar);
        return marker;
    }
}