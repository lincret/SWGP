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
        next_inst = null;

        dragging = true;
        ptr.enabled = true;
        StartCoroutine(DragArrow());
    }

    IEnumerator DragArrow()
    {
        Vector3 v;
        while (dragging)
        {
            v = Input.mousePosition;
            v.z = 100; // Camera.main.farClipPlane;

            ptr.transform.SetPositionAndRotation(Camera.main.ScreenToWorldPoint(v), Quaternion.FromToRotation(line.GetPosition(0), line.GetPosition(1)));
            line.SetPosition(1, ptr.transform.localPosition);
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

                line.SetPosition(1, line.transform.InverseTransformPoint(tmp_next.position));
                ptr.transform.SetPositionAndRotation(tmp_next.position, Quaternion.FromToRotation(line.GetPosition(0), line.GetPosition(1)));

                ptr.enabled = false;
            }
            else
            {
                ptr.transform.SetPositionAndRotation(line.GetPosition(0), Quaternion.identity);
                line.SetPosition(1, line.GetPosition(0));

                ptr.enabled = false;
            }
        }
        else
        {
            ptr.transform.SetPositionAndRotation(line.GetPosition(0), Quaternion.identity);
            line.SetPosition(1, line.GetPosition(0));

            ptr.enabled = false;
        }

        dragging = false;
    }

    public void DetachArrow()
    {
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
