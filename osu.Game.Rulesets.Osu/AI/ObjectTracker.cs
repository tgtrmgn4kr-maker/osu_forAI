// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;

using osu.Framework.Logging;
using osu.Game.Rulesets.Osu.Objects.Drawables;



namespace osu.Game.Rulesets.Osu.AI
{
    public class ObjectTracker
    {
        public ObjectTracker(OsuPlayfield.AIPlayfield playfield, SharedTrackerState state)
        {
            this.playfield = playfield;
            playfield.OnAIPlayFieldNewDrawableHitObject += getNext8Objects;
            this.state = state;
        }

        public struct FrameObservation
        {
            public float CursorX;
            public float CursorY;

        }

        private readonly SharedTrackerState state;
        private OsuPlayfield.AIPlayfield? playfield;

        private void getNext8Objects(DrawableHitObject _)
        {
            var nextObjects = playfield?.HitObjectContainer.Get8AliveObjects();

            if (nextObjects is null) return;

            foreach (DrawableHitObject obj in nextObjects)
            {/*
                switch (obj)
                {
                    case DrawableHitCircle hitCircle:
                        Logger.Log(
                            $"Circle Pos: {hitCircle.HitObject.StackedPosition}"
                        );
                        break;

                    case DrawableSlider slider:

                        Logger.Log(
                            $"Slider Pos: {slider.HitObject.StackedPosition}"
                        );

                        Logger.Log(
                            $"Slider StartTime: {slider.HitObject.StartTime}"
                        );

                        break;
                }*/
                Logger.Log($"Type: {obj.GetType()}");
                Logger.Log($"Type: {obj.HitObject.GetType()}");
            }
        }
        public void Update()
        {
            //trackSliderBall();
        }

        private void trackSliderBall()
        {
            var hitObject = playfield?.HitObjectContainer.GetNextSlider();
            if (hitObject is DrawableSlider slider)
            {
                if (slider.Ball != null)
                {
                    //Logger.Log($"Slider ball position: {slider.Ball.Position}");
                }
            }

        }



    }
}
