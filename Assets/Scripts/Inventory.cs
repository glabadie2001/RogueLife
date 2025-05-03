using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : SerializedMonoBehaviour
{
    [SerializeField]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine)]
    //TODO: Index by string or ItemInfo?
    Dictionary<string, InventoryItem> inventory;

    public void Add(ItemInfo item)
    {
        if (inventory.ContainsKey(item.name))
        {
            inventory[item.name].stackSize++;
        }
        else
        {
            inventory.Add(item.name, new InventoryItem(item));
        }
    }

    public void Remove(ItemInfo item)
    {
        if (inventory.ContainsKey(item.name))
        {
            if (inventory[item.name].stackSize == 1)
                inventory.Remove(item.name);
            else
                inventory[item.name].stackSize--;
        }
    }

    public void RemoveStack(ItemInfo item)
    {
        inventory.Remove(item.name);
    }
}

[System.Serializable]
public class InventoryItem {
    public ItemInfo info;
    public int stackSize;

    public InventoryItem(ItemInfo _info, int amount = 1)
    {
        info = _info;
        stackSize = amount;
    }
}

[System.Serializable]
public class ItemInfo : ScriptableObject
{
    [MultiLineProperty]
    public string description;
}