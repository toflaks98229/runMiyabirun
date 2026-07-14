# runMiyabirun

Unity(URP) 기반의 3D 무한 러너(Endless Runner) 프로토타입입니다. 플레이어는 자동으로 전진하며 좌우 이동과 점프로 절차적으로 생성되는 지형을 통과합니다.

## 개요

`runMiyabirun`은 Unity 2022.3 LTS로 제작 중인 3인칭 무한 러닝 게임 프로젝트입니다. 플레이어 캐릭터는 매 프레임 전방으로 자동 전진하고, 사용자는 좌우 입력(`Horizontal`)과 점프 입력(`Jump`)만으로 조작합니다. 맵은 `MapGenerator`가 타일 프리팹을 이어 붙여 무한히 생성/삭제하며, 진흙(MudZone)·통나무·땟목 같은 지형이 착지 시 서로 다른 반응을 일으키는 "상호작용 지형" 구조가 핵심입니다. 연출은 DOTween 기반의 스쿼시 & 스트레치, 흔들림 효과로 처리합니다.

현재 단일 씬(`SampleScene`)에서 동작하는 초기 프로토타입 단계이며, 점수·UI·게임오버 등의 메타 시스템은 아직 코드에 존재하지 않습니다.

## 기술 스택

- **엔진**: Unity `2022.3.62f2` (LTS)
- **언어**: C# (Assembly-CSharp / Assembly-CSharp-Editor / Assembly-CSharp-firstpass)
- **렌더 파이프라인**: Universal Render Pipeline (URP) `14.0.12` — Performant / Balanced / HighFidelity 3종 품질 에셋 구성
- **주요 패키지**: Terrain Tools `5.0.6`, TextMeshPro `3.0.7`, Timeline `1.7.7`, Visual Scripting `1.9.4`, uGUI, Test Framework `1.1.33`
- **서드파티 플러그인**: DOTween (Demigiant) — `Assets/Plugins/Demigiant`
- **빌드 대상 기본 해상도**: 1024x768 (버전 `0.1.0`)

## 주요 기능 / 시스템

- **자동 전진 러너 이동 (`PlayerController`)**
  - 루트(부모) 오브젝트는 `forwardSpeed`로 전방 전진, 자식 `playerModel`은 로컬 좌표에서 좌우(X) 이동과 점프(Y)를 담당하는 부모/자식 분리 구조
  - 상승/하강에 서로 다른 중력 배율(`risingGravityScale` / `fallingGravityScale`)을 적용한 체공감 튜닝
  - `Physics.SphereCast` 기반 접지 판정(`groundLayer`)과 접지 시 지면 높이 스냅
  - `SetExternalModifier(speedMult, jumpMult)`로 외부 지형이 이동 속도·점프력을 일시적으로 변경
- **상호작용 지형 인터페이스 (`IInteractiveTerrain`)**
  - 착지 순간 `OnLand(PlayerController)`가 호출되어, 플레이어가 지형 종류를 몰라도 다형적으로 반응 처리
  - `MudZone`: 착지 시 이동 속도(기본 0.5배)와 점프력(기본 0.7배)에 페널티 부여
  - `WobblyObject`: `Log`(통나무 - 구르는 회전 반응) / `Raft`(땟목 - 물 위 출렁임) 두 타입의 DOTween 흔들림 연출
- **절차적 맵 생성 (`MapGenerator`)**
  - 타일 프리팹 배열(평지 / 진흙 / 통나무 / 물 등)을 `tileLength` 간격으로 이어 스폰, 화면 밖 타일은 `Destroy`로 회수
  - 특수 타일이 연속되지 않도록 직전 타일 인덱스를 기억해 평지를 강제 삽입하는 난이도 완급 로직
  - 플레이어의 진행 거리(`Vector3.Dot`)와 `safeZone`을 비교해 스폰/삭제 시점 결정
- **캐릭터 비주얼 피드백 (`PlayerVisualController`, DOTween)**
  - 점프 시 `DOPunchScale` 스트레치, 착지 시 스쿼시 + `DOShakePosition`
  - 좌우 입력에 따른 캐릭터 기울임(`PlayTilt`), 피격용 `PlayHitShake`
