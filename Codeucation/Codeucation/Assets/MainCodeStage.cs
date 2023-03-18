using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainCodeStage : MonoBehaviour
{
    public List<VarInfo> var;

    public InstObj start;

    public Transform addinst;
    public Transform vararea;
    readonly WaitForSeconds wfs = new WaitForSeconds(2f);

    public void OnClickRun()
    {
        ExportCodeTxt();
        StartCoroutine(RunCode());
    }

    void ExportCodeTxt()
    {
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/Codeucation.txt");

        string str;
        for (int i = 0; i < var.Count; i++)
        {
            string t = var[i].type switch
            {
                1 => "int",
                2 => "char",
                3 => "float",
                4 => "string",
                5 => "bool",
                _ => "var"
            };
            str = string.Format("var:{0} {1} = {2}", t, var[i].varname, var[i].value);
            sw.WriteLine(str);        
        }

        InstObj inst = start;
        int ln = 0;
        while (inst.next_arr != null && inst.next_arr.next_inst != null)
        {
            str = string.Format("ln {0}\t{1}\t\t{2}", ln, inst.ExportAsText(), inst.rect.anchoredPosition);
            sw.WriteLine(str);

            ln++;
            inst = inst.next_arr.next_inst;
        }

        sw.Flush();
        sw.Close();
    }

    IEnumerator RunCode()
    {
        yield return wfs;

        InstObj inst = start;
        while (inst.next_arr != null && inst.next_arr.next_inst != null)
        {
            inst.outline.enabled = true;
            inst.Operate();
            yield return wfs;
            inst.outline.enabled = false;

            inst = inst.next_arr.next_inst;
        }
    }

    public void OnClickNewInst(string name)
    {
        GameObject fab = Resources.Load(string.Format("Prefabs/{0}", name)) as GameObject;
        if (name.StartsWith("Variable_"))
        {
            GameObject obj = Instantiate(fab, vararea);
            VarInfo vInfo = obj.GetComponent<VarInfo>();
            VarInfo.VAL val = new VarInfo.VAL();
            val.Init();
            vInfo.SetValue(val);
            var.Add(vInfo);
            obj.GetComponent<Button>().onClick.AddListener(delegate {
                OnClickNewVarBlock(vInfo);
            });
        }
        else
        {
            Instantiate(fab, addinst);
        }
    }

    public void OnClickNewVarBlock(VarInfo v)
    {
        GameObject obj = Resources.Load("Prefabs/VarObj") as GameObject;
        obj.GetComponent<VarObj>().varInfo = v;
        Instantiate(obj, addinst);
    }
}
