using ECM.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using UltimateClean;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardEffects : MonoBehaviour
{
    private GameObject player;
    private IngredientData meat;
    private ToolData gun;
    private ConsumableData bullet;
    private void Awake()
    {
        player = GameObject.Find("ECM_EthanPlatformer");
    }

    private void Start()
    {
        meat = InGameManager.Instance.meat;
        gun = InGameManager.Instance.gun;
        bullet = InGameManager.Instance.bullet;
    }

    public void ColdControll01()
    {
        // 추위 관리 추위 증가량 10% 감소
        UserStatusManager.Instance.ColdChangeAmounts = UserStatusManager.Instance.ColdChangeAmounts * 0.9f;
        InGameManager.Instance.AvailableCards -= 1;
    }
    public void ColdControll02()
    {
        // 추위 관리 추위 증가량 20% 감소
        UserStatusManager.Instance.ColdChangeAmounts = UserStatusManager.Instance.ColdChangeAmounts * 0.8f;
        InGameManager.Instance.AvailableCards -= 1;
    }
    public void ColdControll03()
    {
        // 추위 관리 추위 증가량 30% 감소
        UserStatusManager.Instance.ColdChangeAmounts = UserStatusManager.Instance.ColdChangeAmounts * 0.7f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxHPUp01()
    {
        // 맷집 최대 체력 증가
        UserStatusManager.Instance.MaxHP += 5f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxHPUp02()
    {
        UserStatusManager.Instance.MaxHP += 10f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxHPUp03()
    {
        UserStatusManager.Instance.MaxHP += 15f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxStaminaUp01()
    {
        // 마라톤 최대 스테미나 증가
        UserStatusManager.Instance.MaxStamina += 5f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxStaminaUp02()
    {
        UserStatusManager.Instance.MaxStamina += 10f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxStaminaUp03()
    {
        UserStatusManager.Instance.MaxStamina += 15f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxColdUp01()
    {
        // 두꺼운 지방 최대 스테미나 증가
        UserStatusManager.Instance.MaxCold += 5f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxColdUp02()
    {
        UserStatusManager.Instance.MaxCold += 10f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxColdUp03()
    {
        UserStatusManager.Instance.MaxCold += 15f;
        InGameManager.Instance.AvailableCards -= 1;
    }
    // 10
    public void MaxHungryUp01()
    {
        // 단식 최대 배고픔 증가
        UserStatusManager.Instance.MaxHungry += 5f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxHungryUp02()
    {
        UserStatusManager.Instance.MaxHungry += 10f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void MaxHungryUp03()
    {
        UserStatusManager.Instance.MaxHungry += 15f;
        InGameManager.Instance.AvailableCards -= 1;
    }

    public void FortuneCoin25()
    {
        // 포츈코인 (25) 체력 회복 또는 데미지
        int percentage = Random.Range(0, 100);
        if (percentage <= 49)
        {
            UserStatusManager.Instance.HP += 25;
        } 
        else
        {
            UserStatusManager.Instance.HP -= 25;
        }
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void FortuneCoin50()
    {
        int percentage = Random.Range(0, 100);
        if (percentage <= 49)
        {
            UserStatusManager.Instance.HP += 50;
        } 
        else
        {
            UserStatusManager.Instance.HP -= 50;
        }
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void FortuneCoinFull()
    {
        int percentage = Random.Range(0, 100);
        if (percentage <= 49)
        {
            UserStatusManager.Instance.HP = UserStatusManager.Instance.MaxHP;
        } 
        else
        {
            UserStatusManager.Instance.HP = 0;
        }
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void GetGun()
    {
        InventoryManager.Instance.AddItem(gun, 1 , gun.toolDurability);
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void GetBullet()
    {
        InventoryManager.Instance.AddItem(bullet, 10);
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void HawkEye()
    {
        // 카메라 시야 증가
        FollowCameraController cam = Camera.main.GetComponent<FollowCameraController>();
        cam.distanceToTarget = 20;
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void DefenseUp01()
    {
        // 갑옷 데미지 감소량 증가
        UserStatusManager.Instance.Defense += 1f;
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void DefenseUp02()
    {
        // 갑옷 데미지 감소량 증가
        UserStatusManager.Instance.Defense += 3f;
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void DefenseUp03()
    {
        // 갑옷 데미지 감소량 증가
        UserStatusManager.Instance.Defense += 5f;
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void GetMeat01()
    {
        InventoryManager.Instance.AddItem(meat, 2);
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void GetMeat02()
    {
        InventoryManager.Instance.AddItem(meat, 4);
        InGameManager.Instance.AvailableCards -= 1;
    }
    
    public void GetMeat03()
    {
        InventoryManager.Instance.AddItem(meat, 10);
        InGameManager.Instance.AvailableCards -= 1;
    }
}
