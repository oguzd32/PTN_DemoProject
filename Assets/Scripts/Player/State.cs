using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class State
{
    public enum STATE
    {
        IDLE, RUN, WIN, FAIL
    }

    public enum EVENT
    {
        ENTER, EXIT
    }

    public STATE name;
    protected EVENT stage;
    protected Animator anim;
    protected State nextState;

    public State(Animator _anim)
    {
        anim = _anim;
        stage = EVENT.ENTER;
    }
    
    public virtual void Enter() { stage = EVENT.ENTER; }
    public virtual void Exit() { stage = EVENT.EXIT; }
    
    public State Process()
    {
        if (stage == EVENT.ENTER) Enter();
        if (stage == EVENT.EXIT)
        {
            Exit();
            return nextState;
        }
        return this;
    }
}

public class Idle : State
{
    public Idle(Animator _anim) : base(_anim)
    {
        name = STATE.IDLE;
    }

    public override void Enter()
    {
        anim.SetTrigger("isIdle");
        base.Enter();
    }

    public override void Exit()
    {
        nextState = new Move(anim);
        stage = EVENT.EXIT;
        anim.ResetTrigger("isIdle");
        base.Exit();
    }
}

public class Move : State
{
    public Move(Animator _anim) : base(_anim)
    {
        stage = EVENT.EXIT;
        name = STATE.RUN;
    }

    public override void Enter()
    {
        anim.SetTrigger("isRunning");
        base.Enter();
    }

    public override void Exit()
    {
        anim.ResetTrigger("isRunning");
        base.Exit();
    }
}
