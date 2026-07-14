/// <summary>
/// 착지 시 특수한 반응(소리, 애니메이션 등)이 있는 모든 지형이 구현해야 하는 인터페이스입니다.
/// PlayerController는 이 인터페이스를 통해 지형의 종류를 몰라도 상호작용할 수 있습니다.
/// </summary>
public interface IInteractiveTerrain
{
    /// <summary>
    /// 플레이어가 지형에 착지하는 순간 반드시 실행될 함수입니다.
    /// </summary>
    void OnLand();

    /// <summary>
    /// 플레이어가 지형에 착지하는 순간 반드시 실행될 함수입니다.
    /// </summary>
    void OnLand(PlayerController player);
}