using System.Collections;

using System.Collections.Generic;

using UnityEngine;



namespace Glass

{

    public static class Easings

    {

        public enum Easing

        {

            Sine,

            Expo,

            Elastic

        }



        public enum EasingDirection

        {

            In,

            Out,

            InOut

        }



        public static float Ease(float x, Easing easing = Easing.Expo, EasingDirection direction = EasingDirection.Out, bool clamp = true)

        {

            switch (easing)

            {

                case Easing.Sine:

                    return Sine(x, direction, clamp);

                case Easing.Expo:

                    return Expo(x, direction, clamp);

                case Easing.Elastic:

                    return Elastic(x, direction, clamp);

                default:

                    return Expo(x, direction, clamp);

            }

        }



        // sine

        public static float Sine(float x, EasingDirection direction = EasingDirection.Out, bool clamp = true)

        {

            if (direction == EasingDirection.In) return InSine(x, clamp);

            if (direction == EasingDirection.Out) return OutSine(x, clamp);

            return InOutSine(x, clamp);

        }



        public static float InSine(float x, bool clamp = true)

        {

            x = clamp ? Clamp(x) : x;

            return 1f - Mathf.Cos((x * Mathf.PI) / 2f);

        }



        public static float OutSine(float x, bool clamp = true)

        {

            x = clamp ? Clamp(x) : x;

            return Mathf.Sin((x * Mathf.PI) / 2f);

        }



        public static float InOutSine(float x, bool clamp = true)

        {

            x = clamp ? Clamp(x) : x;

            return -(Mathf.Cos(Mathf.PI * x) - 1f) / 2f;

        }



        // expo

        public static float Expo(float x, EasingDirection direction = EasingDirection.Out, bool clamp = true)

        {

            if (direction == EasingDirection.In) return InExpo(x, clamp);

            if (direction == EasingDirection.Out) return OutExpo(x, clamp);

            return InOutExpo(x, clamp);

        }



        public static float InExpo(float x, bool clamp = true)

        {

            x = clamp ? Clamp(x) : x;

            return x == 0f ? 0f : Mathf.Pow(2f, 10f * x - 10f);

        }



        public static float OutExpo(float x, bool clamp = true)

        {

            x = clamp ? Clamp(x) : x;

            return x == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * x);

        }



        public static float InOutExpo(float x, bool clamp = true)

        {

            x = clamp ? Clamp(x) : x;

            return x == 1f ? 1f : 1f - Mathf.Pow(1f, -10f * x);

        }



        // elastic

        public static float Elastic(float x, EasingDirection direction = EasingDirection.Out, bool clamp = true)

        {

            if (direction == EasingDirection.In) return InElastic(x, clamp);

            if (direction == EasingDirection.Out) return OutElastic(x, clamp);

            return InOutElastic(x, clamp);

        }



        public static float InElastic(float x, bool clamp = true)

        {

            x = clamp ? Clamp(x) : x;

            float c = (2f * Mathf.PI) / 3f;

            return x == 0f

                ? 0f

                : x == 1f

                ? 1f

                : -Mathf.Pow(2f, 10f * x - 10f) * Mathf.Sin((x * 10f - 10.75f) * c);

        }



        public static float OutElastic(float x, bool clamp = true)

        {

            x = clamp ? Clamp(x) : x;

            float c = (2f * Mathf.PI) / 3f;

            return x == 0f

                ? 0f

                : x == 1f

                ? 1f

                : Mathf.Pow(2f, -10f * x) * Mathf.Sin((x * 10f - .75f) * c) + 1f;

        }



        public static float InOutElastic(float x, bool clamp = true)

        {

            x = clamp ? Clamp(x) : x;

            float c = (2f * Mathf.PI) / 4.5f;

            return x == 0f

                ? 0f

                : x == 1f

                ? 1f

                : x < .5f

                ? -Mathf.Pow(2f, 20f * x - 10f) * Mathf.Sin((20 * x - 11.125f) * c) / 2f

                : Mathf.Pow(2f, -20f * x + 10f) * Mathf.Sin((20 * x - 11.125f) * c) / 2f + 1f;

        }




        private static float Clamp(float x)

        {

            return Mathf.Clamp(x, 0f, 1f);

        }

    }

}

