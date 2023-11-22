using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int quantity;
    public int itemDurability;

    public InventoryItem(ItemData data, int qty = 1, int itemDurability = 1)
    {
        this.itemData = data;
        this.quantity = qty;
        this.itemDurability = itemDurability;
    }
}
public class InventoryManager : MonoBehaviour
{
    // UI이동을 위한 현재 드래그하는중인 아이템 관리
    public Transform draggedItem;
    public Transform draggingItem;
    public bool isDragging;

    public static InventoryManager Instance { get; private set; }

    public List<InventoryItem> inventory;
    public int maxQty = 10;
    // 0~4 핫키, 5~19 인벤토리
    public int maxInventorySlot = 20;
    public InventoryItem equippedItem;
    public PlayerHotKey hotkey;

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

        // 인벤토리를 인벤토리 최대 크기만큼 선언하고
        inventory = new List<InventoryItem>(maxInventorySlot);

        // null로 가득채운다
        for(int a = 0; a < maxInventorySlot; a++)
        {
            inventory.Add(null);
            inventory[a] = null;
        }
        equippedItem = null;
    }

    // 테스트용 초기값
    private void Start()
    {

        List<ToolData> toolDataList = ToolManager.Instance.toolDataList;
        //List<IngredientData> ingredientDataList = IngredientManager.Instance.ingredientDataList;

        for (int a = 0; a < toolDataList.Count; a++)
        {
            AddItem(toolDataList[a], 1, 100);
        }
    }

    // 인벤토리에 아이템 추가
    public int AddItem(ItemData newItem, int qty = 1, int toolDurability = 1)
    {
        if (newItem.itemType!=ItemType.Ingredient && newItem.itemType!=ItemType.Consumable) // 아이템 타입이 셀 수 없는 종류인 경우
        {
            if (newItem is ToolData toolData) // 아이템이 도구라면
            {
                return AddNewItemSlot(toolData, 1, toolDurability) ? 0:1; // 슬롯을 하나 더해서 내구도와 함께 아이템을 넣어준다.
            }
            // 도구가 아니라면 슬롯을 추가해서 그냥 더해준다
            return AddNewItemSlot(newItem, 1) ? 0 : 1;
        }

        foreach (var slot in inventory)
        {
            // slot이 빈칸이 아니고, 아이템이 더하려는 아이템과 일치하고 꽉 안차있다면
            if (slot!=null &&slot.itemData == newItem && slot.quantity < maxQty)
            {
                // 남은 용량을 구하고
                int spaceInSlot = maxQty - slot.quantity;
                // 남은 용량과 더하려는 양의 최소값을 구한 뒤
                int addToSlot = Mathf.Min(qty, spaceInSlot);
                // 더해준다.
                slot.quantity += addToSlot;
                // 남은 양을 구하고
                qty -= addToSlot;

                if (qty == 0) // 남은 양이 0이라면 다 더한것이므로
                {
                    // 0 반환
                    return 0;
                }
            }
        }
        // 인벤토리를 쭉 둘러봤더니 겹치는 아이템이 없거나 수량이 남는다면 빈 슬롯에 넣는다.
        if (AddNewItemSlot(newItem, qty))
        {
            return 0;
        }
        return qty; // 남은 수량 반환
    }

    // 아이템 데이터를 받아서 인벤토리에 몇개 있는지 반환하는 함수
    public int NumberItem(ItemData item)
    {
        int totalQuantityInInventory = inventory
            .Where(slot => slot != null && slot.itemData == item)
            .Sum(slot => slot.quantity);

        return totalQuantityInInventory;
    }
    // 아이템 리스트를 받아서 아이템이 인벤토리에 다 있는지 true,false를 반환하는 함수
    public bool CheckItem(List<InventoryItem> itemList)
    {
        foreach (var itemToCheck in itemList)
        {
            // 인벤토리에서 같은 아이템 데이터를 가진 아이템을 찾고 양을 다 더해준다.
            int totalQuantityInInventory = inventory
                .Where(slot => slot != null && slot.itemData == itemToCheck.itemData)
                .Sum(slot => slot.quantity);

            // 필요한 수량이 인벤토리의 총 수량보다 많으면 false
            if (totalQuantityInInventory < itemToCheck.quantity)
            {
                return false;
            }
        }

        // 모든 아이템이 필요한 수량만큼 있으면 true를 반환한다.
        return true;
    }

    // 인벤토리에 있는 아이템을 사용하는 함수
    public bool UseItem(List<InventoryItem> itemList)
    {
        if (!UserStatusManager.Instance.CanMake)
        {
            return false;
        }

        // 인벤토리에서 필요한 모든 아이템이 있는지 확인
        if (!CheckItem(itemList))
            return false; // 필요한 아이템이 없으면 false 반환

        // 각 아이템에 대해 수량 차감
        foreach (var itemToUse in itemList)
        {
            int remainingQuantityToUse = itemToUse.quantity;

            // 인벤토리를 순회하면서 아이템 찾기
            foreach (var slot in inventory.Where(slot => slot != null && slot.itemData == itemToUse.itemData).Reverse())
            {
                if (slot.quantity >= remainingQuantityToUse)
                {
                    // 필요한 수량만큼 차감하고 종료
                    slot.quantity -= remainingQuantityToUse;
                    remainingQuantityToUse = 0;

                    // 아이템 수량이 0이 되면 슬롯을 비움.
                    if (slot.quantity == 0)
                        inventory[inventory.IndexOf(slot)] = null;

                    break; // 이 아이템에 대한 처리를 완료
                }
                else
                {
                    // 사용 가능한 수량만큼 차감하고 계속 진행
                    remainingQuantityToUse -= slot.quantity;
                    inventory[inventory.IndexOf(slot)] = null; // 슬롯을 비움.
                }
            }
        }

        // 모든 아이템이 성공적으로 사용되면 true 반환
        return true;
    }

    public bool CheckConsumableItem(InventoryItem item)
    {
        // 인벤토리에서 같은 아이템 데이터를 가진 아이템을 찾고 양을 다 더해준다.
        int totalQuantityInInventory = inventory
            .Where(slot => slot != null && slot.itemData == item.itemData)
            .Sum(slot => slot.quantity);

        // 필요한 수량이 인벤토리의 총 수량보다 많으면 false
        if (totalQuantityInInventory < item.quantity)
        {
            return false;
        }
        // 모든 아이템이 필요한 수량만큼 있으면 true를 반환한다.
        return true;
    }

    public bool UseConsumableItem(InventoryItem itemToUse)
    {
        // 인벤토리에서 필요한 아이템이 있는지 확인
        if (!CheckConsumableItem(itemToUse))
            return false; // 필요한 아이템이 없으면 false 반환

        int remainingQuantityToUse = itemToUse.quantity;

        // 인벤토리를 순회하면서 아이템 찾기
        foreach (var slot in inventory.Where(slot => slot != null && slot.itemData == itemToUse.itemData).Reverse())
        {
            if (slot.quantity >= remainingQuantityToUse)
            {
                // 필요한 수량만큼 차감하고 종료
                slot.quantity -= remainingQuantityToUse;
                remainingQuantityToUse = 0;

                // 아이템 수량이 0이 되면 슬롯을 비움.
                if (slot.quantity == 0)
                    inventory[inventory.IndexOf(slot)] = null;

                break; // 이 아이템에 대한 처리를 완료
            }
            else
            {
                // 사용 가능한 수량만큼 차감하고 계속 진행
                remainingQuantityToUse -= slot.quantity;
                inventory[inventory.IndexOf(slot)] = null; // 슬롯을 비움.
            }
        }

        // 필요한 수량이 모두 차감되었는지 확인
        if (remainingQuantityToUse > 0)
            return false; // 필요한 수량을 차감하지 못했다면 false 반환

        // 모든 아이템이 성공적으로 사용되면 true 반환
        return true;
    }

    // 새 아이템 슬롯을 추가하는 보조 메소드
    private bool AddNewItemSlot(ItemData newItem, int qty, int itemDurability = 1)
    {
        // 인벤토리 빈칸을 찾는다.
        int emptySlotIndex = inventory.FindIndex(slot => slot == null || slot.itemData == null);

        // 빈칸이 있다면
        if(emptySlotIndex != -1)
        {
            // 빈칸에 아이템 할당
            inventory[emptySlotIndex] = new InventoryItem(newItem, qty, itemDurability);
            return true;
        }
        else
        {
            // 인벤토리가 가득 찼을 경우에는 false
            Debug.Log("Inventory is full. Cannot add new item.");
            return false;
        }
    }
    // 아이템 장착 메서드
    public void EquipItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventory.Count)
        {
            equippedItem = inventory[slotIndex];
        }
    }

    // 아이템 해제 메서드
    public void UnequipItem()
    {
        equippedItem = null;
    }

    public bool UseTool(InventoryItem item,int amount)
    {
        if (item.itemDurability > 0)
        {
            item.itemDurability -= amount;
            if(item.itemDurability <= 0)
            {
                RemoveItemFromInventory(item);
                hotkey.UnequipTool();
                UnequipItem();
                hotkey.hotkeyIndex = -1;
                return false;
            }
            return true;
        }
        else
        {
            RemoveItemFromInventory(item);
            hotkey.UnequipTool();
            UnequipItem();
            hotkey.hotkeyIndex = -1;
            return false;
        }
    }

    private void RemoveItemFromInventory(InventoryItem item)
    {
        int index = inventory.IndexOf(item);
        if (index != -1)
        {
            inventory[index] = null; // 해당 인벤토리 슬롯을 null로 설정
        }
    }

}
