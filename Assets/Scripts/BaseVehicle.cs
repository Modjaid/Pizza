using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for rideable vehicles. (Can be rided by both enemies and players).
/// </summary>
public abstract class BaseVehicle : MonoBehaviour
{
    protected bool occupied;
    public PlayerAxisController player;

    public abstract void UpdateInput(Vector3 input);

    public abstract void GetIn();

    public abstract void GetOut();

    public abstract void Move(Vector3 direction);
}