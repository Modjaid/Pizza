using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficAICar : MonoCached
{
    // When the car reaches and of the route, it then selects random route to go to next via route connection.


    // The route the car is on currently.
    public TrafficRoute currentRoute;
    // Node the car is going to.
    public TrafficRouteNode currentNode;
    // Current route connection;
    public TrafficRouteConnection currentRouteConnection;

    public Transform sensorRaycastOrigin;
    public float sensorRaycastDistance;
    public float movementSpeed;
    public float rotationSpeed;
    public LayerMask sensorMask;

    [HideInInspector] public bool onRouteConnection;
    public bool stopped;

    new private Rigidbody rigidbody;
    new private Transform transform;
    private Vector3 directionToCurrentNode; 
    private bool completedRotationToNode;

    private void Awake()
    {
        transform = gameObject.transform;

        rigidbody = GetComponent<Rigidbody>();
    }

    public override void TickFixed()
    {
        if (currentNode != null)
        {
            MoveToNode();
        }
    }

    public Vector3 GetMovementDirection()
    {
        return directionToCurrentNode;
    }

    public void SetCurrentRoute(TrafficRoute route, TrafficRouteNode node)
    {
        currentRoute = route;
        currentNode = node;

        directionToCurrentNode = (currentNode.position - transform.position).normalized;
    }

    private void MoveToNode()
    {
        if(SensorUpdate()) return;

        Vector3 oldPosition = transform.position;

        // Rotate to the current node.
        if (!completedRotationToNode)
        {
            if (directionToCurrentNode != Vector3.zero)
            {
                float rotSpeed = rotationSpeed * Time.fixedDeltaTime;

                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(directionToCurrentNode), rotSpeed);
                rigidbody.MoveRotation(newRotation);

                // Check if completed rotation to node.
                if (Mathf.Approximately(Vector3.Dot(transform.forward, directionToCurrentNode), 1f))
                {
                    completedRotationToNode = true;
                }
            }
        }

        // Move towards the node.
        float speed = movementSpeed * Time.fixedDeltaTime;
        Vector3 newPosition = Vector3.MoveTowards(oldPosition, currentNode.position, speed);
        rigidbody.MovePosition(newPosition);

        // Check if node is already reached.
        float distanceToNode = Vector3.Distance(transform.position, currentNode.position);

        if (distanceToNode <= 1f)
        {
            NodeReached();

            return;
        }
    }

    private void NodeReached()
    {
        if (currentRoute.HasNextNode(currentNode))
        {
            currentNode = currentRoute.NextNode(currentNode);
        }
        else
        {
            GoToNextRoute();
        }

        directionToCurrentNode = (currentNode.position - transform.position).normalized;
        completedRotationToNode = false;
    }

    private void GoToNextRoute()
    {
        if (onRouteConnection)
        {
            currentRoute = currentRouteConnection.toRoute;
            currentNode = currentRoute.FirstNode();
            onRouteConnection = false;
            currentRouteConnection = null;
        }
        else
        {
            if (TrafficManager.Instance.RouteHasConnection(currentRoute))
            {
                // Select random route to go next.
                List<TrafficRouteConnection> connections = TrafficManager.Instance.GetTrafficConnections(currentRoute);

                if (connections.Count == 0) return;

                TrafficRouteConnection selectedConnection = connections[Random.Range(0, connections.Count)];

                // Select random connection.
                currentRoute = selectedConnection;

                onRouteConnection = true;
                currentRouteConnection = selectedConnection;
                currentRoute = currentRouteConnection;
                currentNode = currentRoute.FirstNode();
            }
        }

        completedRotationToNode = false;
    }

    private bool SensorUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(sensorRaycastOrigin.position, directionToCurrentNode, out hit, 
        sensorRaycastDistance, sensorMask, QueryTriggerInteraction.Collide))
        {
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Traffic Light Obstacle") && onRouteConnection)
            {
                stopped = false;
            }
            else
            {
                stopped = true;
            }
        }
        else
        {
            stopped = false;
        }

        return stopped;
    }
}
