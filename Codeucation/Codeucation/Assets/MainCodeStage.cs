using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainCodeStage : MonoBehaviour
{
    public List<VarInfo> var;
    public List<VarInfo> var_cpy;
    public List<InstObj> all_inst;

    public InstObj start;
    public Text ConsoleArea, ConsoleAreaCompared;
    public GameObject ConsoleObjCompared;
    public InputField ScanInputArea;

    public Button TestRun, Evaluate, StopRun;

    public Transform addinst;
    public Transform vararea;
    string exactAnswer;

    public bool onInput = false;
    readonly WaitForSeconds wfs = new WaitForSeconds(2f);

    public void OnClickRun()
    {
        SetConsoleCompared(false);
        SetButtonState(false);

        InitInfo();
        ExportCodeTxt();
        StartCoroutine(RunCode(false));
    }

    public void OnClickStop()
    {
        SetConsoleCompared(false);
        StopAllCoroutines();

        foreach (VarInfo vInfo in var)
        {
            vInfo.v.SetVar(vInfo.v_init, vInfo.type);
            vInfo.SetValue(vInfo.v);
        }
        SetButtonState(true);
    }

    public void OnClickEvaluate()
    {
        SetConsoleCompared(true);
        SetButtonState(false);

        InitInfo();
        ExportCodeTxt();
        StartCoroutine(RunCode(true));
    }

    void ParseSampleCode()
    {
        StreamReader sr = new StreamReader(Application.persistentDataPath + "/Codeucation_samplecode.txt");
        string codetxt = sr.ReadToEnd().Replace("\r", string.Empty);

        string[] line = codetxt.Split('\n');

        for (int i = 0; i < line.Length; i++)
        {
            string[] w = line[i].Split(' ', '\t');

            if (w[0].Equals("VAR"))
            {

            }
            else if (w[0].Equals("ln"))
            { 
            
            }
        }
    }

    void SetButtonState(bool runnable)
    {
        TestRun.interactable = runnable;
        Evaluate.interactable = runnable;
        StopRun.interactable = !runnable;
    }

    void SetConsoleCompared(bool activate)
    {
        if (activate)
        {
            StreamReader sr = new StreamReader(Application.persistentDataPath + "/Codeucation_ans.txt");
            exactAnswer = sr.ReadToEnd().Replace("\r", string.Empty);

            ConsoleObjCompared.SetActive(true);
            ConsoleAreaCompared.text = exactAnswer;
        }
        else
        {
            ConsoleObjCompared.SetActive(false);
            ConsoleAreaCompared.text = string.Empty;
        }

    }

    void CheckAnswers()
    {
        if (exactAnswer.Equals(ConsoleArea.text))
        {
            Debug.Log("Correct!");
        }
        else
        {
            Debug.Log("Wrong...");
            Debug.Log(exactAnswer);
            Debug.Log(ConsoleArea.text);
        }
    }

    void InitInfo()
    {
        ConsoleArea.text = string.Empty;

        foreach (InstObj i in all_inst)
        {
            i.export_checked = false;
            i.inst_index = 0;
        }
        all_inst.Clear();

        foreach (VarInfo vInfo in var)
        {
            vInfo.v_init.SetVar(vInfo.v, vInfo.type);
        }
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
            str = string.Format("VAR {0} {1} {2}", t, var[i].varname, var[i].value);
            sw.WriteLine(str);        
        }

        InstObj inst = start;
        int ln = 0;
        int total_ln = SetInstIndex(inst, ln);

        all_inst.Sort((x, y) => {
            return (x.inst_index).CompareTo(y.inst_index);
        });

        foreach (InstObj i in all_inst)
        {
            str = (i.inst_type) switch
            {
                -1 => string.Format("ln {0}\t(END)\n{1}\t-1\t-1", i.inst_index, i.rect.anchoredPosition),
                0 => string.Format("ln {0}\t{1}\n{2}\t{3}\t-1", i.inst_index, i.ExportAsText(), i.rect.anchoredPosition, i.arr0.next_inst.inst_index),
                1 => string.Format("ln {0}\t{1}\n{2}\t{3}\t{4}", i.inst_index, i.ExportAsText(), i.rect.anchoredPosition, i.arr0.next_inst.inst_index, i.arr1.next_inst.inst_index),
                2 => string.Format("ln {0}\t{1}\n{2}\t{3}\t-1", i.inst_index, i.ExportAsText(), i.rect.anchoredPosition, i.arr0.next_inst.inst_index),
                3 => string.Format("ln {0}\t{1}\n{2}\t{3}\t-1", i.inst_index, i.ExportAsText(), i.rect.anchoredPosition, i.arr0.next_inst.inst_index),
                _ => string.Format("ln {0}\t(???)\n{1}\t-1\t-1", i.inst_index, i.rect.anchoredPosition),
            };
            sw.WriteLine(str);
        }
        
        sw.Flush();
        sw.Close();
    }

    int SetInstIndex(InstObj inst, int ln)
    {
        inst.inst_index = ++ln;
        inst.export_checked = true;
        all_inst.Add(inst);

        if (inst.inst_type == 0) // inst
        {
            if (inst.arr0 != null && inst.arr0.next_inst != null)
            {
                if (!inst.arr0.next_inst.export_checked)
                {
                    ln = SetInstIndex(inst.arr0.next_inst, ln);
                }
            }
        }
        else if (inst.inst_type == 1) // if
        {
            if (inst.arr0 != null && inst.arr0.next_inst != null)
            {
                if (!inst.arr0.next_inst.export_checked)
                {
                    ln = SetInstIndex(inst.arr0.next_inst, ln);
                }
            }
            if (inst.arr1 != null && inst.arr1.next_inst != null)
            {
                if (!inst.arr1.next_inst.export_checked)
                {
                    ln = SetInstIndex(inst.arr1.next_inst, ln);
                }
            }
        }
        else if (inst.inst_type == 2) // scan
        {
            if (inst.arr0 != null && inst.arr0.next_inst != null)
            {
                if (!inst.arr0.next_inst.export_checked)
                {
                    ln = SetInstIndex(inst.arr0.next_inst, ln);
                }
            }
        }
        else if (inst.inst_type == 3) // print
        {
            if (inst.arr0 != null && inst.arr0.next_inst != null)
            {
                if (!inst.arr0.next_inst.export_checked)
                {
                    ln = SetInstIndex(inst.arr0.next_inst, ln);
                }
            }
        }

        return ln;
    }

    IEnumerator RunCode(bool eval)
    {
        onInput = false;
        yield return wfs;

        InstObj inst = start;
        VarInfo.VAL val;
        while (true)
        {
            inst.outline.enabled = true;

            if (inst.inst_type == 2)
            {
                ScanInputArea.text = string.Empty;
                ScanInputArea.gameObject.SetActive(true);
                ScanInputArea.Select();
                onInput = true;
                while (onInput)
                {
                    yield return null;
                }
            }

            if ((val = inst.Operate(ConsoleArea, ScanInputArea)).type < 0)
            {
                ConsoleArea.text = string.Format("{0}<color=#FF4444>{1}</color>\n", ConsoleArea.text, val.err);
                inst.outline.enabled = false;
                break;
            }
            
            yield return wfs;
            inst.outline.enabled = false;

            if (inst.next_arr == null)
                break;

            inst = inst.next_arr.next_inst;
        }

        if (eval)
            CheckAnswers();

        OnClickStop();
    }

    public void OnEndEnterInput()
    {
        ScanInputArea.Select();
        ScanInputArea.gameObject.SetActive(false);
        onInput = false;
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
