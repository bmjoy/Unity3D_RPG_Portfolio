using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * File :   ItemData.cs
 * Desc :   모든 아이템의 부모
 *
 & Functions
 &  : ItemClone()  - 아이템 깊은 복사
 *
 */

[Serializable]
public abstract class ItemData
{
    public int id;
    public string itemName;
    public Define.ItemType itemType = Define.ItemType.Unknown;
    public Define.itemGrade itemGrade = Define.itemGrade.Common;
    public int itemPrice;
    public int itemMaxCount = 99;

    public GameObject itemObject;

    [TextArea]
    public string itemDesc;
    
    public Sprite itemIcon;

    // Deep Copy (깊은 복사)
    public ItemData ItemClone()
    {
        if (this is EquipmentData)
        {
            if (this is ArmorItemData)
            {
                return AddItemValue<ArmorItemData>((this as ArmorItemData).ArmorClone());
            }
            else if (this is WeaponItemData)
            {
                return AddItemValue<WeaponItemData>((this as WeaponItemData).WeaponClone());
            }
        }
        else if (this is UseItemData)
        {
            return AddItemValue<UseItemData>((this as UseItemData).UseClone());
        }

        return null;
    }

    T AddItemValue<T>(T item) where T : ItemData
    {
        item.id = this.id;
        item.itemName = this.itemName;
        item.itemType = this.itemType;
        item.itemGrade = this.itemGrade;
        item.itemPrice = this.itemPrice;
        item.itemMaxCount = this.itemMaxCount;
        item.itemObject = this.itemObject;
        item.itemDesc = this.itemDesc;
        item.itemIcon = this.itemIcon;

        return item;
    }
}
