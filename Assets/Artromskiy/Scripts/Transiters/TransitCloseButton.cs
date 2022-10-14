using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitCloseButton : TransitButton
{
    public override void Transit()
    {
	ParentMenu?.gameObject.SetActive(false);
	TransitTo?.ParentMenu?.gameObject.SetActive(true);
    }
}
