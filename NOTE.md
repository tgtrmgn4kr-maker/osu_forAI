NOTE
===
The Note is for the better developing experience

## Osu.Game \ Rulesets \ objects \ scoring
- \ HitResult.cs
  - HitResult的定義
- \ scoreprocesser.cs
  - ln:178 MaximumStatistics
  - ln:225 ApplyResultInternal的定義
- \ judgementprocessor.cs
  -  ln:68 ApplyResult
- \ Scoreinfo.cs
  - Scoreinfo的定義
- \ HitWindows.cs
  - **Important** ln:82 ResultFor 判定的邏輯

## Osu.Game \ Rulesets \ Judgements
- \ JudgementResult.cs
  - ln:14 JudgementResult 的定義

## Osu.Game \ Rulesets \ Objects
- \ HitObject.cs HitObject的定義
- \ Drawables \ DrawableHitObject.cs
  - ln:762 UpdateResults 紀錄HitCircle的時機
  - ln:784 CheckForResult (有用)
- \ UI \ HitObjectContainer.cs
  - ln:24 AliveObjects 記錄存在畫面的Object
- \ HitObjectLifetimeEntry.cs
  - ln:132 SetInitialLifetime 在Circle出現前先初始化

## Osu.Game.Rulesets.Osu
- \ Drawables \ DrawableHitCircle.cs
  - ln:131 CheckForResult
- \ UI \ StartTimeOrderedHitPolicy.cs
  - **Important** 尋找畫面上的hit
- \ Objects \ OsuHitObjects.cs
  - ln:64 記錄Hit的位置
