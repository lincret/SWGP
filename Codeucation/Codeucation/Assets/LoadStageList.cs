using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadStageList : MonoBehaviour
{
    public Transform content;
    StageData sLoad = new StageData();
    LocalSavedDt localDt = new LocalSavedDt();

    void Start()
    {
        SetStageListTransforms();
        Refs.InitRefs();
    }

    void SetStageListTransforms()
    {
        sLoad.GetStagesData();
        localDt.LoadLocalData(sLoad);

        Transform src = Resources.Load<Transform>("Prefabs/Stage");

        for (int i = 0; i < sLoad.stages.Count && i < localDt.progress.Count; i++)
        {
            Transform tmp = Instantiate(src, content);
            StageInfo sInfo = tmp.GetComponent<StageInfo>();
            sInfo.SetStageInfo(sLoad.stages[i], localDt.progress[i]);
        }
    }
}
