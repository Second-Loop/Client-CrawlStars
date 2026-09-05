# Team Spectating Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task.

**Goal:** 팀전 개인 사망 후 안전한 아군 관전과 최종 서버 결과 수신을 구현해요.
**Architecture:** 순수 관전 대상 선택 로직을 두고 기존 PlayerManager, ClientGameLoop, 부쉬 시야, PlaySceneHandler에 연결해요. 서버 승패 규칙은 유지해요.
**Tech Stack:** Unity 6000.3.15f1, C#, NUnit EditMode tests.
**Spec:** docs/superpowers/specs/2026-09-05-team-spectating.md

## Global Constraints

- 서버 GameEnd 전에는 개인 사망을 Win/Lose로 확정하지 않아요.
- 사망한 내 캐릭터의 입력·예측·카메라 참조를 해제해요.
- 생존 아군 선택은 Slot 오름차순, 동률은 ID ordinal 오름차순이에요.
- 독립 검증과 Unity Editor 실행 결과를 구분해요.
- 다른 작업자가 있어요. 타인의 수정은 되돌리지 않아요.

### Task 1: 관전 선택과 Unity 흐름 연결

**Files:**
- Create: `CrawlStars/Assets/Scripts/Core/Player/SpectatorState.cs` and Unity `.meta`.
- Create: `CrawlStars/Assets/Editor/Tests/Core/SpectatorStateTests.cs` and `.meta`.
- Modify: `CrawlStars/Assets/Scripts/Core/Player/PlayerManager.cs`.
- Modify: `CrawlStars/Assets/Scripts/Core/ClientGameLoop.cs`.
- Modify: `CrawlStars/Assets/Scripts/Core/Map/BushVisibilityController.cs`.
- Modify: `CrawlStars/Assets/Scripts/Scene/PlaySceneHandler.cs`.
- Modify `GameManager.cs` only to expose the resulting spectator state if needed.
- Document: `CrawlStars/Docs/TeamSpectating.md`.

**Interfaces:**
- Consumes: existing PlayerData Id, Team, Slot, IsDead; PlayerManager.MyId/MyTeam/GetListener; server final GameEnd.
- Produces: read-only local-dead/IsSpectating state and current camera/view target. Prefer a small `SpectatorState` with `Observe(IReadOnlyList<PlayerData> players, string myId)` and `Reset()`; no external dependencies beyond PlayerData/System.
- Keep no mutable Unity objects in pure state. Store target ID and resolve live listener after pool changes.

- [ ] Write failing NUnit tests before production changes. Cover live self, dead self + two allies in reversed order, stable target, target death, no survivors, reset/new match, missing/null snapshot entries, enemies excluded.

```csharp
var state = new SpectatorState();
state.Observe(new[] {
    new PlayerData { Id = "me", Team = "red", IsDead = true },
    new PlayerData { Id = "ally-b", Team = "red", Slot = 2 },
    new PlayerData { Id = "ally-a", Team = "red", Slot = 1 }
}, "me");
Assert.That(state.TargetPlayerId, Is.EqualTo("ally-a"));
Assert.That(state.IsSpectating, Is.True);
```

- [ ] Execute failing pure C# tests with the SDK the controller is preparing at `/private/tmp/crawlstars-dotnet/dotnet`. If not available yet, report NEEDS_CONTEXT rather than pretending Unity ran. The controller can provide a standalone harness linking production SpectatorState and a minimal PlayerData data-only stub; keep harness files outside Unity project.
- [ ] Implement selection preserving an eligible existing target, then sorting eligible same-team alive players. Never choose the dead self or an enemy. Reset clears target/dead state. Treat missing self conservatively, avoiding unintended input reactivation.
- [ ] Integrate PlayerManager: clear MyListener on local elimination, process all player pool removals before resolving the selected target, update camera target or null. Recompute after target death. FocusCamera uses live resolved view target.
- [ ] Integrate ClientGameLoop: observe death before applying prediction, never send gameplay input or update prediction for dead self, never reactivate dead input from SetActiveInput(true). Snapshot receiving remains active so team results and surviving players update.
- [ ] Integrate BushVisibilityController: use selected live view listener; no dereference of null MyListener. Without any view target avoid newly exposing opponents. Keep team allegiance unchanged.
- [ ] Integrate PlaySceneHandler: reflect spectating state in existing info label, hide aim/cooldown during death; leave scene navigation and server GameEnd handling intact. Avoid unrelated UI rewrites.
- [ ] Add integration-focused EditMode tests where existing fixture setup permits. Read snapshot flow to verify leave-cancel cannot undo death guard and lifecycle Reset is wired.
- [ ] Run pure tests GREEN, inspect all touched call sites and `git diff --check`. Record any unavailable Unity check as blocked, with exact environment reason.
- [ ] Commit only task files using `[SL-124] fix(client): 팀전 사망 후 아군 관전` with Korean bullet body. Write report containing RED/GREEN evidence, files, self-review and Unity limitation. No push, no external messages, no subagents.
