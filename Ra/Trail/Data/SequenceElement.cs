using System;
using System.Collections.Generic;

namespace Ra.Trail
{
    public class SequenceElement
    {
        public string name;
        public int sequenceIndex;
        public bool awaken;
        public bool openBracket = false;
        public bool closeBracket = false;
        public bool waitForNull = false;
        public bool isCompleted = false;
        public object data;
        public string argumentInfo = "";
        public Func<object> get;
        public Action<List<SequenceElement>> onBracketCollected = (data) => {};
        public Action<SequenceElement> awakeAction, startAction;
        public Func<SequenceElement, object> action;
        public SequenceElement parent;
        public SequenceElement next;
        public SequenceElement back;
        public List<SequenceElement> children = new List<SequenceElement>();
    }
}
