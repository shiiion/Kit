using System;

namespace Kit.Core
{
    public enum KitEasingMode
    {
        EaseIn,
        EaseOut,
        EaseInOut
    }

    public enum KitEasingType
    {
        Line,
        Sine,
        Quad,
        Cubic,
        Quart,
        Quint,
        Expo,
        Circ,
    }

    public class KitEasing
    {
        public KitEasingMode EasingMode { get; set; }
        public KitEasingType EasingType { get; set; }

        public KitEasing(KitEasingMode mode, KitEasingType type)
        {
            EasingMode = mode;
            EasingType = type;
        }

        public double Ease(double timeNormal)
        {
            switch (EasingType)
            {
                case KitEasingType.Line:
                    return lineEase(timeNormal);
                case KitEasingType.Sine:
                    return sineEase(timeNormal);
                case KitEasingType.Quad:
                    return quadEase(timeNormal);
                case KitEasingType.Cubic:
                    return cubicEase(timeNormal);
                case KitEasingType.Quart:
                    return quartEase(timeNormal);
                case KitEasingType.Quint:
                    return quintEase(timeNormal);
                case KitEasingType.Expo:
                    return expoEase(timeNormal);
                case KitEasingType.Circ:
                    return circEase(timeNormal);
                default:
                    return lineEase(timeNormal);
            }
        }

        private double handleEase(Func<double, double> easeFn, double t)
        {
            switch (EasingMode)
            {
                case KitEasingMode.EaseIn:
                    return easeFn(t);
                case KitEasingMode.EaseOut:
                    return 1 - easeFn(1 - t);
                case KitEasingMode.EaseInOut:
                    if (t <= 0.5)
                    {
                        return easeFn(t * 2) / 2;
                    }
                    else
                    {
                        return (1 - easeFn(1 - ((t - 0.5) * 2))) / 2 + 0.5;
                    }
                default:
                    return easeFn(t);
            }
        }

        private double lineEase(double t)
        {
            return t;
        }

        private double sineEase(double t)
        {
            return handleEase((double time) => { return Math.Sin((Math.PI / 2) * time); }, t);
        }

        private double quadEase(double t)
        {
            return handleEase((double time) => { return (time * time); }, t);
        }

        private double cubicEase(double t)
        {
            return handleEase((double time) => { return (time * time * time); }, t);
        }

        private double quartEase(double t)
        {
            return handleEase((double time) => { return (time * time * time * time); }, t);
        }

        private double quintEase(double t)
        {
            return handleEase((double time) => { return (time * time * time * time * time); }, t);
        }

        private double expoEase(double t)
        {
            return handleEase((double time) => { return Math.Pow(2, 10 * (time - 1)); }, t);
        }

        private double circEase(double t)
        {
            return handleEase((double time) => { return 1 - Math.Sqrt(1 - (time * time)); }, t);
        }
    }
}