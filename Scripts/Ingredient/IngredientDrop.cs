using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class IngredientDrop : MonoBehaviour
{

    // 오브젝트가 가지고 있는 재료 리스트를 받아와서
    public void DropIngredients(List<IngredientAmount> interactIngredients)
    {
        Vector3 temp = new Vector3 (0, -1, 1);
        // 리스트로 for문을 돌린다.
        foreach (var ingredientAmount in interactIngredients)
        {
            // 각 재료의 프리팹을 가져와서
            string ingredientPrefabPath = "Ingredient/" + ingredientAmount.ingredient.itemName;
            // 재료 오브젝트 생성
            GameObject ingredientObj = PhotonNetwork.InstantiateRoomObject(ingredientPrefabPath, transform.position - temp, Quaternion.identity);
            Debug.Log(ingredientObj);
            temp -= new Vector3(1, 0, 0);
            // 재료 오브젝트에 붙어있는 Status 스크립트를 불러와서
            ItemStatus itemStatus = ingredientObj.GetComponent<ItemStatus>();
            IngredientStatus ingredientStatus = ingredientObj.GetComponent<IngredientStatus>();
            if (itemStatus != null)
            {
                // 저장된 개수만큼 할당
                itemStatus.itemQuantity = ingredientAmount.count;
            }
            if (ingredientStatus != null)
            {
                // 저장된 개수만큼 할당
                ingredientStatus.ingredientQuantity = ingredientAmount.count;

            }
        }
    }
}
