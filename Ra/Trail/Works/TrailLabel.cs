using System;

namespace Ra.Trail.Works
{
    [System.Serializable]
    public class TrailLabel
    {
        public string name;
        public SequenceElement element;
        public bool activated;
        public Action onActivation = () => {};
    }
}