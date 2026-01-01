using UnityEngine;

public enum EPlayerState
{
    IDLE=0,
    RUNNING=1,
    DRAGGING=2,
    BENDING=3,
    AIRBORNE=4

}

public class PlayerFSM
{
    public EPlayerState CurrentState;
    private PlayerController PC;
    public PlayerFSM(PlayerController iPC)
    {
        PC = iPC;
        Setup();
    }

    public void Setup()
    {
        CurrentState = EPlayerState.IDLE;
    }

    public void Refresh()
    {
        switch (CurrentState)
        {
            case EPlayerState.IDLE:
                if (PC.IsMoving && !PC.IsDragging && !PC.IsBending)
                {
                    OnRunningEnter();
                }
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
        
    }
    public void OnIdleExit()
    {
        
    }

    /// <summary>
    /// Running
    /// </summary>
    public void OnRunningEnter()
    {
        
    }
    public void OnRunningExit()
    {
        
    }

    /// <summary>
    /// bending
    /// </summary>
    public void OnBendingEnter()
    {
        
    }
    public void OnBendingExit()
    {
        
    }

    /// <summary>
    /// Dragging
    /// </summary>
    public void OnDraggingEnter()
    {
        
    }
    public void OnDraggingExit()
    {
        
    }

    /// <summary>
    /// Airborne
    /// </summary>
    public void OnAirborneEnter()
    {
        
    }
    public void OnAirborneExit()
    {
        
    }

    
}