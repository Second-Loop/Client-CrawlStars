# 대시 상태 동기화

## 전체 흐름

1. 서버 gameplay snapshot이 내 `PlayerData.IsDashing`을 보내요.
2. `AuthoritativeCombatState`가 기존 tick 순서 검사 안에서 대시 상태와 공격 가능 여부를 함께 갱신해요.
3. `ClientGameLoop`는 먼저 snapshot의 최신 서버 위치를 `LocalMovementPredictor`에 관찰시켜요.
4. 대시 중이면 예측을 취소하고 그 최신 서버 위치를 화면에 적용해요.
5. `IsDashing=false`인 새 snapshot 뒤의 다음 이동 입력부터 로컬 예측을 다시 시작할 수 있어요.

```text
새 snapshot → 내 player 확인 → 서버 위치 관찰 → IsDashing?
                                           ├─ true  → 예측 취소·서버 위치 적용
                                           └─ false → 정상 예측 허용
```

## 입력과 예측 규칙

- 대시 중에도 이동 입력은 서버로 계속 전송해요. 서버가 대시 구간 동안 일반 이동을 무시하고 대시 방향을 유지해요.
- 대시 중 `InputSubmitted`는 새 로컬 예측을 만들지 않고, 프레임별 예측 위치 갱신도 취소해요.
- 일반 공격과 스킬 공격은 `IsDashing=true`이면 클라이언트에서 보내지 않아요. 서버도 같은 상태와 `AttackReadyTick`으로 다시 검증해요.
- 로컬 player가 사망하거나 snapshot에서 누락되면 예측과 공격 상태를 함께 막아요.
- `Players=null`인 tick 0 시작 snapshot은 의도한 prestart 상태로 취급해요. 양수 gameplay tick에서 `Players=null`이면 stale 예측과 전투 상태를 지워요.
- `ClientGameLoop.Clear`는 대시와 예측을 초기화해요. 다음 매치에서 실제 player snapshot을 받은 뒤에만 다시 예측해요.

`LocalMovementPredictor`의 계산 규칙은 바꾸지 않아요. 대시 여부는 별도 예측 cache를 만들지 않고 `AuthoritativeCombatState`가 채택한 최신 snapshot 한 곳에서 읽어요.

## 서버 계약

- Shelly 대시는 총 `2.7 tiles`를 `9`개 tick 구간으로 이동해요. 마지막 구간, 충돌 또는 사망 snapshot은 `IsDashing=false`, `AttackReadyTick=0`을 보내요.
- Colt 스킬은 10발이에요. activation tick이 `1`이면 마지막 발은 tick `17`에 나오고, `AttackReadyTick=18`부터 일반 공격을 다시 승인해요.
- WebSocket 필드와 예시는 `Docs/References/API/asyncapi.yaml` 0.10.0을 따라요.
- 표시용 `Assets/StreamingAssets/game-config.json`은 서버 `client-config/game-config.json` v3와 byte 단위로 같아야 해요.

## 인수 빌드 선행 조건

서버 변경은 stacked PR 순서인 PR #75 → PR #76 → 최종 PR #77로 `main`에 반영되어야 해요. Unity의 `FileSynchronizer`는 로컬 복사본이 아니라 서버 `main`의 client config를 build callback order `0`에서 다시 내려받아요. 따라서 PR #77까지 `main`에 반영된 뒤 client build를 실행하고, callback order `100`의 validator가 내려받은 최종 v3 파일을 검사해야 해요.

현재 환경에서는 standalone NUnit 하네스로 상태와 예측 경계를 검증해요. Unity Editor build, UI·카메라 이동, 프레임 체감은 별도 Unity 인수 확인이 필요해요.
