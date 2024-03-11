using SkillRelated;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSkillRoute", menuName = "CharacterData/CharacterSkillRoute")]
public class CharacterSkillRoute : ScriptableObject
{
    /*
    public List<float> skillCoolDownTime;
    public List<float> skillCastingTime;
    public List<float> skillChannelingTime;
    public List<SkillRouteData> skillRoutes;

    public List<float> plainCastingTime;
    public List<float> plainChannelingTime;
    private bool isRef = true;

    public CharacterSkillRoute(bool isRef)
    {
        this.isRef = isRef;
    }

    public SkillType ChangeNextRoute(int index)
    {
        if (!isRef)
        {
            if (index < skillRoutes.Count)
            {
                skillRoutes[index].routeIndex++;
                if (skillRoutes[index].routeIndex >= skillRoutes[index].skillRoute.Count)
                    skillRoutes[index].routeIndex = 0;
                return skillRoutes[index].GetSkillType();
            }
        }
        return SkillType.Error;
    }

    public CharacterSkillRoute InstantiateNew()
    {
        CharacterSkillRoute _skillRoute = new CharacterSkillRoute(false);

        for (int i = 0; i < skillCoolDownTime.Count; i++)
            _skillRoute.skillCoolDownTime.Add(0);
        for (int i = 0; i < skillCastingTime.Count; i++)
            _skillRoute.skillCastingTime.Add(0);
        for (int i = 0; i < skillChannelingTime.Count; i++)
            _skillRoute.skillChannelingTime.Add(0);
        for(int i = 0; i < skillRoutes.Count; i++)
            _skillRoute.skillRoutes.Add(Instantiate(skillRoutes[i]));


        return _skillRoute;
    }

    public bool IsProgressing()
    {
        if (!isRef)
        {
            for (int i = 0; i < skillCastingTime.Count; i++)
            {
                if (skillCastingTime[i] > 0f)
                    return true;
            }

            return false;
        }
        else
            return true;
    }
    */
}
