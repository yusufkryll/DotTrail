using System.Collections;
using System.Collections.Generic;
using Ra.Trail;
using UnityEngine;
using UnityEngine.UI;

public class InspectorCodeView : MonoBehaviour
{
    public Image image;
    public Text indexText, codeText;
    public SequenceElement element;
    public void Apply(SequenceElement sequenceElement)
    {
        element = sequenceElement;
        codeText.text = $".{element.name}({element.argumentInfo})";
        if(element.children.Count > 0) codeText.text += $" [{element.children.Count} children]";
    }

    public void RefreshIndex()
    {
        indexText.text = (transform.GetSiblingIndex() + 1).ToString();
    }

    public void SetActive(bool isActive)
    {
        image.color = isActive ? Color.yellow : Color.white;
    }

    public void OnClick()
    {
        DotTrailContainer.OpenSequenceDetails(element);
    }
}
