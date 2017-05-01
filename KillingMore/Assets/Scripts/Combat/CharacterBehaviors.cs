using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehaviors : ActorBehaivorProvider
{
    public const int IDLE = 0;
    public const int MOVE = 1;
    public const int ATTACK = 2;
    public const int MOVE_ATTACK = 3;
    public const int ROLL_OVER = 4;
    public const int Turn = 5;
    public const int DIE = 6;

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
        mRollOverDirection = Vector3.zero;
        mRollOverTime = 0;
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
            Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dir = (mp - be.transform.position).normalized;
            Quaternion Rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            be.transform.rotation = Quaternion.Euler(Vector3.up * Rot.eulerAngles.y);
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
            Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dir = (mp - be.transform.position).normalized;
            Quaternion Rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            be.transform.rotation = Quaternion.Euler(Vector3.up * Rot.eulerAngles.y);
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
        Debug.DrawRay(character.transform.position, Vector3.forward);
        Debug.DrawRay(character.transform.position, Vector3.back);
        Debug.DrawRay(character.transform.position, Vector3.left);
        Debug.DrawRay(character.transform.position, Vector3.right);
        Debug.DrawRay(character.transform.position, new Vector3(0.7f, 0, 0.7f));
        Debug.DrawRay(character.transform.position, new Vector3(-0.7f, 0, -0.7f));
        Debug.DrawRay(character.transform.position, new Vector3(-0.7f, 0, 0.7f));
        Debug.DrawRay(character.transform.position, new Vector3(0.7f, 0, -0.7f));
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 movement = Vector3.zero;
        movement.Set(h, 0f, v);
        int dir = CalculateDirection(character);
        if (movement != Vector3.zero)
        {
            if (character.ActorAnimator_.CurrentAnimState != ROLL_OVER)
            {
                character.ActorAnimator_.SwitchAnimation(MOVE, dir);
                character.transform.position += movement * character.Speed * dt;
                if (Input.GetKey(KeyCode.Space) && mRollOverTime <= 0)
                {
                    mRollOverDirection = movement;
                    mRollOverTime = character.RollOverCD;
                    int rd = CalculateRollOverDirection(character, movement);
                    character.ActorAnimator_.SwitchAnimation(ROLL_OVER, rd);
                }
            }
        }
        else
        {
            if (character.ActorAnimator_.CurrentAnimState != ROLL_OVER)
            {
                character.ActorAnimator_.SwitchAnimation(IDLE);
            }
        }
        if (character.ActorAnimator_.CurrentAnimState == IDLE || character.ActorAnimator_.CurrentAnimState == Turn)
        {
            character.ActorAnimator_.SwitchAnimation(Turn, dir);
        }
        if (character.ActorAnimator_.CurrentAnimState == ROLL_OVER)
        {
            character.transform.position += mRollOverDirection * character.Speed * dt;
        }
        //RollOver cd
        if (mRollOverTime > 0) mRollOverTime -= dt;
        if (mRollOverTime != 0) Game.DebugString = "RollOverCooldown " + mRollOverTime;
    }

    static int CalculateDirection(Character character)
    {
        Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Polar.Angle2((mp - character.transform.position).normalized, Vector3.forward);
        angle = angle < 0 ? angle + 360 : angle;
        int dir = 0;
        float off8 = 360 / 8;
        float off16 = 360 / 16;
        if (angle > off8 * 1 - off16 && angle <= off8 * 1 + off16)
        {
            dir = ActorAnimator.UP_RIGHT;
        }
        else if (angle > off8 * 2 - off16 && angle <= off8 * 2 + off16)
        {
            dir = ActorAnimator.RIGHT;
        }
        else if (angle > off8 * 3 - off16 && angle <= off8 * 5 + off16)
        {
            dir = ActorAnimator.DOWN;
        }
        else if (angle > off8 * 6 - off16 && angle <= off8 * 6 + off16)
        {
            dir = ActorAnimator.LEFT;
        }
        else if (angle > off8 * 7 - off16 && angle <= off8 * 7 + off16)
        {
            dir = ActorAnimator.UP_LEFT;
        }
        else
        {
            dir = ActorAnimator.UP;
        }
        return dir;
    }

    static int CalculateRollOverDirection(Character character, Vector3 movement)
    {
        int dir = 0;
        float dr = Vector3.Dot(Vector3.right, movement.normalized);
        float db = Vector3.Dot(Vector3.back, movement.normalized);
        if (movement.normalized == Vector3.forward)
        {
            dir = ActorAnimator.UP;
        }
        else if (movement.normalized == Vector3.back)
        {
            dir = ActorAnimator.DOWN;
        }
        else if (movement.normalized == Vector3.left)
        {
            dir = ActorAnimator.LEFT;
        }
        else if (movement.normalized == Vector3.right)
        {
            dir = ActorAnimator.RIGHT;
        }
        else if (dr > 0.5f && dr < 1)
        {
            dir = ActorAnimator.UP_RIGHT;
            if (db > 0.5f && db < 1)
            {
                dir = ActorAnimator.RIGHT;
            }
        }
        else if (dr > -1f && dr < -0.5f)
        {
            dir = ActorAnimator.UP_LEFT;
            if (db > 0.5f && db < 1)
            {
                dir = ActorAnimator.LEFT;
            }
        }
        return dir;
    }
    static Vector3 mRollOverDirection;
    static float mRollOverTime;
}
