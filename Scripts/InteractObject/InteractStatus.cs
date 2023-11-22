using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractStatus : MonoBehaviourPunCallbacks
{
    public InteractData interactData;
    private string interactName;
    private string interactExplanation;
    public int interactHp;
    // 이 물체가 가지고 있는 재료
    private List<IngredientAmount> interactIngredients;
    // 재료를 떨어트리는 스크립트
    private bool hasDropped = false; // 재료가 떨어졌는지의 상태를 저장할 변수
    public PhotonView pv;
    private WoodFallAnimation fall;

    // 나무는 직접 데이터를 건드리면 안되기 때문에 따로 빼서 처리
    void Awake()
    {
        interactName = interactData.InteractName;
        interactExplanation = interactData.InteractExplanation;
        interactHp = interactData.InteractHp;
        interactIngredients = interactData.ingredientList;
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        fall = GetComponent<WoodFallAnimation>();
    }

    void OnEnable()
    {
        if (pv.IsMine)
        {
            pv.RPC("RPCRespawn", RpcTarget.All);
        }
    }

    private void CheckInteractStatus()
    {
        if (interactHp <= 0 && !hasDropped && pv.IsMine)
        {
            Vector3 temp = new Vector3(0, 0, 1);
            for (int a=0; a < interactData.ingredientList.Count; a++)
            {
                if (interactData.ingredientList[a] == null) continue;
                NetworkManager.Instance.DropIngredient(interactData.ingredientList[a], transform.position - temp);
                temp -= new Vector3(1, 0, 0);
            }
            hasDropped = true;
            if (!gameObject.CompareTag("Tree"))
            {
                StartCoroutine(DestroyObject(gameObject));
            }

        }
        else if (interactHp > 0 && hasDropped)
        {
            hasDropped = false;
        }
    }

    [PunRPC]
    private void CheckObject(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView.gameObject.CompareTag("Tree"))
        {
            InteractManager.Instance.ReturnTree(gameObject);
        } 
        else if (targetView.gameObject.CompareTag("Rock"))
        {
            InteractManager.Instance.ReturnRock(gameObject);
        }
        else if (targetView.gameObject.CompareTag("Iron"))
        {
            InteractManager.Instance.ReturnIron(gameObject);
        }
        else if (targetView.gameObject.CompareTag("Brimstone"))
        {
            InteractManager.Instance.ReturnBrimstone(gameObject);
        }
    }
    
    private IEnumerator DestroyObject(GameObject gameObject)
    {
        if (pv.IsMine)
        {
            pv.RPC("RPCDestroy", RpcTarget.All);
        }
        while (true)
        {
            // 3초 뒤 시체 사라짐
            yield return new WaitForSeconds(3);
            if (pv.IsMine)
            {
                pv.RPC("CheckObject", RpcTarget.All, gameObject.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    [PunRPC]
    public void GetDamage(int damage, float characterRotation)
    {
        interactHp -= damage;
        if (interactHp <= 0 && !hasDropped)
        {
            if (fall != null)
            {
                fall.StartFallingAnimation(characterRotation);
            }
        }
        CheckInteractStatus();
    }

    [PunRPC]
    public void RPCInteractSetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    [PunRPC]
    public void RPCInteractSetTransform(Vector3 vector, Quaternion rotation)
    {
        transform.position = vector;
        transform.rotation = rotation;
    }
    
    [PunRPC]
    public void RPCDestroy()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, -5, transform.position.z), 2f);
    }
    
    [PunRPC]
    public void RPCRespawn()
    {
        transform.position = Vector3.Lerp(new Vector3(transform.position.x, -5, transform.position.z), new Vector3(transform.position.x, 0, transform.position.z), 2f);
        interactHp = interactData.InteractHp;
    }

}
