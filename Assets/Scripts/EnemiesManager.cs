using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager Instance { get; private set; }

    private List<Enemy> enemies;

    private void Awake()
    {
        Instance = this;

        PutEnemiesInList();
    }
    
    /// <summary>
    /// Remove *count* enemies from scene.
    /// </summary>
    public void RemoveEnemies(int count)
    {
        for(int i = 0; i <= count; i++)
        {
            if(i == enemies.Count) return;
            
            Enemy enemy = enemies[i];

            if(enemies == null) continue;

            Destroy(enemy.gameObject);
            enemies.Remove(enemy);
        }
    }

    private void PutEnemiesInList()
    {
        enemies = new List<Enemy>(GameObject.FindObjectsOfType<Enemy>());
    }
}