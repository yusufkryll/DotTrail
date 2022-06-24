using Ra.Trail;
using UnityEngine;

namespace Ra.Transition
{
    public class Transition : TrailObject<Transition>
    {
        private Transform transform;

        public Transition Configure(Transform _transform)
        {
            transform = _transform;
            return this;
        }

        public Transition SetPosition(Vector3 position)
        {
            if (transform == null) return this;
            transform.position = position;
            return this;
        }
        
        public Transition Move(Vector3 pos, float speed, bool additive = false)
        {
            if (transform == null) return this;
            Pick(() => additive ? transform.position + pos : pos, out var arg);
            While(() =>
            {
                transform.position = Vector3.Lerp(transform.position, arg.value, Time.deltaTime * speed);
                if (!(Vector3.Distance(transform.position, arg.value) < 0.1f)) return true;
                transform.position = arg.value;
                return false;
            });
            return this;
        }

        public Transition AddMove(Vector3 position, float speed)
        {
            Move(position, speed, true);
            return this;
        }
    }
}
