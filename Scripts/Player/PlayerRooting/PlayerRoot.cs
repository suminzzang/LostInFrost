using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PlayerRoot : MonoBehaviour
{
    // 루팅할 수 있는 아이템 레이어
    private int canRootLayer;
    // 화살표 프리펩
    private GameObject arrowPrefab;
    // 캐릭터가 루팅할 수 있는 범위의 시작점
    public Vector3 rootPosition;
    // 캐릭터가 루팅할 수 있는 범위의 크기
    public Vector3 rootSize;
    // 캐릭터가 회전함에 따라서 이 범위도 같이 회전해야함
    public Quaternion rootRotation;
    // 중심이 캐릭터가 아니기 떄문에 새로 잡아줘야함
    private Vector3 rotatedRootPosition;
    public Animator anim;

    // Arrow 진자운동 변수들
    // 화살표 처음 위치
    private Vector3 arrowInitialPosition;
    // 진자운동 속도
    public float bobbingSpeed = 8f;
    // 진자운동 범위
    public float bobbingAmount = 0.2f;
    // 화살표가 활성화 되어있는지
    private bool isArrowActive = false;
    // 루팅할 수 있는 아이템 전,후 값 비교
    private GameObject previousNearest = null;

    private PhotonView pv;

    // Start is called before the first frame update
    void Start()
    {

        canRootLayer = 1 << LayerMask.NameToLayer("CanRoot");
        rootPosition = new Vector3(0, 0.5f, 0.3f);
        rootSize = new Vector3(0.8f, 1f, 1f);
        rootRotation = new Quaternion(0,0,0,1);
        arrowPrefab = Instantiate(Resources.Load<GameObject>("Arrow"));
        pv = GetComponent<PhotonView>();
        Debug.Log(arrowPrefab.transform);
        arrowPrefab.transform.position = new Vector3(0,100f,0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine) return;
        // 루팅 범위 박스의 중심점을 다시 잡아준다 -> 중심에 따라서 회전하기 때문에
        rotatedRootPosition = transform.rotation * rootPosition;
        // 박스로 범위 체크
        Collider[] colliders = Physics.OverlapBox(transform.position + rotatedRootPosition, rootSize, transform.rotation, canRootLayer);
        // 가장 가까운 오브젝트 선언
        Collider nearest = null;
        // 최소값을 초기화
        float minDistance = float.MaxValue;

        // collider를 순회하면서
        foreach (Collider col in colliders)
        {
            // 거리 체크후
            float distance = Vector3.Distance(transform.position, col.transform.position);
            // nearst를 최소값으로 갱신
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = col;
            }
        }

        // 가장 가까운 아이템이 존재한다면
        if (nearest != null)
        {
            // 화살표가 활성화가 안되어있거나 가장 가까운 아이템이 변경되었다면 
            if (!isArrowActive || (nearest.gameObject != previousNearest))
            {
                // 화살표 위치를 갱신
                arrowInitialPosition = nearest.transform.position + Vector3.up * 2.5f;
                // 화살표가 활성화 되었으므로 true로 변경
                isArrowActive = true;
            }
            // 진자운동 관련 코드
            float verticalMovement = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            arrowPrefab.transform.position = arrowInitialPosition + new Vector3(0, verticalMovement, 0);

            
            if (UserStatusManager.Instance.CanRoot && Input.GetKeyDown(KeyCode.F))
            {
                // 줍는 애니메이션
                anim.SetTrigger("Pickup");
                UserStatusManager.Instance.IsRoot = true;

                // 코루틴 시작
                StartCoroutine(ResetIsRootAfterDelay(1.0f));

                // 땅에 떨어진 아이템이의 정보를 불러와서
                ItemStatus itemStatus = nearest.GetComponent<ItemStatus>();
                
                // 인벤토리가 꽉찬 경우를 대비
                int remainingQuantity = 0;

                if (itemStatus != null) // 아이템 정보가 존재한다면
                {
                    // 아이템 정보를 불러와서
                    ItemData itemData = itemStatus.itemData;
                    if (itemData != null) // 아이템 정보가 존재한다면
                    {
                        if (itemData.itemType == ItemType.Tool) // 아이템이 도구라면
                        {
                            // 땅에 떨어진 도구의 ToolStatus 컴퍼넌트를 가져와서
                            ToolStatus toolStatus = nearest.GetComponent<ToolStatus>();
                            // 인벤토리에 더하고 남은 개수를 받는다.
                            remainingQuantity = InventoryManager.Instance.AddItem(itemData, itemStatus.itemQuantity, toolStatus.toolDurability);
                            if (remainingQuantity > 0) // 남은 개수가 존재한다면 인벤토리에 추가하지 못한 것이므로
                            {
                                // 인벤토리아이템을 새로 생성해서
                                InventoryItem temp = new InventoryItem(itemData, remainingQuantity, toolStatus.toolDurability);
                                // 다시 땅에 뿌려준다.
                                NetworkManager.Instance.DropTool(temp, transform.position);
                            }
                        }
                        else // 아이템이 도구를 제외한 나머지 종류라면
                        {
                            // 인벤토리에 더해주고 남은 개수를 받는다.
                            remainingQuantity = InventoryManager.Instance.AddItem(itemData, itemStatus.itemQuantity);
                            if (remainingQuantity > 0) // 남은 개수가 0개 이상이라면 인벤토리에 꽉찬 것이므로
                            {
                                // 객체를 새로 생성해서
                                InventoryItem temp = new InventoryItem(itemData, remainingQuantity);
                                // 다시 땅에 뿌린다.
                                NetworkManager.Instance.DropIngredient(temp, transform.position);
                            }
                        }
                    }
                }
                // 먹은템을 삭제 처리
                itemStatus.RequestDestroyItem();
            }
        }
        // 가장 가까운 아이템이 존재하지 않는다면
        else
        {
            // 화살표를 유저 눈에 안보이는 곳으로 보낸다
            arrowPrefab.transform.position = new Vector3(0, 100f, 0);
            // 화살표가 활성화 되어있지 않으므로 false
            isArrowActive = false;
        }

        // 최근 근접 아이템을 갱신
        previousNearest = nearest != null ? nearest.gameObject : null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position + rotatedRootPosition, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, rootSize);
    }

    private IEnumerator ResetIsRootAfterDelay(float delay)
    {
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(delay);

        // IsRoot 상태를 false로 변경
        UserStatusManager.Instance.IsRoot = false;
    }
}
