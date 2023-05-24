using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeMapper
{
    public List<MapperVarInfo> mapperVarInfo;
    public List<MapperLineInfo> mapperLineInfo;
    public int[] varindexcnt;

    public CodeMapper()
    {
        mapperVarInfo = new List<MapperVarInfo>();
        mapperLineInfo = new List<MapperLineInfo>();
        varindexcnt = new int[6];
    }

    public void AddMapperVarinfo(int vartype, string initval)
    {
        mapperVarInfo.Add(new MapperVarInfo(vartype, initval));
        varindexcnt[vartype]++;

    }

    public void AddMapperLineinfo(int ln, int next0, int next1, string line)
    {
        mapperLineInfo.Add(new MapperLineInfo(ln, next0, next1, line));
    }
}

public class LineMatcher
{
    public bool mapped;
    public MapperLineInfo targetLine;
    public MapperLineInfo mappedLine;
    public double similarity;

    public LineMatcher(MapperLineInfo targetLine)
    {
        mapped = false;
        this.targetLine = targetLine;
        mappedLine = null;
    }

    public void MatchLine(MapperLineInfo mappedLine, double similarity)
    {
        this.mappedLine = mappedLine;
        this.similarity = similarity;
        mapped = true;

        Debug.LogFormat("{0} matched to {1}", targetLine.line, this.mappedLine.line);
    }
}

public class VarMatcher
{
    public bool mapped;
    public MapperVarInfo targetVar;
    public MapperVarInfo mappedVar;

    public VarMatcher(MapperVarInfo targetVar)
    {
        mapped = false;
        this.targetVar = targetVar;
        mappedVar = null;
    }

    public void MatchVar(MapperVarInfo mappedVar)
    {
        this.mappedVar = mappedVar;
        mapped = true;
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
