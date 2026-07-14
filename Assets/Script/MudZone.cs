using UnityEngine;

/// <summary>
/// 플레이어가 진입하면 이동 속도와 점프력에 페널티를 부여하는 진창 구역입니다.
/// </summary>
public class MudZone : MonoBehaviour, IInteractiveTerrain
{
    // ==================================================================================
    // 1. Public Member Variables (공개 멤버 변수)
    // ==================================================================================

    [Header("Mud Penalty Settings")]
    [Tooltip("이 구역에 있을 때 적용될 속도 비율입니다. (0.5 = 50% 속도)")]
    [Range(0.1f, 1.0f)] public float speedMultiplier = 0.5f;

    [Tooltip("이 구역에 있을 때 적용될 점프력 비율입니다. (낮을수록 점프가 힘들어짐)")]
    [Range(0.1f, 1.0f)] public float jumpPowerMultiplier = 0.7f;

    // ==================================================================================
    // 3. Unity Event Functions (유니티 이벤트 함수)
    // ==================================================================================

    // ==================================================================================
    // 4. Public Methods (공개 메서드)
    // ==================================================================================

    /// <summary>
    /// IInteractiveTerrain 인터페이스 구현.
    /// </summary>
    public void OnLand()
    {

    }

    /// <summary>
    /// IInteractiveTerrain 인터페이스 구현.
    /// </summary>
    public void OnLand(PlayerController player)
    {
        if (player != null)
        {
            OnLand();
            player.SetExternalModifier(speedMultiplier, jumpPowerMultiplier);
        }
    }
}