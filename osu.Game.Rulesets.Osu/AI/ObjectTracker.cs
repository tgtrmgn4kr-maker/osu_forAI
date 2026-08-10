// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.AI;
using osuTK;



namespace osu.Game.Rulesets.Osu.AI
{
    public class ObjectTracker
    {
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

        // HitCircle and SliderHead
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

        // SliderBall only
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
            previousCursorX = playfield!.CursorPosition.X;
            previousCursorY = playfield!.CursorPosition.Y;
            GetData = new OsuObjectsData[10];
            hasPreviousCursor = false;
        }

        private void getNext10Objects(long frameID)
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

            foreach (DrawableHitObject obj in nextObjects)
            {
                // All the data is normalised
                OsuObjectsData data = new();
                int objTypeInt = objectType[obj.GetType()];
                //Logger.Log($"objTypeInt: {objTypeInt}");
                if (objTypeInt == 1) // HitCircle
                {
                    data.IsCircle = 1;
                    // Make sure that the position is relative position
                    Vector2 position = ((OsuHitObject)obj.HitObject).StackedPosition;

                    // Relative Position
                    data.X = position.X / 256f;
                    data.Y = position.Y / 192f;
                    data.DistanceToCursorX = (position.X - playfield!.CursorPosition.X) / 256f;
                    data.DistanceToCursorY = (position.Y - playfield!.CursorPosition.Y) / 192f;
                    data.ScalarDistance = (float)Math.Sqrt(Math.Pow(data.DistanceToCursorX, 2f) + Math.Pow(data.DistanceToCursorY, 2f));

                    double TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = TimeToHit / 1000f;
                }
                else if (objTypeInt == 4) // Slider
                {
                    data.IsSlider = 1;
                    var slider = (DrawableSlider)obj;
                    Vector2 position = slider.HitObject.StackedPosition;

                    // Relative Position
                    data.X = position.X / 256f;
                    data.Y = position.Y / 192f;
                    data.DistanceToCursorX = (position.X - playfield!.CursorPosition.X) / 256f;
                    data.DistanceToCursorY = (position.Y - playfield!.CursorPosition.Y) / 192f;
                    data.ScalarDistance = (float)Math.Sqrt(Math.Pow(data.DistanceToCursorX, 2f) + Math.Pow(data.DistanceToCursorY, 2f));

                    double TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = TimeToHit / 1000f;
                }
                else if (objTypeInt == 7) // Spinner
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
            getNext10Objects(frameID); // Here a new frame observation is created
            trackSliderBall();
            trackSpinner();
            trackCursor();
            trackGameState();
        }

        private void trackGameState()
        {
            // It works only when AI player is activated
            frameObservation.PlayingState = (byte)playingStateContainer.LocalUserPlayingState;
        }
        private void trackSliderBall()
        {
            var hitObject = playfield?.HitObjectContainer.NextSlider;
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
                    frameObservation.SliderRuntimeData.X = position.X / 256f;
                    frameObservation.SliderRuntimeData.Y = position.Y / 192f;
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
            else
            {
                frameObservation.SliderRuntimeData = new();
            }
        }
        private void trackSpinner()
        {
            var hitObject = playfield?.HitObjectContainer.NextSpinner;
            if (hitObject is DrawableSpinner spinner)
            {
                // The spm calculated by osu for each frame
                frameObservation.SpinnerRuntimeData.SpinsPerMinute = spinner.SpinsPerMinute.Default / 1000f;
                frameObservation.SpinnerRuntimeData.RequiredSPM = spinner.HitObject.SpinsRequiredForBonus / 1000f;

                double remainingTime = spinner.HitObject.EndTime - playfield!.CurrentTime;
                double totalTime = spinner.HitObject.Duration;
                frameObservation.SpinnerRuntimeData.Progress = Math.Clamp(1f - (remainingTime / totalTime), 0f, 1f);
                frameObservation.SpinnerRuntimeData.RemainingTime = remainingTime / 1000f;
            }
            else
            {
                frameObservation.SpinnerRuntimeData = new();
            }
        }
        private void trackCursor()
        {
            double dt = (frameObservation.CurrentTime - previousTime) / 1000f;

            frameObservation.CursorRuntimeData.X = (playfield!.CursorPosition.X - 256f) / 256f;
            frameObservation.CursorRuntimeData.Y = (playfield!.CursorPosition.Y - 192f) / 192f;

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

                frameObservation.CursorRuntimeData.VelocityX = Math.Clamp(dx / dt, -30f, 30f);
                frameObservation.CursorRuntimeData.VelocityY = Math.Clamp(dy / dt, -30f, 30f);
            }

            previousTime = frameObservation.CurrentTime;
            previousCursorX = frameObservation.CursorRuntimeData.X;
            previousCursorY = frameObservation.CursorRuntimeData.Y;
        }
    }
}
