using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using System;

using UnityEngine.UI;



namespace Glass

{

    public class AnimationProcessor : MonoBehaviour

    {

        public static AnimationProcessor instance;



        public List<AnimationProcess> processes = new List<AnimationProcess>();



        void Awake()

        {

            if (instance == null)

            {

                instance = this;

            }

            else if (instance != this)

            {

                Destroy(this);

            }

        }



        void Update()

        {

            List<AnimationProcess> done = new List<AnimationProcess>();

            foreach (AnimationProcess process in processes)

            {

                if (process.Update())

                {

                    done.Add(process);

                }

            }



            foreach (AnimationProcess process in done)

            {

                processes.Remove(process);

            }

        }

    }



    public class AnimationProcess

    {

        object affecting;

        object start;

        object end;



        float duration;

        float startTime;



        bool isPlaying = false;



        public Easings.Easing easing;

        public Easings.EasingDirection easingDirection;



        public AnimationProcess(object _affecting, Easings.Easing _easing = Easings.Easing.Expo, Easings.EasingDirection _easingDirection = Easings.EasingDirection.Out)

        {

            affecting = _affecting;

            easing = _easing;

            easingDirection = _easingDirection;

        }



        public void Start(object _end, float _duration)

        {

            Stop();



            end = _end;

            duration = _duration;

            startTime = Time.time;



            if (DoPosition())

            {

                start = (affecting as Transform).localPosition;

            }

            if (DoRectPosition())

            {

                start = (affecting as RectTransform).localPosition;

            }

            else if (DoImageColor())

            {

                start = (affecting as Image).color;

            }

            else if (DoTextColor())

            {

                start = (affecting as Text).color;

            }



            AnimationProcessor.instance.processes.Add(this);

            isPlaying = true;

        }



        public bool Update()

        {

            float percentage = (Time.time - startTime) / duration;



            if (DoPosition())

            {

                (affecting as Transform).localPosition = Vector3.Lerp((Vector3)start, (Vector3)end, Easings.Ease(percentage, easing, easingDirection));

            }

            else if (DoRectPosition())

            {

                (affecting as RectTransform).localPosition = Vector3.Lerp((Vector3)start, (Vector3)end, Easings.Ease(percentage, easing, easingDirection));

            }

            else if (DoImageColor())

            {

                (affecting as Image).color = Color.Lerp((Color)start, (Color)end, Easings.Ease(percentage, easing, easingDirection));

            }

            else if (DoTextColor())

            {

                (affecting as Text).color = Color.Lerp((Color)start, (Color)end, Easings.Ease(percentage, easing, easingDirection));

            }



            if (percentage >= 1f) isPlaying = false;

            if (!isPlaying) return true;

            return false;

        }



        public void Stop()

        {

            if (!isPlaying) return;

            if (DoPosition())

            {

                (affecting as Transform).localPosition = (Vector3)end;

            }

            else if (DoRectPosition())

            {

                (affecting as RectTransform).localPosition = (Vector3)end;

            }

            else if (DoImageColor())

            {

                (affecting as Image).color = (Color)end;

            }

            else if (DoTextColor())

            {

                (affecting as Text).color = (Color)end;

            }



            isPlaying = false;

        }



        bool CheckType(Type t0, Type t1)

        {

            return affecting.GetType() == t0 && end.GetType() == t1;

        }



        bool DoPosition()

        {

            return CheckType(typeof(Transform), typeof(Vector3));

        }



        bool DoRectPosition()

        {

            return CheckType(typeof(RectTransform), typeof(Vector3));

        }



        bool DoImageColor()

        {

            return CheckType(typeof(Image), typeof(Color));

        }



        bool DoTextColor()

        {

            return CheckType(typeof(Text), typeof(Color));

        }

    }

}