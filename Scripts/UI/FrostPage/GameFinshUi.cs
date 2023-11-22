using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFinshUi : MonoBehaviour
{
    public static GameFinshUi instance = null;
    public static GameFinshUi Instance
    {
        get { return instance; }
    }
    private bool isFinish = false;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private bool isFail = false;
    private void Update()
    {
        // 유저가 죽었을때
        if (!isFail && (int) UserStatusManager.Instance.HP <=0)
        {
            FailFinish();
        }

        // 타임오바 됬을때 : 헬게탑승 실패
        if (InGameManager.Instance.escapteTime <= 0f)
        {
            TimeOverFinish();
        }

    }

    public GameObject clearUi;
    public GameObject failUi;
    public GameObject timeOverUi;
    public GameObject infoText; // 안내 텍스트

    // 헬기 탑승 피니쉬
    public void ClearFinish()
    {
        if(!isFinish)
        {
            isFinish = true;
            clearUi.SetActive(true);
            infoText.SetActive(false);
        }
    }

    // 게임오버(사망)시 피니쉬
    public void FailFinish()
    {
        if (!isFinish)
        {
            isFinish = true;
            isFail = true;
            StartCoroutine(finishdelay());

            failUi.SetActive(true);
            infoText.SetActive(false);
        }
    }

    // 타입오버(헬기탑승x) 피니쉬
    public void TimeOverFinish()
    {
        if (!isFinish)
        {
            isFinish = true;
            timeOverUi.SetActive(true);
            infoText.SetActive(false);
        }

    }
    public IEnumerator finishdelay()
    {
        yield return new WaitForSeconds(2f);
    }
}
