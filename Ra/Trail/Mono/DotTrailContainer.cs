using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ra.Trail;
using Ra.Trail.Mono;
using UnityEngine;
using UnityEngine.UI;

public class DotTrailContainer : MonoBehaviour
{
    public static DotTrailContainer Instance;
    public Transform monoTrailListTransform;
    public Transform monoTrailCodeViewTransform;
    public InspectorCodeView codeViewPrefab;
    public Button historyButton;
    public Button backButton;
    public Button nextButton;
    public GameObject listButtonPrefab;
    public Text hierarchyText;
    public GameObject trailInspector;
    public KeyCode inspectorKey1, inspectorKey2;
    public bool lockCursor;

    public MonoTrail openMonoTrail;
    public SequenceElement openSequenceElement;

    private bool history = false;
    private int historyIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        StartCoroutine(ContainerEnumerator());
    }

    private void Update()
    {
        OnInspector();
    }

    public void HistoryOrRuntime()
    {
        if (openMonoTrail == null) return;
        history = !history;
        historyButton.GetComponentInChildren<Text>().text =
            history ? "History" : "Runtime";
        backButton.interactable = history;
        nextButton.interactable = history;
        RefreshHistory();
    }

    void RefreshHistory()
    {
        foreach (Transform child in Instance.monoTrailCodeViewTransform)
        {
            child.GetComponent<InspectorCodeView>().SetActive(false);
        }
        var view = FindCodeViewByHistoryIndex(historyIndex);
        if (view != null) view.SetActive(true);
    }

    private InspectorCodeView FindCodeViewByHistoryIndex(int index)
    {
        return (from Transform child 
            in Instance.monoTrailCodeViewTransform 
            where openMonoTrail.elementsExecuted[historyIndex] 
                  == child.GetComponent<InspectorCodeView>().element 
            select child.GetComponent<InspectorCodeView>()).FirstOrDefault();
    }

    public void Back()
    {
        if(openSequenceElement?.parent != null) OpenSequenceDetails(openSequenceElement.parent);
        else OpenTrailDetails(openMonoTrail);
    }
    
    public void BackElement()
    {
        if (historyIndex > 0) historyIndex--;
        var nowElement = openMonoTrail.elementsExecuted[historyIndex];
        if(historyIndex == 0) OpenTrailDetails(openMonoTrail);
        else
        {
            if(nowElement.parent != null) OpenSequenceDetails(nowElement.parent);
            else OpenTrailDetails(openMonoTrail);
        }
        RefreshHistory();
    }
    
    public void NextElement()
    {
        if (openMonoTrail.elementsExecuted.Count > historyIndex + 1) historyIndex++;
        else return;
        var nowElement = openMonoTrail.elementsExecuted[historyIndex];
        if(nowElement.parent != null) OpenSequenceDetails(nowElement.parent);
        else OpenTrailDetails(openMonoTrail);
        RefreshHistory();
    }

    public void RefreshHierarchy()
    {
        hierarchyText.text = "Main";
        if (openSequenceElement == null) return;
        var nowElement = openSequenceElement;
        var list = new List<string>();
        while (nowElement != null)
        {
            list.Add(nowElement.name);
            nowElement = nowElement.parent;
        }
        list.Reverse();
        hierarchyText.text += " / " + string.Join(" / ", list);
    }

    public static void OnTrailCreated(MonoTrail monoTrail)
    {
        Instantiate(Instance.listButtonPrefab, Instance.monoTrailListTransform)
            .GetComponent<InspectorListButton>().Apply(monoTrail);
    }
    
    public static void OpenTrailDetails(MonoTrail monoTrail)
    {
        Instance.openMonoTrail = monoTrail;
        Instance.openSequenceElement = null;
        Instance.historyButton.interactable = true;
        foreach (Transform child in Instance.monoTrailCodeViewTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (var sequenceElement in monoTrail.sequence)
        {
            Instantiate(Instance.codeViewPrefab, Instance.monoTrailCodeViewTransform).Apply(sequenceElement);
        }
        Instance.RefreshHierarchy();
    }

    public static void OpenSequenceDetails(SequenceElement element)
    {
        if (element.children.Count == 0) return;
        Instance.openSequenceElement = element;
        foreach (Transform child in Instance.monoTrailCodeViewTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (var sequenceElement in element.children)
        {
            Instantiate(Instance.codeViewPrefab, Instance.monoTrailCodeViewTransform).Apply(sequenceElement);
        }
        Instance.RefreshHierarchy();
    }

    private void OnInspector()
    {
        if (!Input.GetKey(inspectorKey1) || !Input.GetKeyDown(inspectorKey2)) return;
        trailInspector.SetActive(!trailInspector.activeSelf);
        if (lockCursor) Cursor.lockState = trailInspector.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private IEnumerator ContainerEnumerator()
    {
        while (true)
        {
            foreach (Transform child in Instance.monoTrailCodeViewTransform)
            {
                child.GetComponent<InspectorCodeView>().RefreshIndex();
            }
            if (history)
            {
                RefreshHistory();
                yield return null;
                continue;
            }
            if (openMonoTrail != null)
            {
                foreach (Transform child in Instance.monoTrailCodeViewTransform)
                {
                    var view = child.GetComponent<InspectorCodeView>();
                    view.SetActive(false);
                }
                foreach (var element in openMonoTrail.elementsExecuting)
                {
                    foreach (Transform child in Instance.monoTrailCodeViewTransform)
                    {
                        var view = child.GetComponent<InspectorCodeView>();
                        if (view.element != element) continue;
                        view.SetActive(true);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
