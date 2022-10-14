using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SkillData 
{
    public SkillType skillType;
    public SkillActiveOrPassive activeOrPassive;

    [Tooltip("Skills required to be bought in order for this skill to be unlocked.")]
    public List<SkillType> unlockedBy;
    public bool boughByDefault;

    public float cost;
}