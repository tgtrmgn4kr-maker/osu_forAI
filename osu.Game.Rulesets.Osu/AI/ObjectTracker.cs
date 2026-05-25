// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Objects;


using System;


namespace osu.Game.Rulesets.Osu.AI
{
    public class ObjectTracker
    {
        private OsuPlayfield.AIPlayfield? playfield;

        public void AIObjectRegister(OsuPlayfield.AIPlayfield playfield)
        {
            this.playfield = playfield;
            playfield.NewResult += onNewResult;
        }
        private void onNewResult(DrawableHitObject obj, JudgementResult result)
        {
            getNext8Objects();
        }
        private void getNext8Objects()
        {
            var nextObjects = playfield?.HitObjectContainer.Get8AliveObjects();

            if (nextObjects is null) return;

            foreach (DrawableHitObject obj in nextObjects)
            {
                if (obj.HitObject is OsuHitObject @object)
                {
                    var osuObject = @object;
                    osuTK.Vector2 position = osuObject.StackedPosition;
                    double timeToHit = osuObject.StartTime;
                    var type = osuObject.GetType();
                    Console.WriteLine($"Position:{position}");
                    Console.WriteLine($"Time to hit:{timeToHit}");
                    Console.WriteLine(type);
                }

            }
        }
    }
}
