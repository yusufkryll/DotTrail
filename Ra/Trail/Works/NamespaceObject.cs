using System;
using System.Collections.Generic;

namespace Ra.Trail.Works
{
    [Serializable]
    public class NamespaceObject
    {
        public string name;
        public List<Variable> variables = new List<Variable>();
        public List<TrailLabel> labels = new List<TrailLabel>();
    }
}
