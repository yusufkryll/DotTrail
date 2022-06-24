using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ra.Trail.Data;
using Ra.Trail.Works;
using UnityEngine;

namespace Ra.Trail.Mono
{
    public sealed class MonoTrail : MonoBehaviour
    {
        public static MonoTrail Instance;
        public List<IntervalData> intervals = new List<IntervalData>();
        public List<SequenceElement> sequence = new List<SequenceElement>();
        public List<SequenceElement> elementsExecuting = new List<SequenceElement>();
        public List<SequenceElement> elementsExecuted = new List<SequenceElement>();
        public static readonly List<NamespaceObject> namespaces = new List<NamespaceObject>();
        public List<NamespaceObject> namespacesInUse = new List<NamespaceObject>();
        [SerializeField] private List<Variable> variables = new List<Variable>();
        [SerializeField] private List<TrailLabel> labels = new List<TrailLabel>();
        public Action onNameChanged = () => { };
        public Action loop = () => { };
        public Action fixedLoop = () => { };

        private List<Variable> VariablesInUse()
        {
            var all = new List<Variable>();
            return namespacesInUse
                .Aggregate(all, (current, namespaceObject) => 
                    current.Concat(namespaceObject.variables).ToList());
        }
        
        private List<TrailLabel> LabelsInUse()
        {
            var all = new List<TrailLabel>();
            return namespacesInUse
                .Aggregate(all, (current, namespaceObject) => 
                    current.Concat(namespaceObject.labels).ToList());
        }
        
        public List<Variable> GetVariables()
        {
            return variables.Concat(VariablesInUse()).ToList();
        }
        
        public List<TrailLabel> GetLabels()
        {
            return labels.Concat(LabelsInUse()).ToList();
        }

        public void AddToVariables(Variable variable)
        {
            variables.Add(variable);
        }
        
        public void AddToLabels(TrailLabel label)
        {
            labels.Add(label);
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            Run(sequence);
            if(FindObjectOfType<DotTrailContainer>() != null) DotTrailContainer.OnTrailCreated(this);
        }

        private void Update()
        {
            loop();
        }
        
        private void FixedUpdate()
        {
            fixedLoop();
        }

        public Coroutine Run(List<SequenceElement> elements, Func<bool> exec = null, bool breaking = false)
        {
            StartCoroutine(AwakeWorkEnumerator(elements));
            return StartCoroutine(WorkEnumerator(elements, exec, breaking));
        }

        private IEnumerator AwakeWorkEnumerator(List<SequenceElement> sequenceElements)
        {
            var started = false;
            SequenceElement lastElement = null;
            SequenceElement element = null;
            while (true)
            {
                if (sequenceElements.Count == 0)
                {
                    yield return null;
                    continue;
                }
                
                if (!started)
                {
                    element = sequenceElements[0];
                    started = true;
                }
                
                if (element == null || element.awaken || (element == lastElement && element.next == null))
                {
                    yield return null;
                    continue;
                }
                
                element.awakeAction?.Invoke(element);
                element.awaken = true;
                lastElement = element;
                if (element.next != null)
                {
                    if (element.waitForNull) yield return null;
                    element = element.next;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator WorkEnumerator(List<SequenceElement> sequenceElements, Func<bool> exec = null, bool breaking = false)
        {
            var started = false;
            SequenceElement lastElement = null;
            SequenceElement element = null;
            while (true)
            {
                if (sequenceElements.Count == 0)
                {
                    yield return null;
                    continue;
                }

                if (!started)
                {
                    element = sequenceElements[0];
                    started = true;
                }
                //possible problem
                if (element == null || !element.awaken || (element == lastElement && element.next == null))
                {
                    yield return null;
                    continue;
                }

                element.startAction?.Invoke(element);
                elementsExecuting.Add(element);
                AddExecuted(element);
                exec ??= () => true;
                var result = exec() ? element.action?.Invoke(element) : null;
                if(element.waitForNull || result != null) yield return result;
                elementsExecuting.Remove(element);
                lastElement = element;
                if (element.next != null)
                {
                    element = element.next;
                }
                else
                {
                    if (breaking) yield break;
                    yield return null;
                }
            }
        }

        private void AddExecuted(SequenceElement element)
        {
            elementsExecuted.Add(element);
            if(elementsExecuted.Count > 30) elementsExecuted.RemoveRange(0, elementsExecuted.Count - 30);
        }

        private void OnApplicationQuit()
        {
            Destroy(gameObject);
        }
    }
}
