using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    private PhotonView pv;
    public Animator anim;
    public float hp;
    private void Start()
    {
        pv = GetComponent<PhotonView>();
        hp = 100;
        if (!pv.IsMine) return;
        UserStatusManager.Instance.anim = this.anim;
        PhotonManagerFrost.instance.Player = gameObject;
    }

    private void Update()
    {
        if (!pv.IsMine) return;
        MySetHp();
        if (hp<=(int) UserStatusManager.Instance.MinHP)
        {
            UserStatusManager.Instance.IsDead = true;
        }
    }

    public void MySetHp()
    {
        pv.RPC("SetHP", RpcTarget.All);
    }

    [PunRPC]
    public void SetHP()
    {
        hp = UserStatusManager.Instance.HP;
    }

    [PunRPC]
    public void GetDamage(int damage)
    {
        if (!pv.IsMine) return;
        UserStatusManager.Instance.HP -= damage;
    }

    [PunRPC]

    public void SetCollider(bool isColiider)
    {
        gameObject.GetComponent<Collider>().enabled = isColiider;
    }
}
