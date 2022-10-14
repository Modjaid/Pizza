using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TransitBehaviour: MonoBehaviour, ITransit
{
    [SerializeField] private TransitBehaviour transitTo;
    [SerializeField] private GameObject parentMenu;
	[SerializeField] protected static TransitBehaviour currentTransit;

    public ITransit TransitTo
    {
	get
	    {
		return transitTo;
	    }
	set
	    {
		transitTo = value as TransitBehaviour;
	    }
    }

    public GameObject ParentMenu
    {
	get
	    {
		return parentMenu;
	    }
	set
	    {
		parentMenu = value;
	    }
    }
    
    public virtual void Transit(){}
}
