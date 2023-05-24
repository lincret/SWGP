using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageInfo : MonoBehaviour
{
    public Text Num, State, Diff;
    public Stage stg;
    public Button btn;

    public void SetStageInfo(Stage s, StageProgress p)
    {
        Num.text = string.Format("Stage {0}", s.num);
        State.text = p.cleared ? "CLEARED" : string.Empty;
        Diff.text = s.GetDifficulty();

        stg = s;
        btn.onClick.AddListener(() => {
            Refs.stage = s;
            SceneManager.LoadScene("MainCodeStage");
        });
    }
}
