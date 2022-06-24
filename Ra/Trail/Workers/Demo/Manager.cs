using Ra;
using UnityEngine;
 
[CreateAssetMenu(fileName = nameof(Manager), menuName = "Workers/" + nameof(Manager), order = 1)] 
public class Manager : Worker 
{

    public override void Start()
    {
        
    }

    public override void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            var m = container.GetWorker<Movement>();
            m.enabled = !m.enabled;
        }
    }
}

