using Ra.Trail.Mono;
using UnityEngine;
using UnityEngine.UI;

public class InspectorListButton : MonoBehaviour
{
    public MonoTrail monoTrail;
    public Text monoTrailNameText;

    public void Apply(MonoTrail newMonoTrail)
    {
        monoTrail = newMonoTrail;
        monoTrail.onNameChanged += () =>
        {
            monoTrailNameText.text = monoTrail.name;
        };
    }
    
    public void OnClick()
    {
        DotTrailContainer.OpenTrailDetails(monoTrail);
    }
}
