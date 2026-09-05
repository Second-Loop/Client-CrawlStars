# Client Dash Sync Implementation Plan

> Required workflow: superpowers:subagent-driven-development.

**Spec:** docs/superpowers/specs/2026-09-05-client-dash-sync.md
**Dependency:** server gameplay Task3 IsDashing contract, existing SL-124 config task.

## Global constraints
- 신규 게임 규칙·예측 알고리즘·시각 효과를 추가하지 않아요.
- LocalMovementPredictor.cs와 기존 열린 PR36 파일은 수정하지 않아요.
- 기존 온라인/오프라인 공격 분기와 관전의 사망 latch를 유지해요.
- 작업자 외부 전송·push·merge·배포·서브에이전트 금지.

### Task 1: 대시 snapshot과 클라이언트 입력/예측 동기화

**Files:** modify CrawlStars/Assets/Scripts/Core/Player/{PlayerData.cs,AuthoritativeCombatState.cs,AttackManager.cs}, Core/ClientGameLoop.cs; update/add corresponding Editor/Tests/Core regression tests + meta; copy final server api/asyncapi.yaml to CrawlStars/Docs/References/API/asyncapi.yaml and client config to CrawlStars/Assets/StreamingAssets/game-config.json; document CrawlStars/Docs/DashStateSync.md, update current combat/config docs where needed.
- RED tests: IsDashing=true + otherwise-ready state blocks both attacks; newest snapshot wins; missing/reset removes stale state; JSON roundtrip field. Add meaningful actual ClientGameLoop coverage for prediction setup/update suppression during dash, false-state resumption, Clear/dead/missing self. Real NUnit sources in temporary harness with explicitly documented Unity stubs are acceptable; never claim Unity build passed.
- Add IsDashing to DTO and expose through AuthoritativeCombatState/AttackManager. Reuse existing Observe/reset ordering so a second independent dash cache cannot diverge.
- ClientGameLoop consumes dash gate; after current server position is observed, cancel prediction during dash and force normal local snapshot position apply. Skip prediction creation and updates during dash but continue movement transport. Missing local player cancels outstanding prediction. Clear/reset unblocks only live states via next snapshot.
- Preserve current spectator input handling and offline attack mode. No changes to LocalMovementPredictor.
- Copy final server artifacts byte-identically; Colt skill now10bullets activation1 last17 ready18; Shelly9steps total2.7tiles. Record dependency on final server PRs, including FileSynchronizer main download.
- GREEN targeted tests plus prior combat/spectator regressions when shared code changed. git diff --check, self-review, scoped ticketed commit and complete report with commands/results/Unity limitations.
