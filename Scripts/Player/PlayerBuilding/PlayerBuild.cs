using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    
    private BuildingData buildingData;
    private GameObject BuildObject;
    private BuildObjectCollision buildObjectCollisionScript;
    public Material grayMaterial;
    private Material[] originalMaterials;
    public Animator anim;
    private bool preBuildInitialized = false;
    // 건설 시작 지연을 위한 변수를 추가
    private bool waitForBuildStart = false;
    private float buildStartDelay = 0.1f; // 0.1초 지연
    private PhotonView pv;


    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        // 유저가 현재 조종하는 캐릭터가 아니라면 종료
        if (!pv.IsMine) return;
        // 유저가 현재 건설 대기 모드라면
        if(UserStatusManager.Instance.IsBuild)
        {
            if(!preBuildInitialized)
            {
                buildingData = BuildingManager.Instance.GetBuildingData(BuildingManager.Instance.NowBuilding);
                PreBuild(buildingData.buildingName);
                preBuildInitialized = true;
                waitForBuildStart = true; // 지연 시작
                StartCoroutine(DelayBuildStart());
            }
            // 건설위치에 대한 예비건물을 실시간 갱신해줘야함
            UpdatePreBuildPosition();

            if (!waitForBuildStart)
            {
                // 마우스 우클릭을 누르면
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    // 예비 건물을 파괴
                    Destroy(BuildObject);
                    // 건설대기 모드 취소
                    UserStatusManager.Instance.IsBuild = false;
                }

                // 마우스 좌클릭을 누르면
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    // 현재 예비건물에 겹치는게 없으면
                    if (!buildObjectCollisionScript.isColliding)
                    {
                        BuildingManager.Instance.NowBuild(BuildingManager.Instance.NowBuilding);
                        // 그 위치에 건물을 짓는다
                        BuildAtPosition(BuildObject.transform.position);
                        // 예비건물을 지우고
                        Destroy(BuildObject);
                        // 다른 스크립트가 참조하지 못하게 null처리
                        BuildObject = null;
                    }
                    else // 예비건물에 겹치는게 있으면 여기에 효과음이나 애니메이션을 넣는다.
                    {
                        Debug.Log("충돌충돌");
                    }
                }
            }
        }
        else
        {
            // 유저가 건설 대기 모드가 아니라면 플래그를 리셋
            if (preBuildInitialized)
            {
                preBuildInitialized = false;
            }
        }
    }

    // 건설 대기모드에서 예비건물을 불러오는 메서드
    private void PreBuild(string buildingName)
    {
        // 예비건물 프리펩을 Resouce폴더에서 불러와서
        GameObject preBuildPrefab = Resources.Load<GameObject>("Building/pre"+buildingName);
        if (preBuildPrefab) // 그 prefab에 건물이 있다면
        {
            // 생성
            BuildObject = Instantiate(preBuildPrefab, GetBuildPosition(), Quaternion.Euler(0, buildingData.rotation, 0));
            // 예비건물에 달린 충돌 감지 스크립트를 불러온다.
            buildObjectCollisionScript = BuildObject.GetComponent<BuildObjectCollision>();
        }
    }

    // 건물위치에 대한 메서드
    private Vector3 GetBuildPosition()
    {
        // 캐릭터의 위치를 불러오고
        Transform playerTrnasform = gameObject.transform;
        // 빌딩 데이터에서 가져온 건물과 사람에 대한 거리를 불러와서
        float distanceFromPlayer = buildingData.distance;

        // 조합
        Vector3 buildPosition = playerTrnasform.position + playerTrnasform.forward * distanceFromPlayer;

        return buildPosition;
    }

    // 예비 건물에 대한 위치를 갱신하는 메서드
    private void UpdatePreBuildPosition()
    {
        // 현재 예비건물이 있다면
        if (BuildObject)
        {
            // 그 예비건물의 다음 위치를 불러와서
            Vector3 targetPosition = GetBuildPosition();
            // 이동시킨다.
            BuildObject.transform.position = Vector3.Lerp(BuildObject.transform.position, targetPosition, Time.deltaTime * 5);

        }
    }
    // 예비건물을 짓는 메서드
    private void BuildAtPosition(Vector3 position)
    {
        UserStatusManager.Instance.IsBuild = false;
        UserStatusManager.Instance.IsBuilding = true;
        // NetworkManager를 통해 건물 건설 요청
        NetworkManager.Instance.BuildAtPosition(buildingData.buildingName, position, buildingData.rotation, buildingData.buildingTime);
        StartCoroutine(RestoreOriginalMaterialAfterDelay(buildingData.buildingTime));
    }

    // 건축 시간을 주는 코루틴
    private IEnumerator RestoreOriginalMaterialAfterDelay(float delay)
    {
        // 지정된 지연 시간 동안 기다린다
        yield return new WaitForSeconds(delay);

        anim.SetBool("Building", false);
        // 건설모드 취소
        UserStatusManager.Instance.IsBuilding = false;
        
    }
    private IEnumerator DelayBuildStart()
    {
        yield return new WaitForSeconds(buildStartDelay);
        waitForBuildStart = false; // 지연 종료
    }
}
