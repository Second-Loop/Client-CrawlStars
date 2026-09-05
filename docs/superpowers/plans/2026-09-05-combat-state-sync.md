# Combat State Sync Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task.

**Goal:** 온라인 전투 UI와 입력 허용 상태를 서버 snapshot에 맞춰요.
**Architecture:** 순수 AuthoritativeCombatState가 snapshot을 해석해요. AttackManager는 온라인에서는 이 상태를 사용하고 offline legacy 경로는 기존 CooldownController를 유지해요.
**Tech Stack:** Unity 6000.3.15f1, C#, NUnit.
**Spec:** docs/superpowers/specs/2026-09-05-combat-state-sync.md

## Global Constraints

- 온라인 클릭만으로 탄약이나 스킬을 소모하지 않아요.
- 서버 snapshot이 없거나 사망 상태면 공격을 허용하지 않아요.
- 표시용 maxBullets는 3/3/2, skillAttackCoolDown은 12/13/11초예요.
- 기존 관전 death guard와 offline CooldownController를 유지해요.
- 외부 메시지·push·배포·서브에이전트 없이 이 작업만 수행해요.

### Task 1: 서버 승인 상태 DTO와 온라인 전투 UI

**Files:**
- Create `CrawlStars/Assets/Scripts/Core/Player/AuthoritativeCombatState.cs` + `.meta`.
- Create `CrawlStars/Assets/Editor/Tests/Core/AuthoritativeCombatStateTests.cs` + `.meta`.
- Modify `CrawlStars/Assets/Scripts/Core/Player/PlayerData.cs`, `AttackManager.cs`, `CrawlStars/Assets/Scripts/Core/ClientGameLoop.cs`.
- Sync `CrawlStars/Assets/StreamingAssets/game-config.json` byte-for-byte from `/private/tmp/crawlstars-closeout-server/client-config/game-config.json`.
- Sync `CrawlStars/Docs/References/API/asyncapi.yaml` from `/private/tmp/crawlstars-closeout-server/api/asyncapi.yaml`; review paired OpenAPI but no REST change.
- Document `CrawlStars/Docs/CombatStateSync.md`.

**Interfaces:**
- DTO fields: `PressedSkill: bool`, `SkillReadyTick: long`, `AttackCharges: int`, `NextAttackChargeTick: long`, `AttackReadyTick: long`.
- Pure state suggested API: `AuthoritativeCombatState(int maxCharges, float rechargeSeconds, float skillCooldownSeconds)`, `Observe(long snapshotTick, PlayerData player)`, `Tick(float deltaSeconds)`, `Reset()`. Properties match IAttackCooldownSource plus `CanNormalAttack` and `CanSkillAttack`.
- `AttackManager.Initialize(bool serverAuthoritative = false)` preserves legacy callers. Online `ClientGameLoop.Initialize` passes true. `AttackManager.ObserveSnapshot(long tick, PlayerData player)` delegates only in online mode. Online Try methods return current server-based permission without local spending.

- [ ] Write RED tests for initial blocked, skill approval at tick1/ready361 with unchanged charges; early retry remains blocked without restarting timer; charges and recharge tick correction; Shelly reload updates to max; burst lock while charges positive; inclusive next-input boundary; dead player; stale/duplicate snapshots; Reset; huge delta affects display but never creates charges or input permission.

```csharp
var state = new AuthoritativeCombatState(3, 1f, 12f);
state.Observe(1, new PlayerData { AttackCharges = 2, NextAttackChargeTick = 31, SkillReadyTick = 361 });
Assert.That(state.CurrentCharges, Is.EqualTo(2));
Assert.That(state.CanSkillAttack, Is.False);
state.Tick(20f);
Assert.That(state.CanSkillAttack, Is.False);
Assert.That(state.CurrentCharges, Is.EqualTo(2));
state.Observe(360, new PlayerData { AttackCharges = 3, SkillReadyTick = 361 });
Assert.That(state.CanSkillAttack, Is.True);
```

- [ ] Run pure C# harness with `/private/tmp/crawlstars-dotnet/dotnet` using actual state file and minimal data-only PlayerData stub. Test fails before implementation for expected missing behavior. Keep harness at `/private/tmp/crawlstars-combat-tests`, outside Unity project.
- [ ] Implement monotonic snapshot adoption and snapshot-derived permission. Use `snapshotTick + 1` safely without overflow. Progress ranges 0..1; NextAttackChargeTick=0/max charges means full normal progress. Do not locally add charges when estimated time advances.
- [ ] Wire online mode and observation before next input decisions, preserve spectator lifecycle guard. Reset state for new matches and Clear; no network send after death.
- [ ] Copy confirmed shared config and schema from server branch. Add tests that JSON DTO fields deserialize when possible with local Newtonsoft package/stubs. Preserve unrelated config version validation for later task.
- [ ] Run GREEN pure tests and existing legacy cooldown tests if the harness can link Unity Mathf stub. State clearly that these are not Unity Editor tests. `git diff --check` and inspect call sites.
- [ ] Commit scoped files with `[SL-124] fix(client): 서버 탄약과 쿨타임 상태 동기화` and Korean bullet body. Full report includes RED/GREEN, limitations, files and self-review. Do not claim shipped or Unity validated.
