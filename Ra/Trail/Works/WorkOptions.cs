using System;
using System.Collections.Generic;
using Ra.Trail.Data;

namespace Ra.Trail.Works
{
    [Serializable]
    public class WorkOptions
    {
        public static WorkOptions NoSequence => new WorkOptions {sequencing = false};
        public bool sequencing = true;
        public bool openBracket = false;
        public bool closeBracket = false;
        public bool waitForNull = false;
        public Func<object> get;
        public Action<SequenceElement> awakeAction;
        public Action<SequenceElement> startAction;
        public Func<SequenceElement, object> action;
        public Action<List<SequenceElement>> onBracketCollected = (data) => {};
    }
}
