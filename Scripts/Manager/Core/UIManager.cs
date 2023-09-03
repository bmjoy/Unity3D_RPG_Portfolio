using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * File :   UIManager.cs
 * Desc :   UI의 Scene, Popup, WorldSpace 생성/제거를 도와주는 매니저
 *          [ Rookiss의 MMORPG Game Part 3 참고. ]
 */

public class UIManager
{
    int _order = 10;

    List<UI_Popup> _popupList = new List<UI_Popup>();
    UI_Scene _sceneUI = null;

    // 프리팹 오브젝트 부모 (하이라이커 깔끔하게 정리하려고 사용)
    public GameObject Root
    {
        get{
            GameObject root = GameObject.Find("@UI_Root");// 오브젝트 찾기

            if (root.IsNull() == true)
                root = new GameObject{name = "@UI_Root"}; // 오브젝트 이름 설정

            return root;
        }
    }

    // 오브젝트에 Canvas를 추가하고 order을 설정
    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sort)
            SetOrder(canvas);
        else
            canvas.sortingOrder = 0;
    }

    public void SetOrder(Canvas canvas)
    {
        canvas.sortingOrder = _order;
        _order++;
    }

    // 3D 안에 있는 WorldSpace에서 UI 생성 (캐릭터 체력 UI ...)
    public T MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/WorldSpace/{name}");

        if (parent.IsNull() == false)
            go.transform.SetParent(parent);

        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        return go.GetOrAddComponent<T>();
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}");

        if (parent.IsNull() == false)
            go.transform.SetParent(parent);

        return go.GetOrAddComponent<T>();
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        // name = null 경우
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

        return sceneUI;
    }

    // UI에 만들어질 프리팹을 stack에 넣어 order을 관리
    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        // 이미 생성된 Popup이면 종료
        if (_popupList.Contains(Managers.Resource.Load<T>($"UI/Popup/{name}")) == true)
            return null;

        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        T popup = Util.GetOrAddComponent<T>(go);
        _popupList.Add(popup);

        go.transform.SetParent(Root.transform);

        return popup;
    }

    // 팝업창 켜기
    public void OnPopupUI(UI_Popup popup)
    {
        // 이미 켜져있으면 진행 X
        if (popup.gameObject.activeSelf == true)
            return;

        _popupList.Add(popup);
        Managers.Pool.Pop(popup.gameObject);
        SetOrder(popup.GetComponent<Canvas>());

        Managers.Game.isPopups[popup.popupType] = true;

        popup.transform.SetParent(Root.transform);
    }

    // 리스트에 popup이 있나 확인 후 삭제
    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupList.Count == 0)
            return;
        
        if (_popupList.Contains(popup) == false)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }

        _order--;

        Managers.Game.isPopups[popup.popupType] = false;
        _popupList.Remove(popup);
        Managers.Resource.Destroy(popup.gameObject);
    }

    public bool ClosePopupUI()
    {
        if (_popupList.Count == 0)
            return false;

        foreach(UI_Popup popup in _popupList)
        {
            if (popup.IsNull() == false)
            {
                OnClosePopup(popup);
                return true;
            }
        }

        return false;
    }

    void OnClosePopup(UI_Popup popup)
    {
        Managers.Game.isPopups[popup.popupType] = false;
        _popupList.Remove(popup);

        // fake null 체크
        if (popup.IsFakeNull() == true)
            popup = null;
        else
            Managers.Resource.Destroy(popup.gameObject);
            
        _order--;
    }

    // List 전체 Close
    public void CloseAllPopupUI()
    {
        while (true)
        {
            if (ClosePopupUI() == false)
                break;
        }

        _popupList.Clear();
    }

    public void Clear()
    {
        CloseAllPopupUI();
    }
}
