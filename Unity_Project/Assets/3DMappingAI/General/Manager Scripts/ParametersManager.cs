using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParametersManager : MonoBehaviour
{

    [SerializeField]
    private Parameters currentParameters = null;

    public Parameters Current
    {
        get { return currentParameters; }
    }
}
