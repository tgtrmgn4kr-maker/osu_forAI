// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.AI;
using osuTK;



namespace osu.Game.Rulesets.Osu.AI
{
    public class ObjectTracker
    {
        /// <summary>
        /// Frame observation
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FrameObservation
        {
            public byte PlayingState;
            public long FrameID;
            public double CurrentTime;

            public CursorRuntimeData CursorRuntimeData;
            public SliderRuntimeData SliderRuntimeData;
            public SpinnerRuntimeData SpinnerRuntimeData;
            public FrameObservation() // Prevent from uninitialised when being written in memory or file
            {
                PlayingState = 0;
                FrameID = 0;
                CurrentTime = 0;
                CursorRuntimeData = new();
                SliderRuntimeData = new();
                SpinnerRuntimeData = new();
            }
        }

        /// <summary>
        /// HitCircle and SliderHead
        /// All the variables is normalised
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct OsuObjectsData
        {
            // One hot encoding
            public byte IsCircle;
            public byte IsSlider;
            public byte IsSpinner;
            public float X;
            public float Y;
            public float DistanceToCursorX;
            public float DistanceToCursorY;
            public float ScalarDistance;
            public double TimeToHit;
            public OsuObjectsData()
            {
                IsCircle = 0;
                IsSlider = 0;
                IsSpinner = 0;
                X = 0;
                Y = 0;
                DistanceToCursorX = 0;
                DistanceToCursorY = 0;
                ScalarDistance = -1;
                TimeToHit = -1;
            }
        }

