using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TrafficRouteConnection : TrafficRoute
{
    public TrafficRoute fromRoute;
    public TrafficRoute toRoute;

    #if UNITY_EDITOR
    [ExecuteAlways]
    private void OnDrawGizmos()
    {
        DrawRouteGizmos(Color.yellow);
        if(Selection.activeGameObject == this.gameObject)
        DrawRouteGizmos(Color.red);
    }
    #endif
}
