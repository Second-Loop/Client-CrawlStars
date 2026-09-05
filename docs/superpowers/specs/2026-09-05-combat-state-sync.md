# SL-124 서버 전투 상태 동기화

확정된 최종 점검 수정 범위예요. 클라이언트의 로컬 탄약·쿨타임 판단으로 서버가 거절한 행동까지 소모되는 문제를 해결해요.

- 온라인 ClientGameLoop는 서버 snapshot의 `PressedSkill`, `SkillReadyTick`, `AttackCharges`, `NextAttackChargeTick`, `AttackReadyTick`을 읽어요. 마지막 필드는 서버 closeout branch의 additive AsyncAPI 0.9.0 계약이에요.
- 로컬 클릭만으로 탄약이나 스킬을 소모하지 않아요. 서버가 확정한 상태만 게임 입력의 사용 가능 여부에 반영해요. 시간 보간은 UI 진행률에만 사용해요.
- snapshot이 없거나 자신이 죽었으면 공격을 허용하지 않아요. 오래된·같은 tick snapshot을 무시하고 매치 종료/시작 때 reset해요.
- 온라인 입력 허용: 탄약이 양수이고 (`AttackReadyTick == 0` 또는 `snapshotTick + 1 >= AttackReadyTick`)이면 일반 공격을 보낼 수 있어요. 스킬은 `snapshotTick + 1 >= SkillReadyTick`일 때 보낼 수 있어요. 서버가 실제 도착 tick에서 다시 검증해요.
- `AttackReadyTick`는 마지막 발사 다음 tick이고, 연사가 끝나면 0이에요. 입력이 실제로 승인됐다는 뜻은 아니에요.
- 표시용 maxBullets는 Shelly/Colt/Lily 3/3/2, skillAttackCoolDown은 12/13/11초예요. client config v3 모양과 거리 보조값은 유지해요.
- legacy offline Simulator와 기존 CooldownController 동작은 유지해요. AttackManager.Initialize의 온라인 모드를 명시하거나 별도 경로로 분리해요.
- 이번 작업은 입력 큐·액션 보존 방식, 대시·밸런스·Ready timeout을 구현하지 않아요. 그것들은 별도 사용자 질문의 결정이 필요해요.
- Unity Editor 미설치 제한을 정확히 보고하고 순수 상태 로직의 C# 실행과 Unity 실행을 구분해요.
