using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 게임의 진행 관한 매니저
// 현재 게임 진행 시간, 게임 종료, 게임 시작, 낮 밤, 웨이브 기타 등등
public class InGameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static InGameManager Instance { get; private set; }
    public GameObject player;
    public TextMeshProUGUI playStatusText;
    public TextMeshProUGUI playTimeText;
    public float endTime = 120.0f;
    public float escapteTime = 180.0f;
    public float distanceThreshold = 10.0f;

    public IngredientData meat;
    public ToolData gun;
    public ConsumableData bullet;

    public GameObject helicopter;

    private bool onHelicopter = false;
    // public Transform helicopterSpawnPoint;

    AudioSource helicopterSound; // 헬기소리
    private bool ishelicopterComming = false; // 헬기가 왔느냐
    public float fadeInDuration = 1.0f; // 서서히 fadeIn이 완료되는 데 걸리는 시간
    public TextMeshProUGUI InfoText; // 인게임 안내사항 텍스트

    public GameObject loading; // 로딩창

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        playTimeText.text = "0";
        isTriggeredWithHelicopter = false;
    }
    private void Start()
    {
        helicopterSound = GetComponentsInChildren<AudioSource>()[0];
        Invoke("StartDelay", 5.0f);
    }

    private float playTime;
    public float PlayTime
    {
        get { return playTime; }
        //set { PlayTime = value; }
    }

    public bool isTriggeredWithHelicopter;
    public bool IsTriggeredWithHelicopter
    {
        get { return isTriggeredWithHelicopter; }
        set { isTriggeredWithHelicopter = value; }
    }

    private int availableCards;
    public int AvailableCards
    {
        get { return availableCards; }
        set { availableCards = value; }
    }

    // Update is called once per frame
    void Update()
    {
        playTime += 1 * Time.deltaTime;
        int minutes = Mathf.FloorToInt(playTime / 60);
        int seconds = Mathf.FloorToInt(playTime % 60);

        // 시간을 "분:초" 형식으로 표시
        if (playTime < endTime)
        {
            playTimeText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }
        else if (playTime >= endTime && !onHelicopter)
        {
            onHelicopter = true;
            playTimeText.text = "구조대 도착!!";
            Instantiate(helicopter);
            while (true)
            {
                if (ObjectCheck(helicopter))
                {
                    break;
                }
            }

            // helicopter.transform.position = helicopterSpawnPoint.position;
            // 이후 헬리콥터에서 충돌 판정 확인함
        }
        else if (onHelicopter)
        {
            escapteTime -= 1 * Time.deltaTime;
            int leftMinutes = Mathf.FloorToInt(escapteTime / 60);
            int leftSeconds = Mathf.FloorToInt(escapteTime % 60);
            playStatusText.text = "남은 시간 : ";
            playTimeText.text = leftMinutes.ToString("00") + ":" + leftSeconds.ToString("00");
        }

        //헬기 도착 10초 전에 헬기소리 on
        if (!ishelicopterComming && playTime >= endTime - 10.0f) 
        {
            InfoText.text = "잠시 후 구조대가 도착합니다!!";
            ishelicopterComming = true;
            helicopterSound.Play();
            Invoke("PlayHelicopterSound", 7.0f);
        }
    }
    private bool ObjectCheck(GameObject hObject)
    {
        // 리스폰 지역 랜덤 할당
        int randomX = Random.Range(-170, 22);
        int randomZ = Random.Range(-120, 102);
        Vector3 randomVector = new Vector3(randomX, 0, randomZ);
        // 헬리콥터 크기 만큼 콜라이더 탐색 후 콜라이더 배열에 할당
        Collider[] colliders = Physics.OverlapBox(randomVector, hObject.GetComponent<BoxCollider>().size);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
            {
                return false;
            }
        }

        hObject.transform.position = randomVector;
        return true;
    }

    public void PlayHelicopterSound()
    {
        StartCoroutine(FadeOutSoundVolume());
    }
    IEnumerator FadeOutSoundVolume()
    {
        float elapsed = 0.0f;
        float originalVolume = helicopterSound.volume;

        while (elapsed < fadeInDuration)
        {
            float targetVolume = Mathf.Lerp(originalVolume, 0f, elapsed / fadeInDuration);
            helicopterSound.volume = targetVolume;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // fadeOut 완료 후 필요한 작업 수행
        // 헬기 도착 안내 문구.
        InfoText.text = "구조대가 도착하였습니다. 서둘러 이동해주세요!";
    }

    public void StartDelay()
    {
        loading.SetActive(false);
        UserStatusManager.Instance.resetValues();
    }
}
