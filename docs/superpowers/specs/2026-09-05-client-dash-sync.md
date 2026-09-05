# SL-124 서버 대시 상태 연동

SL-123의 9 tick 대시를 클라이언트 예측·입력 표시와 연결해요.

- PlayerData는 additive IsDashing을 역직렬화해요. AuthoritativeCombatState가 기존 snapshot 순서 검사와 함께 이 값을 보관하고, 대시 중 일반·스킬 공격 승인을 막아요. 서버 탄약·쿨타임 수치를 소비하는 기존 경로를 유지해요.
- AttackManager가 이 상태를 ClientGameLoop에 제공해요. 대시 snapshot을 받으면 최신 서버 위치를 관찰한 뒤 로컬 이동 예측을 취소하고 서버 위치를 렌더해요.
- 대시 중 InputSubmitted와 UpdateLocalPrediction은 새 예측을 시작하거나 위치를 덮어쓰지 않아요. 이동 입력 전송은 계속하되 서버가 대시 중 이동을 무시하는 계약을 따라요. 대시 종료 뒤 다음 정상 이동 입력은 예측할 수 있어요.
- Clear/재매칭과 로컬 player 누락에서 stale 예측·대시 상태가 남지 않아요. 사망 관전의 입력 차단도 유지해요. 기존 LocalMovementPredictor(PR #36 범위)는 수정하지 않아요.
- 최종 서버 AsyncAPI와 client config를 동일하게 보존하고, 서버 변경 PR이 main에 반영되어야 FileSynchronizer로 인수 빌드할 수 있음을 기록해요.
- 실제 Unity 실행 없이 C# 상태/NUnit 및 명시적 Unity stub 검증만 실행할 수 있어요. UI·카메라·프레임 체감은 Unity 인수 확인표에 남겨요.
