using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITransit
{
    ITransit TransitTo { get; set; }
    GameObject ParentMenu { get; set; }

    void Transit();
}