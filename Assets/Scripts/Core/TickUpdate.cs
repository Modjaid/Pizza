using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The only Update.
/// </summary>
public class TickUpdate : MonoBehaviour
{
    private static List<MonoCached> _objects = new List<MonoCached>(0);

    public static void Add(MonoCached obj)
    {
        _objects.Add(obj);
    }

    public static void Remove(MonoCached obj)
    {
        _objects.Remove(obj);
    }

    private void Update()
    {
        for(int i = 0; i < _objects.Count; i++)
            _objects[i].Tick();
    }

    private void FixedUpdate()
    {
        for(int i = 0; i < _objects.Count; i++)
            _objects[i].TickFixed();
    }
}
