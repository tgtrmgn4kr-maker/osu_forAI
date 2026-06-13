// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using System.Collections.Generic;
using osuTK;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects.Types;
using System.Runtime.InteropServices;



namespace osu.Game.Rulesets.Osu.AI
{
    public class ObjectTracker
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FrameObservation
        {
            public byte Playing;
            public long FrameID;
            public double CurrentTime;

            public CursorRuntimeData CursorRuntimeData;
            public SliderRuntimeData SliderRuntimeData;
            public SpinnerRuntimeData SpinnerRuntimeData;
        }

        // HitCircle and SliderHead
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct OsuObjectsData
        {
            public byte IsCircle;
            public byte IsSlider;
            public byte IsSpinner;
            public float DistanceToCursorX;
            public float DistanceToCursorY;
            public float ScalarDistance;
            public double TimeToHit;
        }

        // SliderBall only
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SliderRuntimeData
        {
            public float DistanceToCursorX;
            public float DistanceToCursorY;
            public float ScalarDistance;
            public double Velocity;
            public double Progress;
            public float DirectionX;
            public float DirectionY;
            public SliderRuntimeData()
            {
                Progress = -1;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SpinnerRuntimeData
        {
            public double SpinsPerMinute;
            public double RequiredSPM;
            public double Progress;
            public double RemainingTime;
            public SpinnerRuntimeData()
            {
                Progress = -1;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CursorRuntimeData
        {
            public float X;
            public float Y;
            public double VelocityX;
            public double VelocityY;
        }
        private Dictionary<Type, int> objectType = new()
        {
            [typeof(DrawableHitCircle)] = 1,

            [typeof(DrawableSliderHead)] = 2,
            [typeof(DrawableSliderTail)] = 3,
            [typeof(DrawableSlider)] = 4,
            [typeof(DrawableSliderRepeat)] = 5,
            [typeof(DrawableSliderTick)] = 6,

            [typeof(DrawableSpinner)] = 7,
            [typeof(DrawableSpinnerTick)] = 8,
            [typeof(DrawableSpinnerBonusTick)] = 9,
        };

        private OsuPlayfield.AIPlayfield? playfield;
        private FrameObservation frameObservation;

        public OsuObjectsData[] GetData { get; private set; }
        public FrameObservation GetFrameObservation => frameObservation;
        private int count;
        private double previousTime;
        private float previousCursorX;
        private float previousCursorY;
        private long frameID;

        public ObjectTracker(OsuPlayfield.AIPlayfield playfield)
        {
            this.playfield = playfield;

            // The first frame
            previousCursorX = playfield!.CursorPosition.X;
            previousCursorY = playfield!.CursorPosition.Y;
            GetData = new OsuObjectsData[10];
        }

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
                SliderRuntimeData = new(),
                SpinnerRuntimeData = new(),
                FrameID = frameID
            };
            GetData = new OsuObjectsData[10];

            if (playfield.HitObjectContainer.Playing)
                frameObservation.Playing = 1;
            else
                frameObservation.Playing = 0;

            foreach (DrawableHitObject obj in nextObjects)
            {
                // All the data is normalised
                OsuObjectsData data = new();
                int objTypeInt = objectType[obj.GetType()];
                if (objTypeInt == 0) // HitCircle
                {
                    data.IsCircle = 1;
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
                    data.IsSlider = 1;
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
                    data.IsSpinner = 1;
                    // There is no need to calculate the position of a spinner
                    double TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = TimeToHit / 1000f;
                }

                GetData[count] = data;
                count++;
            }
        }
        public void Update(long frameID)
        {
            getNext10Objects();
            trackSliderBall();
            trackSpinner();
            trackCursor();
            this.frameID = frameID;
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
                    return;
                }
            }

            frameObservation.SliderRuntimeData.Progress = -1;
        }
        private void trackSpinner()
        {
            var hitObject = playfield?.HitObjectContainer.GetNextSpinner();
            if (hitObject is DrawableSpinner spinner)
            {
                // The spm calculated by osu for each frame
                frameObservation.SpinnerRuntimeData.SpinsPerMinute = spinner.SpinsPerMinute.Default / 1000f;
                frameObservation.SpinnerRuntimeData.RequiredSPM = spinner.HitObject.SpinsRequiredForBonus / 1000f;

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
