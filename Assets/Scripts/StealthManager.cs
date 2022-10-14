using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class manages all things connected with stealth.
/// Such as sending triggers to all enemy units.
/// </summary>
public class StealthManager : MonoBehaviour
{
    public static StealthManager Instance { get; private set; }

    public List<Enemy> enemies = new List<Enemy>();

    public List<Vector3> drawTriggersPositions = new List<Vector3>();
    public List<float> drawTriggerRadiuses = new List<float>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        enemies = new List<Enemy>(FindObjectsOfType<Enemy>());
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < drawTriggersPositions.Count; i++)
        {
            if(drawTriggersPositions[i] == Vector3.zero) continue;

            Gizmos.DrawSphere(drawTriggersPositions[i], drawTriggerRadiuses[i]);
        }
    }

    private IEnumerator RemoveGizmos(int i)
    {
        yield return new WaitForSeconds(0.4f);

        drawTriggersPositions[i] = Vector3.zero;
    }

    public void AddEnemy(Enemy enemyToAdd)
    {
        if (!enemies.Contains(enemyToAdd))
        {
            enemies.Add(enemyToAdd);
        }
    }

    public void SendTriggerToEnemies(Vector3 triggerPosition, float triggerRadius)
    {
        drawTriggerRadiuses.Add(triggerRadius);
        drawTriggersPositions.Add(triggerPosition);

        StartCoroutine(RemoveGizmos(drawTriggersPositions.Count - 1));

        foreach (Enemy enemy in enemies)
        {
            if(enemy == null) return;
            
            float distanceToTriggerCenter = Vector3.Distance(enemy.transform.position, triggerPosition);
            float actualDistance = distanceToTriggerCenter - triggerRadius;

            if (actualDistance <= 0)
            {
                enemy.SendTrigger(triggerPosition, 1f);
            }
        }
    }
}