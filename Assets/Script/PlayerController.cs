using UnityEngine;

/// <summary>
/// 플레이어의 물리적 이동, 입력 처리, 그리고 환경과의 상호작용을 담당하는 메인 컨트롤러입니다.
/// 부모 객체는 전진만 담당하고, 자식 모델이 실제 점프와 좌우 이동을 수행하는 구조를 가집니다.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ==================================================================================
    // 1. Public Member Variables (공개 멤버 변수)
    // ==================================================================================

    [Header("Movement Settings")]
    [Tooltip("플레이어의 전진 속도입니다.")]
    public float forwardSpeed = 5f;
    [Tooltip("플레이어의 좌우 이동 속도입니다.")]
    public float sideSpeed = 10f;
    [Tooltip("점프 시 위로 솟구치는 힘입니다.")]
    public float jumpForce = 12f;

    [Header("Gravity Settings")]
    [Tooltip("기본 중력 값입니다.")]
    public float gravity = -30f;
    [Tooltip("점프 상승 시 중력 배율입니다. (낮을수록 체공 시간이 김)")]
    public float risingGravityScale = 1.0f;
    [Tooltip("점프 하강 시 중력 배율입니다. (높을수록 빠르게 떨어짐)")]
    public float fallingGravityScale = 3.0f;

    [Header("Ground Detection")]
    [Tooltip("바닥 감지에 사용할 구체(Sphere)의 반지름입니다.")]
    public float sphereRadius = 0.45f;
    [Tooltip("바닥 감지 레이캐스트의 거리입니다.")]
    public float castDistance = 0.2f;
    [Tooltip("바닥으로 인식할 레이어입니다.")]
    public LayerMask groundLayer;

    [Header("Visual & Effects")]
    [Tooltip("시각적 연출을 담당하는 컨트롤러 참조입니다.")]
    public PlayerVisualController visualEffect;
    [Tooltip("실제로 움직이는 자식 모델의 Transform입니다.")]
    public Transform playerModel;

    // ==================================================================================
    // 2. Private Member Variables (비공개 멤버 변수)
    // ==================================================================================

    // 외부(진창 등) 요인에 의한 이동 속도 변화율 (기본 1.0)
    private float _moveSpeedMultiplier = 1.0f;

    // 외부 요인에 의한 점프력 변화율 (기본 1.0)
    private float _jumpPowerMultiplier = 1.0f;

    // 모델의 수직 속도(Y축)를 계산하기 위한 변수
    private Vector3 _modelVelocity;

    // 현재 바닥에 닿아있는지 여부
    private bool _isGrounded;

    // 이전 프레임에 바닥에 닿아있었는지 여부 (착지 순간 감지용)
    private bool _wasGrounded;

    // 좌우 입력값 (-1 ~ 1)
    private float _horizontalInput;

    // 점프 요청 플래그
    private bool _jumpRequested;

    // 바닥 감지 레이캐스트 결과 저장용
    private RaycastHit _groundHit;

    // ==================================================================================
    // 3. Unity Event Functions (유니티 이벤트 함수)
    // ==================================================================================

    /// <summary>
    /// 매 프레임마다 입력을 받고 이동 로직을 수행합니다.
    /// </summary>
    void Update()
    {
        // 사용자 입력 수신
        _horizontalInput = Input.GetAxisRaw("Horizontal");

        // 점프 입력 처리 (바닥에 있을 때만 가능)
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _jumpRequested = true;
        }

        // 시각적 기울기 효과 적용
        if (visualEffect != null)
            visualEffect.PlayTilt(_horizontalInput);

        // 착지 순간 감지 및 처리
        if (!_wasGrounded && _isGrounded)
        {
            HandleLanding();
        }
        _wasGrounded = _isGrounded;

        // 실제 이동 로직 수행
        HandleMovement();
    }

    // ==================================================================================
    // 4. Public Methods (공개 메서드)
    // ==================================================================================

    /// <summary>
    /// 외부(예: 진창, 아이템)에서 플레이어의 속도와 점프력을 조절할 때 사용합니다.
    /// </summary>
    /// <param name="speedMult">이동 속도 배율</param>
    /// <param name="jumpMult">점프력 배율</param>
    public void SetExternalModifier(float speedMult, float jumpMult)
    {
        _moveSpeedMultiplier = speedMult;
        _jumpPowerMultiplier = jumpMult;
    }

    // ==================================================================================
    // 5. Private Methods (비공개 메서드)
    // ==================================================================================

    /// <summary>
    /// 플레이어의 전진, 좌우 이동, 점프 및 중력을 계산하여 적용합니다.
    /// </summary>
    private void HandleMovement()
    {
        if (playerModel == null) return;

        // 1. 루트(부모) 이동: 오직 전진만 수행 (외부 배율 적용)
        transform.position += transform.forward * (forwardSpeed * _moveSpeedMultiplier) * Time.deltaTime;

        // 2. 바닥 상태 갱신
        CheckGrounded();

        // 3. 모델의 로컬 좌표 계산 시작
        Vector3 currentLocalPos = playerModel.localPosition;

        // [좌우 이동] 로컬 X축 변경 (외부 배율 적용)
        currentLocalPos.x += _horizontalInput * (sideSpeed * _moveSpeedMultiplier) * Time.deltaTime;

        // [수직 이동 & 중력] 로컬 Y축 변경
        if (_isGrounded && _modelVelocity.y <= 0)
        {
            // 바닥에 있을 때는 수직 속도를 약간의 음수로 유지하여 접지력 확보
            _modelVelocity.y = -0.1f;

            // 경사면 등에서도 모델이 바닥에 딱 붙도록 월드 좌표를 로컬로 변환하여 보정
            Vector3 localGroundPoint = transform.InverseTransformPoint(_groundHit.point);
            currentLocalPos.y = localGroundPoint.y + sphereRadius;
        }
        else
        {
            // 공중에 있을 때 중력 적용 (상승/하강 시 다른 중력 계수 사용)
            float currentGravityScale = _modelVelocity.y > 0 ? risingGravityScale : fallingGravityScale;
            _modelVelocity.y += gravity * currentGravityScale * Time.deltaTime;
            currentLocalPos.y += _modelVelocity.y * Time.deltaTime;
        }

        // 4. 점프 로직 수행
        if (_jumpRequested)
        {
            // 점프력에 외부 배율 적용
            _modelVelocity.y = jumpForce * _jumpPowerMultiplier;
            _isGrounded = false;
            _jumpRequested = false;

            // 점프 시각 효과 재생
            if (visualEffect != null)
                visualEffect.PlayJumpEffect();
        }

        // 5. 최종 계산된 로컬 좌표 적용
        playerModel.localPosition = currentLocalPos;
    }

    /// <summary>
    /// 바닥 감지를 위한 스피어캐스트(SphereCast)를 수행합니다.
    /// </summary>
    private void CheckGrounded()
    {
        Vector3 origin = playerModel.position + Vector3.up * sphereRadius;
        _isGrounded = Physics.SphereCast(origin, sphereRadius, Vector3.down, out _groundHit, castDistance, groundLayer);

        // 디버깅을 위한 레이 그리기 (초록: 바닥 닿음, 빨강: 공중)
        Debug.DrawRay(origin, Vector3.down * castDistance, _isGrounded ? Color.green : Color.red);
    }

    /// <summary>
    /// 착지 순간에 호출되어 시각 효과 및 지형 상호작용을 처리합니다.
    /// </summary>
    private void HandleLanding()
    {
        SetExternalModifier(1.0f, 1.0f);

        // 착지 시각 효과 재생
        if (visualEffect != null)
            visualEffect.PlayLandEffect();

        // 지형 상호작용 처리 (인터페이스 활용)
        if (_groundHit.collider != null)
        {
            // 충돌한 물체(혹은 부모)가 상호작용 가능한 지형인지 확인
            IInteractiveTerrain terrain = _groundHit.collider.GetComponentInParent<IInteractiveTerrain>();

            // 인터페이스가 존재한다면 약속된 반응 함수(OnLand) 호출
            if (terrain != null)
            {
                terrain.OnLand(this);
            }
        }
    }
}