# 서버 전투 상태 동기화

## 전체 흐름

1. 서버 gameplay snapshot에서 내 `PlayerData`를 찾아요.
2. `AuthoritativeCombatState`가 이전보다 새로운 tick만 받아들여요.
3. 탄약·연사 잠금·스킬 준비 시점을 서버 값으로 교체해요.
4. 다음 server tick에 적용될 입력이 가능한지 판단해요.
5. 프레임 시간은 UI 진행률만 부드럽게 표시하는 데 사용해요.

## 온라인 입력 규칙

| 입력 | 클라이언트 전송 조건 |
| --- | --- |
| 일반 공격 | 살아 있고 대시 중이 아니며 `AttackCharges > 0`이고, `AttackReadyTick == 0` 또는 `snapshotTick + 1 >= AttackReadyTick`예요. |
| 스킬 공격 | 살아 있고 대시 중이 아니며 `SkillReadyTick == 0` 또는 `snapshotTick + 1 >= SkillReadyTick`예요. |

`snapshotTick + 1`은 입력이 서버에서 적용될 다음 tick이에요. `long.MaxValue`에서는 overflow가 생기지 않도록 값을 그대로 유지해요. 서버는 입력을 받은 뒤 실제 적용 tick의 탄약과 cooldown을 다시 검증해요.

클릭만으로 `AttackCharges`나 `SkillReadyTick`을 바꾸지 않아요. 서버가 거절한 입력은 자원을 소모하지 않고, 다음 snapshot이 동일한 준비 시점을 보내면 기존 진행 상태를 이어서 표시해요. Colt 연사 중에는 탄약이 남아 있어도 `AttackReadyTick`까지 일반 공격을 막아요.

## UI 표시와 snapshot 수명

- `CurrentCharges`는 마지막으로 채택한 snapshot 값이에요.
- `NormalProgress`는 `NextAttackChargeTick`과 30Hz server tick을 기준으로 계산해요.
- `SkillProgress`는 `SkillReadyTick`과 캐릭터 cooldown을 기준으로 계산해요.
- `Tick(deltaSeconds)`는 두 진행률을 0~1 사이에서 보간하지만 탄약이나 입력 권한을 만들지 않아요.
- 같은 tick이나 더 오래된 snapshot은 무시해요.
- snapshot이 없거나 내 데이터가 없거나 내가 사망했거나 `IsDashing=true`이면 공격을 막아요.
- 새 매치 초기화와 `ClientGameLoop.Clear`에서 상태를 초기화해요.

## 온라인과 legacy 경계

온라인 `ClientGameLoop`는 `AttackManager.Initialize(serverAuthoritative: true)`를 사용해요. legacy offline Simulator는 인자 없는 `Initialize()`를 계속 사용하므로 기존 `CooldownController`가 로컬 탄약 소비와 회복을 담당해요.

표시용 client config v3의 캐릭터 값은 다음과 같아요.

| 캐릭터 | 최대 탄약 | 스킬 cooldown |
| --- | ---: | ---: |
| Shelly | 3 | 12초 |
| Colt | 3 | 13초 |
| Lily | 2 | 11초 |

WebSocket 계약은 `Docs/References/API/asyncapi.yaml`의 AsyncAPI 0.10.0을 따라요. `PressedSkill`, `SkillReadyTick`, `AttackCharges`, `NextAttackChargeTick`, `AttackReadyTick`, `IsDashing`은 서버가 보내는 `PlayerData` 필드예요. 대시 중 위치 예측 처리와 reset 경계는 `DashStateSync.md`에서 설명해요.

## 검증 범위

순수 C# 하네스는 실제 상태·manager·legacy cooldown·DTO 소스를 컴파일해 경계와 JSON 역직렬화를 확인해요. Unity Editor가 설치되지 않은 환경에서는 EditMode/PlayMode 실행과 실제 화면 렌더링을 검증할 수 없으므로 별도 Unity 실행이 필요해요.
