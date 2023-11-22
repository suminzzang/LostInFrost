using Photon.Pun;
using UnityEngine;

public class PlayerAudio : MonoBehaviourPunCallbacks
{
    // 오디오 클립 정의
    public AudioClip pickupSound;
    public AudioClip equipSound;
    public AudioClip hammerSound;
    public AudioClip axeWoodSound;
    public AudioClip pickAxeSound;
    public AudioClip brokenSound;
    public AudioClip swingSound;
    public AudioClip punchSound;
    public AudioClip hitSound;
    public AudioClip bowSound;
    public AudioClip hitArrow;
    public AudioClip gunSound;
    public AudioClip emptyGunSound;
    public AudioClip makeSound;
    public AudioClip walkSound;
    public AudioClip deadSound;
    public AudioClip eatSound;
    public float someMaxDistance = 20f;
    private PhotonView pv;

    public AudioSource audioSource;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    // 애니메이션 이벤트에 사용할 메서드
    public void PlaySoundByName(string clipName)
    {
        if (pv.IsMine)
        {
            PlaySound(GetAudioClipByName(clipName));
        }
    }

    // RPC를 호출하는 함수
    public void PlaySound(AudioClip clip)
    {
        // 캐릭터가 내 캐릭터이고 clip이 존재한다면
        if (pv.IsMine && clip != null)
        {
            // 나를 포함해서 모든 캐릭에게 RPC를 쏜다.
            pv.RPC("PlaySoundRPC", RpcTarget.All, pv.ViewID, clip.name);
        }
    }

    [PunRPC]
    void PlaySoundRPC(int viewID, string clipName)
    {
        // 소리를 재생할 지점을 카메라 위치로 받는다.
        Vector3 localPlayerPosition = Camera.main.transform.position;
        // 카메라 위치를 조정해주고
        localPlayerPosition.y = this.transform.parent.position.y;
        localPlayerPosition.z += 8f;
        //GameObject player = PhotonView.Find(viewID).gameObject;

        // 소리가 나는 위치와 메인 카메라 위치와의 거리를 게산해서
        float distance = Vector3.Distance(localPlayerPosition, this.transform.parent.position);

        // someMaxDistance 거리 이하이면
        if (distance < someMaxDistance) // 'someMaxDistance'는 최대 들리는 거리를 나타냄
        {
            // 볼륨을 조정한후
            float volume = CalculateVolumeBasedOnDistance(distance);
            // 클립을 불러와서
            AudioClip clip = GetAudioClipByName(clipName+"Sound");
            if (clip != null)
            {
                // 재생한다.
                audioSource.PlayOneShot(clip, volume);
            }
        }
    }

    // string으로 AudioClip을 반환하는 함수
    private AudioClip GetAudioClipByName(string clipName)
    {
        //PlaySoundByName
        switch (clipName)
        {
            case "pickupSound":
                return pickupSound;
            case "equipSound":
                return equipSound;
            // 다른 클립에 대한 처리...
            case "hammerSound":
                return hammerSound;
            case "axeWoodSound":
                return axeWoodSound;
            case "pickAxeSound":
                return pickAxeSound;
            case "brokenSound":
                return brokenSound;
            case "swingSound":
                return swingSound;
            case "punchSound":
                return punchSound;
            case "hitSound":
                return hitSound;
            case "bowSound":
                return bowSound;
            case "hitArrow":
                return hitArrow;
            case "gunSound":
                return gunSound;
            case "emptyGunSound":
                return emptyGunSound;
            case "makeSound":
                return makeSound;
            case "walkSound":
                return walkSound;
            case "eatSound":
                return eatSound;
            case "deadSound":
                return deadSound;
            default:
                return null;
        }
    }

    // 거리에 따라서 볼륨을 조절하는 함수
    private float CalculateVolumeBasedOnDistance(float distance)
    {
        // 예: 거리에 따라 0에서 1 사이의 볼륨을 반환
        return Mathf.Clamp(1 - distance / someMaxDistance, 0, 1);
    }

    // 이 아래 함수들은 위 함수들을 쓰기전에 이전에 쓰던 함수들
    public void PlayPickupSound()
    {
        pv.RPC("PlayPickupSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayPickupSoundRPC()
    {
        audioSource.PlayOneShot(pickupSound);
    }

    public void PlayerEquipSound()
    {
        pv.RPC("PlayerEquipSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayerEquipSoundRPC()
    {
        audioSource.PlayOneShot(equipSound);
    }

    public void PlayHammerSound()
    {
        pv.RPC("PlayHammerSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayHammerSoundRPC()
    {
        audioSource.PlayOneShot(hammerSound);
    }

    public void PlayAxeWoodSound()
    {
        pv.RPC("PlayAxeWoodSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayAxeWoodSoundRPC()
    {
        audioSource.PlayOneShot(axeWoodSound);
    }

    public void PlayPickAxeSound()
    {
        pv.RPC("PlayPickAxeSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayPickAxeSoundRPC()
    {
        audioSource.PlayOneShot(pickAxeSound);
    }

    public void PlayBrokenSound()
    {
        pv.RPC("PlayBrokenSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayBrokenSoundRPC()
    {
        audioSource.PlayOneShot(brokenSound);
    }

    public void PlaySwingSound()
    {
        pv.RPC("PlaySwingSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlaySwingSoundRPC()
    {
        audioSource.PlayOneShot(swingSound);
    }

    public void PlayPunchSound()
    {
        pv.RPC("PlayPunchSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayPunchSoundRPC()
    {
        audioSource.PlayOneShot(punchSound);
    }

    public void PlayHitSound()
    {
        pv.RPC("PlayHitSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayHitSoundRPC()
    {
        audioSource.PlayOneShot(hitSound);
    }

    public void PlayBowSound()
    {
        pv.RPC("PlayBowSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayBowSoundRPC()
    {
        audioSource.PlayOneShot(bowSound);
    }

    public void PlayHitArrow()
    {
        pv.RPC("PlayHitArrowRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayHitArrowRPC()
    {
        audioSource.PlayOneShot(hitArrow);
    }

    public void PlayGunSound()
    {
        pv.RPC("PlayGunSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayGunSoundRPC()
    {
        audioSource.PlayOneShot(gunSound);
    }

    public void PlayEmptyGunSound()
    {
        pv.RPC("PlayEmptyGunSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayEmptyGunSoundRPC()
    {
        audioSource.PlayOneShot(emptyGunSound);
    }

    public void PlayMakeSound()
    {
        pv.RPC("PlayMakeSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayMakeSoundRPC()
    {
        audioSource.PlayOneShot(makeSound);
    }
}
