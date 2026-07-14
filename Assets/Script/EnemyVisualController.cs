using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 적 캐릭터에게 기괴하고 불안정한(Glitch) 움직임을 부여하는 시각적 컨트롤러입니다.
/// </summary>
public class EnemyVisualController : MonoBehaviour
{
    // ==================================================================================
    // 1. Public Member Variables (공개 멤버 변수)
    // ==================================================================================

    // (None - SerializeField로 노출된 변수들은 Private 섹션에 포함됩니다)

    // ==================================================================================
    // 2. Private Member Variables (비공개 멤버 변수)
    // ==================================================================================

    [Header("Target Settings")]
    [Tooltip("기괴한 효과를 적용할 타겟 트랜스폼입니다.")]
    [SerializeField] private Transform visualTransform;

    [Header("Twitch & Glitch Settings")]
    [Tooltip("회전 뒤틀림의 강도입니다.")]
    [SerializeField] private float twitchIntensity = 20f;
    [SerializeField] private float minTwitchInterval = 0.05f;
    [SerializeField] private float maxTwitchInterval = 0.5f;

    [Header("Floating & Unstable Movement")]
    [Tooltip("부유 효과의 진폭입니다.")]
    [SerializeField] private float hoverAmplitude = 0.2f;
    [Tooltip("부유 속도입니다.")]
    [SerializeField] private float hoverSpeed = 2f;

    [Header("Scale Distortion")]
    [Tooltip("스케일 왜곡 강도입니다.")]
    [SerializeField] private Vector3 glitchScaleStrength = new Vector3(0.2f, -0.2f, 0.2f);

    private Vector3 originalScale;
    private Quaternion originalRotation;

    // ==================================================================================
    // 3. Unity Event Functions (유니티 이벤트 함수)
    // ==================================================================================

    void Start()
    {
        if (visualTransform == null) visualTransform = transform;

        originalScale = visualTransform.localScale;
        originalRotation = visualTransform.localRotation;

        // 각종 기괴한 루프 코루틴 실행
        StartCoroutine(TwitchRoutine());
        StartCoroutine(GlitchScaleRoutine());
        StartHoverEffect();
    }

    // ==================================================================================
    // 4. Public Methods (공개 메서드)
    // ==================================================================================

    /// <summary>
    /// 외부 호출용: 플레이어 발견 시 격렬하게 흔들리는 반응을 보입니다.
    /// </summary>
    public void PlayAggressiveTwitch()
    {
        visualTransform.DOKill(true);
        visualTransform.DOShakeRotation(0.5f, 90f, 30, 90, true);
        visualTransform.DOComplete();
    }

    /// <summary>
    /// 외부 호출용: 기괴하게 멈추는(Freeze) 연출을 실행합니다.
    /// </summary>
    public void FreezeGlitch()
    {
        StopAllCoroutines();
        visualTransform.DOKill();
        visualTransform.DOShakePosition(100f, 0.05f, 50, 90, false, false).SetUpdate(true);
    }

    // ==================================================================================
    // 5. Private Methods (비공개 메서드)
    // ==================================================================================

    /// <summary>
    /// 상시 부유하는 연출을 시작합니다.
    /// </summary>
    private void StartHoverEffect()
    {
        visualTransform.DOLocalMoveY(visualTransform.localPosition.y + hoverAmplitude, hoverSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // ==================================================================================
    // 6. Coroutines (코루틴)
    // ==================================================================================

    /// <summary>
    /// 불규칙한 간격으로 회전을 뒤틀어버리는 루틴입니다.
    /// </summary>
    private IEnumerator TwitchRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTwitchInterval, maxTwitchInterval));

            Vector3 randomRotation = new Vector3(
                Random.Range(-twitchIntensity, twitchIntensity),
                Random.Range(-twitchIntensity, twitchIntensity),
                Random.Range(-twitchIntensity, twitchIntensity)
            );

            visualTransform.DOLocalRotate(randomRotation, 0.03f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Flash)
                .OnComplete(() => {
                    // 확률적으로 뒤틀린 상태 유지
                    if (Random.value > 0.7f)
                        visualTransform.localRotation = originalRotation;
                });

            // 확률적으로 위치 순간이동 (프레임 드랍 연출)
            if (Random.value > 0.9f)
            {
                visualTransform.DOShakePosition(0.1f, 0.2f, 30, 90, false, true);
            }
        }
    }

    /// <summary>
    /// 불규칙한 간격으로 스케일을 왜곡시키는 루틴입니다.
    /// </summary>
    private IEnumerator GlitchScaleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 3f));

            // 순간적인 찌그러짐
            visualTransform.DOPunchScale(glitchScaleStrength, 0.2f, 10, 1f);

            // 가끔 발생하는 비정상적인 늘어짐
            if (Random.value > 0.8f)
            {
                visualTransform.DOScaleY(originalScale.y * 1.5f, 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.InOutBounce);
            }
        }
    }
}