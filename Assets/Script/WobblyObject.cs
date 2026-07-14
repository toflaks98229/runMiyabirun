using UnityEngine;
using DG.Tweening;

/// <summary>
/// 플레이어가 착지했을 때 흔들리거나 출렁거리는 반응을 보여주는 지형 오브젝트입니다.
/// IInteractiveTerrain 인터페이스를 구현하여 PlayerController와 소통합니다.
/// </summary>
public class WobblyObject : MonoBehaviour, IInteractiveTerrain
{
    public enum ObjectType
    {
        Log,    // 통나무: 구르는 회전 반응
        Raft    // 땟목: 물 위 출렁임 반응
    }

    // ==================================================================================
    // 1. Public Member Variables (공개 멤버 변수)
    // ==================================================================================

    [Header("Settings")]
    [Tooltip("오브젝트의 유형(통나무/땟목)을 설정합니다.")]
    public ObjectType type = ObjectType.Log;
    [Tooltip("애니메이션 재생 시간입니다.")]
    public float duration = 0.5f;
    [Tooltip("흔들림의 강도입니다.")]
    public float strength = 1.0f;
    [Tooltip("진동 횟수입니다.")]
    public int vibrato = 10;

    // ==================================================================================
    // 2. Private Member Variables (비공개 멤버 변수)
    // ==================================================================================

    private Vector3 _initialPos;
    private Quaternion _initialRot;
    private bool _isInitialized = false;

    // ==================================================================================
    // 3. Unity Event Functions (유니티 이벤트 함수)
    // ==================================================================================

    void Start()
    {
        // 초기 위치와 회전값 저장
        _initialPos = transform.localPosition;
        _initialRot = transform.localRotation;
        _isInitialized = true;
    }

    // ==================================================================================
    // 4. Public Methods (공개 메서드 - 인터페이스 구현)
    // ==================================================================================

    /// <summary>
    /// IInteractiveTerrain 인터페이스 구현. 
    /// 플레이어가 이 오브젝트에 착지하면 자동으로 호출됩니다.
    /// </summary>
    public void OnLand()
    {
        PlayLandAnimation();
    }

    public void OnLand(PlayerController player)
    {
        OnLand();
    }

    // ==================================================================================
    // 5. Private Methods (비공개 메서드)
    // ==================================================================================

    /// <summary>
    /// 타입에 따른 DOTween 애니메이션을 실행합니다.
    /// </summary>
    private void PlayLandAnimation()
    {
        if (!_isInitialized) return;

        // 기존 애니메이션 초기화 (연속 점프 대응)
        transform.DOKill();
        transform.localPosition = _initialPos;
        transform.localRotation = _initialRot;

        if (type == ObjectType.Log)
        {
            // 통나무: Z축 회전(구르기)과 살짝 눌리는 연출
            transform.DOShakeRotation(duration, new Vector3(0, 0, strength * 15f), vibrato, 90f);
            transform.DOPunchPosition(Vector3.down * (strength * 0.1f), duration, vibrato, 1f);
        }
        else if (type == ObjectType.Raft)
        {
            // 땟목: 전체적인 기울어짐과 물에 가라앉는 연출
            transform.DOShakeRotation(duration, strength * 5f, vibrato, 90f);
            transform.DOPunchPosition(Vector3.down * (strength * 0.3f), duration, vibrato, 0.5f);
        }
    }
}