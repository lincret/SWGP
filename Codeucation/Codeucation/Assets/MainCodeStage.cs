using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainCodeStage : MonoBehaviour
{
    public List<VarInfo> var;
    public List<VarInfo> var_cpy;
    public List<InstObj> all_inst;

    public InstObj start;
    public Text ConsoleArea, ConsoleAreaCompared, QuestionArea;
    public GameObject ConsoleObjCompared, StageClear, Question;
    public InputField ScanInputArea;

    public Button TestRun, Evaluate, StopRun;

    public Transform addinst;
    public Transform vararea;
    string exactAnswer;

    public List<VarMatcher> vm = new List<VarMatcher>();
    public List<LineMatcher> lm = new List<LineMatcher>();

    public CodeMapper cm = new CodeMapper();
    public CodeMapper cm_ans = new CodeMapper();

    public bool onInput = false;
    readonly WaitForSeconds wfs = new WaitForSeconds(0.7f);
    readonly Dictionary<string, string> vardic = new Dictionary<string, string>();

    private void Start()
    {
        QuestionArea.text = string.Format("Stage {0}.\n{1}", Refs.stage.num, Refs.stage.question);
    }
    public void OnClickReset()
    {
        SceneManager.LoadScene("MainCodeStage");
    }

    public void OnClickQuit()
    {
        SceneManager.LoadScene("SelectStage");
    }
    public void OnClickRun()
    {
        Question.SetActive(false);
        SetConsoleCompared(false);
        SetButtonState(false);

        InitInfo();
        ExportCodeTxt();
        StartCoroutine(RunCode(false));
    }

    public void OnClickStop()
    {
        Question.SetActive(true);
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
        vardic.Clear();

        Question.SetActive(false);
        SetConsoleCompared(true);
        SetButtonState(false);

        InitInfo();
        ExportCodeTxt();
        StartCoroutine(RunCode(true));

        ParseSampleCode();
    }

    void MakeFeedback()
    {
        vm.Clear();
        lm.Clear();

        for (int i = 0; i < cm_ans.mapperVarInfo.Count; i++)
        {
            vm.Add(new VarMatcher(cm_ans.mapperVarInfo[i]));
        }

        for (int i = 0; i < cm_ans.mapperLineInfo.Count; i++)
        {
            lm.Add(new LineMatcher(cm_ans.mapperLineInfo[i]));
        }

        CheckLineMapper(lm[1], 1);

        lm.Sort((a, b) => b.similarity.CompareTo(a.similarity));

        for (int i = 0; i < lm.Count; i++)
        {
            if (lm[i].mapped && lm[i].similarity < 1 && lm[i].similarity > 0.9)
            {
                Debug.LogFormat("[{2}%]\t[{0}] => [{1}]", lm[i].mappedLine.line, lm[i].targetLine.line, 100 * lm[i].similarity);

                string oldline = lm[i].mappedLine.line, newline = lm[i].targetLine.line;

                foreach (KeyValuePair<string, string> k in vardic)
                {
                    oldline = oldline.Replace(k.Key, k.Value);
                    newline = newline.Replace(k.Key, k.Value);
                }

                ConsoleAreaCompared.text = string.Format("<color=#CCCCCC>Your Original Code,</color>\n<color=#FF4444>{0}</color>\n<color=#CCCCCC>Did you mean...?</color>\n{1}", oldline, newline);

                //all_inst[lm[i].mappedLine.ln].img.color = new Color(1.0f, 0.4f, 0.4f, 1.0f);

                break;
            }
        }

        StopAllCoroutines();

        foreach (VarInfo vInfo in var)
        {
            vInfo.v.SetVar(vInfo.v_init, vInfo.type);
            vInfo.SetValue(vInfo.v);
        }
        SetButtonState(true);
    }

    public double Similarity(string s1, string s2)
    {
        string longer = s1, shorter = s2;
        if (s1.Length < s2.Length)
        {
            longer = s2; shorter = s1;
        }
        int longerLength = longer.Length;
        if (longerLength == 0) { return 1.0; }
        return (longerLength - EditDistance(longer, shorter)) / (double)longerLength;
    }

    public int EditDistance(string s1, string s2)
    {
        int[] costs = new int[s2.Length + 1];
        for (int i = 0; i <= s1.Length; i++)
        {
            int lastValue = i;
            for (int j = 0; j <= s2.Length; j++)
            {
                if (i == 0)
                    costs[j] = j;
                else
                {
                    if (j > 0)
                    {
                        int newValue = costs[j - 1];
                        if (s1[i - 1] != s2[j - 1])
                            newValue = Math.Min(Math.Min(newValue, lastValue),
                                costs[j]) + 1;
                        costs[j - 1] = lastValue;
                        lastValue = newValue;
                    }
                }
            }
            if (i > 0)
                costs[s2.Length] = lastValue;
        }
        return costs[s2.Length];
    }

    int CheckLineMapper(LineMatcher lineMatcher, int ln)
    {
        double similarity = 0, temp;
        int bestln = 0;

        for (int i = 1; i < cm.mapperLineInfo.Count; i++)
        {
            temp = Similarity(lineMatcher.targetLine.line, cm.mapperLineInfo[i].line);
            Debug.LogFormat("[{0}] and [{1}] have {2}% similarity.", lineMatcher.targetLine.line, cm.mapperLineInfo[i].line, 100 * temp);
            if (temp > similarity)
            {
                similarity = temp;
                bestln = i;
            }
        }

        lineMatcher.MatchLine(cm.mapperLineInfo[bestln], similarity);

        if (lineMatcher.targetLine.next0 > 0)
        {
            if (!lm[lineMatcher.targetLine.next0].mapped)
            {
                ln = CheckLineMapper(lm[lineMatcher.targetLine.next0], lineMatcher.mappedLine.next0);
            }
        }
        if (lineMatcher.targetLine.next1 > 0)
        {
            if (!lm[lineMatcher.targetLine.next1].mapped)
            {
                ln = CheckLineMapper(lm[lineMatcher.targetLine.next1], lineMatcher.mappedLine.next1);
            }
        }

        return ln;
    }

    bool CheckLinesAllMatched()
    {
        for (int i = 1; i < cm_ans.mapperLineInfo.Count; i++)
        {
            if (!lm[i].mapped)
                return false;
        }

        return true;
    }

    bool CheckVarsAllMatched()
    {
        for (int i = 1; i < cm_ans.mapperVarInfo.Count; i++)
        {
            if (!vm[i].mapped)
                return false;
        }

        return true;
    }

    void ParseSampleCode()
    {
        TextAsset textAsset = (TextAsset)Resources.Load("codestxt/Codeucation_samplecode_" + Refs.stage.num.ToString("D3"));
        System.IO.StringReader reader = new System.IO.StringReader(textAsset.text);
        string codetxt = reader.ReadToEnd().Replace("\r", string.Empty);

        cm_ans.AddMapperLineinfo(0, -1, -1, string.Empty);

        string[] line = codetxt.Split('\n');

        for (int i = 0; i < line.Length; i++)
        {
            string[] w = line[i].Split('\t');

            if (w[0].Equals("VAR"))
            {
                int vartype = w[1] switch
                {
                    "int"=> 1,
                    "float" => 2,
                    "char" => 3,
                    "string" => 4,
                    "bool" => 5,
                    _ => 0,
                };

                cm_ans.AddMapperVarinfo(vartype, w[3]);
            }
            else if (w[0].StartsWith("ln"))
            {
                string[] wn = line[++i].Split('\t');
                Debug.Log(line[i] + ".");
                cm_ans.AddMapperLineinfo(cm_ans.mapperLineInfo.Count, int.Parse(wn[0]), int.Parse(wn[1]), w[1]);
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
            TextAsset textAsset = (TextAsset)Resources.Load("codestxt/Codeucation_ans_" + Refs.stage.num.ToString("D3"));
            System.IO.StringReader reader = new System.IO.StringReader(textAsset.text);
            exactAnswer = reader.ReadToEnd().Replace("\r", string.Empty);

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

            StageData sLoad = new StageData();
            LocalSavedDt localDt = new LocalSavedDt();
            localDt.LoadLocalData(sLoad);
            localDt.progress[Refs.stage.num - 1].cleared = true;
            localDt.SaveLocalData();

            StageClear.SetActive(true);
        }
        else
        {
            Debug.Log("Wrong...");

            MakeFeedback();
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
        cm = new CodeMapper();
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/Codeucation_" + Refs.stage.num.ToString("D3") + ".txt");

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
            str = string.Format("VAR\t{0}\t{1}\t{2}", t, var[i].varname, var[i].value);
            sw.WriteLine(str);

            cm.AddMapperVarinfo(var[i].type, var[i].value);
            if (var[i].type > 0)
            {
                var[i].mappername = string.Format("{0}{1}", t, cm.varindexcnt[var[i].type]);
                vardic[string.Format("v:{0}", var[i].mappername)] = var[i].varname;
            }
        }

        InstObj inst = start;
        int ln = 0;
        int total_ln = SetInstIndex(inst, ln);

        all_inst.Sort((x, y) => {
            return (x.inst_index).CompareTo(y.inst_index);
        });

        cm.AddMapperLineinfo(0, -1, -1, string.Empty);

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

            str = (i.inst_type) switch
            {
                -1 => "(END)",
                0 => i.ExportAsText(),
                1 => i.ExportAsText(),
                2 => i.ExportAsText(),
                3 => i.ExportAsText(),
                _ => "(???)",
            };

            cm.AddMapperLineinfo(cm_ans.mapperLineInfo.Count, (i.arr0 != null && i.arr0.next_inst != null) ? i.arr0.next_inst.inst_index : -1,
                                                            (i.arr1 != null && i.arr1.next_inst != null) ? i.arr1.next_inst.inst_index : -1, str);
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
        else
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
