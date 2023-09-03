using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * File :   DungeonScene.cs
 * Desc :   DungeonScene이 Load되면 호출된다.
 */

public class DungeonScene : BaseScene
{
    [SerializeField]
    Transform playerSpawn;

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Dungeon;  // 타입 설정

        Camera.main.gameObject.GetOrAddComponent<CameraController>().SetPlayer(Managers.Game.GetPlayer());

        Managers.Game.GetPlayer().transform.position = playerSpawn.position;

        if (Managers.Game.GetPlayer().IsNull() == false)
        {
            GameObject clickMoveEffect = Managers.Resource.Instantiate("Effect/ClickMoveEffect");
            clickMoveEffect.SetActive(false);

            Managers.Game.GetPlayer().GetComponent<PlayerController>().clickMoveEffect = clickMoveEffect;
        }

        Managers.Game._playScene.IsMiniMap(false);

        Managers.Game.StopPlayer();
    }

    public override void Clear()
    {
        
    }
}
