using UnityEngine;
using DG.Tweening;

/// <summary>
/// 플레이어의 시각적 피드백(Squash & Stretch, 기울기 등)을 담당하는 클래스입니다.
/// DOTween을 사용하여 절차적 애니메이션을 구현합니다.
/// </summary>
public class PlayerVisualController : MonoBehaviour
{
    // ==================================================================================
    // 1. Public Member Variables (공개 멤버 변수)
    // ==================================================================================

    [Header("Visual Settings")]
    [Tooltip("효과를 적용할 실제 이미지 또는 모델의 Transform입니다.")]
    [SerializeField] private Transform visualTarget;
    [Tooltip("애니메이션 기본 지속 시간입니다.")]
    [SerializeField] private float animationDuration = 0.2f;

    [Header("Jump & Land Effects")]
    [Tooltip("점프 시 적용할 스케일 (길쭉해짐).")]
    [SerializeField] private Vector3 jumpScale = new Vector3(0.8f, 1.2f, 0.8f);
    [Tooltip("착륙 시 적용할 스케일 (넙적해짐).")]
    [SerializeField] private Vector3 landScale = new Vector3(1.3f, 0.7f, 1.3f);
    [Tooltip("착륙 시 흔들림 강도입니다.")]
    [SerializeField] private float landShakeStrength = 0.5f;

    [Header("Movement Effects")]
    [Tooltip("좌우 이동 시 회전할 기울기 각도입니다.")]
    [SerializeField] private float tiltAngle = 10f;

    // ==================================================================================
    // 2. Private Member Variables (비공개 멤버 변수)
    // ==================================================================================

    // 원래 크기를 저장하여 복구할 때 사용
    private Vector3 originalScale;

    // ==================================================================================
    // 3. Unity Event Functions (유니티 이벤트 함수)
    // ==================================================================================

    void Start()
    {
        if (visualTarget == null) visualTarget = transform;
        originalScale = visualTarget.localScale;
    }

    // ==================================================================================
    // 4. Public Methods (공개 메서드)
    // ==================================================================================

    /// <summary>
    /// 점프 시 위아래로 길쭉해지는(Stretch) 연출을 재생합니다.
    /// </summary>
    public void PlayJumpEffect()
    {
        visualTarget.DOKill();
        // PunchScale로 순간적인 탄성 표현
        visualTarget.DOPunchScale(new Vector3(-0.2f, 0.3f, -0.2f), animationDuration, 5, 1f);
    }

    /// <summary>
    /// 착륙 시 바닥에 눌리는(Squash) 연출을 재생합니다.
    /// </summary>
    public void PlayLandEffect()
    {
        visualTarget.DOKill();

        // 넙적해졌다가 원래대로 돌아옴
        visualTarget.DOScale(landScale, animationDuration * 0.5f)
            .OnComplete(() => visualTarget.DOScale(originalScale, animationDuration));

        // 착지 충격으로 인한 약간의 흔들림
        visualTarget.DOShakePosition(animationDuration, new Vector3(0, -landShakeStrength, 0), 10, 0);
    }

    /// <summary>
    /// 좌우 입력에 따라 캐릭터를 기울이는 연출을 재생합니다.
    /// </summary>
    /// <param name="horizontalInput">좌우 입력 값 (-1 ~ 1)</param>
    public void PlayTilt(float horizontalInput)
    {
        float targetZRotation = -horizontalInput * tiltAngle;
        visualTarget.DORotate(new Vector3(0, 0, targetZRotation), 0.1f);
    }

    /// <summary>
    /// 피격 등 충격 발생 시 화면을 흔드는 연출을 재생합니다.
    /// </summary>
    public void PlayHitShake()
    {
        visualTarget.DOShakePosition(0.3f, 0.5f, 20, 90);
    }
}