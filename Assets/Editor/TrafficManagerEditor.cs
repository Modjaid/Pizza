using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrafficManager))]
public class TrafficManagerEditor : Editor
{
    TrafficManager trafficManager;

    private void OnEnable()
    {
        trafficManager = (TrafficManager) target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("carPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("trafficDensity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("trafficDensityRandomModifier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("routeConnections"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("routes"));

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        if(GUILayout.Button("Find all routes"))
        {
            trafficManager.FindAllRoutes();
            
            Debug.Log("[Traffic Manager Editor] All traffic routes are now found and enlisted.");
        }

        if(GUILayout.Button("Generate traffic routes"))
        {
            trafficManager.FindAllRoutes();
            trafficManager.GenerateTrafficRoutes();

            Debug.Log("[Traffic Manager Editor] Traffic routes generated.");
        }
    }
}