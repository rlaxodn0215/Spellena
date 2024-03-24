using UnityEngine;
using SkillInfo;
using Photon.Pun;

public class PlayerSkill1ElementalOrder : PlayerSkill
{
    public PlayerCastingAuraComponent castingAura;

    public override void Play()
    {
        if(skillState == SkillState.None)
        {
            skillState = SkillState.Unique;
            castingAura.ActiveCastingAura(skillData.distance, skillData.scale);
        }
        else if(skillState == SkillState.Unique)
        {
            skillState = SkillState.Casting;
            skillCastingTime = 5f;

            castingAura.InactiveCastingAura();

            PlaySkillLogic();
        }
    }

    /*
    기능 : 스킬 로직 수행
    */
    private void PlaySkillLogic()
    {
        PhotonNetwork.Instantiate("SkillObjects/ElementalOrder/Skill1",
            castingAura.transform.position,
            Quaternion.identity,
            data: GetInstantiateData());
    }

    protected override void ChangeNextRoute()
    {
        if (skillState == SkillState.Casting)
        {
            skillState = SkillState.None;
            skillCoolDownTime = skillData.skillCoolDownTime;
            isReady = false;
        }
    }
}
