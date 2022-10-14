using UnityEngine;

[CreateAssetMenu(fileName = "Levels data", menuName = "ScriptableObjects/Levels data", order = 1)]
public class LevelsData : ScriptableObject
{
    public int levelCount;
    // Number of deliveries at one session aka game.
    public LevelCountData deliveriesCount;
    // How many enemies are removed from scene.
    public LevelCountData deductedEnemiesCount;
}

[System.Serializable]
public class LevelCountData
{
    public FromTo[] fromtos;

    public int GetCountAt(int level)
    {
        for(int i = 0; i < fromtos.Length; i++)
        {
            if(fromtos[i].from <= level && fromtos[i].to >= level)
            {
                return fromtos[i].count;
            }
        }

        Debug.LogError("Specified level count data not found.");

        return 0;
    }
}

[System.Serializable]
public class FromTo
{
    public int count;
    public float from;
    public float to;
}