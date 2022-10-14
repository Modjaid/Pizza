using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TrafficRoute : MonoBehaviour
{
    public List<TrafficRouteNode> nodes = new List<TrafficRouteNode>();

    #if UNITY_EDITOR
    [ExecuteAlways]
    private void OnDrawGizmos()
    {
        DrawRouteGizmos(Color.green);
        if (Selection.activeGameObject == this.gameObject)
            DrawRouteGizmos(Color.red);
    }
    #endif

    private void Awake()
    {
        PutAllNodesInList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentNode"></param>
    /// <returns></returns>
    public TrafficRouteNode NextNode(TrafficRouteNode currentNode)
    {
        int index = nodes.IndexOf(currentNode);

        // If there is continuation to this route.
        if (index < nodes.Count - 1)
        {
            // Return next node.
            return nodes[index + 1];
        }
        else
        {
            Debug.Log("Route has no more nodes.");

            return null;
        }
    }

    public TrafficRouteNode FirstNode()
    {
        return nodes[0];
    }


    public bool HasNextNode(TrafficRouteNode currentNode)
    {
        int index = nodes.IndexOf(currentNode);
        // If there is continuation to this route.
        if (index < nodes.Count - 1)
        {
            return true;
        }
        return false;
    }

    public void PutAllNodesInList()
    {
        nodes.Clear();

        foreach (Transform children in transform)
        {
            TrafficRouteNode node = children.GetComponent<TrafficRouteNode>();

            if (node != null)
            {
                nodes.Add(node);
            }
        }
    }

    protected void DrawRouteGizmos(Color color)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 currentNode = transform.GetChild(i).transform.position + Vector3.up * 0.1f;
            Vector3 nextNode;

            Gizmos.color = color;
            Gizmos.DrawWireCube(currentNode, Vector3.one);

            if (i < transform.childCount - 1)
            {
                nextNode = transform.GetChild(i + 1).transform.position + Vector3.up * 0.1f;
                Gizmos.DrawLine(currentNode, nextNode);
            }
            else
            {
                nextNode = currentNode * 10f;
            }

            Vector3 directionFromPreviousNode = (currentNode - nextNode).normalized;
            float distanceBetweenNodes = Vector3.Distance(currentNode, nextNode);

            // Draw arrow.
            Vector3 originPosition = Vector3.MoveTowards(currentNode, nextNode, distanceBetweenNodes / 2);

            if (i < transform.childCount - 1)
            {
                Debug.DrawRay(originPosition, Quaternion.AngleAxis(30, Vector3.up) * directionFromPreviousNode * 2.5f, color);
                Debug.DrawRay(originPosition, Quaternion.AngleAxis(-30, Vector3.up) * directionFromPreviousNode * 2.5f, color);
            }
        }
    }
}
