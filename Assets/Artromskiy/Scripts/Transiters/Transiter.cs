using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transiter: ITransit
{
    private ITransit transitTo;
    private GameObject parentMenu;

    public ITransit TransitTo
    {
	get
	    {
		return transitTo;
	    }
	set
	    {
		transitTo = value;
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
