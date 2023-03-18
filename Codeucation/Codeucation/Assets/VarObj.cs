using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VarObj : MonoBehaviour
{
    public VarInfo varInfo;

    public Image img;
    public Text text;

    public Outline outline;
    public RectTransform rect;

    public Vector2 size;

    public Vector3 tmp_loc;
    public Transform tmp_slot;
    public SlotInfo parent_slot;
    public InstObj parent_inst;

    public Transform main_canvas;

    bool isHolded = false;

    public List<Sprite> spr;

    private void Start()
    {
        SetSprite();
        SetValueToText();
        main_canvas = transform.parent;
    }

    public void SetSprite()
    {
        if (varInfo.varname != string.Empty)
        {
            img.sprite = spr[varInfo.type];
        }
        else
        {
            img.sprite = spr[5];
        }
    }

    public void SetValueToText()
    {
        if (varInfo.varnameText != null)
        {
            text.text = varInfo.varname.ToString();
        }
        else
        {
            text.text = varInfo.value.ToString();
        }
    }

    public void OnDragBegin(BaseEventData data)
    {
        PointerEventData pointer_data = (PointerEventData)data;

        outline.enabled = true;
        isHolded = true;
        tmp_loc = transform.position - Camera.main.ScreenToWorldPoint(pointer_data.position);
    }

    public void OnDrag(BaseEventData data)
    {
        PointerEventData pointer_data = (PointerEventData)data;
        transform.position = Camera.main.ScreenToWorldPoint(pointer_data.position) + tmp_loc;
    }

    public void OnDragEnd(BaseEventData data)
    {
        PointerEventData pointer_data = (PointerEventData)data;

        if (tmp_slot != null)
        {
            parent_slot = tmp_slot.GetComponent<SlotInfo>();
            parent_slot.var = this;
            parent_slot.coll.enabled = false;
            parent_slot.rect.sizeDelta = rect.sizeDelta;
            parent_slot.parent_inst.ResizeObj();

            transform.position = parent_slot.transform.position;
            transform.SetParent(parent_slot.transform);
        }
        else
        {
            if (parent_slot != null)
            {
                parent_slot.var = null;
                parent_slot.coll.enabled = true;
                parent_slot.rect.sizeDelta = new Vector2(100, 100);
                parent_slot.parent_inst.ResizeObj();
            }

            transform.position = Camera.main.ScreenToWorldPoint(pointer_data.position) + tmp_loc;
            transform.SetParent(main_canvas);
        }

        outline.enabled = false;
        isHolded = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isHolded && other.CompareTag("slot") && Math.Abs(other.transform.position.x - transform.position.x) < 40 && Math.Abs(other.transform.position.y - transform.position.y) < 80)
        {
            tmp_slot = other.transform;
            tmp_slot.GetComponent<Outline>().enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isHolded && other.CompareTag("slot") && tmp_slot != null)
        {
            tmp_slot.GetComponent<Outline>().enabled = false;
            tmp_slot = null;
        }
    }
}
