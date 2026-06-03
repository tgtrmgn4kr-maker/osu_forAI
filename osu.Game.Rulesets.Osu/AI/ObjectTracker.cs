// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Framework.Logging;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using System.Collections.Generic;
using osuTK;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects.Types;



namespace osu.Game.Rulesets.Osu.AI
{
    public class ObjectTracker
    {
        public struct FrameObservation
        {
            public CursorRuntimeData CursorRuntimeData;
            public double CurrentTime;
            public OsuObjectsData[] Data;
            public SliderRuntimeData SliderRuntimeData;
            public SpinnerRuntimeData SpinnerRuntimeData;
        }

        // HitCircle and SliderHead
        public struct OsuObjectsData
        {
            public int ObjectType;
            public float DistanceToCursorX;
            public float DistanceToCursorY;
            public float ScalarDistance;
            public double TimeToHit;
        }

        // SliderBall only
        public struct SliderRuntimeData
        {
            public float DistanceToCursorX;
            public float DistanceToCursorY;
            public float ScalarDistance;
            public double Velocity;
            public double Progress;
            public float DirectionX;
            public float DirectionY;
        }
        public struct SpinnerRuntimeData
        {
            public double SpinsPerMinute;
            public double RequiredSPM;
            public double Progress;
            public double RemainingTime;
        }
        public struct CursorRuntimeData
        {
            public float X;
            public float Y;
            public double VelocityX;
            public double VelocityY;
        }
        private Dictionary<Type, int> objectType = new()
        {
            [typeof(DrawableHitCircle)] = 0,

            [typeof(DrawableSliderHead)] = 1,
            [typeof(DrawableSliderTail)] = 2,
            [typeof(DrawableSlider)] = 3,
            [typeof(DrawableSliderRepeat)] = 4,
            [typeof(DrawableSliderTick)] = 5,

            [typeof(DrawableSpinner)] = 6,
            [typeof(DrawableSpinnerTick)] = 7,
            [typeof(DrawableSpinnerBonusTick)] = 8,
        };

        public ObjectTracker(OsuPlayfield.AIPlayfield playfield, SharedTrackerState state)
        {
            this.playfield = playfield;
            this.state = state;

            // The first frame
            previousCursorX = playfield!.CursorPosition.X;
            previousCursorY = playfield!.CursorPosition.Y;
        }

        private SharedTrackerState state;
        private OsuPlayfield.AIPlayfield? playfield;
        private FrameObservation frameObservation;
        public FrameObservation GetFrameObservation => frameObservation;
        private int count;
        private double previousTime;
        private float previousCursorX;
        private float previousCursorY;

        private void getNext10Objects()
        {
            // The objects here have been sorted by StartTime
            var nextObjects = playfield?.HitObjectContainer.Get10AliveObjects();

            double currentTime = playfield!.CurrentTime;

            if (nextObjects is null) return;

            count = 0;

            frameObservation = new()
            {
                CurrentTime = currentTime,
                Data = new OsuObjectsData[10],
                SliderRuntimeData = new(),
                SpinnerRuntimeData = new(),
            };

            foreach (DrawableHitObject obj in nextObjects)
            {
                // All the data is normalised
                OsuObjectsData data = new();
                int objTypeInt = objectType[obj.GetType()];
                data.ObjectType = objTypeInt;
                if (objTypeInt == 0) // HitCircle
                {
                    // Make sure that the position is relative position
                    Vector2 position = ((OsuHitObject)obj.HitObject).StackedPosition;

                    // Relative Position
                    data.DistanceToCursorX = (position.X - playfield!.CursorPosition.X) / 256f;
                    data.DistanceToCursorY = (position.Y - playfield!.CursorPosition.Y) / 192f;
                    data.ScalarDistance = (float)Math.Sqrt(Math.Pow(data.DistanceToCursorX, 2f) + Math.Pow(data.DistanceToCursorY, 2f));

                    double TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = TimeToHit / 1000f;
                }
                else if (objTypeInt == 3) // Slider
                {
                    var slider = (DrawableSlider)obj;
                    Vector2 position = slider.HitObject.StackedPosition;

                    // Relative Position
                    data.DistanceToCursorX = (position.X - playfield!.CursorPosition.X) / 256f;
                    data.DistanceToCursorY = (position.Y - playfield!.CursorPosition.Y) / 192f;
                    data.ScalarDistance = (float)Math.Sqrt(Math.Pow(data.DistanceToCursorX, 2f) + Math.Pow(data.DistanceToCursorY, 2f));

                    double TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = TimeToHit / 1000f;
                }
                else if (objTypeInt == 6) // Spinner
                {
                    Logger.Log("Spinner");
                    // There is no need to calculate the position of a spinner
                    double TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = TimeToHit / 1000f;
                }

                frameObservation.Data[count] = data;
                count++;
            }
        }
        public void Update()
        {
            getNext10Objects();
            trackSliderBall();
            trackSpinner();
            trackCursor();
        }

