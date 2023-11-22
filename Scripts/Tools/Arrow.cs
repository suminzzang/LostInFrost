using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasHit;
    public PlayerAudio arrowAudio;
    public int arrowDamage = 30;
    private PhotonView pv;
    public bool isShoot;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        hasHit = false;
        pv = GetComponent<PhotonView>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasHit && isShoot)
        {
            int animalLayer = LayerMask.NameToLayer("Animal");
            int buildingLayer = LayerMask.NameToLayer("Building");
            int playerLayer = LayerMask.NameToLayer("Player");

            // 충돌 발생 체크
            hasHit = true;
            // 물리 상호작용 정지
            rb.isKinematic = true;

            // 화살을 부딪힌 객체에 고정
            transform.parent = collision.transform;

            // 혹시 몰라서 중력 작용 x
            rb.useGravity = false;

            // 혹시 몰라서 콜라이더 x
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // 화살이 대상에 박힌 것처럼 보이도록 조정합니다.
            // 충돌 지점에서 가장 가까운 점을 찾습니다.
            ContactPoint contact = collision.contacts[0];

            // 화살을 충돌 지점으로 이동시킵니다.
            transform.position = contact.point;

            // 화살이 일정 깊이로 대상에 박히도록 위치를 조정합니다.
            // 여기서는 화살의 끝부분이 대상 안으로 0.1 유닛 들어가도록 설정합니다.
            transform.position -= transform.forward * 0.1f;
            PlaySound(20f);
            PhotonView targetPhotonView = collision.gameObject.GetPhotonView();

            if (!pv.IsMine) return;
            // 데미지 처리
            if (collision.gameObject.layer == animalLayer)
            {
                Debug.Log("화살동물");
                targetPhotonView.RPC("GetDamage", RpcTarget.All, arrowDamage);
            }
            else if (collision.gameObject.layer == buildingLayer)
            {
                Debug.Log("화살빌딩");
                targetPhotonView.RPC("GetDamage", RpcTarget.All, arrowDamage);
            }
            else if (collision.gameObject.layer == playerLayer && !collision.gameObject.GetComponent<PhotonView>().IsMine)
            {
                // 유저 status 스크립트가 필요함
                targetPhotonView.RPC("GetDamage", RpcTarget.All, arrowDamage);
            }

        }
    }

    public void PlaySound(float maxDistance, float volume = 1.0f) // volume 파라미터를 추가합니다.
    {
        // 최대거리보다 거리가 더 길면 소리 안들리게하는 조건문
        if (maxDistance > 0)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, transform.position);

            if (distance > maxDistance)
            {
                return;
            }
        }

        // 볼륨을 설정합니다.
        arrowAudio.PlayHitArrow();
    }
}
