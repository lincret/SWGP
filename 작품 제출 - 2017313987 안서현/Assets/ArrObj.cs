using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrObj : MonoBehaviour
{
    public InstObj parent_inst;
    public InstObj next_inst;

    public LineRenderer line;
    public Image ptr;

    bool dragging = false;

    public Transform tmp_next;

    public void OnDragBegin()
    {
        if (next_inst != null)
            next_inst.arr_from.Remove(this);
        next_inst = null;

        dragging = true;
        ptr.enabled = true;
        StartCoroutine(DragArrow());
    }

    IEnumerator DragArrow()
    {
        Vector3 v;
        Vector3[] nd = new Vector3[4];
        while (dragging)
        {
            v = Input.mousePosition;
            v.z = 100; // Camera.main.farClipPlane;

            ptr.transform.SetPositionAndRotation(Camera.main.ScreenToWorldPoint(v), Quaternion.FromToRotation(line.GetPosition(2), line.GetPosition(3)));
            line.SetPosition(3, ptr.transform.localPosition);

            for (int i = 0; i < 4; i++)
            {
                nd[i] = line.GetPosition(i);
            }

            if (ptr.transform.localPosition.y > 0)
            {
                if (Math.Abs(ptr.transform.localPosition.x) < parent_inst.rect.sizeDelta.x * 0.7f
                    && Math.Abs(ptr.transform.localPosition.y) < parent_inst.rect.sizeDelta.y * 1.1f)
                {
                    float xtmp = (ptr.transform.localPosition.x >= 0 ? 1 : -1) * parent_inst.rect.sizeDelta.x * 0.7f;
                    line.SetPosition(1, new Vector2(xtmp, nd[0].y));
                    line.SetPosition(2, new Vector2(xtmp, nd[3].y));
                }
                else
                {
                    float xtmp = (ptr.transform.localPosition.x >= 0 ? 1 : -1) * parent_inst.rect.sizeDelta.x * 0.7f;
                    line.SetPosition(1, new Vector2(xtmp, nd[0].y));
                    line.SetPosition(2, new Vector2(nd[1].x, nd[3].y));
                }
            }
            else
            {
                line.SetPosition(1, new Vector2(nd[0].x, (nd[0].y + nd[3].y) * 0.5f));
                line.SetPosition(2, new Vector2(nd[3].x, (nd[0].y + nd[3].y) * 0.5f));
            }

            yield return null;
        }
    }

    public void OnCancelDrag()
    {
        if (tmp_next != null)
        {
            tmp_next.GetComponent<Outline>().enabled = false;

            InstObj i;
            if ((i = tmp_next.parent.GetComponent<InstObj>()) != null)
            {
                next_inst = i;
                next_inst.arr_from.Add(this);

                Vector3 v3 = line.transform.InverseTransformPoint(tmp_next.position);
                line.SetPosition(3, v3);

                Vector3 v2 = line.GetPosition(2);
                v2 = new Vector2((Math.Abs(v3.x - v2.x) < 40) ? v3.x : v2.x, (Math.Abs(v3.y - v2.y) < 40) ? v3.y : v2.y);
                line.SetPosition(2, v2);

                ptr.enabled = false;
            }
            else
            {
                Vector3 v0 = line.GetPosition(0);
                ptr.transform.SetPositionAndRotation(v0, Quaternion.identity);
                for (int k = 1; k < line.positionCount; k++)
                {
                    line.SetPosition(k, v0);
                }

                ptr.enabled = false;
            }
        }
        else
        {
            Vector3 v0 = line.GetPosition(0);
            ptr.transform.SetPositionAndRotation(v0, Quaternion.identity);
            for (int k = 1; k < line.positionCount; k++)
            {
                line.SetPosition(k, v0);
            }

            ptr.enabled = false;
        }

        dragging = false;
    }

    public void DetachArrow()
    {
        if (next_inst != null)
            next_inst.arr_from.Remove(this);
        next_inst = null;

        ptr.transform.SetPositionAndRotation(line.GetPosition(0), Quaternion.identity);
        line.SetPosition(1, line.GetPosition(0));

        ptr.enabled = false;

        dragging = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (dragging && other.CompareTag("lnp"))
        {
            tmp_next = other.transform;
            tmp_next.GetComponent<Outline>().enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (dragging && other.CompareTag("lnp") && tmp_next != null)
        {
            tmp_next.GetComponent<Outline>().enabled = false;
            tmp_next = null;
        }
    }
}
