using UnityEngine;

public class CameraDynamicFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("반드시 PlayerModel(실제 움직이는 자식 객체)을 연결하세요.")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("값이 작을수록 빠릿하게, 클수록 부드럽게(느리게) 따라갑니다.")]
    public float smoothTime = 0.2f;

    [Header("Axis Influence (0 ~ 1)")]
    [Tooltip("X축(좌우) 이동을 얼마나 반영할지 (0: 고정, 0.5: 절반만 따라감, 1: 완전 추적)")]
    [Range(0f, 1f)] public float xInfluence = 0.5f;

    [Tooltip("Y축(점프) 이동을 얼마나 반영할지 (0: 고정, 1: 완전 추적)")]
    [Range(0f, 1f)] public float yInfluence = 0.6f;

    [Tooltip("Z축(전진)은 러닝 게임 특성상 보통 1(완전 추적)을 유지합니다.")]
    [Range(0f, 1f)] public float zInfluence = 1.0f;

    [Header("Offset Correction")]
    [Tooltip("시작 시 자동으로 오프셋을 계산할지 여부")]
    public bool autoOffset = true;
    public Vector3 manualOffset = new Vector3(0, 5, -8);

    private Vector3 _currentVelocity;
    private Vector3 _initialOffset;
    private float _initialTargetX;
    private float _initialTargetY;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraDynamicFollow: Target이 설정되지 않았습니다.");
            return;
        }

        // 초기 오프셋 계산 또는 수동 설정 값 사용
        if (autoOffset)
            _initialOffset = transform.position - target.position;
        else
            _initialOffset = manualOffset;

        // 타겟의 초기 위치(기준점)를 저장해둡니다.
        _initialTargetX = target.position.x;
        _initialTargetY = target.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 현재 타겟의 위치에서 초기 기준점을 뺀 '이동량(Delta)'을 계산합니다.
        float deltaX = target.position.x - _initialTargetX;
        float deltaY = target.position.y - _initialTargetY;
        // Z축은 계속 전진하므로 델타가 아닌 현재 위치를 그대로 사용하거나 오프셋을 더합니다.

        // 2. 목표 위치 계산 (Influence 적용)
        // 핵심: 플레이어가 10만큼 이동해도 영향력이 0.5라면 카메라는 5만큼만 이동합니다.
        // 이렇게 하면 플레이어가 화면 중앙에서 벗어나는 연출이 가능합니다.

        float desiredX = _initialTargetX + _initialOffset.x + (deltaX * xInfluence);
        float desiredY = _initialTargetY + _initialOffset.y + (deltaY * yInfluence);
        float desiredZ = target.position.z + _initialOffset.z; // Z축은 보통 계속 따라갑니다.

        Vector3 targetPosition = new Vector3(desiredX, desiredY, desiredZ);

        // 3. 부드러운 이동 (SmoothDamp)
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
    }
}