- **적 연출 (`EnemyVisualController`)**
  - 코루틴 기반의 불규칙한 트위치(회전 떨림), 글리치 스케일 왜곡, 무한 호버링(Yoyo 루프)
  - 외부 호출용 `PlayAggressiveTwitch()` / `FreezeGlitch()`
- **동적 카메라 (`CameraDynamicFollow`)**
  - 축별 추종 영향력(`xInfluence`, `yInfluence`, `zInfluence`)을 개별 조절해 플레이어가 화면 중앙에서 벗어나는 연출 가능
  - `Vector3.SmoothDamp`를 이용한 부드러운 추적 및 자동 오프셋 계산

## 프로젝트 구조

```
runMiyabirun/
├── Assets/
│   ├── Script/                     # 게임 로직 (핵심)
│   │   ├── PlayerController.cs        # 전진/좌우/점프/중력/접지, 지형 상호작용 트리거
│   │   ├── PlayerVisualController.cs  # DOTween 스쿼시&스트레치, 기울임, 셰이크
│   │   ├── CameraDynamicFollow.cs     # 축별 영향력 기반 스무스 카메라 추종
│   │   ├── MapGenerator.cs            # 무한 타일 스폰/삭제 및 난이도 배치 로직
│   │   ├── IInteractiveTerrain.cs     # 착지 반응 지형 공통 인터페이스
│   │   ├── MudZone.cs                 # 진흙: 속도/점프력 페널티 지형
│   │   ├── WobblyObject.cs            # 통나무/땟목: 착지 시 흔들리는 지형
│   │   └── EnemyVisualController.cs   # 적의 글리치/트위치/호버 연출
│   ├── Scenes/SampleScene.unity    # 유일한 플레이 씬 (Player, MapGenerator, Terrain, enemy)
│   ├── Data/                       # 캐릭터 스프라이트, run 애니메이션/컨트롤러, Plane 프리팹
│   ├── Settings/                   # URP 렌더러 및 품질 프로파일(Performant/Balanced/HighFidelity)
│   ├── Plugins/Demigiant/          # DOTween
│   ├── Resources/DOTweenSettings.asset
│   └── *.asset (Terrain / TerrainData)  # 지형 데이터
├── Packages/manifest.json          # 패키지 의존성
├── ProjectSettings/                # 에디터 버전(2022.3.62f2), 프로젝트 설정
└── runMiyabirun.sln, *.csproj      # Unity가 자동 생성하는 C# 솔루션/프로젝트
```

## 실행 방법

1. **Unity Hub**에서 `2022.3.62f2` (동일 LTS 버전 권장) 에디터를 설치합니다.
2. Unity Hub의 `Add` → `E:\GamePJ\runMiyabirun` 폴더를 프로젝트로 추가하고 엽니다.
3. Unity 에디터에서 `Assets/Scenes/SampleScene.unity`를 엽니다.
4. Play 버튼으로 실행합니다.
   - **조작**: 좌우 방향키 / A·D (`Horizontal` 축) — 좌우 이동, Space (`Jump`) — 점프
5. 빌드는 `File > Build Settings`에서 대상 플랫폼을 선택하고 `SampleScene`을 씬 목록에 포함한 뒤 빌드합니다.

> 참고: `Library/`, `Temp/`, `obj/`, `*.csproj`, `*.sln`은 Unity가 재생성하는 산출물이며 `.gitignore`에 반영되어 있습니다. DOTween이 처음 임포트될 때 `Tools > Demigiant > DOTween Utility Panel`에서 Setup이 필요할 수 있습니다.

## 개발 현황

- Git 히스토리는 `Initial commit` 단일 커밋이며, 프로젝트 버전은 `0.1.0`입니다.
- 게임 로직 스크립트는 8개(플러그인 제외 7개 + 인터페이스)로, 이동·맵 생성·지형 상호작용·연출 등 **코어 러너 루프의 프로토타입까지 구현**된 상태입니다.
- 아직 구현되지 않은 영역: 점수/거리 UI, 게임오버 및 리스타트 흐름, 적과의 실제 충돌·추격 로직(`EnemyVisualController`는 시각 연출만 존재), 사운드, 씬 전환.
- 코드는 섹션 주석과 XML 문서 주석으로 정리되어 있으나, 일부 파일의 한글 주석이 인코딩 문제로 깨져 있습니다(UTF-8 재저장 권장).
