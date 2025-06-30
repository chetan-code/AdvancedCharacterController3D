using UnityEngine;

public class IdleState : BaseState
{

    private Camera cam;

    private Rigidbody rb;
    public override void OnStateEnter()
    {
        rb = machine.GetPlayerRB();
        //set idle animation
        rb.angularVelocity = Vector3.zero;
        machine.PlayAnimation("Idle");
    }

    public override void ProcessTransition()
    {
        if (machine.IsJumpTrigger() && machine.IsGrounded() && !machine.HasClimbNode())
        {
            machine.ChangeState(PlayerState.Jump);
        }

        if (machine.GetMoveInput().magnitude > 0) {
            machine.ChangeState(PlayerState.Move);
        }

        if (machine.IsInteractionTrigger() && machine.CanGrabLadder())
        {
            machine.ChangeState(PlayerState.LadderClimb);
        }

        if(machine.IsJumpTrigger() && machine.HasClimbNode())
        {
            machine.ChangeState(PlayerState.Climb);
        }

        if (machine.IsInAir()) {
            machine.ChangeState(PlayerState.InAir);
        }
    }

    public override void OnUpdate(float deltaTime)
    {
         ProcessTransition();
    }

    public override void OnPhysicUpdate(float fixedDeltaTime)
    {
        if (machine.GetMovableVelocity().magnitude != 0)
        {
            //player is on a moving platform
            Vector3 newPos = rb.position + (machine.GetMovableVelocity() * fixedDeltaTime);
            rb.MovePosition(newPos);
        }
        else {
            //player is stationary
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    public override void OnStateExit()
    {
        //reset other values
    }
}
