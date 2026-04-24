# step-217 bind post-move attack to strategy intent and expand day6 ai coverage

date: 2026-04-24

changed:
- `Scripts/Battle/AI/EnemyAiDecision.cs`
  - `Move(...)` now carries an optional preferred follow-up attack target id
- `Scripts/Battle/AI/EnemyTurnResolver.cs`
  - post-move attack now first tries the strategy-provided preferred target
  - if no preferred target is provided or it is no longer valid, resolver falls back to opponent-unit-only attack checks
  - this avoids random obstacle follow-up attacks during normal player chase moves
- `Scripts/Battle/AI/EnemyAiTactics.cs`
  - chase-or-break decisions now attach the correct intended post-move target id:
    - player target when route is open
    - blocking obstacle target when the route is blocked by a destructible obstacle
- `Scripts/Battle/AI/Strategies/SupportHealerEnemyAiStrategy.cs`
  - final chase fallback now uses the shared chase-or-break path analysis instead of a weaker direct approach heuristic
- `Scripts/Battle/AI/Strategies/ObstacleBomberEnemyAiStrategy.cs`
  - post-move obstacle attacks are now intentionally targeted only when the bomber is actually advancing toward a blocking obstacle

result:
- move-then-attack now better matches strategy intent instead of opportunistically hitting unrelated obstacles
- regular chase moves now favor continuing pressure on the player
- blocked-path obstacle pressure is still preserved when the strategy explicitly decides to break through
- support healer now benefits from the same path-block reasoning as the other core enemy strategies