        private void trackSliderBall()
        {
            var hitObject = playfield?.HitObjectContainer.GetNextSlider();
            if (hitObject is DrawableSlider slider)
            {
                // Multiple sliders may be in the active state at the same time, but only the first slider has a slider ball.
                if (slider.Ball != null)
                {
                    double velocity = slider.HitObject.Velocity;
                    frameObservation.SliderRuntimeData.Velocity = velocity / 10f;

                    double remainingTime = slider.HitObject.EndTime - playfield!.CurrentTime;
                    double totalTime = slider.HitObject.Duration;
                    double progress = remainingTime / totalTime;
                    frameObservation.SliderRuntimeData.Progress = Math.Clamp(1f - progress, 0f, 1f);

                    var position = slider.Ball.Position + slider.HitObject.StackedPosition;
                    frameObservation.SliderRuntimeData.DistanceToCursorX = (position.X - playfield!.CursorPosition.X) / 256f;
                    frameObservation.SliderRuntimeData.DistanceToCursorY = (position.Y - playfield!.CursorPosition.Y) / 192f;
                    frameObservation.SliderRuntimeData.ScalarDistance =
                        (float)Math.Sqrt(
                            Math.Pow(frameObservation.SliderRuntimeData.DistanceToCursorX, 2) +
                            Math.Pow(frameObservation.SliderRuntimeData.DistanceToCursorY, 2));

                    var nextPosition = slider.HitObject.StackedPosition + slider.HitObject.CurvePositionAt(Math.Clamp(progress + 0.1f, 0, 1));
                    var deltaPosition = nextPosition - position;
                    frameObservation.SliderRuntimeData.DirectionX = deltaPosition.X / 256f;
                    frameObservation.SliderRuntimeData.DirectionY = deltaPosition.Y / 192f;
                }
            }
        }
        private void trackSpinner()
        {
            var hitObject = playfield?.HitObjectContainer.GetNextSpinner();
            if (hitObject is DrawableSpinner spinner)
            {
                // The spm calculated by osu for each frame
                frameObservation.SpinnerRuntimeData.SpinsPerMinute = spinner.SpinsPerMinute.Default / 100f;
                frameObservation.SpinnerRuntimeData.RequiredSPM = spinner.HitObject.SpinsRequiredForBonus / 100f;

                double remainingTime = spinner.HitObject.EndTime - playfield!.CurrentTime;
                double totalTime = spinner.HitObject.Duration;
                frameObservation.SpinnerRuntimeData.Progress = Math.Clamp(1f - (remainingTime / totalTime), 0f, 1f);
                frameObservation.SpinnerRuntimeData.RemainingTime = remainingTime;
            }
        }
        private void trackCursor()
        {
            double dt = (frameObservation.CurrentTime - previousTime) / 1000f;

            frameObservation.CursorRuntimeData.X = (playfield!.CursorPosition.X - 256f) / 256f;
            frameObservation.CursorRuntimeData.Y = (playfield!.CursorPosition.Y - 192f) / 192f;

            float dx = frameObservation.CursorRuntimeData.X - previousCursorX;
            float dy = frameObservation.CursorRuntimeData.Y - previousCursorY;

            frameObservation.CursorRuntimeData.VelocityX = dx / dt;
            frameObservation.CursorRuntimeData.VelocityY = dy / dt;

            previousTime = frameObservation.CurrentTime;
            previousCursorX = frameObservation.CursorRuntimeData.X;
            previousCursorY = frameObservation.CursorRuntimeData.Y;

        }
    }
}
