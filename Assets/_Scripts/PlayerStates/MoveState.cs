using Unity.Cinemachine;
using UnityEngine;

public class MoveState : BaseState
{
    [SerializeField]
    private float moveSpeed = 5f;
    


    private Camera cam;
    private Rigidbody rb;
    Vector3 direction = Vector3.zero;
    Vector3 delta = Vector3.zero;
    public override void OnStateEnter()
    {
        cam = Camera.main;
        rb = machine.GetPlayerRB();
        machine.PlayAnimation("Run");
    }

    public override void ProcessTransition()
    {

        if (machine.IsJumpTrigger() && machine.IsGrounded() && !machine.HasClimbNode())
        {
            machine.ChangeState(PlayerState.Jump);
        }

        if (machine.GetMoveInput().magnitude <= 0) {
            machine.ChangeState(PlayerState.Idle);
        }

        if (machine.IsInteractionTrigger() && machine.CanGrabLadder()) {
            machine.ChangeState(PlayerState.LadderClimb);
        }


        if (machine.IsJumpTrigger() && machine.HasClimbNode())
        {
            machine.ChangeState(PlayerState.Climb);
        }


        if (machine.IsInAir())
        {
            machine.ChangeState(PlayerState.InAir);
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        ProcessTransition();
    }


    public override void OnPhysicUpdate(float fixedDeltaTime)
    {

        if (rb.linearVelocity.y < 0)
        {
            //extra downword for - quick donward movement - keep player close to ground
            rb.AddForce(Vector3.down * 30, ForceMode.Acceleration);
        }

        Vector2 input = machine.GetMoveInput();

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        direction = (input.x * camRight) + (input.y * camForward);
        delta = direction * moveSpeed * fixedDeltaTime;

        Vector3 newPos = rb.position + delta + (machine.GetMovableVelocity() * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
        rb.MoveRotation(Quaternion.LookRotation(direction));
    }

    public override void OnStateExit()
    {

        //reset other values
    }
}
