using System.Diagnostics;
using Kit.Core.Exceptions;

namespace Kit.Core
{
    public class GlobalTimer
    {
        private static Stopwatch programTimer = new Stopwatch();

        public static void StartTimer()
        {
            if (programTimer.IsRunning)
            {
                programTimer.Stop();
            }
            programTimer.Start();
        }

        public static double GetCurTime()
        {
            if (programTimer.IsRunning)
            {
                return programTimer.ElapsedMilliseconds;
            }
            throw new TimerStoppedException();
        }

        public static bool IsTimerRunning()
        {
            return programTimer.IsRunning;
        }
    }
}