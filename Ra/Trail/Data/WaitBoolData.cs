using System;

namespace Ra.Trail.Data
{
    public class WaitBoolData
    {
        public Func<bool> action;
        public float timeout = 0;
    }
}
