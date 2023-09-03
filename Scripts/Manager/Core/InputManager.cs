using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

/*
 * File :   InputManager.cs
 * Desc :   플레이어의 모든 입력(마우스, 키보드)을 확인하고 반환
 *          [ Rookiss의 MMORPG Game Part 3 참고. ]
 */

public class InputManager
{
    // 키 입력 메소드들을 한번에 실행하기 위한 변수
    public Action KeyAction = null;
    public Action<Define.MouseEvent> MouseAction = null;

    bool _leftPressed = false;
    bool _rightPressed = false;

    float _leftPressedTime;
    float _rightPressedTime;

    public void OnUpdate()
    {
        // 상호작용 확인
        if (Managers.Game.IsInteract == true || Managers.Game.isPopups[Define.Popup.Talk] == true)
            return;

        // 키입력 메소드가 KeyAction안에 존재하는가?
        if (Input.anyKey && KeyAction.IsNull() == false)
            KeyAction.Invoke();

        // UI를 클릭했을 때
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        
        if (MouseAction.IsNull() == false)
        {
            if (Input.GetMouseButton(1))
            {
                if (!_rightPressed)
                {
                    MouseAction.Invoke(Define.MouseEvent.RightDown);
                    _rightPressedTime = Time.time;
                }
                MouseAction.Invoke(Define.MouseEvent.RightPress);
                _rightPressed = true;
            }
            else{
                if (_rightPressed)
                {
                    if (Time.time < _rightPressedTime * 0.2f)
                        MouseAction.Invoke(Define.MouseEvent.RightClick);
                        
                    MouseAction.Invoke(Define.MouseEvent.RightUp);
                }
                _rightPressed = false;
                _rightPressedTime = 0f;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!_leftPressed)
                    MouseAction.Invoke(Define.MouseEvent.LeftDown);
            }

            if (Input.GetMouseButton(0))
            {
                _leftPressedTime = Time.time;
                MouseAction.Invoke(Define.MouseEvent.LeftPress);
                _leftPressed = true;
            }
            else{
                if (_leftPressed)
                {
                    if (Time.time < _leftPressedTime * 0.2f)
                        MouseAction.Invoke(Define.MouseEvent.LeftClick);
                        
                    MouseAction.Invoke(Define.MouseEvent.LeftUp);
                }
                _leftPressed = false;
                _leftPressedTime = 0f;
            }
        }
    }

    public void Clear()
    {
        KeyAction = null;
        MouseAction = null;
    }
}
