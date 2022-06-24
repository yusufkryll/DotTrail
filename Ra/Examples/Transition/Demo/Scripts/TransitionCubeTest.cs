using UnityEngine;

namespace Ra.Transition.Demo
{
    public class TransitionCubeTest : MonoBehaviour
    {
        private void Start()
        {
            Transition.Trail
                .Configure(transform)
                .AddMove(new Vector3(0, 0, 10), 5f)
                .Wait()
                .AddMove(new Vector3(10, 0, 0), 5f)
                .Wait()
                .AddMove(new Vector3(0, 10, 0), 5f)
                .Wait()
                .Move(new Vector3(0, 0, 0), 5f);
        }
    }
}
