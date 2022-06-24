using System;
using System.Collections.Generic;

namespace Ra.Trail.Data
{
    public class ChangeData
    {
        public Func<double> changeDelay = () => -1;
        public List<SequenceElement> elements = new List<SequenceElement>();
    }
}