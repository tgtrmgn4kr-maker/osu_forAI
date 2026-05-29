// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;

using osu.Framework.Logging;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using System.Collections.Generic;
using osuTK;



namespace osu.Game.Rulesets.Osu.AI
{
    public class ObjectTracker
    {
        public struct FrameObservation
        {
            public CursorPosition CursorPosition;
            public double CurrentTime;
            public OsuObjectsData[] Data;
            public SliderRuntimeData SliderRuntimeData;
        }

        public struct OsuObjectsData
        {
            public int ObjectType;
            public float DistanceToCursorX;
            public float DistanceToCursorY;

            public double TimeToHit;
        }
        public struct SliderRuntimeData
        {
            public float DistanceToCursorX;
            public float DistanceToCursorY;
            public double Velocity;
        }
        public struct CursorPosition
        {
            public float X;
            public float Y;
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
            frameObservation = new()
            {
                CurrentTime = playfield.CurrentTime,
                Data = new OsuObjectsData[10],
                SliderRuntimeData = new(),
            };

        }

        private SharedTrackerState state;
        private OsuPlayfield.AIPlayfield? playfield;
        private FrameObservation frameObservation;
        public SliderRuntimeData? SliderPosition;
        private int count;

        private void getNext10Objects()
        {
            var nextObjects = playfield?.HitObjectContainer.Get10AliveObjects();

            if (nextObjects is null) return;

            count = 0;

            foreach (DrawableHitObject obj in nextObjects)
            {
                // All the data is normalised
                OsuObjectsData data = new();
                int objTypeInt = objectType[obj.GetType()];
                data.ObjectType = objTypeInt;
                if (objTypeInt == 0) // HitCircle
                {
                    Logger.Log("0");
                    Vector2 position = obj.Position;

                    // Relative Position
                    data.DistanceToCursorX = (position.X - playfield!.CursorPosition.X) / 512;
                    data.DistanceToCursorY = (position.Y - playfield!.CursorPosition.Y) / 384;

                    data.TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = Math.Clamp(data.TimeToHit / 1000f, -1f, 1f);
                }
                else if (objTypeInt == 3) // Slider
                {
                    Logger.Log("3");
                    var slider = (DrawableSlider)obj;
                    Vector2 position = slider.HitObject.StackedPosition;

                    // Relative Position
                    data.DistanceToCursorX = (position.X - playfield!.CursorPosition.X) / 512;
                    data.DistanceToCursorY = (position.Y - playfield!.CursorPosition.Y) / 384;

                    data.TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = Math.Clamp(data.TimeToHit / 1000f, -1f, 1f);
                }
                else if (objTypeInt == 6) // Spinner
                {
                    Logger.Log("Spinner");
                    // There is no need to calculate the position if a spinner
                    data.TimeToHit = obj.HitObject.StartTime - playfield!.CurrentTime;
                    data.TimeToHit = Math.Clamp(data.TimeToHit / 1000f, -1f, 1f);
                }

                frameObservation.Data[count] = data;
                count++;
            }
        }
        public void Update()
        {
            getNext10Objects();
            trackSliderBall();
            trackCursor();
        }

        private void trackSliderBall()
        {
            var hitObject = playfield?.HitObjectContainer.GetNextSlider();
            if (hitObject is DrawableSlider slider)
            {
                if (slider.Ball != null)
                {
                    frameObservation.SliderRuntimeData.Velocity = slider.HitObject.Velocity;

                    var position = slider.Ball.Position + slider.HitObject.StackedPosition;
                    frameObservation.SliderRuntimeData.DistanceToCursorX = (position.X - playfield!.CursorPosition.X) / 512;
                    frameObservation.SliderRuntimeData.DistanceToCursorY = (position.Y - playfield!.CursorPosition.Y) / 384;
                }
                else
                {
                    SliderPosition = null;
                }
            }
        }
        private void trackCursor()
        {
            frameObservation.CursorPosition.X = playfield!.CursorPosition.X / 512;
            frameObservation.CursorPosition.Y = playfield!.CursorPosition.Y / 384;
        }
    }
}
