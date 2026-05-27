NOTE
===
The Note is for the better developing experience

## Osu.Game \ Skinning
- \ IAnimationTimeReference.cs
  - AnimationStartTime The time which animations should be started from

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
- \ Drawables
  - \ DrawableHitObject.cs
    - ln:762 UpdateResults 紀錄HitCircle的時機
    - ln:784 CheckForResult (有用)
- \ UI
  - \ HitObjectContainer.cs
    - ln:24 AliveObjects 記錄存在畫面的Object
- \ HitObjectLifetimeEntry.cs
  - ln:132 SetInitialLifetime 在Circle出現前先初始化
- \ SliderPath.cs
  - ln:289 calculatePath 根據Control Point種類計算Path
  - ln:338 calculateSubPath
- \ SliderEventGenerator.cs
  - 畫出Slider需要的事件

## Osu.Game \ Graphics \ Containers
- \ ParallaxContainer.cs
  - ln:69 Update 每一幀的Mouse Position

## Osu.Game.Rulesets.Osu
- \ Drawables
  - \ DrawableHitCircle.cs
    - ln:131 CheckForResult
  - \ DrawableSlider.cs
    - 有DrawableSliderball
  - \ DrawableSliderBall.cs
    - ln:64 UpdateProgress 計算每一幀Sliderball的位置
- \ UI
  - \ StartTimeOrderedHitPolicy.cs
    - **Important** 尋找畫面上的hit
- \ Objects
  - \ OsuHitObjects.cs
    - ln:64 記錄Hit的位置
  - \ Slider.cs
    - Slider的定義

## Osu! 內的各物件
- Hit
  - osu.Game.Rulesets.Osu.Objects.Drawables.DrawableHitCircle
    - 有 Great, Good, ok, meh, miss
- Slider
  - osu.Game.Rulesets.Osu.Objects.Drawables.DrawableSliderHead
    - 和 HitCircle 一樣
  - osu.Game.Rulesets.Osu.Objects.Drawables.DrawableSliderTail
    - SliderTailHit
  - osu.Game.Rulesets.Osu.Objects.Drawables.DrawableSlider
    - IgnoreHit, IgnoreMiss
  - osu.Game.Rulesets.Osu.Objects.Drawables.DrawableSliderRepeat
    - LargeTickHit, LargeTickMiss
  - osu.Game.Rulesets.Osu.Objects.Drawables.DrawableSliderTick
    - LargeTickHit, LargeTickMiss
- Spinner
  - osu.Game.Rulesets.Osu.Objects.Drawables.DrawableSpinner
    - 和 HitCircle 一樣
  - osu.Game.Rulesets.Osu.Objects.Drawables.DrawableSpinnerTick
    - SmallBonus
  - osu.Game.Rulesets.Osu.Objects.Drawables.DrawableSpinnerBonusTick
    - LargeBonus, IgnoreMiss

## 查看 Debug Console 的篩選條件
- [verbose], ![network], ![performance], !OsuScreenStack#559