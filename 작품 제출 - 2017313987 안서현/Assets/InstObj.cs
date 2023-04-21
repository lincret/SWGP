using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InstObj : MonoBehaviour
{
    public int inst_type;

    public SlotInfo slot_L;
    public SlotInfo slot_R;
    public SlotInfo slot_O;

    public Outline outline;
    public RectTransform rect;

    public Vector2 size;

    public Vector3 tmp_loc;
    public Transform tmp_slot;
    public SlotInfo parent_slot;
    public InstObj parent_inst;

    public ArrObj next_arr;
    public ArrObj arr0, arr1;
    public List<ArrObj> arr_from;
    public Transform inline;

    public List<GameObject> hider;

    public Transform main_canvas;
    public Image img;

    public bool export_checked;
    public int inst_index;

    readonly Color[] color = {
        new Color(0.8f, 0.8f, 0.8f, 1.0f),
        new Color(0.5f, 0.6f, 1.0f, 1.0f),
        new Color(0.2f, 0.7f, 0.4f, 1.0f),
        new Color(0.55f, 0.95f, 1.0f, 1.0f),
        new Color(0.4f, 0.8f, 0.65f, 1.0f),
        new Color(1.0f, 0.95f, 0.55f, 1.0f),
        new Color(1.0f, 0.4f, 0.4f, 1.0f)
    };

    public bool passthru;
    public int type;

    bool isHolded = false;

    private void Start()
    {
        main_canvas = transform.parent;
    }

    public VarInfo.VAL Operate(Text con, InputField scn)
    {
        VarInfo.VAL rv = new VarInfo.VAL();

        rv.Init();

        if (passthru) return rv;

        if (inst_type == 1)     // if
        {
            VarInfo.VAL temp_val = ReturnValue(slot_L, con, scn);

            if (temp_val.type == 5) // bool type should be in the slot
            {
                if (temp_val.GetBool())
                {
                    next_arr = arr1;
                }
                else
                {
                    next_arr = arr0;
                }
                return temp_val;
            }
            else
            {
                // type error
                return rv;
            }
        }
        else if (inst_type == 2)    // scan
        {
            rv.SetValue(scn.text, slot_L.var.varInfo.type);
            AssignValue(slot_L, rv);
            next_arr = arr0;

            return rv;
        }
        else if (inst_type == 3)    // print
        {
            con.text = string.Format("{0}{1}\n", con.text, ReturnValue(slot_L, con, scn).GetVal());
            next_arr = arr0;

            return ReturnValue(slot_L, con, scn);
        }

        int op = 0;
        if (slot_O.oprt != null)
            op = slot_O.oprt.oprt_type;

        VarInfo.VAL val_L = ReturnValue(slot_L, con, scn);
        VarInfo.VAL val_R = ReturnValue(slot_R, con, scn);

        VarInfo.VAL val_res = new VarInfo.VAL();
        val_res.Init();

        if (val_L.type < 0)
        {
            return val_L;
        }
        if (val_R.type < 0)
        {
            return val_R;
        }

        if (op / 10 == 1) // if op is "+ - * / %"
        {
            switch (Math.Max(slot_L.var.varInfo.type, slot_R.var.varInfo.type))
            {
                case 1:
                case 2:
                    if (slot_L.var.varInfo.type == 1)
                    {
                        val_res.OpInt(val_L.GetInt(), val_R.GetInt(), op);
                        val_res.type = val_L.type < 0 || val_R.type < 0 ? -1 : 1;
                        break;
                    }
                    else
                    {
                        val_res.OpChar(val_L.GetInt(), val_R.GetInt(), op);
                        val_res.type = val_L.type < 0 || val_R.type < 0 ? -1 : 2; 
                        break;
                    }
                case 3:
                    val_res.OpFloat(val_L.GetFloat(), val_R.GetFloat(), op);
                    val_res.type = val_L.type < 0 || val_R.type < 0 ? -1 : 3; 
                    break;
                case 4:
                    val_res.OpString(val_L.GetString(), val_R.GetString(), op);
                    val_res.type = val_L.type < 0 || val_R.type < 0 ? -1 : 4;
                    break;
                default:
                    val_res.err = "Invalid Type";
                    val_res.type = -1;
                    break; // do not allow bool variables
            };

            type = val_res.type;
        }
        else if (op / 10 == 2) // if op is "== > >= < <="
        {
            switch (Math.Max(slot_L.var.varInfo.type, slot_R.var.varInfo.type))
            {
                case 1:
                case 2:
                    val_res.CompInt(val_L.GetInt(), val_R.GetInt(), op);
                    val_res.type = val_L.type < 0 || val_R.type < 0 ? -1 : 5;
                    break;
                case 3:
                    val_res.CompFloat(val_L.GetFloat(), val_R.GetFloat(), op);
                    val_res.type = val_L.type < 0 || val_R.type < 0 ? -1 : 5;
                    break;
                case 4:
                    val_res.CompString(val_L.GetString(), val_R.GetString(), op);
                     val_res.type = val_L.type < 0 || val_R.type < 0 ? -1 : 5;
                    break;
                case 5:
                    val_res.CompBool(val_L.GetBool(), val_R.GetBool(), op);
                    val_res.type = val_L.type < 0 || val_R.type < 0 ? -1 : 5;
                    break;
                default:
                    val_res.err = "Invalid Type";
                    val_res.type = -1;
                    break; // do not allow other variables?
            };

            type = val_res.type;
        }
        else // if op is "<-" (=)
        {
            switch (slot_L.var.varInfo.type)
            {
                case 1:
                    if (val_R.Assignable(1))
                        val_res.SetInt(val_R.GetInt());
                    else
                        val_res.SetError("Invalid Type");
                    break;
                case 2:
                    if (val_R.Assignable(2))
                        val_res.SetChar((char)val_R.GetInt());
                    else
                        val_res.SetError("Invalid Type");
                    break;
                case 3:
                    if (val_R.Assignable(3))
                        val_res.SetFloat(val_R.GetFloat());
                    else
                        val_res.SetError("Invalid Type");
                    break;
                case 4:
                    if (val_R.Assignable(4))
                        val_res.SetString(val_R.GetString());
                    else
                        val_res.SetError("Invalid Type");
                    break;
                case 5:
                    if (val_R.Assignable(5))
                        val_res.SetBool(val_R.GetBool());
                    else
                        val_res.SetError("Invalid Type");
                    break;
                default:
                    val_res.err = "Invalid Type";
                    val_res.type = -1;
                    break; // do not allow bool variables
            };

            AssignValue(slot_L, val_res);
            type = val_res.type;
        }


        if (parent_slot != null)
        {
            img.color = type switch
            {
                1 => color[1],
                2 => color[2],
                3 => color[3],
                4 => color[4],
                5 => color[5],
                -1 => color[6],
                _ => color[0],
            };
        }
        else
        {
            img.color = type switch
            {
                -1 => color[6],
                _ => color[0],
            };
            next_arr = arr0;
        }


        return val_res;
    }

    public string ExportAsText()
    {
        if (passthru) return "(START)";

        int op = 0;
        if (slot_O.oprt != null)
        {
            op = slot_O.oprt.oprt_type;

            return op switch
            {
                10 => string.Format("[ {0} ] + [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                11 => string.Format("[ {0} ] - [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                12 => string.Format("[ {0} ] * [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                13 => string.Format("[ {0} ] / [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                14 => string.Format("[ {0} ] % [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                20 => string.Format("[ {0} ] == [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                21 => string.Format("[ {0} ] > [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                22 => string.Format("[ {0} ] >= [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                23 => string.Format("[ {0} ] < [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                24 => string.Format("[ {0} ] <= [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                30 => string.Format("[ {0} ] && [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                31 => string.Format("[ {0} ] || [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
                _ => string.Format("[ {0} ] = [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R)),
            };
        }
        else if (inst_type == 1)
        {
            return string.Format("(IF) {0}", ReturnValueAsText(slot_L));
        }
        else if (inst_type == 2)
        {
            return string.Format("(SCAN) {0}", ReturnValueAsText(slot_L));
        }
        else if (inst_type == 3)
        {
            return string.Format("(PRINT) {0}", ReturnValueAsText(slot_L));
        }
        else
        {
            return string.Format("(???) [ {0} ] ??? [ {1} ]", ReturnValueAsText(slot_L), ReturnValueAsText(slot_R));
        }

    }

    public void AssignValue(SlotInfo slot, VarInfo.VAL val)
    {
        if (slot.var != null)
        {
            slot.var.varInfo.SetValue(val);
        }
    }

    public VarInfo.VAL ReturnValue(SlotInfo slot, Text con, InputField scn)
    {
        if (slot.var != null)
        {
            return slot.var.varInfo.v;
        }
        else if (slot.inst != null)
        {
            return slot.inst.Operate(con, scn);
        }
        else
        {
            return slot.var.varInfo.v; // 0 //temp;;
        }
    }

    public string ReturnValueAsText(SlotInfo slot)
    {
        if (slot.var != null)
        {
            if (string.IsNullOrWhiteSpace(slot.var.varInfo.varname))
            {
                return slot.var.varInfo.value.ToString();
            }
            else
            {
                return string.Format("v:{0}", slot.var.varInfo.varname.ToString());

            }
        }
        else if (slot.inst != null)
        {
            return slot.inst.ExportAsText();
        }
        else
        {
            return string.Empty;
        }
    }

    public void ResizeObj()
    {
        Vector2 v = new Vector2(0, 0);
        
        v.y += Math.Max(slot_L.rect.sizeDelta.y, slot_R.rect.sizeDelta.y) + 32;

        v.x = -(slot_L.rect.sizeDelta.x + slot_O.rect.sizeDelta.x + slot_R.rect.sizeDelta.x + 64) * 0.5f;


        if (inst_type == 0)
        {
            v.x += 16 + (slot_L.rect.sizeDelta.x) * 0.5f;
            slot_L.rect.anchoredPosition = new Vector2(v.x, slot_L.rect.anchoredPosition.y);
            v.x += slot_L.rect.sizeDelta.x * 0.5f + 16 + slot_O.rect.sizeDelta.x * 0.5f;
            slot_O.rect.anchoredPosition = new Vector2(v.x, slot_O.rect.anchoredPosition.y);
            v.x += slot_O.rect.sizeDelta.x * 0.5f + 16 + slot_R.rect.sizeDelta.x * 0.5f;
            slot_R.rect.anchoredPosition = new Vector2(v.x, slot_R.rect.anchoredPosition.y);
            v.x += slot_R.rect.sizeDelta.x * 0.5f + 16;
        }
        else
        {
            v.x += 32 + slot_L.rect.sizeDelta.x * 0.5f;
            slot_L.rect.anchoredPosition = new Vector2(v.x, slot_L.rect.anchoredPosition.y);
            v.x += slot_L.rect.sizeDelta.x * 0.5f + 32;
        }

        v.x *= 2;

        rect.sizeDelta = v;

        if (slot_L.inst != null)
            slot_L.inst.rect.position = slot_L.rect.position;
        if (slot_O.inst != null)
            slot_O.inst.rect.position = slot_O.rect.position;
        if (slot_R.inst != null)
            slot_R.inst.rect.position = slot_R.rect.position;

        if (parent_slot != null)
        {
            parent_slot.rect.sizeDelta = v;
            parent_slot.parent_inst.ResizeObj();
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

        foreach (ArrObj a in arr_from)
        {
            Vector3 v3_o = a.line.GetPosition(3);
            Vector3 v3 = a.line.transform.InverseTransformPoint(inline.position);
            a.line.SetPosition(3, v3);

            Vector3 v2 = a.line.GetPosition(2);
            v2 = new Vector2((Math.Abs(v3_o.x - v2.x) < 1) ? v3.x : v2.x, (Math.Abs(v3_o.y - v2.y) < 1) ? v3.y : v2.y);
            a.line.SetPosition(2, v2);
        }

        if (arr0 != null && arr0.next_inst != null)
        {
            Vector3 v3_o = arr0.line.GetPosition(3);
            Vector3 v3 = arr0.line.transform.InverseTransformPoint(arr0.next_inst.inline.position);
            arr0.line.SetPosition(3, v3);

            Vector3 v2 = arr0.line.GetPosition(2);
            v2 = new Vector2((Math.Abs(v3_o.x - v2.x) < 1) ? v3.x : v2.x, (Math.Abs(v3_o.y - v2.y) < 1) ? v3.y : v2.y);
            arr0.line.SetPosition(2, v2);
        }

        if (arr1 != null && arr1.next_inst != null)
        {
            Vector3 v3_o = arr1.line.GetPosition(3);
            Vector3 v3 = arr1.line.transform.InverseTransformPoint(arr1.next_inst.inline.position);
            arr1.line.SetPosition(3, v3);

            Vector3 v2 = arr1.line.GetPosition(2);
            v2 = new Vector2((Math.Abs(v3_o.x - v2.x) < 1) ? v3.x : v2.x, (Math.Abs(v3_o.y - v2.y) < 1) ? v3.y : v2.y);
            arr1.line.SetPosition(2, v2);
        }
    }

    public void OnDragEnd(BaseEventData data)
    {
        PointerEventData pointer_data = (PointerEventData)data;

        if (tmp_slot != null)
        {
            parent_slot = tmp_slot.GetComponent<SlotInfo>();
            parent_slot.inst = this;
            parent_slot.coll.enabled = false;
            parent_slot.rect.sizeDelta = rect.sizeDelta;
            parent_slot.parent_inst.ResizeObj();

            transform.position = parent_slot.transform.position;
            transform.SetParent(parent_slot.transform);

            arr0.DetachArrow();
            foreach (GameObject h in hider)
            {
                h.SetActive(false);
            }
        }
        else
        {
            if (parent_slot != null)
            {
                parent_slot.inst = null;
                parent_slot.coll.enabled = true;
                parent_slot.rect.sizeDelta = new Vector2(100, 100);
                parent_slot.parent_inst.ResizeObj();

                foreach (GameObject h in hider)
                {
                    h.SetActive(true);
                }
            }

            transform.position = Camera.main.ScreenToWorldPoint(pointer_data.position) + tmp_loc;
            transform.SetParent(main_canvas);
        }

        outline.enabled = false;
        isHolded = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (inst_type == 0 && isHolded && other.CompareTag("slot")
            && Math.Abs(other.transform.position.x - transform.position.x) < 40 && Math.Abs(other.transform.position.y - transform.position.y) < 80)
        {
            tmp_slot = other.transform;
            tmp_slot.GetComponent<Outline>().enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (inst_type == 0 && isHolded && other.CompareTag("slot") && tmp_slot != null)
        {
            tmp_slot.GetComponent<Outline>().enabled = false;
            tmp_slot = null;
        }
    }
}
