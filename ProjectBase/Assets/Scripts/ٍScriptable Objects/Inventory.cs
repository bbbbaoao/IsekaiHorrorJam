using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory system/Inventory", order = 0)]
public class Inventory : ScriptableObject
{
    [SerializeField] private List<inventoryItem> items = new List<inventoryItem>();
    public void AddItem(ItemObject itemObject, int amount = 1)
    {
        if (itemObject == null) return;
        foreach (var item in items)
        {
            if (item.itemObject == itemObject)
            {
                item.AddAmount(amount);
                return;
            }
        }
        items.Add(new inventoryItem(itemObject, amount));
    }

    public void EmptyInventory()
    {
        items.Clear();
    }
}
[System.Serializable]
public class inventoryItem
{
    public ItemObject itemObject;
    public int amount;
    public inventoryItem(ItemObject itemObject, int amount)
    {
        this.itemObject = itemObject;
        this.amount = amount;
    }
    public void AddAmount(int amountToAdd)
    {
        amount += amountToAdd;
    }
}