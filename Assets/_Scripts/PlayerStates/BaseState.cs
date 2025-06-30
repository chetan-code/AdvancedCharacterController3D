using UnityEngine;

public class BaseState : MonoBehaviour
{
    protected PlayerStateMachine machine;
    public PlayerState state;
    public void SetStateMachine(PlayerState state,PlayerStateMachine stateMachine) {
        this.machine = stateMachine;
        this.state = state;
    }
    public virtual void OnStateEnter() { }

    public virtual void ProcessTransition() { }
    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnPhysicUpdate(float fixedDeltaTime) { }
    public virtual void OnStateExit() { }
}
