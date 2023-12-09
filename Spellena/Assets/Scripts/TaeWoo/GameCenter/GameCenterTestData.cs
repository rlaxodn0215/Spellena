using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameCenterTestData", menuName = "ScriptableObject/GameCenterTestData")]
public class GameCenterTestData : ScriptableObject
{
    [Header("게임센터 데이터")]

    [Tooltip("맵, 캐릭터 로딩 타임")]
    public float loadingTime;
    [Tooltip("캐릭터 선택 타임")]
    public float characterSelectTime;
    [Tooltip("대기실 준비 시간")]
    public float readyTime;
    [Tooltip("플레이어 리스폰 타임")]
    public float playerRespawnTime;
    [Tooltip("어시스트 타임")]
    public float assistTime;

    [Tooltip("민병대 쿨타임")]
    public float angelStatueCoolTime;
    [Tooltip("민병대 초당 체력 증가량")]
    public int angelStatueHpPerTime;
    [Tooltip("민병대 효과 지속 시간")]
    public int angelStatueContinueTime;

    [Tooltip("거점 전환 원 먹는 비율")]
    public float occupyingGaugeRate;
    [Tooltip("거점 전환하는 시간")]
    public float occupyingReturnTime;
    [Tooltip("거점 % 먹는 비율")]
    public float occupyingRate;
    [Tooltip("추가시간이 발생하는 기준 게이지")]
    public float occupyingComplete;
    [Tooltip("추가 시간")]
    public float roundEndTime;
    [Tooltip("라운드 결과 확인 시간")]
    public float roundEndResultTime;
}
