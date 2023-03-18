using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VarInfo : MonoBehaviour
{
    public string varname;

    public struct VAL
    {
        public int i;
        public char c;
        public float f;
        public string s;
        public bool b;

        public int type;
        public string err;

        public void SetInt(int i)
        {
            this.i = i;
            this.f = 0;
            this.c = (char)0;
            this.s = string.Empty;
            this.b = false;

            type = 1;
            err = string.Empty;
        }

        public void SetChar(char c)
        {
            this.i = 0;
            this.f = 0;
            this.c = c;
            this.s = string.Empty;
            this.b = false;

            type = 2;
            err = string.Empty;
        }

        public void SetFloat(float f)
        {
            this.i = 0;
            this.f = f;
            this.c = (char)0;
            this.s = string.Empty;
            this.b = false;

            type = 3;
            err = string.Empty;
        }

        public void SetString(string s)
        {
            this.i = 0;
            this.f = 0;
            this.c = (char)0;
            this.s = s;
            this.b = false;

            type = 4;
            err = string.Empty;
        }

        public void SetBool(bool b)
        {
            this.i = 0;
            this.f = 0;
            this.c = (char)0;
            this.s = string.Empty;
            this.b = b;

            type = 5;
            err = string.Empty;
        }

        public void SetVar(VAL v, int type)
        {
            switch (type)
            {
                case 1: SetInt(v.i); break;
                case 2: SetChar(v.c); break;
                case 3: SetFloat(v.f); break;
                case 4: SetString(v.s); break;
                case 5: SetBool(v.b); break;
                default: err = "Invalid Type"; this.type = -1; break;
            };
        }

        public void OpInt(int i1, int i2, int op)
        {
            int res = op switch
            {
                10 => i1 + i2,
                11 => i1 - i2,
                12 => i1 * i2,
                13 => i1 / i2,
                14 => i1 % i2,
                _ => 0,
            };

            SetInt(res);
        }

        public void CompInt(int i1, int i2, int op)
        {
            bool res = op switch
            {
                20 => i1 == i2,
                21 => i1 > i2,
                22 => i1 >= i2,
                23 => i1 < i2,
                24 => i1 <= i2,
                _ => false,
            };

            SetBool(res);
        }

        public void OpFloat(float f1, float f2, int op)
        {
            float res = op switch
            {
                10 => f1 + f2,
                11 => f1 - f2,
                12 => f1 * f2,
                13 => f1 / f2,
                14 => f1 % f2,
                _ => 0,
            };

            SetFloat(res);
        }

        public void CompFloat(float f1, float f2, int op)
        {
            bool res = op switch
            {
                20 => f1 == f2,
                21 => f1 > f2,
                22 => f1 >= f2,
                23 => f1 < f2,
                24 => f1 <= f2,
                _ => false,
            };

            SetBool(res);
        }

        public void OpString(string s1, string s2, int op)
        {
            string res = op switch
            {
                10 => s1 + s2,
                _ => string.Empty,
            };

            SetString(res);
        }

        public void CompString(string s1, string s2, int op)
        {
            bool res = op switch
            {
                20 => s1.CompareTo(s2) == 0,
                21 => s1.CompareTo(s2) > 0,
                22 => s1.CompareTo(s2) >= 0,
                23 => s1.CompareTo(s2) < 0,
                24 => s1.CompareTo(s2) <= 0,
                _ => false,
            };

            SetBool(res);
        }

        public void CompBool(bool b1, bool b2, int op)
        {
            bool res = op switch
            {
                20 => b1.CompareTo(b2) == 0,
                21 => b1.CompareTo(b2) > 0,
                22 => b1.CompareTo(b2) >= 0,
                23 => b1.CompareTo(b2) < 0,
                24 => b1.CompareTo(b2) <= 0,
                _ => false,
            };

            SetBool(res);
        }

        public void Init()
        {
            this.i = 0;
            this.f = 0;
            this.c = (char)0;
            this.s = string.Empty;
            this.b = false;

            type = 0;
            err = string.Empty;
        }

        public int GetInt()
        {
            switch (type)
            {
                case 1: return i;
                case 2: return c;
                default: err = "Invalid Type"; type = -1; return 0;
            }
        }

        public float GetFloat()
        {
            switch (type)
            {
                case 1: return i;
                case 2: return c;
                case 3: return f;
                default: err = "Invalid Type"; type = -1; return 0;
            }
        }

        public string GetString()
        {
            switch (type)
            {
                case 1: return i.ToString();
                case 2: return c.ToString();
                case 3: return f.ToString();
                case 4: return s;
                default: err = "Invalid Type"; type = -1; return string.Empty;
            }
        }

        public bool GetBool()
        {
            if (type != 5)
            {
                err = "Invalid Type";
                type = -1;
            }

            return b;
        }
    }

    public VAL v;

    public int type;
    public string value;

    public Text varnameText;
    public Text valueText;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (varnameText != null)
        {
            varnameText.text = varname.ToString();
            valueText.text = value;
        }
    }

    public void SetValue(string value)
    {
        this.value = value;

        switch (type)
        {
            case 1: v.SetInt(int.Parse(value)); break;
            case 2: v.SetChar(char.Parse(value)); break; 
            case 3: v.SetFloat(float.Parse(value)); break;
            case 4: v.SetString(value); break;
            case 5: v.SetBool(value.Equals("true")); break;
            default: break;
        }

        valueText.text = value;
    }

    public void SetValue(VAL v)
    {
        this.v = v;

        value = type switch
        {
            1 => v.i.ToString(),
            2 => v.c.ToString(),
            3 => v.f.ToString(),
            4 => v.s.ToString(),
            5 => v.b.ToString(),
            _ => v.i.ToString(),
        };

        valueText.text = value;
    }

    public void SetVarname()
    {
        this.varname = varnameText.text;
    }

    public void ParseString()
    {
        SetValue(valueText.text);
    }
}

