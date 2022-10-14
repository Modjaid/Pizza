using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitOverButton : TransitButton
{
    public override void Transit()
    {
	     TransitTo?.ParentMenu?.gameObject.SetActive(true);
    }
}
