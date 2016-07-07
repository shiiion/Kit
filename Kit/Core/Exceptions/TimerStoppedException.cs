using System;

namespace Kit.Core.Exceptions
{
    public class TimerStoppedException : Exception
    {
        public TimerStoppedException() : base("Timer is not running.")
        {
        }
    }
}