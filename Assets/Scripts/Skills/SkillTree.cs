using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    public static SkillTree Instance { get; private set; }

    public SkillTreeData skillTreeData;
    private List<SkillType> boughtSkills;

    private const string saveFileName = "boughtSkills";

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        if (skillTreeData == null)
        {
            Debug.LogError("[SkillTree] Gimme refference to Skill Tree Data, meatbag.");
        }

        if (SavesManager.Instance.FileExists(saveFileName))
        {
            // Load the dictionary from JSON, if the save file existss.
            BoughtSkills temp = JsonUtility.FromJson<BoughtSkills>(
                SavesManager.Instance.Load(saveFileName));
            boughtSkills = new List<SkillType>(temp.skills);
        }
        else
        {
            // Else, create and save new dictionary.
            InitSaveFile();
        }
    }

    public bool SkillBought(SkillType skill)
    {
        return true;
    }

    private void InitSaveFile()
    {
        List<SkillType> temp = new List<SkillType>();

        // "Buy" all skills that are bought by default.
        foreach (var skillData in skillTreeData.skills)
        {
            if (skillData.boughByDefault)
            {
                temp.Add(skillData.skillType);
            }
        }

        boughtSkills = new List<SkillType>(temp);
        string saveValue = JsonUtility.ToJson(new BoughtSkills(temp.ToArray()));

        SavesManager.Instance.Save(saveValue, saveFileName);
    }
}

[Serializable]
public class BoughtSkills
{
    public SkillType[] skills;

    public BoughtSkills(SkillType[] skills)
    {
        this.skills = skills;
    }
}