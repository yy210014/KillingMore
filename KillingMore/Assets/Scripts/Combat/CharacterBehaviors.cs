using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehaviors : ActorBehaivorProvider
{
    public const int IDLE = 0;
    public const int MOVE = 1;
    public const int ATTACK = 2;
    public const int MOVE_ATTACK = 3;
    public const int DIE = 4;

    public override bool InitializeFSM(ActorBehaviorFSM fsm)
    {
        fsm.InitialAddBehavior(new ActorBehavior(IDLE, IdleEnter,
            IdleUpdate, IdleLeave, null, "Idle"));

        fsm.InitialAddBehavior(new ActorBehavior(MOVE, MoveEnter,
            MoveUpdate, MoveLeave, null, "Move"));

        fsm.InitialAddBehavior(new ActorBehavior(ATTACK, AttackEnter,
            AttackUpdate, AttackLeave, null, "Attack"));

        fsm.InitialAddBehavior(new ActorBehavior(MOVE_ATTACK, MoveNAttackEnter,
            MoveNAttackUpdate, MoveNAttackLeave, null, "MoveNAttack"));

        fsm.InitialAddBehavior(new ActorBehavior(DIE, DieEnter,
            DieUpdate, DieLeave, null, "DIE"));
        return true;
    }

    public override void Reset()
    {

    }

    public static void IdleEnter(Actor ac, ActorBehavior last)
    {
        Character ca = ac as Character;
        ca.IteraterEmiters((be) =>
        {
            be.gameObject.SetActive(false);
        });
    }

    public static int IdleUpdate(Actor ac)
    {
        return IDLE;
    }

    public static void IdleLeave(Actor ac, ActorBehavior next)
    {
        Character ca = ac as Character;
        ca.IteraterEmiters((be) =>
        {
            be.gameObject.SetActive(true);
        });
    }

    public static void MoveEnter(Actor ac, ActorBehavior last)
    {
        Character ca = ac as Character;
        ca.IteraterEmiters((be) =>
        {
            be.gameObject.SetActive(false);
        });
    }

    public static int MoveUpdate(Actor ac)
    {
        float dt = ac.SelfTimeScale * GameScene.Singleton.TimeDelta;
        Character ca = ac as Character;
        Move(ca, dt);
        return MOVE;
    }

    public static void MoveLeave(Actor ac, ActorBehavior next)
    {
        Character ca = ac as Character;
        ca.IteraterEmiters((be) =>
        {
            be.gameObject.SetActive(true);
        });
    }

    public static void AttackEnter(Actor ac, ActorBehavior last)
    {
    }

    public static int AttackUpdate(Actor ac)
    {
        float dt = ac.SelfTimeScale * GameScene.Singleton.TimeDelta;
        Character ca = ac as Character;
        ca.IteraterEmiters((be) =>
        {
            be.OnGameUpdate(dt);
        });
        return ATTACK;
    }

    public static void AttackLeave(Actor ac, ActorBehavior next)
    {

    }

    public static void MoveNAttackEnter(Actor ac, ActorBehavior last)
    {
    }

    public static int MoveNAttackUpdate(Actor ac)
    {
        float dt = ac.SelfTimeScale * GameScene.Singleton.TimeDelta;
        Character ca = ac as Character;
        Move(ca, dt);
        ca.IteraterEmiters((be) =>
        {
            be.OnGameUpdate(dt);
        });

        return MOVE_ATTACK;
    }

    public static void MoveNAttackLeave(Actor ac, ActorBehavior next)
    {

    }


    public static void DieEnter(Actor ac, ActorBehavior last)
    {
        ac.ActorAnimator_.SwitchAnimation(DIE);
    }

    public static int DieUpdate(Actor ac)
    {
        return DIE;
    }

    public static void DieLeave(Actor ac, ActorBehavior next)
    {

    }


    static void Move(Character character, float dt)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 movement = Vector3.zero;
        movement.Set(h, 0f, v);
        if (movement != Vector3.zero)
        {
            character.ActorAnimator_.SwitchAnimation(MOVE);
            character.transform.position += movement * character.Speed * dt;
        }
        else
        {
            character.ActorAnimator_.SwitchAnimation(IDLE);
        }
        float dot = Vector3.Dot(Vector3.right, movement.normalized);
        if (dot == 0)
        {
            return;
        }
        if (Mathf.Sign(dot) > 0)
        {
            if (character.SpriteRenderer.flipX) character.SpriteRenderer.flipX = false;
        }
        else
        {
            if (!character.SpriteRenderer.flipX) character.SpriteRenderer.flipX = true;
        }
        Debug.Log(dot);
    }

    static void Turning(Character character)
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;
        if (Physics.Raycast(camRay, out floorHit, 1 << LayerMask.NameToLayer("Background")))
        {
            Vector3 playerToMouse = floorHit.point - character.transform.position;
            playerToMouse.y = 0f;
            Quaternion newRotatation = Quaternion.LookRotation(playerToMouse);
            character.transform.rotation = newRotatation;
        }
    }
}
