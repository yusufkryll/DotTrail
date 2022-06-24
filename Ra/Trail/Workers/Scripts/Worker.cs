using System;
using Ra.Trail;
using UnityEngine;

public class Worker : TrailObject<Worker>
{
    public bool enabled = true;
    [HideInInspector] public WorkContainer container;
    public virtual void Start()
    {
        
    }
    
    public virtual void Update()
    {
        
    }
}
