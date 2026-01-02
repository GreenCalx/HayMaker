using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
public enum EPlayerState
{
    IDLE=0,
    RUNNING=1,
    DRAGGING=2,
    BENDING=3,
    AIRBORNE=4,
    DEAD=5
}

public class Arc
{
    public EPlayerState from;
    public EPlayerState to;
    public Func<bool> condition;

    public Arc(EPlayerState iFrom, EPlayerState iTo, Func<bool> iCond)
    {
        from = iFrom;
        to = iTo;
        condition = iCond;
    }

    public bool Check(EPlayerState iCurrentPlayerState)
    {
        if (iCurrentPlayerState != from)
            return false;
        return condition.Invoke();
    }
}

public class PlayerFSM
{
    public readonly EPlayerState RootState = EPlayerState.IDLE;
    public EPlayerState CurrentState;
    private PlayerController PC;
    private List<Arc> arcs;
    public PlayerFSM(PlayerController iPC)
    {
        PC = iPC;
        Setup();
    }

    public void Setup()
    {
        CurrentState = RootState;

        arcs = new List<Arc>()
        {
            new Arc(EPlayerState.IDLE, EPlayerState.RUNNING, RunningCondition),

            new Arc(EPlayerState.RUNNING, EPlayerState.IDLE, IdleCondition),
            new Arc(EPlayerState.RUNNING, EPlayerState.BENDING, BendingCondition),
            
            new Arc(EPlayerState.RUNNING, EPlayerState.DRAGGING, DraggingCondition),
            new Arc(EPlayerState.DRAGGING, EPlayerState.RUNNING, RunningCondition),
            new Arc(EPlayerState.DRAGGING, EPlayerState.BENDING, BendingCondition),

            new Arc(EPlayerState.BENDING, EPlayerState.AIRBORNE, AirborneCondition),

            new Arc(EPlayerState.AIRBORNE, EPlayerState.RUNNING, RunningCondition)
        };
    }

    public void Refresh()
    {
        foreach(Arc arc in arcs)
        {
            if (arc.Check(CurrentState))
            {
                OnCurrentExit();
                CurrentState = arc.to;
                OnCurrentEnter();
            }
        }
    }

    public void OnCurrentExit()
    {
        switch (CurrentState)
        {
            case EPlayerState.IDLE:
                OnIdleExit();
                break;
            case EPlayerState.RUNNING:
                OnRunningExit();
                break;
            case EPlayerState.DRAGGING:
                OnDraggingExit();
                break;
            case EPlayerState.BENDING:
                OnBendingExit();
                break;
            case EPlayerState.AIRBORNE:
                OnAirborneExit();
                break;
            default:
                break;
        }
    }

    public void OnCurrentEnter()
    {
        switch (CurrentState)
        {
            case EPlayerState.IDLE:
                OnIdleEnter();
                break;
            case EPlayerState.RUNNING:
                OnRunningEnter();
                break;
            case EPlayerState.DRAGGING:
                OnDraggingEnter();
                break;
            case EPlayerState.BENDING:
                OnBendingEnter();
                break;
            case EPlayerState.AIRBORNE:
                OnAirborneEnter();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// IDLE
    /// </summary>
    public void OnIdleEnter()
    {
        Debug.Log("IdleEnter");
    }
    public void OnIdleExit()
    {
        Debug.Log("IdleExit");
    }

    public bool IdleCondition()
    {
        return !PC.IsMoving && !PC.IsBending && !PC.IsAirborne && !PC.IsDragging && !PC.isDead;
    }

    /// <summary>
    /// Running
    /// </summary>
    public void OnRunningEnter()
    {
        Debug.Log("RunningEnter");
        PC.Run(true);
    }
    public void OnRunningExit()
    {
        Debug.Log("RunningExit");
        PC.Run(false);
    }
    public bool RunningCondition()
    {
        return PC.IsMoving && !PC.IsBending && !PC.IsAirborne && !PC.IsDragging && !PC.isDead;
    }

    /// <summary>
    /// bending
    /// </summary>
    public void OnBendingEnter()
    {
        Debug.Log("BendingEnter");
    }
    public void OnBendingExit()
    {
        Debug.Log("BendingExit");
    }
    public bool BendingCondition()
    {
        return PC.IsMoving && PC.IsBending && !PC.IsAirborne && !PC.isDead;
    }
    /// <summary>
    /// Dragging
    /// </summary>
    public void OnDraggingEnter()
    {
        Debug.Log("DraggingEnter");
        PC.DragPS.Play();
        PC.dragSFX.Play();
    }
    public void OnDraggingExit()
    {
        Debug.Log("DraggingExit");
        PC.DragPS.Stop();
        PC.dragSFX.Stop();
    }
    public bool DraggingCondition()
    {
        return PC.IsMoving && !PC.IsBending && !PC.IsAirborne && PC.IsDragging && !PC.isDead;
    }
    /// <summary>
    /// Airborne
    /// </summary>
    public void OnAirborneEnter()
    {
        Debug.Log("AirborneEnter");
    }
    public void OnAirborneExit()
    {
        Debug.Log("AirborneExit");
    }
    public bool AirborneCondition()
    {
        return PC.IsAirborne && !PC.IsBending && !PC.isDead;
    }
    
    public void Kill()
    {
        CurrentState = EPlayerState.DEAD;
    }
}