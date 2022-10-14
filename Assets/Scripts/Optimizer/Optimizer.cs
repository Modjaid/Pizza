using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class prevents costly operations done with objects that are not even visible
/// To the player.
/// </summary>
public class Optimizer : MonoBehaviour
{
    // The distance from target the objects get optimized at.
    [SerializeField] private float optimizationRange;
    // Delay between optimization checks.
    [SerializeField] private float optimizationDelay;
    // The object all optimize distances are measured from.
    [SerializeField] private Transform target;

    [SerializeField] private IOptimizable[] optimizables;
    private IEnumerator optimizationRoutine;
    private CameraController cameraController;
    private bool optimizationRoutineRunning;

    private void Start()
    {
        cameraController = CameraController.Instance;
        cameraController.onCameraModeChanged += OnCameraModeChanged;

        // Get all objects implementing IOptimizable in the array.
        var temp = new List<IOptimizable>();

        // note: Consider using FindSceneObjectsOfType here?
        foreach(var monoBehaviour in GameObject.FindObjectsOfType<Object>())
        {
            if(monoBehaviour is IOptimizable)
            {
                temp.Add(monoBehaviour as IOptimizable);
            }
        }

        optimizables = new IOptimizable[temp.Count];
        temp.CopyTo(optimizables);

        Debug.Log("Optimized objects count: " + temp.Count);

        StartOptimizationRoutine();
    }

    private void OnCameraModeChanged(CameraMode newMode)
    {
        // Don't optimize objects when camera is in fly-by mode.
        // Don't want optimization to worsen the look of the game.

        if(newMode == CameraMode.FlyBy)
        {
            SetAllObjectsOptimized(false);

            StopOptimizationRoutine();
        }
        else
        {
            // If switched from camera mode, start optimization routine again.
            if(!optimizationRoutineRunning)
            {
                StartOptimizationRoutine();
            }
        }
    }

    private void StartOptimizationRoutine()
    {
        if(optimizationRoutine != null)
        StopCoroutine(optimizationRoutine);
        optimizationRoutine = OptimizationRoutine();
        StartCoroutine(optimizationRoutine);

        optimizationRoutineRunning = true;
    }

    private void StopOptimizationRoutine()
    {
        if(optimizationRoutine != null)
        StopCoroutine(optimizationRoutine);

        optimizationRoutineRunning = false;
    }

    private IEnumerator OptimizationRoutine()
    {
        while(true)
        {
            OptimizationCheck();

            yield return new WaitForSeconds(optimizationDelay);
        }
    }

    private void OptimizationCheck()
    {
        for(int i = 0; i < optimizables.Length; i++)
        {
            IOptimizable optimizable = optimizables[i];
            MonoBehaviour mono = optimizable as MonoBehaviour;

            if(mono == null)
            {
                continue;
            }
            
            // todo: Vector3.Distance's cost is pretty high. Consider using some cheaper way
            // to compare distance between target and optimized object with optimization range.

            float distance = Vector3.Distance(target.position, mono.gameObject.transform.position);

            if(distance < optimizationRange)
            {
                optimizable.DeOptimize();
            }
            else
            {
                optimizable.Optimize();
            }
        }
    }

    private void SetAllObjectsOptimized(bool optimized)
    {
        for (int i = 0; i < optimizables.Length; i++)
        {
            IOptimizable optimizable = optimizables[i];

            if(optimizable.Equals(null))
            {
                continue;
            }

            if(optimized)
                optimizable.Optimize();
            else
                optimizable.DeOptimize();
        }
    }
}