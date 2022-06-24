using Ra;
using UnityEngine;
 
[CreateAssetMenu(fileName = nameof(Movement), menuName = "Workers/" + nameof(Movement), order = 1)] 
public class Movement : Worker
{
    public float speed;
    private GameObject go;

    public override void Start()
    {
        go = container.Which("Cube");
    }

    public override void Update()
    {
        go.transform.position += Vectors.standardInputDeltaRawHorizontal * Time.deltaTime * speed;
    }
}

