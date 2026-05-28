// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Objects;
using osu.Framework.Logging;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;



namespace osu.Game.Rulesets.Osu.AI
{
    public class ObjectTracker
    {
        public ObjectTracker(OsuPlayfield.AIPlayfield playfield)
        {
            this.playfield = playfield;
            playfield.NewResult += onNewResult;
        }

        private OsuPlayfield.AIPlayfield? playfield;

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
                if (obj.HitObject is SliderHeadCircle sliderHeadCircle)
                {
                    var osuObject = sliderHeadCircle;
                    Vector2 position = osuObject.StackedPosition;
                    double timeToHit = osuObject.StartTime;
                    var type = osuObject.GetType();

                }
                if (obj.HitObject is Slider slider)
                {
                    var sliderHead = slider.HeadCircle;
                    var type = slider.GetType();
                    double timeToHit = slider.StartTime;
                    Vector2 HeadPos = sliderHead.Position;
                }
            }
        }
        public void Update()
        {
            getObjectTiming();
            getSliderBallPosition();
        }

        private void getObjectTiming()
        {
        }
        private void getSliderBallPosition()
        {
            var obj = playfield?.HitObjectContainer.GetNextSlider();
            if (obj is null) return;
            if (obj is DrawableSlider slider)
            {
                if (slider.Ball is not null)
                {
                    Vector2 ballPos = slider.Ball.Position;
                    //Logger.Log($"Sliderball Position: {ballPos}");
                }

                Vector2 headPos = slider.HeadCircle.Position;
                //Logger.Log("SliderHeadPos: ", headPos.ToString());
            }
        }
    }
}
