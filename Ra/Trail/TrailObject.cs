using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ra.Trail.Data;
using Ra.Trail.Mono;
using Ra.Trail.Works;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Ra.Trail
{
    public class TrailObject<T> : ScriptableObject where T : ScriptableObject, new()
    {
        public static T Trail
        {
            get
            {
                var _instance = CreateInstance<T>();
                (_instance as TrailObject<T>)?.OnTrailStarted();
                Dot.dots.Add(_instance);
                return _instance;
            }
        }

        protected virtual void OnTrailStarted()
        {
            
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string trailName { get; private set; } = "MonoTrail";
        public MonoTrail monoTrail;
        private NamespaceObject currentNamespace;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            monoTrail = CreateMonoTrail();
            monoTrail.name = $"MonoTrail({GetType().FullName})";
        }

        private static MonoTrail CreateMonoTrail()
        {
            var monoTrailObject = new GameObject().AddComponent<MonoTrail>();
            var containerObject = GameObject.Find("DotTrail");
            if (containerObject == null) containerObject = new GameObject("DotTrail");
            monoTrailObject.transform.parent = containerObject.transform;
            return monoTrailObject;
        }

        private void ApplyName()
        {
            monoTrail.name = $"{trailName}({typeof(T)})";
            monoTrail.onNameChanged();
        }

        private object ValueOrAction(object value = null, Func<object> action = null)
        {
            return action == null ? value : action();
        }

        private bool IsVariableInfo(object property)
        {
            return property?.GetType() == typeof(VariableInfo);
        }
        
        private bool IsVariable(object property)
        {
            return property?.GetType() == typeof(Variable);
        }

        private object Resolve(object property)
        {
            if (IsVariableInfo(property))
            {
                var info = (property as VariableInfo);
                return info?.Value;
            }

            if (IsVariable(property))
            {
                var info = (property as Variable);
                return info?.value;
            }
            return property;
        }

        public VariableInfo this[string variableName]
        {
            get => new VariableInfo(this, variableName);
            set
            {
                var match = monoTrail.GetVariables().FirstOrDefault(x => x.name.Equals(variableName));
                if (match != null) match.value = value;
            }
        }
        
        private enum ifEnum
        {
            isTrue,
            isFalse,
            isEnd
        }

        private SequenceElement lastElement = new SequenceElement();
        
        private void DirectWork(WorkOptions options = default)
        {
            var argumentInfo = "";
            var workName = "Unnamed";
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrames()?[1];
            if (frame != null)
            {
                var method = frame.GetMethod();
                workName = method.Name;
                argumentInfo = string.Join(", ", method
                    .GetParameters()
                    .Select(x => x.ParameterType.Name + " " + x.Name)
                    .ToArray());
            }

            options ??= new WorkOptions();
            if (options.sequencing)
            {
                var se = new SequenceElement
                {
                    name = workName,
                    argumentInfo = argumentInfo,
                    awakeAction = options.awakeAction,
                    startAction = options.startAction,
                    onBracketCollected = options.onBracketCollected,
                    get = options.get,
                    back = lastElement,
                    action = options.action,
                    openBracket = options.openBracket,
                    closeBracket = options.closeBracket,
                    waitForNull = options.waitForNull
                };
                if (se.closeBracket)
                {
                    try
                    {
                        if (lastElement.openBracket)
                        {
                            lastElement.onBracketCollected(lastElement.children);
                        }
                        else if (lastElement.closeBracket)
                        {
                            var p2 = lastElement.parent.parent;
                            p2.onBracketCollected(p2.children);
                            se.back = p2;
                        }
                        else
                        {
                            lastElement.parent.onBracketCollected(lastElement.parent.children);   
                        }
                    }
                    catch (Exception)
                    {
                        Debug.LogError("<color=red>[DotTrail] Unexpected 'End'</color>");
                        return;
                    }
                }
                if (lastElement.closeBracket)
                {
                    var p2 = lastElement.parent.parent;
                    se.parent = p2;
                    lastElement.parent.next = se;
                    se.back = lastElement.parent;
                    if (se.parent == null)
                    {
                        monoTrail.sequence.Add(se);
                    }
                    else
                    {
                        se.parent.children.Add(se);
                    }
                }
                else
                {
                    if (lastElement.parent == null && !lastElement.openBracket)
                    {
                        lastElement.next = se;
                        monoTrail.sequence.Add(se);
                    }
                    else if (lastElement.openBracket)
                    {
                        se.parent = lastElement;
                        lastElement.children.Add(se);
                    }
                    else
                    {
                        lastElement.next = se;
                        se.parent = lastElement.parent;
                        lastElement.parent?.children.Add(se);
                    }
                }
                lastElement = se;
            }
            else
            {
                options.awakeAction(default);
                options.action(default);
            }
        }

        private IEnumerator AlternativeEnumerator(List<ChangeData> changes)
        {
            var first = true;
            var startDelay = changes[0].changeDelay;
            var lastDelay = startDelay();
            while (true)
            {
                for (int i = startDelay() >= 0 ? 0 : changes.Count - 1; i < changes.Count && i >= 0;)
                {
                    var changeData = changes[i];
                    var delay = (changeData.changeDelay ?? startDelay)();
                    if (first) first = false;
                    else yield return new WaitForSeconds(Mathf.Abs((float) delay));
                    if (changeData.changeDelay != null)
                    {
                        if(lastDelay < 0 || changeData.changeDelay() >= 0) 
                            monoTrail.Run(changeData.elements, null, true);
                        i += changeData.changeDelay() >= 0 ? 1 : -1;
                    }
                    else
                    {
                        monoTrail.Run(changeData.elements, null, true);
                        i += startDelay() >= 0 ? 1 : -1;
                    }

                    lastDelay = delay;
                }
            }
        }
        
        private static IEnumerator WhileEnumerator(Func<bool> fn, Func<double> delay, SequenceElement element)
        {
            var play = true;
            while (play)
            {
                play = fn();
                yield return delay != null && delay() > 0 ? new WaitForSeconds((float) delay()) : null;
            }
            element.isCompleted = true;
            yield return null;
        }
        
        private IEnumerator ActivatorEnumerator(Func<string> labelName, SequenceElement element)
        {
            while (true)
            {
                var exists = monoTrail.GetLabels().Exists(x => x.name.Equals(labelName()));
                if (exists)
                {
                    var label = monoTrail.GetLabels().Find(x => x.name.Equals(labelName()));
                    label.onActivation += () =>
                    {
                        monoTrail.Run(element.children, null, true);
                    };
                    yield break;
                }
                yield return null;
            }
        }
        
        private IEnumerator ForEnumerator(Func<int> times, Func<double> delay, SequenceElement element)
        {
            for (var i = 0; i < times(); i++)
            {
                monoTrail.Run(element.children, null,true);
                if(i < times()) yield return new WaitForSeconds((float) delay());
            }

            element.isCompleted = true;
            yield return null;
        }

        private static IEnumerator WhenEnumerator(Func<bool> condition, Action action)
        {
            var turn = false;
            while (true)
            {
                if (!turn && condition())
                {
                    turn = true;
                    action();
                }

                if (turn && !condition())
                {
                    turn = false;
                }
                yield return null;
            }
        }

        private static IEnumerator OnChangeEnumerator<Z>(Func<Z> condition, Action<Z> action)
        {
            var value = condition();
            while (true)
            {
                if (value.Equals(condition()))
                {
                    yield return null;
                    continue;
                }
                action(value);
                value = condition();   
            }
        }

        private static IEnumerator IntervalEnumerator(IntervalData intervalData)
        {
            while (true)
            {
                yield return new WaitForSeconds((float) intervalData.seconds());
                if(intervalData.running) intervalData.action();
            }
        }

        private static IEnumerator WaitKeyEnumerator(TrailLabel label, SequenceElement element)
        {
            while (true)
            {
                element.isCompleted = label.element.back.isCompleted;
                if(element.isCompleted) yield break;
                yield return null;
            }
        }

        #region Trail

        public T SetName(string newTrailName)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    trailName = newTrailName;
                    ApplyName();
                    return null;
                }
            });
            return this as T;
        }

        public T WaitKey(string labelName)
        {
            DirectWork(new WorkOptions
            {
                awakeAction = data =>
                {
                    var label  = monoTrail.GetLabels().FindLast(x => labelName.Equals(x.name));
                    monoTrail.StartCoroutine(WaitKeyEnumerator(label, data));
                }
            });
            return this as T;
        }
        
        public T Wait()
        {
            DirectWork(new WorkOptions
            {
                action = data => new WaitUntil(() => data.back.isCompleted)
            });
            return this as T;
        }
        
        public T Wait(double duration)
        {
            DirectWork(new WorkOptions
            {
                action = data => new WaitForSeconds((float)duration)
            });
            return this as T;
        }
        
        public T Print(object text = null, Func<object> action = null)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    Debug.Log(Resolve(ValueOrAction(text,action)));
                    return null;
                }
            });
            return this as T;
        }
        
        public T PrintAll<Z>(Func<List<Z>> action = null)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    foreach (var e in action())
                    {
                        Debug.Log(Resolve(e));
                    }

                    return null;
                }
            });
            return this as T;
        }
        
        public T Print(Func<object> action = null)
        {
            Print(null, action);
            return this as T;
        }
        
        public T After(Action afterAction)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    afterAction();
                    return null;
                }
            });
            return this as T;
        }
        
        public T Label(string labelName)
        {
            TrailLabel lbl = null;
            DirectWork(new WorkOptions
            {
                awakeAction = data =>
                {
                    lbl = new TrailLabel
                    {
                        name = labelName,
                        element = data
                    };
                    monoTrail.AddToLabels(lbl);
                    currentNamespace?.labels.Add(lbl);   
                },
                action = data =>
                {
                    lbl.activated = true;
                    lbl.onActivation();
                    return null;
                }
            });
            return this as T;
        }
        
        public T Goto(string labelName)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    var labelFind = monoTrail.GetLabels().Find(o => o.name == labelName);
                    if (labelFind == null)
                    {
                        Debug.LogError($"<color=red>There is no label named with '{labelName}'.</color>");
                        return null;
                    }
                    data.next = labelFind.element;
                    return null;
                },
                waitForNull = true
            });
            return this as T;
        }
        
        public T End()
        {
            DirectWork(new WorkOptions
            {
                closeBracket = true
            });
            return this as T;
        }
        
        public T After()
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.Run(data.children, null,true);
                    return null;
                },
                openBracket = true
            });
            return this as T;
        }
        
        public T Parallel()
        {
            DirectWork(new WorkOptions
            {
                awakeAction = data =>
                {
                    monoTrail.Run(data.children, null,true);
                },
                openBracket = true
            });
            return this as T;
        }
        
        public T Alternative(double changeDelay)
        {
            Alternative(() => changeDelay);
            return this as T;
        }
        
        public T Alternative(Func<double> changeDelay)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.Run(data.children, () => false,true);
                    return null;
                },
                openBracket = true,
                onBracketCollected = data =>
                {
                    var changes = new List<ChangeData> {new ChangeData { changeDelay = changeDelay }};
                    foreach (var sequenceElement in data)
                    {
                        if (sequenceElement.name.Equals(nameof(Change))) 
                            changes.Add(new ChangeData { changeDelay = (Func<double>) sequenceElement.get() });
                        else
                            changes.Last().elements.Add(sequenceElement);
                    }
                    monoTrail.StartCoroutine(AlternativeEnumerator(changes));
                }
            });
            return this as T;
        }
        
        public T Change(double customChangeDelay)
        {
            Change(() => customChangeDelay);
            return this as T;
        }

        public T Change(Func<double> customChangeDelay = null)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.Run(data.children, null,true);
                    return null;
                }, 
                get = () => customChangeDelay
            });
            return this as T;
        }

        public T If(bool condition)
        {
            If(() => condition);
            return this as T;
        }
        
        public T If(Func<bool> condition)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    data.data = condition() ? ifEnum.isTrue : ifEnum.isFalse;
                    return monoTrail.Run(data.children, () => ((ifEnum)data.data) == ifEnum.isTrue,true);
                },
                openBracket = true
            });
            return this as T;
        }
        
        public T ElseIf(bool condition)
        {
            ElseIf(() => condition);
            return this as T;
        }

        public T ElseIf(Func<bool> condition)
        {
            DirectWork(new WorkOptions
            {
                startAction = data =>
                {
                    var @enum = (ifEnum) data.parent.data;
                    if (@enum == ifEnum.isFalse) 
                        data.parent.data = condition() ? ifEnum.isTrue : ifEnum.isFalse;
                    else if(@enum == ifEnum.isTrue) data.parent.data = ifEnum.isEnd;
                }
            });
            return this as T;
        }
        
        public T Else()
        {
            DirectWork(new WorkOptions
            {
                startAction = data =>
                {
                    data.parent.data = ((ifEnum) data.parent.data == ifEnum.isFalse) ? ifEnum.isTrue : ifEnum.isEnd;
                }
            });
            return this as T;
        }

        public T SetInterval(Action action, double seconds, int id = -1)
        {
            SetInterval(action, () => seconds, id);
            return this as T;
        }

        public T SetInterval(Action action, Func<double> seconds, int id = -1)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    var interval = new IntervalData
                    {
                        action = action,
                        seconds = seconds,
                        id = id,
                        running = true
                    };
                    monoTrail.StartCoroutine(IntervalEnumerator(interval));
                    monoTrail.intervals.Add(interval);
                    return null;
                }
            });
            return this as T;
        }
        
        public T PauseInterval(int id)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.intervals
                        .FindAll(x => x.id == id)
                        .ForEach(x =>
                        {
                            x.running = false;
                        });
                    return null;
                }
            });
            return this as T;
        }
        
        public T ResumeInterval(int id)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.intervals
                        .FindAll(x => x.id == id)
                        .ForEach(x =>
                        {
                            x.running = true;
                        });
                    return null;
                }
            });
            return this as T;
        }

        public T Wait(string labelName)
        {
            Wait(() => labelName);
            return this as T;
        }
        
        public T Wait(Func<string> labelName)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    return new WaitUntil(() =>
                    {
                        return monoTrail.GetLabels().Exists(x => x.name.Equals(labelName()) && x.activated);
                    });
                }
            });
            return this as T;
        }

        public T Wait(Func<bool> action, double timeout)
        {
            Wait(action, () => timeout);
            return this as T;
        }
        
        public T Wait(Func<bool> action, Func<double> timeout = null)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    if (timeout == null)
                    {
                        return new WaitUntil(action);
                    }
                    var time = 0d;
                    return new WaitUntil(() =>
                    {
                        if (action())
                        {
                            time += Time.deltaTime;
                            if (time > timeout()) return true;   
                        }
                        else
                        {
                            time = 0;
                        }
                        return false;
                    });
                }
            });
            return this as T;
        }
        
        public T Loop()
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.loop += () => monoTrail.Run(data.children, null,true);
                    return null;
                },
                openBracket = true
            });
            return this as T;
        }
        
        public T Loop(Action inLoop)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.loop += inLoop;
                    return null;
                }
            });
            return this as T;
        }
        
        public T FixedLoop()
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.fixedLoop += () => monoTrail.Run(data.children, null,true);
                    return null;
                },
                openBracket = true
            });
            return this as T;
        }
        
        public T FixedLoop(Action inFixedLoop)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.fixedLoop += inFixedLoop;
                    return null;
                }
            });
            return this as T;
        }
        
        public T Namespace(string namespaceName)
        {
            DirectWork(new WorkOptions
            {
                awakeAction = data =>
                {
                    var ns = new NamespaceObject { name =  namespaceName };
                    MonoTrail.namespaces.Add(ns);
                    currentNamespace = ns;
                }
            });
            return this as T;
        }

        public T EndNamespace()
        {
            DirectWork(new WorkOptions
            {
                awakeAction = data =>
                {
                    currentNamespace = null;
                }
            });
            return this as T;
        }

        public T Using(string namespaceName)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    var selected = MonoTrail.namespaces.Find(x => x.name.Equals(namespaceName));
                    monoTrail.namespacesInUse.Add(selected);
                    return null;
                }
            });
            return this as T;
        }
        
        public T Define(string varName, object value)
        {
            DirectWork(new WorkOptions
            {
                awakeAction = data =>
                {
                    var newVariable = new Variable(varName, value);
                    currentNamespace?.variables.Add(newVariable);
                    monoTrail.AddToVariables(newVariable);
                }
            });
            return this as T;
        }
        
        public T SetVar(string varName, object value = null, Func<object> action = null)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    if (this[varName].Get != null) this[varName].Get.value = ValueOrAction(value, action);
                    return null;
                }
            });
            return this as T;
        }
        
        public T SetVar(string varName, Func<object> action)
        {
            SetVar(varName, null, action);
            return this as T;
        }

        public T IncreaseVar(string varName, object value = null, Func<object> action = null)
        {
            DirectWork(new WorkOptions
            {
               action = data =>
               {
                   if(this[varName].Get != null) 
                       this[varName].Get.value = 
                           float.Parse(this[varName].Value.ToString()) + 
                           float.Parse(ValueOrAction(value, action).ToString());
                   return null;
               }
            });
            return this as T;
        }
        
        public T IncreaseVar(string varName, Func<object> action)
        {
            IncreaseVar(varName, null, action);
            return this as T;
        }

        public T While(Func<bool> fn, Func<double> delay = null)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.StartCoroutine(WhileEnumerator(fn, delay, data));
                    return null;
                }
            });
            return this as T;
        }
        
        public T While(Func<bool> fn, double delay)
        {
            While(fn, () => delay);
            return this as T;
        }
        
        public T For(Func<int> times, Action action)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    for (int i = 0; i < times(); i++)
                    {
                        action();
                    }
                    return null;
                }
            });
            return this as T;
        }
        
        public T For(int times, Action action)
        {
            For(() => times, action);
            return this as T;
        }
        
        public T For(Func<int> times, Func<double> delay = null)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.StartCoroutine(ForEnumerator(times, delay, data));
                    return null;
                },
                openBracket = true
            });
            return this as T;
        }
        
        public T For(Func<int> times, double delay = 0)
        {
            For(times, () => delay);
            return this as T;
        }
        
        public T For(int times, double delay = 0)
        {
            For(() => times, () => delay);
            return this as T;
        }
        
        public T For(int times, Func<double> delay)
        {
            For(() => times, delay);
            return this as T;
        }
        
        public T When(Func<bool> condition, Action action)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.StartCoroutine(WhenEnumerator(condition, action));
                    return null;
                }
            });
            return this as T;
        }

        public T When(Func<bool> condition)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.StartCoroutine(WhenEnumerator(condition, () =>
                    {
                        monoTrail.Run(data.children, null, true);
                    }));
                    return null;
                },
                openBracket = true
            });
            return this as T;
        }

        public T OnChange<Z>(Func<Z> condition, Action<Z> action)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.StartCoroutine(OnChangeEnumerator(condition, action));
                    return null;
                }
            });
            return this as T;
        }
        
        public T OnChange<Z>(Func<Z> condition)
        {
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    monoTrail.StartCoroutine(OnChangeEnumerator(condition, value =>
                    {
                        monoTrail.Run(data.children, null, true);
                    }));
                    return null;
                },
                openBracket = true
            });
            return this as T;
        }

        public T Pick<Z>(Func<Z> pickValue, out Variable<Z> variable)
        {
            var v = new Variable<Z>("picked", default);
            variable = v;
            DirectWork(new WorkOptions
            {
                action = data =>
                {
                    v.value = pickValue();
                    return null;
                }
            });
            return this as T;
        }

        public T Activator(Func<string> labelName)
        {
            DirectWork(new WorkOptions
            {
                awakeAction = data =>
                {
                    monoTrail.StartCoroutine(ActivatorEnumerator(labelName, data));
                },
                openBracket = true
            });
            return this as T;
        }
        
        public T Activator(string labelName)
        {
            Activator(() => labelName);
            return this as T;
        }

        #endregion
        
        public class BoolInfo
        {
            public Func<VariableInfo> action;
            public object val;
            public BoolInfo(Func<VariableInfo> val1, object val2)
            {
                action = val1;
                val = val2;
                Debug.Log(action().Value);
            }
            public bool Get => action().Value.ToFloat() > action().Value.ToFloat();
        }
        
        public class VariableInfo
        {
            public TrailObject<T> trailObject;
            public string name;
            public VariableInfo(TrailObject<T> trailObject, string name)
            {
                this.trailObject = trailObject;
                this.name = name;
            }
            
            public static bool operator >(VariableInfo info1, float i) => info1.Value.ToFloat() > i;
            public static bool operator <(VariableInfo info1, float i) => info1.Value.ToFloat() < i;
            public static float operator -(VariableInfo info1, float i) => info1.Value.ToFloat() - i;
            public static float operator +(VariableInfo info1, float i) => info1.Value.ToFloat() + i;

            public static VariableInfo operator --(VariableInfo info1)
            {
                info1.Get.value = info1.Value.ToFloat() - 1;
                return null;
            }

            public static VariableInfo operator ++(VariableInfo info1)
            {
                info1.Get.value = info1.Value.ToFloat() + 1;
                return null;
            }
            
            public Variable Get => Compile();
            public object Value => Get?.value;
            public Variable Compile()
            {
                return trailObject.monoTrail.GetVariables().FirstOrDefault(x => x.name.Equals(name));
            }
        }
    }

    [Serializable]
    public class Variable
    {
        public string name;
        public object value;

        public Variable(string name, object value)
        {
            this.name = name;
            this.value = value;
        }
    }
    
    [Serializable]
    public class Variable<F>
    {
        public string name;
        public F value;

        public Variable(string name, F value)
        {
            this.name = name;
            this.value = value;
        }
    }
}