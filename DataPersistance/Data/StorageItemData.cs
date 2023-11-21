using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StorageItemData
{
    public int id;
    public int quantity;

    public StorageItemData()
    {
        id = 0;
        quantity = 0;
    }

    public StorageItemData(int id, int quantity)
    {
        this.id = id;
        this.quantity = quantity;
    }

}
