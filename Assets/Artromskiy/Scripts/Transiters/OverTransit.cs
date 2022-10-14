using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverTransit : Transiter
{
    public override void Transit()
    {
	TransitTo?.ParentMenu?.gameObject.SetActive(true);
    }
}
