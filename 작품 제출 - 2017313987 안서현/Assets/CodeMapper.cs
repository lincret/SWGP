using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeMapper
{
    //public List<MapperVarInfo> mapperVarInfo;
    public List<MapperLineInfo> mapperLineInfo;
    public int[] varindexcnt;

    public CodeMapper()
    {
        //mapperVarInfo = new List<MapperVarInfo>();
        mapperLineInfo = new List<MapperLineInfo>();
        varindexcnt = new int[6];
    }
}

public class MapperVarInfo
{
    public int vartype;
    public int index;  // index of type
    public string initval;
    public VarInfo mappedtarget;

    public MapperVarInfo(int vartype, string initval)
    {
        this.vartype = vartype;
        this.initval = initval;

        mappedtarget = null;
    }
}

public class MapperLineInfo
{
    public int ln;
    public int next0;
    public int next1;
    public string line;
    public InstObj mappedtarget;

    public MapperLineInfo(int ln, int next0, int next1, string line)
    {
        this.ln = ln;
        this.next0 = next0;
        this.next1 = next1;
        this.line = line;

        mappedtarget = null;
    }

    public void CompareToMap(InstObj target, string[] var_o, string[] var_r)
    {
        string txt = target.ExportAsText();
        for (int i = 0; i < var_o.Length && i < var_r.Length; i++)
        {
            txt = txt.Replace(var_o[i], var_r[i]);
        }

        if (line.Equals(txt))
        {
            mappedtarget = target;
        }
    }
}
