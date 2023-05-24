using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

[Serializable]
public class StageData
{
    public List<Stage> stages = new List<Stage>();

    public void GetStagesData()
    {
        TextAsset itemAsset = (TextAsset)Resources.Load("codestxt/StageInfo");
        System.IO.StringReader reader = new System.IO.StringReader(itemAsset.text);
        string jsonString = reader.ReadToEnd();
        stages = JsonUtility.FromJson<StageData>(jsonString).stages;
    }
}

[Serializable]
public class Stage
{
    public int num;
    public int diff;
    public string question;

    public string GetDifficulty()
    {
        return diff switch
        {
            1 => "¡Ú",
            2 => "¡Ú¡Ú",
            3 => "¡Ú¡Ú¡Ú",
            4 => "¡Ú¡Ú¡Ú¡Ú",
            5 => "¡Ú¡Ú¡Ú¡Ú¡Ú",
            _ => "???",
        };
    }
}


[Serializable]
public class StageProgress
{
    public int num;
    public bool cleared;

    public StageProgress(int num)
    {
        this.num = num;
        cleared = false;
    }

    public void SetCleared()
    {
        cleared = true;
    }
}


[Serializable]
public class LocalSavedDt
{
    public List<StageProgress> progress;

    public void SetNewLocalDt(StageData sdata)
    {
        progress = new List<StageProgress>();

        for (int i = 0; i < sdata.stages.Count; i++)
        {
            progress.Add(new StageProgress(sdata.stages[i].num));
        }

        SaveLocalData();
    }

    public void LoadLocalData(StageData sdata)
    {
        string filepath = string.Format("{0}{1}", Application.persistentDataPath, "/locdt.dat");

        if (File.Exists(filepath))
        {
            try
            {
                string code = File.ReadAllText(filepath);

                byte[] bytes = Convert.FromBase64String(code);
                string FromJsonData = Encoding.UTF8.GetString(bytes);
                LocalSavedDt lsd = JsonUtility.FromJson<LocalSavedDt>(FromJsonData);

                progress = lsd.progress;
            }
            catch (Exception e)
            {
                Debug.Log(e);

                return;
            }
        }
        else
        {
            SetNewLocalDt(sdata);
        }
    }

    public void SaveLocalData()
    {
        try
        {
            string ToJsonData = JsonUtility.ToJson(this);
            byte[] bytes = Encoding.UTF8.GetBytes(ToJsonData);
            string code = Convert.ToBase64String(bytes);
            string filepath = string.Format("{0}{1}", Application.persistentDataPath, "/locdt.dat");
            File.WriteAllText(filepath, code);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }
    }
}
