using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotInfo : MonoBehaviour
{
    public InstObj inst;
    public VarObj var;
    public OprtObj oprt;

    public RectTransform rect;
    public BoxCollider2D coll;

    public InstObj parent_inst;

    void Start()
    {
        coll.size = rect.sizeDelta;
    }
}
