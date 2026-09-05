# 클라이언트 게임 설정 검증

## 구조

1. `FileSynchronizer`가 build callback order `0`에서 서버 `main`의 `game-config.json`을 내려받아요.
2. `GameConfigBuildValidator`가 callback order `100`에서 최종 파일을 검증해요.
3. 앱 실행 중에는 `GameConfig.LoadAsync`가 같은 `GameConfigParser`로 다시 검증한 뒤 정적 설정을 한 번에 반영해요.

```text
서버 main 다운로드 (order 0)
           ↓
공유 v3 parser로 build 검증 (order 100)
           ↓
runtime 다운로드 → 공유 v3 parser → 전체 설정 반영
```

## 검증 계약

클라이언트는 서버 `client-config/game_config.go`와 같은 v3 소비 계약을 적용해요.

- `version`은 정수 `3`이어야 해요.
- 모든 필수 필드가 있어야 하며 `null`은 허용하지 않아요.
- 거리와 반지름은 유한한 양수여야 해요. `NaN`과 무한대는 거절해요.
- 공격 쿨타임과 탄약 수는 양의 정수여야 해요.
- `characters`에는 type `0`, `1`, `2`가 각각 한 번씩 있어야 해요.
- 알 수 없는 추가 필드는 이후 서버 확장을 위해 허용해요.

검증에 실패하면 build는 `BuildFailedException`으로 중단돼요. runtime에서는 `LoadAsync`가 `false`를 반환하며 이전에 정상 반영된 정적 설정을 유지해요.

## 인수 빌드 선행 조건

`FileSynchronizer`는 서버 저장소의 `main` 브랜치를 읽어요. v3 파일을 도입하는 서버 PR #75가 먼저 merge되어야 해요. SL-124 대시까지 포함한 최종 인수 build는 stacked server PR #75 → PR #76 → PR #77이 모두 `main`에 반영된 뒤 실행해요. 그래야 callback order `0`에서 최종 client config를 내려받고 order `100`에서 같은 파일을 검증해요.

현재 환경에는 Unity Editor가 없어서 Editor build 실행은 별도 확인이 필요해요. 순수 C# parser와 runtime 반영 경로는 standalone NUnit harness로 검증해요.
