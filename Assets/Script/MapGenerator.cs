using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 진행에 따라 무한히 맵 타일을 생성하고 삭제하는 관리자입니다.
/// 단순 무작위가 아닌, 지형의 난이도와 리듬을 고려한 생성 로직을 포함합니다.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    // ==================================================================================
    // 1. Public Member Variables (공개 멤버 변수)
    // ==================================================================================

    [Header("Prefabs")]
    [Tooltip("생성할 맵 타일 프리팹 배열입니다. (0:땅, 1:진창, 2:통나무, 3:땟목 순서 권장)")]
    public GameObject[] tilePrefabs;
    [Tooltip("맵 생성의 기준이 될 플레이어 Transform입니다.")]
    public Transform playerTransform;

    [Header("Settings")]
    [Tooltip("타일 하나의 길이(Z축)입니다.")]
    public float tileLength = 20f;
    [Tooltip("화면에 유지할 타일의 개수입니다.")]
    public int numberOfTiles = 5;
    [Tooltip("플레이어가 지나간 후 타일을 삭제하기 전까지의 여유 거리입니다.")]
    public float safeZone = 25f;

    // ==================================================================================
    // 2. Private Member Variables (비공개 멤버 변수)
    // ==================================================================================

    // 현재 활성화된 타일 리스트
    private List<GameObject> _activeTiles = new List<GameObject>();

    // 다음 타일이 생성될 Z축 위치
    private float _spawnZ = 0f;

    // 타일 생성 방향 (플레이어의 전방)
    private Vector3 _direction;

    // 타일의 회전값
    private Quaternion _tileRotation;

    // 게임 시작 시 플레이어 위치
    private Vector3 _startPos;

    // 마지막으로 생성된 타일의 인덱스 (패턴 생성용)
    private int _lastPrefabIndex = 0;

    // ==================================================================================
    // 3. Unity Event Functions (유니티 이벤트 함수)
    // ==================================================================================

    /// <summary>
    /// 초기 설정을 수행하고 첫 타일들을 생성합니다.
    /// </summary>
    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform이 설정되지 않았습니다!");
            return;
        }

        // 1. 플레이어 기준 시작 위치 및 방향 설정
        _startPos = playerTransform.position;
        _startPos.y = _startPos.y - 1; // 타일 높이 보정
        _direction = playerTransform.forward;
        _tileRotation = playerTransform.rotation;

        // 2. 초기 타일 생성 (초반 2개는 안전한 땅, 이후는 로직 적용)
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i < 2) SpawnTile(0);
            else SpawnWithLogic();
        }
    }

    /// <summary>
    /// 플레이어 이동 거리를 체크하여 새로운 타일을 생성하고 지나간 타일을 삭제합니다.
    /// </summary>
    void Update()
    {
        if (playerTransform == null) return;

        // 플레이어의 진행 거리 계산
        float playerProgress = Vector3.Dot(playerTransform.position - _startPos, _direction);

        // 생성 지점에 도달했는지 확인
        if (playerProgress - safeZone > (_spawnZ - numberOfTiles * tileLength))
        {
            SpawnWithLogic();
            DeleteTile();
        }
    }

    // ==================================================================================
    // 5. Private Methods (비공개 메서드)
    // ==================================================================================

    /// <summary>
    /// 난이도와 지형 리듬을 고려하여 다음에 생성할 타일 인덱스를 결정합니다.
    /// </summary>
    private void SpawnWithLogic()
    {
        int randomIndex = 0;

        // 프리팹이 4개 이상일 때 (땅, 진창, 통나무, 땟목 등)
        if (tilePrefabs.Length >= 4)
        {
            // 이전에 특수 지형이었다면 이번엔 안전한 땅(0) 생성하여 휴식 부여
            if (_lastPrefabIndex != 0)
            {
                randomIndex = 0;
            }
            else
            {
                // 안전한 땅 다음에는 확률적으로 위험한 지형 선택
                float r = Random.value;
                if (r < 0.33f) randomIndex = 1;      // 진창
                else if (r < 0.66f) randomIndex = 2; // 통나무
                else randomIndex = 3;                // 땟목
            }
        }
        // 프리팹이 2개 이상일 때 (땅, 진창)
        else if (tilePrefabs.Length >= 2)
        {
            // 이전에 진창(1)이었다면 땅(0) 생성
            if (_lastPrefabIndex == 1)
                randomIndex = 0;
            else
                randomIndex = (Random.value > 0.4f) ? 1 : 0;
        }
        // 프리팹이 하나뿐일 때
        else
        {
            randomIndex = Random.Range(0, tilePrefabs.Length);
        }

        SpawnTile(randomIndex);
    }

    /// <summary>
    /// 실제 타일 오브젝트를 인스턴스화하고 리스트에 추가합니다.
    /// </summary>
    /// <param name="prefabIndex">생성할 프리팹 인덱스</param>
    private void SpawnTile(int prefabIndex)
    {
        _lastPrefabIndex = prefabIndex;

        Vector3 spawnPos = _startPos + (_direction * _spawnZ);
        GameObject tile = Instantiate(tilePrefabs[prefabIndex], spawnPos, _tileRotation);

        tile.transform.SetParent(this.transform);
        _activeTiles.Add(tile);
        _spawnZ += tileLength;
    }

    /// <summary>
    /// 가장 오래된 타일을 삭제하여 메모리를 관리합니다.
    /// </summary>
    private void DeleteTile()
    {
        if (_activeTiles.Count > 0)
        {
            Destroy(_activeTiles[0]);
            _activeTiles.RemoveAt(0);
        }
    }
}