        /// <summary>
        /// SliderBall only
        /// All the variables is normalised
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SliderRuntimeData
        {
            public float X;
            public float Y;
            public float DistanceToCursorX;
            public float DistanceToCursorY;
            public float ScalarDistance;
            public double Velocity;
            public double Progress;
            public float DirectionX;
            public float DirectionY;
            public SliderRuntimeData()
            {
                X = 0;
                Y = 0;
                DistanceToCursorX = 0;
                DistanceToCursorY = 0;
                ScalarDistance = -1;
                Velocity = 0;
                Progress = -1;
                DirectionX = 0;
                DirectionY = 0;
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
                SpinsPerMinute = -1;
                RequiredSPM = -1;
                Progress = -1;
                RemainingTime = -1;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CursorRuntimeData
        {
            public float X;
            public float Y;
            public double VelocityX;
            public double VelocityY;
            public CursorRuntimeData()
            {
                X = 0;
                Y = 0;
                VelocityX = 0;
                VelocityY = 0;
            }
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

        public OsuObjectsData[] GetData;
        public FrameObservation GetFrameObservation => frameObservation;
        private int count;
        private double previousTime;
        private bool hasPreviousCursor;
        private float previousCursorX;
        private float previousCursorY;
        private PlayingStateContainer playingStateContainer;

        public ObjectTracker(OsuPlayfield.AIPlayfield playfield, PlayingStateContainer playingStateContainer)
        {
            this.playfield = playfield;
            this.playingStateContainer = playingStateContainer;
            // The first frame
            previousCursorX = (playfield.CursorPosition.X - 256f) / 256f;
            previousCursorY = (playfield.CursorPosition.Y - 192f) / 192f;
            GetData = new OsuObjectsData[10];
            hasPreviousCursor = false;
        }
        public void Update(long frameID)
        {
            trackCursor(); // Objects need coordinate of the cursor to calculate relative position
            getNext10Objects(frameID); // Here a new frame observation is created
            trackSliderBall();
            trackSpinner();
            trackGameState();
        }
        private void trackCursor()
        {
            // If divided by 1000, 99% of the velocity less than 5,
            // To normalise the velocity, divide by 5000
            double dt = (frameObservation.CurrentTime - previousTime) / 5000f;

            // Normalise cursor position
            frameObservation.CursorRuntimeData.X = Math.Clamp((playfield!.CursorPosition.X - 256f) / 256f, -1f, 1f);
            frameObservation.CursorRuntimeData.Y = Math.Clamp((playfield!.CursorPosition.Y - 192f) / 192f, -1f, 1f);
            if (dt <= 0 || !hasPreviousCursor)
            {
                frameObservation.CursorRuntimeData.VelocityX = 0d;
                frameObservation.CursorRuntimeData.VelocityY = 0d;

                hasPreviousCursor = true;
            }
            else
            {
                float dx = frameObservation.CursorRuntimeData.X - previousCursorX;
                float dy = frameObservation.CursorRuntimeData.Y - previousCursorY;

                frameObservation.CursorRuntimeData.VelocityX = Math.Clamp(dx / dt, -10f, 10f);
                frameObservation.CursorRuntimeData.VelocityY = Math.Clamp(dy / dt, -10f, 10f);
            }

            previousTime = frameObservation.CurrentTime;
            previousCursorX = frameObservation.CursorRuntimeData.X;
            previousCursorY = frameObservation.CursorRuntimeData.Y;
        }
        private void getNext10Objects(long frameID)
        {
            // The objects here have been sorted by StartTime
            var nextObjects = playfield?.HitObjectContainer.Get10AliveObjects();

            if (nextObjects is null) return;

            double currentTime = playfield!.CurrentTime;

            count = 0;

            frameObservation = new()
            {
                CurrentTime = currentTime,
                SliderRuntimeData = new(),
                SpinnerRuntimeData = new(),
                FrameID = frameID
            };
            GetData = new OsuObjectsData[10];

            foreach (DrawableHitObject obj in nextObjects)
            {
                // All the data is normalised
                OsuObjectsData data = new();
                int objTypeInt = objectType[obj.GetType()];
                //Logger.Log($"objTypeInt: {objTypeInt}");
                if (objTypeInt == 1) // HitCircle
                {
                    data.IsCircle = 1;
                    // Make sure that the position is absolute position
                    Vector2 position = ((OsuHitObject)obj.HitObject).StackedPosition;

                    // Relative Position
                    data.X = (position.X - 256f) / 256f;
                    data.Y = (position.Y - 192f) / 192f;
                    data.DistanceToCursorX = (data.X - frameObservation.CursorRuntimeData.X) / 256f;
                    data.DistanceToCursorY = (data.Y - frameObservation.CursorRuntimeData.Y) / 192f;
                    data.ScalarDistance = (float)Math.Sqrt(Math.Pow(data.DistanceToCursorX, 2f) + Math.Pow(data.DistanceToCursorY, 2f));

                    double TimeToHit = obj.HitObject.StartTime - frameObservation.CurrentTime;
                    data.TimeToHit = TimeToHit / 1000f;
                }
                else if (objTypeInt == 4) // Slider
                {
                    data.IsSlider = 1;
                    Vector2 position = ((DrawableSlider)obj).HitObject.StackedPosition;

                    // Relative Position
                    data.X = (position.X - 256f) / 256f;
                    data.Y = (position.Y - 192f) / 192f;
                    data.DistanceToCursorX = (data.X - frameObservation.CursorRuntimeData.X) / 256f;
                    data.DistanceToCursorY = (data.Y - frameObservation.CursorRuntimeData.Y) / 192f;
                    data.ScalarDistance = (float)Math.Sqrt(Math.Pow(data.DistanceToCursorX, 2f) + Math.Pow(data.DistanceToCursorY, 2f));

                    double TimeToHit = obj.HitObject.StartTime - frameObservation.CurrentTime;
                    data.TimeToHit = TimeToHit / 1000f;
                }
                else if (objTypeInt == 7) // Spinner
                {
                    data.IsSpinner = 1;
                    // It is no need to calculate the position of a spinner
                    double TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = TimeToHit / 1000f;
                }

                GetData[count] = data;

                count++;
            }
        }
        private void trackSliderBall()
        {
            var hitObject = playfield?.HitObjectContainer.NextSlider;
            if (hitObject is DrawableSlider slider)
            {
                // Time
                double remainingTime = slider.HitObject.EndTime - playfield!.CurrentTime;
                double totalTime = slider.HitObject.Duration;
                double progress = Math.Clamp(1f - (remainingTime / totalTime), 0f, 1f);

                // Position
                var position = slider.Ball.Position + slider.HitObject.StackedPosition;

                double velocity = slider.HitObject.Velocity;
                frameObservation.SliderRuntimeData.Velocity = velocity / 10f;

                // Progress
                frameObservation.SliderRuntimeData.Progress = progress;

                frameObservation.SliderRuntimeData.X = Math.Clamp((position.X - 256f) / 256f, -1f, 1f);
                frameObservation.SliderRuntimeData.Y = Math.Clamp((position.Y - 192f) / 192f, -1f, 1f);

                // Distance to cursor
                // Both variable are normalised
                frameObservation.SliderRuntimeData.DistanceToCursorX = frameObservation.SliderRuntimeData.X - frameObservation.CursorRuntimeData.X;
                frameObservation.SliderRuntimeData.DistanceToCursorY = frameObservation.SliderRuntimeData.Y - frameObservation.CursorRuntimeData.Y;
                frameObservation.SliderRuntimeData.ScalarDistance =
                    (float)Math.Sqrt(
                          Math.Pow(frameObservation.SliderRuntimeData.DistanceToCursorX, 2)
                        + Math.Pow(frameObservation.SliderRuntimeData.DistanceToCursorY, 2));

                var nextPosition = slider.HitObject.StackedPosition + slider.HitObject.CurvePositionAt(Math.Clamp(progress + 0.1f, 0, 1));
                var deltaPosition = nextPosition - position;
                frameObservation.SliderRuntimeData.DirectionX = deltaPosition.X / 256f;
                frameObservation.SliderRuntimeData.DirectionY = deltaPosition.Y / 192f;
            }
        }
        private void trackSpinner()
        {
            var hitObject = playfield?.HitObjectContainer.NextSpinner;
            if (hitObject is DrawableSpinner spinner)
            {
                // Time
                double remainingTime = spinner.HitObject.EndTime - playfield!.CurrentTime;
                double totalTime = spinner.HitObject.Duration; // ms
                frameObservation.SpinnerRuntimeData.Progress = Math.Clamp(1f - (remainingTime / totalTime), 0f, 1f);
                frameObservation.SpinnerRuntimeData.RemainingTime = remainingTime / 1000f;

                // The spm calculated by osu for each frame
                frameObservation.SpinnerRuntimeData.SpinsPerMinute = spinner.SpinsPerMinute.Value / 60 / 10; // SPM / 600
                frameObservation.SpinnerRuntimeData.RequiredSPM = spinner.HitObject.SpinsRequiredForBonus / totalTime * 100; // SPM / 600
            }
        }
        private void trackGameState()
        {
            frameObservation.PlayingState = (byte)playingStateContainer.LocalUserPlayingState;
        }
    }
}
