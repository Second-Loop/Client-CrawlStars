# SL-124 클라이언트 config 소비 계약 검증

기존 client config v3 계약을 클라이언트에서도 검증해요. 새 게임 규칙을 추가하지 않아요.

- 서버 `client-config/game_config.go`와 동일하게 version 3, 필수 필드, 유한 양수 값, 정수 쿨타임/탄약, 캐릭터 type 0/1/2 각 하나를 요구해요.
- 순수 JSON 검증을 runtime LoadAsync와 Unity build preprocessor가 공유해요. 잘못된 파일은 runtime에서 false, build에서 명확한 BuildFailedException으로 거절해요.
- 기존 `FileSynchronizer`의 build callback order가 0이에요. 새 검증 callback은 100으로 두어 다운로드된 최종 파일을 검증해요. 서버 main에서 내려받으므로 서버 PR #75가 클라이언트 인수 빌드의 선행 조건이에요.
- 성공적으로 검증하기 전에는 GameConfig 정적 값을 바꾸지 않아요. 알 수 없는 추가 필드는 기존 Go parser처럼 허용해요.
- 실제 StreamingAssets config와 잘못된 JSON/누락/null/잘못된 version/중복 type/음수/소수 정수/비유한 값 회귀를 실행해요.
- Unity Editor 미설치이므로 실제 Editor build 성공은 주장하지 않아요. 순수 C# 실행과 Editor 연결 소스 검증을 구분해요.
