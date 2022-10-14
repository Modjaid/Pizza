using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrafficManager : MonoBehaviour
{
    public static TrafficManager Instance { get; private set; }

    public List<TrafficRouteConnection> routeConnections;
    public List<TrafficRoute> routes;
    public List<TrafficAICar> cars;

    // Distance between cars.
    [SerializeField] public float trafficDensity;
    [SerializeField] public float trafficDensityRandomModifier;
    [SerializeField] public GameObject carPrefab;

    private bool autoFind = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FindAllRoutes();

        SpawnCars();
    }

    public void SpawnCars()
    {
        if (trafficDensity == 0) return;

        Transform carContainter = new GameObject("Car Container").transform;
        cars = new List<TrafficAICar>();

        foreach (TrafficRoute route in routes)
        {
            for (int i = 0; i <= route.nodes.Count - 1; i++)
            {
                if (i < route.nodes.Count - 1)
                {
                    float distanceToNextNode = Vector3.Distance(route.nodes[i].position, route.nodes[i + 1].position);

                    Vector3 direction = (route.nodes[i + 1].position - route.nodes[i].position).normalized;
                    Vector3 lastPosition = route.nodes[i].position;
                    TrafficRouteNode node = route.nodes[i + 1];
                    while (distanceToNextNode > trafficDensity)
                    {
                        float dist = trafficDensity + Random.Range(0, trafficDensity * trafficDensityRandomModifier);
                        lastPosition = Vector3.MoveTowards(lastPosition, route.nodes[i + 1].position, dist);
                        CreateCar(lastPosition, Quaternion.LookRotation(direction), route, node, carContainter);
                        distanceToNextNode -= dist;
                    }
                }
                else
                {

                }
            }
        }
    }

    /// <summary>
    /// Finds closest node to original node in routes.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="originalNode"></param>
    /// <returns></returns>
    public TrafficRoute FindClosestRouteForRouteConnection(TrafficRouteConnection connection, TrafficRouteNode originalNode)
    {
        // This method is piece of shit

        TrafficRoute closest = null;
        float minDistance = float.MaxValue;


        foreach (TrafficRoute route in routes)
        {
            foreach (TrafficRouteNode node in route.nodes)
            {
                float distance = Vector3.Distance(node.position, originalNode.position);

                if (distance < minDistance && distance < 5)
                {
                    minDistance = distance;
                    closest = route;
                }
            }
        }

        return closest;
    }

    #if UNITY_EDITOR
    public void GenerateTrafficRoutes()
    {
        var routeContainer = new GameObject("Generated routes");

        //Debug.Log(routeConnections.Count);
        // Loop through all route connections:
        foreach (var routeConnection in routeConnections)
        {
            routeConnection.PutAllNodesInList();
            List<TrafficRouteNode> nodes = routeConnection.nodes;

            // Consider continuing if route is conneted?


            // First, calculate direction from last but one to the last node in route connection.
            Vector3 dir = (nodes[nodes.Count - 1].transform.position - nodes[nodes.Count - 2].transform.position).normalized;

            // Then, find closest "even" direction to it (in local space). -i.e., transform.forward, .left, etc.
            Vector3 nearestDir = SnappedToNearestAxis(dir);
            Debug.DrawRay(nodes[nodes.Count - 1].transform.position, nearestDir * 3f, Color.red, 1000f);

            float mindistToNode = float.MaxValue;
            TrafficRouteConnection closestConnection = null;

            foreach (var connection in routeConnections)
            {
                if (connection == routeConnection) continue;

                connection.PutAllNodesInList();
                TrafficRouteNode firstNode = connection.nodes[0];

                var dir1 = (firstNode.transform.position - nodes[nodes.Count - 1].transform.position).normalized;
                var dot = Vector3.Dot(nearestDir, dir1);

                if (dot < 0.5f) continue;

                float dist = GetDistPointToLine(nodes[nodes.Count - 1].
                transform.position, nearestDir, firstNode.transform.position);
                float distToNode = Vector3.Distance(nodes[nodes.Count - 1].transform.position, firstNode.transform.position);

                if (dist < 1f && distToNode < mindistToNode)
                {
                    closestConnection = connection;
                    mindistToNode = distToNode;
                }
            }

            if (closestConnection != null)
            {
                // Check if there already is a similar route.
                TrafficRoute foundRoute;

                if (FindSimilarRoute(nodes[nodes.Count - 1].transform.position,
                closestConnection.nodes[0].transform.position, out foundRoute))
                {
                    routeConnection.toRoute = foundRoute;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(routeConnection);
                    closestConnection.fromRoute = foundRoute;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(closestConnection);
                }
                else
                {
                    // Create the connection.
                    var route = new GameObject("Created route").AddComponent<TrafficRoute>();

                    var node0 = new GameObject("Node").AddComponent<TrafficRouteNode>();
                    node0.transform.position = nodes[nodes.Count - 1].transform.position;
                    node0.transform.SetParent(route.transform);
                    var node1 = new GameObject("Node").AddComponent<TrafficRouteNode>();
                    node1.transform.position = closestConnection.nodes[0].transform.position;
                    node1.transform.SetParent(route.transform);
                    route.transform.SetParent(routeContainer.transform);
                    route.PutAllNodesInList();
                    routes.Add(route);

                    routeConnection.toRoute = route;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(routeConnection);
                    closestConnection.fromRoute = route;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(closestConnection);
                }
            }
        }

        FindAllRoutes();

        // make sure that there are no route connections without the connection left.
        foreach (TrafficRouteConnection routeConnection1 in routeConnections)
        {
            float minDist = float.MaxValue;
            TrafficRoute closestRoute = null;

            if (routeConnection1.fromRoute == null)
            {
                foreach (TrafficRoute route in routes)
                {
                    TrafficRouteNode firstNode = routeConnection1.nodes[0];
                    float dist = Vector3.Distance(firstNode.transform.position,
                    route.nodes[route.nodes.Count - 1].transform.position);

                    if (dist < 1f && dist < minDist)
                    {
                        minDist = dist;
                        closestRoute = route;
                    }
                }

                if (closestRoute != null)
                {
                    routeConnection1.fromRoute = closestRoute;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(routeConnection1);
                }
            }
        }
    }
    #endif

    private bool FindSimilarRoute(Vector3 node0, Vector3 node1, out TrafficRoute foundRoute)
    {
        foreach (TrafficRoute route1 in routes)
        {
            float dist1 = Vector3.Distance(node0,
            route1.nodes[0].transform.position);

            float dist2 = Vector3.Distance(node1,
            route1.nodes[route1.nodes.Count - 1].transform.position);

            if (dist1 < 2f && dist2 < 2f)
            {
                foundRoute = route1;
                return true;
            }
        }

        foundRoute = null;
        return false;
    }

    private Vector3 FindNerestPointOnLine(Vector3 origin, Vector3 direction, Vector3 point)
    {
        direction.Normalize();
        Vector3 lhs = point - origin;

        float dotP = Vector3.Dot(lhs, direction);
        return origin + direction * dotP;
    }

    static public float GetDistPointToLine(Vector3 origin, Vector3 direction, Vector3 point)
    {
        Vector3 point2origin = origin - point;
        Vector3 point2closestPointOnLine = point2origin - Vector3.Dot(point2origin, direction) * direction;
        return point2closestPointOnLine.magnitude;
    }

    Vector3 SnappedToNearestAxis(Vector3 direction)
    {
        float x = Mathf.Abs(direction.x);
        float y = Mathf.Abs(direction.y);
        float z = Mathf.Abs(direction.z);
        if (x > y && x > z)
        {
            return new Vector3(Mathf.Sign(direction.x), 0, 0);
        }
        else if (y > x && y > z)
        {
            return new Vector3(0, Mathf.Sign(direction.y), 0);
        }
        else
        {
            return new Vector3(0, 0, Mathf.Sign(direction.z));
        }
    }

    private void CreateCar(Vector3 position, Quaternion rotation, TrafficRoute route, TrafficRouteNode node, Transform parent)
    {
        TrafficAICar car = Instantiate(carPrefab, position, rotation, parent).GetComponent<TrafficAICar>();
        car.SetCurrentRoute(route, node);
        cars.Add(car);
    }

    public List<TrafficRouteConnection> GetTrafficConnections(TrafficRoute fromRoute)
    {
        List<TrafficRouteConnection> connections = new List<TrafficRouteConnection>();

        foreach (TrafficRouteConnection connection in routeConnections)
        {
            if (connection.fromRoute == fromRoute && connection.toRoute != null)
            {
                connections.Add(connection);
            }
        }

        return connections;
    }

    public bool RouteHasConnection(TrafficRoute fromRoute)
    {
        foreach (TrafficRouteConnection connection in routeConnections)
        {
            if (connection.fromRoute == fromRoute)
            {
                return true;
            }
        }

        return false;
    }

    public void FindAllRoutes()
    {
        routes = new List<TrafficRoute>();
        routeConnections = new List<TrafficRouteConnection>(GameObject.FindObjectsOfType<TrafficRouteConnection>());

        TrafficRoute[] foundRoutes = GameObject.FindObjectsOfType<TrafficRoute>();

        foreach (TrafficRoute route in foundRoutes)
        {
            if (route.gameObject.GetComponent<TrafficRouteConnection>() == null)
            {
                routes.Add(route);
            }
        }
    }
}
