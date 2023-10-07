using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * File :   UI_InvenSlot.cs
 * Desc :   UI_InvenPopup.cs에서 생성되며  인벤토리 안에서 아이템을 관리하는 Slot
 *
 & Functions
 &  [Public]
 &  : SetInfo()             - 기능 설정
 &  : AddItem()             - 아이템 추가
 &  : ClearSlot()           - 초기화
 &
 &  [Protected]
 &  : OnClickSlot()         - 슬롯 우클릭 시 "장비 장착 또는 아이템 사용"
 &  : OnBeginDragSlot()     - 마우스 클릭을 해제하면 "임시 슬롯 초기화"
 &  : OnDropSlot()          - 현재 슬롯에 마우스 클릭을 때면 "아이템 받기"
 &  : ChangeSlot()          - 슬롯 교체
 &
 &  [Private]
 &  : ItemTypeCheck()       - 현재 슬롯의 아이템 타입 체크
 &  : AddSlot()             - 슬롯 받기
 *
 */

public class UI_InvenSlot : UI_ItemDragSlot
{
    enum GameObjects { Lock, }

    public int  invenNumber;     // 인벤 자리 번호

    // 상점 판매 등록될 시 인벤 Lock
    private bool isLock = false;
    public bool IsLock
    {
        get { return isLock; }
        set
        {
            isLock = value;
            GetObject((int)GameObjects.Lock).SetActive(isLock);
        }
    }

    public override void SetInfo()
    {
        base.SetInfo();

        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.Lock).SetActive(false);
    }

    // 아이템 등록
    public override void AddItem(ItemData _item, int count = 1)
    {
        base.AddItem(_item, count);

        // 매니저에 저장
        if (Managers.Game.InvenItem.ContainsKey(invenNumber) == false)
            Managers.Game.InvenItem.Add(invenNumber, _item);
        else
            Managers.Game.InvenItem[invenNumber] = _item;
    }

    // 슬롯 우클릭
    protected override void OnClickSlot(PointerEventData eventData)
    {
        if (isLock == true)
            return;

        if (item.IsNull() == true || UI_DragSlot.instance.dragSlotItem.IsNull() == false)
            return;

        // 슬롯 우클릭
        if (Input.GetMouseButtonUp(1))
        {
            // 상호작용 중이라면
            if (Managers.Game.IsInteract == true)
            {
                Managers.Game.GetSlotInteract(this);
                return;
            }

            // 장비 or 소비 아이템이라면
            if ((item is EquipmentData) == true)
            {
                // 장착 레벨 확인
                if (Managers.Game.Level >= (item as EquipmentData).minLevel)
                    Managers.Game._playScene._equipment.SetEquipment(this);
                else
                    Managers.UI.MakeSubItem<UI_Guide>().SetInfo("레벨이 부족합니다.", new Color(1f, 0.5f, 0f));
            }
            else if ((item is UseItemData) == true)
            {
                // 아이템 사용이 성공적으로 됐다면 -1 차감
                if ((item as UseItemData).UseItem(this.item) == true)
                    SetCount(-1);
            }
        }
    }

    protected override void OnBeginDragSlot(PointerEventData eventData)
    {
        // Lock 확인
        if (IsLock == true)
            return;

        base.OnBeginDragSlot(eventData);
    }

    // 슬롯 받기
    protected override void OnDropSlot(PointerEventData eventData)
    {
        if (UI_DragSlot.instance.dragSlotItem.IsNull() == true)
            return;
            
        UI_Slot dragSlot = UI_DragSlot.instance.dragSlotItem;

        if (dragSlot == this)
            return;

        // 어떤 슬롯에서 왔는지 체크
        switch (dragSlot)
        {
            case UI_UpgradeSlot upgradeSlot:            // 업그레이드 Slot
            {
                AddSlot<UI_UpgradeSlot>(upgradeSlot);
            }
            break;
            case UI_ArmorSlot armorSlot:                // 방어구 Slot
            {
                // 현재 아이템이 같은 종류의 방어구라면 교체
                if (ItemTypeCheck<ArmorItemData>() == true)
                {
                    if ((armorSlot.armorType == (item as ArmorItemData).armorType))
                    {
                        armorSlot.ChangeArmor(this);
                        return;
                    }
                }

                AddSlot<UI_ArmorSlot>(armorSlot);
            }
            break;
            case UI_WeaponSlot weaponSlot:              // 무기 Slot
            {
                // 현재 아이템이 무기라면 교체
                if (ItemTypeCheck<WeaponItemData>() == true)
                {
                    weaponSlot.ChangeWeapon(this);
                    return;
                }

                AddSlot<UI_WeaponSlot>(weaponSlot);
            }
            break;
            case UI_UseItemSlot useSlot:                // 소비 Slot
            {
                AddSlot<UI_UseItemSlot>(useSlot, useSlot.itemCount);
            }
            break;
            case UI_InvenSlot invenSlot:                // 인벤 Slot
            {
                // 두 슬롯의 아이템이 같은 아이템일 경우 개수 체크
                if (item == invenSlot.item && (invenSlot.item is UseItemData))
                {
                    int addValue = itemCount + invenSlot.itemCount;
                    if (addValue > item.itemMaxCount)
                    {
                        invenSlot.SetCount(-(item.itemMaxCount-itemCount));
                        SetCount(item.itemMaxCount - itemCount);
                    }
                    else
                    {
                        SetCount(invenSlot.itemCount);
                        invenSlot.ClearSlot();  // 들고 있었던 슬롯은 초기화
                    }
                }
                else
                    ChangeSlot(invenSlot);
            }
            break;
        }
    }

    protected override void ChangeSlot(UI_ItemSlot itemSlot)
    {
        // 임시 변수
        ItemData _tempItem = item;
        int _tempItemCount = itemCount;

        // 인벤 가져오기
        UI_InvenSlot invenSlot = itemSlot as UI_InvenSlot;

        // 새로 받은 슬롯 Add
        AddItem(invenSlot.item, invenSlot.itemCount);

        // 자신의 아이템을 상대 슬롯에 전달
        if (_tempItem.IsNull() == false)
            invenSlot.AddItem(_tempItem, _tempItemCount);
        else
            invenSlot.ClearSlot();
    }

    // 현재 슬롯의 아이템 타입 체크
    private bool ItemTypeCheck<T>() where T : EquipmentData
    {
        if (item.IsNull() == false)
        {
            if ((item is T) == true)
                return true;
        }

        return false;
    }

    // 슬롯 받기
    private void AddSlot<T>(T slot, int count = 1) where T : UI_ItemDragSlot
    {
        // 아이템이 있다면 다른 슬롯 || 없다면 지금 슬롯에 넣기
        if (item.IsNull() == false)
            Managers.Game._playScene._inventory.AcquireItem(slot.item, count);
        else
            AddItem(slot.item, count);

        slot.ClearSlot();
    }

    // 슬롯 초기화
    public override void ClearSlot()
    {
        base.ClearSlot();

        IsLock = false;

        // 매니저에 저장
        if (Managers.Game.InvenItem.ContainsKey(invenNumber) == true)
            Managers.Game.InvenItem[invenNumber] = null;
    }
}
