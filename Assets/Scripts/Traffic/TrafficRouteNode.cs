using UnityEngine;

public class TrafficRouteNode : MonoBehaviour
{
    [HideInInspector] public Vector3 position;

    private void Awake()
    {
        position = transform.position;
    }
}
