using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Polyperfect.Crafting.Framework;
using UnityEngine.XR;



public class NetworkManager : MonoBehaviourPunCallbacks
{
    private PhotonView pv;
    public Material grayMaterial;
    private Material[] originalMaterials;

    public Transform inventoryParent;
    public Transform buildingParent;

    [SerializeField]
    public Dictionary<string,Queue<GameObject>> inventoryPools = new Dictionary<string, Queue<GameObject>>();

    [SerializeField]
    public Dictionary<string, Queue<GameObject>> buildingPools = new Dictionary<string, Queue<GameObject>>();

    public static NetworkManager Instance { get; private set; }

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
    }

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        InitializeIngredientPools();
    }

    // 오브젝트 풀링 초기화 함수
    private void InitializeIngredientPools()
    {
        // 인벤토리 parent의 자식 오브젝트들을 순회하면서
        foreach (Transform child in inventoryParent)
        {
            // gameobject를 선언하고
            GameObject ingredient = child.gameObject;
            string ingredientName = ingredient.name.Split('(')[0].Trim(); // "wood", "bone", "branch" 등으로 이름 설정

            // Dictionary에 key가 없다면
            if (!inventoryPools.ContainsKey(ingredientName))
            {
                // key를 넣어준다.
                inventoryPools[ingredientName] = new Queue<GameObject>();
            }
            ingredient.SetActive(false); // 처음에는 비활성화 상태로 설정
            // queue에 오브젝트들을 넣어둔다.
            inventoryPools[ingredientName].Enqueue(ingredient);
        }

        // 위와 동일
        foreach (Transform child in buildingParent)
        {
            GameObject building = child.gameObject;
            string buildingName = building.name.Split('(')[0].Trim(); // "wood", "bone", "branch" 등으로 이름 설정

            if (!buildingPools.ContainsKey(buildingName))
            {
                buildingPools[buildingName] = new Queue<GameObject>();
            }

            building.SetActive(false); // 처음에는 비활성화 상태로 설정
            buildingPools[buildingName].Enqueue(building);
        }
    }

    public void DropIngredient(IngredientAmount ingredient, Vector3 dropPosition)
    {
        string ingredientName = ingredient.ingredient.itemName;
        int ingredientCount = ingredient.count;

        // 템을 떨어트렸다는걸 모든플레이어에게 RPC를 쏜다.
        pv.RPC("RpcDropIngredient", RpcTarget.All, ingredientName, ingredientCount, dropPosition);
    }

    public void DropIngredient(InventoryItem ingredient, Vector3 dropPosition)
    {
        string ingredientName = ingredient.itemData.itemName;
        int ingredientCount = ingredient.quantity;

        // 템을 떨어트렸다는걸 모든플레이어에게 RPC를 쏜다.
        pv.RPC("RpcDropIngredient", RpcTarget.All, ingredientName, ingredientCount, dropPosition);
    }

    [PunRPC]
    public void InventoryPoolsEnque(int viewID,string itemName)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        GameObject inventoryItem = targetView.gameObject;
        inventoryItem.transform.SetParent(inventoryParent);
        inventoryItem.SetActive(false);
        Instance.inventoryPools[itemName].Enqueue(inventoryItem);
    }

    // 아이템을 떨어트리는 RPC함수
    [PunRPC]
    public void RpcDropIngredient(string ingredientName,int ingredientCount, Vector3 dropPosition)
    {

        // pool에 키가 존재하지 않거나 비어있다면
        if (!inventoryPools.ContainsKey(ingredientName) || inventoryPools[ingredientName].Count == 0)
        {
            // 이 방의 주인이 아닐경우에는 함수 종료
            if (!pv.IsMine) return;

            Debug.Log($"{ingredientName}가 부족합니다.");

            // 오브젝트의 프리펩의 경로를 설정하고
            string ingredientPrefabPath = "Ingredient/" + ingredientName;
            // 오브젝트를 생성한다.
            GameObject ingredientObj = PhotonNetwork.InstantiateRoomObject(ingredientPrefabPath, new Vector3(0,100,0), Quaternion.identity);
            // 오브젝트의 ViewId를 받아서
            int viewId = ingredientObj.GetComponent<PhotonView>().ViewID;
            // Queue에 넣어준다.
            pv.RPC("InventoryPoolsEnque",RpcTarget.All, viewId, ingredientName);
        }
        // 템이 떨어질 지점을 구하고
        Vector3 temp = new Vector3(0, -1, 1);
        // queue에서 하나 뺸다음에
        GameObject ingredient = inventoryPools[ingredientName].Dequeue();
        // 값 설정을 해준다.
        ingredient.SetActive(true);
        ingredient.transform.SetParent(null);
        ingredient.transform.position = dropPosition - temp;
        ItemStatus itemStatus = ingredient.GetComponent<ItemStatus>();
        itemStatus.parent = inventoryParent;
        IngredientStatus ingredientStatus = ingredient.GetComponent<IngredientStatus>();

        if (itemStatus != null)
        {
            itemStatus.itemQuantity = ingredientCount;
        }
        if (ingredientStatus != null)
        {
            ingredientStatus.ingredientQuantity = ingredientCount;
        }
    }


    public void DropTool(InventoryItem tool, Vector3 dropPosition)
    {
        string toolName = tool.itemData.itemName;
        int toolDurability = tool.itemDurability;

        // 이제 ingredientNames와 ingredientCounts를 Photon을 통해 전송하거나 처리합니다.
        pv.RPC("RpcDropTool", RpcTarget.All, toolName, toolDurability, dropPosition);
    }

    [PunRPC]
    public void RpcDropTool(string toolName, int toolDurability, Vector3 dropPosition)
    {

        if (!inventoryPools.ContainsKey(toolName) || inventoryPools[toolName].Count == 0)
        {
            if (!pv.IsMine) return;

            Debug.Log($"{toolName}가 부족합니다.");

            string toolPrefabPath = "Ingredient_Tool/" + toolName;
            GameObject toolObj = PhotonNetwork.InstantiateRoomObject(toolPrefabPath, new Vector3(0, 100, 0), Quaternion.identity);
            int viewId = toolObj.GetComponent<PhotonView>().ViewID;
            pv.RPC("InventoryPoolsEnque", RpcTarget.All, viewId, toolName);
        }
        Vector3 temp = new Vector3(0, -1, 1);
        GameObject tool = inventoryPools[toolName].Dequeue();
        tool.SetActive(true);
        tool.transform.SetParent(null);
        tool.transform.position = dropPosition - temp;
        ItemStatus itemStatus = tool.GetComponent<ItemStatus>();
        itemStatus.parent = inventoryParent;
        ToolStatus toolStatus = tool.GetComponent<ToolStatus>();

        if (itemStatus != null)
        {
            itemStatus.itemQuantity = 1;
        }
        if (toolStatus != null)
        {
            toolStatus.toolDurability = toolDurability;
        }
    }

    public void DropConsumable(InventoryItem consumable, Vector3 dropPosition)
    {
        string consumablelName = consumable.itemData.itemName;
        int consumableCount = consumable.quantity;

        // 이제 ingredientNames와 ingredientCounts를 Photon을 통해 전송하거나 처리합니다.
        pv.RPC("RpcDropConsumable", RpcTarget.All, consumablelName, consumableCount, dropPosition);
    }

    [PunRPC]
    public void RpcDropConsumable(string consumableName, int consumableCount, Vector3 dropPosition)
    {
        if (!inventoryPools.ContainsKey(consumableName) || inventoryPools[consumableName].Count == 0)
        {
            if (!pv.IsMine) return;

            Debug.Log($"{consumableName}가 부족합니다.");

            string consumablePrefabPath = "Ingredient_Consumable/" + consumableName;
            GameObject consumableObj = PhotonNetwork.InstantiateRoomObject(consumablePrefabPath, new Vector3(0, 100, 0), Quaternion.identity);
            int viewId = consumableObj.GetComponent<PhotonView>().ViewID;
            pv.RPC("InventoryPoolsEnque", RpcTarget.All, viewId, consumableName);
        }
        Vector3 temp = new Vector3(0, -1, 1);
        GameObject consumable = inventoryPools[consumableName].Dequeue();
        consumable.SetActive(true);
        consumable.transform.SetParent(null);
        consumable.transform.position = dropPosition - temp;
        ItemStatus itemStatus = consumable.GetComponent<ItemStatus>();
        itemStatus.parent = inventoryParent;
        ConsumableStatus consumableStatus = consumable.GetComponent<ConsumableStatus>();
        Rigidbody rb = consumable.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (itemStatus != null)
        {
            itemStatus.itemQuantity = consumableCount;
        }
        if (consumableStatus != null)
        {
            consumableStatus.consumableQuantity = consumableCount;
        }
    }

    [PunRPC]
    public void ReturnInventoryPools(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        GameObject inventoryItem = targetView.gameObject;
        inventoryItem.transform.position = new Vector3(0f, 100f, 0f);
        inventoryItem.SetActive(false);
        inventoryItem.transform.SetParent(inventoryParent);
        string itemName = inventoryItem.GetComponent<ItemStatus>().itemName;
        inventoryPools[itemName].Enqueue(inventoryItem);
    }

    // 건물을 생성하는 메서드
    public void BuildAtPosition(string buildingName, Vector3 position, float rotation, float buildingTime)
    {
        pv.RPC("RpcBuildAtPosition", RpcTarget.All, buildingName, position, rotation, buildingTime);
    }

    [PunRPC]
    public void BuildingPoolsEnque(int viewID, string buildingName)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        GameObject building = targetView.gameObject;
        building.transform.SetParent(buildingParent);
        building.SetActive(false);
        Instance.buildingPools[buildingName].Enqueue(building);
    }

    [PunRPC]
    public void RpcBuildAtPosition(string buildingName, Vector3 position, float rotation, float buildingTime)
    {
        GameObject buildingObj = null;
        if (!buildingPools.ContainsKey(buildingName) || buildingPools[buildingName].Count == 0)
        {
            if (!pv.IsMine) return;
            string buildingPrefabPath = "Building/" + buildingName;
            buildingObj = PhotonNetwork.InstantiateRoomObject(buildingPrefabPath, new Vector3(0, 100, 0), Quaternion.identity);
            int viewId = buildingObj.GetComponent<PhotonView>().ViewID;
            pv.RPC("BuildingPoolsEnque", RpcTarget.All, viewId, buildingName);
        }
        buildingObj = buildingPools[buildingName].Dequeue();
        buildingObj.SetActive(true);
        buildingObj.transform.SetParent(null);
        buildingObj.transform.position = position;
        buildingObj.transform.rotation = Quaternion.Euler(0, rotation, 0);

        BuildingStatus buildingStatus = buildingObj.GetComponent<BuildingStatus>();
        if (buildingStatus != null)
        {
            buildingStatus.isInteractive = false; // 건물이 완성될 때까지 상호작용 금지
        }

        Renderer buildingMaterials = buildingObj.GetComponent<Renderer>();
        Debug.Log(buildingMaterials);
        if (buildingMaterials != null)
        {
            Material[] materials = buildingMaterials.materials;
            originalMaterials = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                originalMaterials[i] = materials[i];
                materials[i] = grayMaterial;
            }
            buildingMaterials.materials = materials;
            // 건설 완료 후 원래 재질로 복구
            StartCoroutine(RestoreOriginalMaterialAfterDelay(buildingObj, buildingMaterials, originalMaterials, buildingTime));
        }
    }

    [PunRPC]
    public void ReturnBuildingPools(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        GameObject bulding = targetView.gameObject;
        bulding.transform.position = new Vector3(0f, 100f, 0f);
        bulding.SetActive(false);
        bulding.transform.SetParent(buildingParent);
        string buildingName = bulding.GetComponent<BuildingStatus>().buildingData.buildingName;
        buildingPools[buildingName].Enqueue(bulding);
    }

    private IEnumerator RestoreOriginalMaterialAfterDelay(GameObject building, Renderer renderer, Material[] originalMaterials, float delay)
    {
        yield return new WaitForSeconds(delay);

        renderer.materials = originalMaterials;
        building.GetComponent<BuildingStatus>().isInteractive = true; // 상호작용 활성화
    }
}
