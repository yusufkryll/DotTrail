using Ra.Trail;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    void Start()
    {
        Dot.MainTrail
            .Label("start")
            .Wait(2)
            .Goto("start")
            .Activator("start")
            .Print("wow")
            .End();
    }
}
