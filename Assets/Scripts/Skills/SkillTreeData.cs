using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillTreeData", menuName = "ScriptableObjects/SkillTreeData", order = 0)]

public class SkillTreeData : ScriptableObject 
{
    public List<SkillData> skills;
}