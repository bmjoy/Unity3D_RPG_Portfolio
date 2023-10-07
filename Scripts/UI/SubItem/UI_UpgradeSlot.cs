using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
 * File :   UI_UpgradeSlot.cs
 * Desc :   UI_UpgradePopup.cs에서 사용되며 장비를 업그레이드하는 등록 Slot
 *
 & Functions
 &  [Public]
 &  : SetInfo()             - 기능 설정
 &  : ClearSlot()           - 초기화
 &
 &  [Protected]
 &  : OnClickSlot()         - 슬롯 우클릭 시 "장비 등록 해제"
 &  : OnEndDragSlot()       - 마우스 클릭을 해제하면 "등록 해제"
 &  : OnDropSlot()          - 현재 슬롯에 마우스 클릭을 때면 "장비 등록"
 &  : ChangeSlot()          - 슬롯 교체
 &
 &  [Private]
 &  : GetSlotInteract()     - 현재 슬롯의 아이템 타입 체크
 *
 */

public class UI_UpgradeSlot : UI_ItemDragSlot
{
    public override void SetInfo()
    {
        base.SetInfo();

        // 인벤으로 부터 우클릭 아이템 받기 등록
        Managers.Game._getSlotInteract -= GetSlotInteract;
        Managers.Game._getSlotInteract += GetSlotInteract;
    }

    protected override void OnClickSlot(PointerEventData eventData)
    {
        if (item.IsNull() == true || UI_DragSlot.instance.dragSlotItem.IsNull() == false)
            return;

        // 슬롯 우클릭 시
        if (Input.GetMouseButtonUp(1))
        {
            // 인벤토리로 이동
            Managers.Game._playScene._inventory.AcquireItem(item);
            ClearSlot();
        }
    }

    protected override void OnEndDragSlot(PointerEventData eventData)
    {
        // 아이템을 버린 위치가 UI가 아니라면
        if (item.IsNull() == false && !EventSystem.current.IsPointerOverGameObject())
        {
            // 인벤토리로 이동
            Managers.Game._playScene._inventory.AcquireItem(item);
            ClearSlot();
        }
        
        base.OnEndDragSlot(eventData);
    }

    protected override void OnDropSlot(PointerEventData eventData)
    {
        UI_Slot dragSlot = UI_DragSlot.instance.dragSlotItem;
            
        // 자기 자신 확인
        if (dragSlot == this)
            return;

        // 슬롯 교체 
        ChangeSlot(dragSlot as UI_ItemSlot);
    }

    protected override void ChangeSlot(UI_ItemSlot itemSlot)
    {
        // 장비가 아니라면
        if ((itemSlot.item is EquipmentData) == false)
            return;

        // 강화 슬롯에 아이템이 있다면 인벤으로 돌려 보내기
        if (item.IsNull() == false)
            Managers.Game._playScene._inventory.AcquireItem(item);

        EquipmentData equipment = itemSlot.item as EquipmentData;

        Managers.Game._playScene._upgrade.RefreshUI(equipment);
        AddItem(itemSlot.item);

        (itemSlot as UI_InvenSlot).ClearSlot();
    }

    // 인벤토리로 부터 우클릭으로 장비 받기
    private void GetSlotInteract(UI_InvenSlot invenSlot)
    {
        // UI_UpgradePopup Prefab이 활성화 되어 있다면
        if (Managers.Game._playScene._upgrade.gameObject.activeSelf == true)
            ChangeSlot(invenSlot);
    }

    public override void ClearSlot()
    {
        base.ClearSlot();
        Managers.Game._playScene._upgrade.Clear();
    }
}
