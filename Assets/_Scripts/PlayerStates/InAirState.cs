using UnityEngine;

public class InAirState : BaseState
{

    [SerializeField]
    private float inAirMovementSpeed;
    private Camera cam;
    private Rigidbody rb;

    private float groundedTime;
    [SerializeField] private float groundedThreshold = 0.1f;

    Vector3 direction = Vector3.zero;
    Vector3 delta = Vector3.zero;
    public override void OnStateEnter()
    {
        //set idle animation
        cam = Camera.main;
        rb = machine.GetPlayerRB();
        groundedTime = 0;
        machine.PlayInstantAnimation("InAir");
    }

    public override void ProcessTransition()
    {
        if (machine.IsGrounded())
        {
            //a delay is added to avoid multiple check just at beganing
            groundedTime += Time.fixedDeltaTime;
            if (groundedTime >= groundedThreshold)
            {
                machine.ChangeState(PlayerState.Idle);
            }
        }
        else
        {
            groundedTime = 0f;
        }


        if (machine.IsInteractionTrigger() && machine.CanGrabLadder())
        {
            machine.ChangeState(PlayerState.LadderClimb);
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
            //extra downword for - quick donward movement
            rb.AddForce(Vector3.down * 20, ForceMode.Acceleration);
        }

        Vector2 input = machine.GetMoveInput();

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        direction = (input.x * camRight) + (input.y * camForward);
        delta = direction * inAirMovementSpeed * fixedDeltaTime;

        Vector3 newPos = rb.position + delta;
        rb.MovePosition(newPos);
        if (direction.magnitude > 0)
        {
            rb.MoveRotation(Quaternion.LookRotation(direction));
        }
    }

    public override void OnStateExit()
    {

    }
}
