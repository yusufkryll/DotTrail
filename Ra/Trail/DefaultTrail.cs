using System.Collections.Generic;
using System.Linq;

namespace Ra.Trail
{
    public class Dot : TrailObject<Dot>
    {
        private static Dot Instance;
        public static readonly List<object> dots = new List<object>();
        public static Dot MainTrail
        {
            get
            {
                if(Instance == null) Instance = Trail.SetName("MainTrail");
                return Instance;
            }
            set => Instance = value;
        }
        
        public static Dot Get()
        {
            return (Dot) dots.Last();
        }
        
        public static Dot Get(string name)
        {
            return Get<Dot>(name);
        }

        public static T Get<T>(string name)
        {
            if (dots != null) return (T) dots.Find(x => x.GetType() == typeof(T));
            return default;
        }
    }
}