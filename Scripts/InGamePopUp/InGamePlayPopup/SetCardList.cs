using System.Collections;
using System.Collections.Generic;
using UltimateClean;
using Unity.VisualScripting;
using UnityEngine;

public class SetCardList : MonoBehaviour
{
    public Transform[] cardContainers;
    private int nowAvailableCards;
    private int preAvailableCards;
    Popup popup;
    [SerializeField]
    List<GameObject> cardPrefabs;
    [SerializeField]
    private GameObject[] normalCardPrefabs;
    [SerializeField]
    private GameObject[] epicCardPrefabs;
    [SerializeField]
    private GameObject[] uniqueCardPrefabs;
    [SerializeField]
    private GameObject[] legendCardPrefabs;

    private void Awake()
    {
        cardPrefabs = new List<GameObject>();
        LoadCards();
    }

    void Start()
    {
        DisplayCards();
        // 창을 닫기 위해 현재 카드 선택 갯수를 저장
        nowAvailableCards = InGameManager.Instance.AvailableCards;
        preAvailableCards = InGameManager.Instance.AvailableCards;
        // 외부 asset GUIPack을 사용하기 위해 선언
        popup = GetComponent<Popup>();
    }

    private void Update()
    {
        // CardEffects.cs에서 값이 변경되어서
        nowAvailableCards = InGameManager.Instance.AvailableCards;
        //  now가 pre 보다 하나 작아진다면 popup을 닫는다
        if (nowAvailableCards ==  preAvailableCards - 1)
        {
            popup.Close();
        }
    }

    private void LoadCards()
    {
        // CardList 폴더에서 모든 프리팹을 불러오기
        normalCardPrefabs = Resources.LoadAll<GameObject>("CardListNormal"); // 75%
        epicCardPrefabs = Resources.LoadAll<GameObject>("CardListEpic");     // 20%
        uniqueCardPrefabs = Resources.LoadAll<GameObject>("CardListUnique"); // 4%
        legendCardPrefabs = Resources.LoadAll<GameObject>("CardListLegend"); // 1%
        cardPrefabs.AddRange(normalCardPrefabs);
        cardPrefabs.AddRange(epicCardPrefabs);
        cardPrefabs.AddRange(uniqueCardPrefabs);
        cardPrefabs.AddRange(legendCardPrefabs);
    }

    void DisplayCards()
    {
        if (cardPrefabs.Count < 3)
        {
            Debug.LogError("카드가 3개보다 적습니다.");
            return;
        }

        // 중복 제거 3개 선택
        List<int> selectedIndexes = GetRandomUniqueIndexes(cardPrefabs.Count, 3);

        // 각 컨테이너에 카드 배치
        for (int i = 0; i < cardContainers.Length; i++)
        {
            PlaceCardInContainer(cardContainers[i], cardPrefabs[selectedIndexes[i]]);
        }
    }

    List<int> GetRandomUniqueIndexes(int maxIndex, int count)
    {
        // 중복 제거 카드 선택 알고리즘
        List<int> indexes = new List<int>();
        for (int i = 0; i < maxIndex; i++)
        {
            indexes.Add(i);
        }

        List<int> result = new List<int>();
        while (count > 0 && indexes.Count > 0)
        {
            int percentage = Random.Range(0, 100);
            int randomIndex = Random.Range(0, indexes.Count);
            result.Add(indexes[randomIndex]);
            indexes.RemoveAt(randomIndex);
            count--;
        }

        return result;
    }

    void PlaceCardInContainer(Transform container, GameObject selectedCard)
    {
        // 카드 프리팹을 복제하여 컨테이너에 표시
        GameObject cardInstance = Instantiate(selectedCard, container);
    }
}