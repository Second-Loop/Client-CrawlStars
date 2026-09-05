# Config Validation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task.

**Goal:** 공유 client config v3 계약의 build/runtime 거절 조건을 구현해요.
**Architecture:** 순수 JSON parser 하나를 runtime와 Editor preprocessor가 재사용해요.
**Tech Stack:** C#, Newtonsoft.Json, Unity Editor build hook, NUnit.
**Spec:** docs/superpowers/specs/2026-09-05-config-validation.md

## Global Constraints

- 게임 수치와 wire 계약을 변경하지 않아요.
- version 3, 필수 필드, 유한 양수, 정수형 쿨타임/탄약, type 0/1/2 유일성을 검증해요.
- 유효하지 않은 파일은 정적 GameConfig에 일부만 반영되지 않아요.
- 외부 전송·push·배포·서브에이전트 없이 이 작업만 수행해요.

### Task 1: 공유 config parser와 소비 지점 검증

**Files:**
- Create `CrawlStars/Assets/Scripts/Core/GameConfigParser.cs` + `.meta`.
- Modify `CrawlStars/Assets/Scripts/Core/GameConfig.cs`.
- Create `CrawlStars/Assets/Editor/GameConfigBuildValidator.cs` + `.meta`.
- Create `CrawlStars/Assets/Editor/Tests/Core/GameConfigParserTests.cs` + `.meta`.
- Document `CrawlStars/Docs/ConfigValidation.md`.

- [ ] Read `/private/tmp/crawlstars-closeout-server/client-config/game_config.go` and existing GameConfig/CharacterInfo consumers. Preserve public consumer API. Parser data should be plain C#; avoid Unity in parser.
- [ ] Write RED tests using actual valid StreamingAssets file and invalid mutations: malformed/empty/null JSON, missing or null each required property, unsupported version, nonpositive values, fractional integer fields, nonfinite numbers, wrong/missing/duplicate character types, wrong array size, valid reordered types and unknown extra property. Verify rejection returns useful error and no partial state.
- [ ] Run tests with standalone `/private/tmp/crawlstars-dotnet/dotnet` harness in `/private/tmp/crawlstars-config-tests`, actual production parser/test sources and real Newtonsoft.Json from SDK or NuGet. Establish failing behavior before implementation.
- [ ] Implement shared strict TryParse API; maintain numeric JSON token type restrictions consistent with Go parser, supported version exactly3; unknown fields allowed. Do not hardcode current balance values.
- [ ] Route LoadAsync through parser, return false for invaliddata with concise diagnostic; commit all static config fields only after successful validation. Keep existing request/network retry flow.
- [ ] Add Editor `IPreprocessBuildWithReport` hook reading `Application.streamingAssetsPath/game-config.json`, throws BuildFailedException on missing/invalid config. Set callbackOrder=100, after existing FileSynchronizer order0, so final downloaded file is validated. Shared parser must be used, no duplicate rules. Document server PR75 merge is needed before acceptance build since synchronizer fetches server main.
- [ ] Run GREEN harness, document actual commands/results and distinguish real C# tests from Unity Editor execution. `git diff --check`, inspect lifecycle consumer compatibility.
- [ ] Commit only scoped files with `[SL-124] fix(config): 클라이언트 설정 소비 계약 검증` and Korean bullet body. Full report includes tests/limitations/self-review. No Unity completion claim.
