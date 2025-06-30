using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LadderClimbState : BaseState
{
    [SerializeField]
    private float moveSpeed = 5f;
    


    private Camera cam;
    private Rigidbody rb;
    Vector3 direction = Vector3.zero;
    Vector3 delta = Vector3.zero;
    Transform player;

    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private float distanceToGround = 5f;
    private bool isTopAvailable;
    [SerializeField]
    private bool checkForTop;
    [SerializeField]
    private float raycastCheckHeight = 2.5f;

    private RaycastHit topHit;
    public override void OnStateEnter()
    {
        cam = Camera.main;
        rb = machine.GetPlayerRB();
        player = rb.transform;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        checkForTop = true;

        player.transform.position = new Vector3(machine.GetLadderTranform().position.x, player.transform.position.y + 0.5f, machine.GetLadderTranform().position.z - 0.4f);
        machine.PlayAnimation("LadderClimb");
    }

    public override void ProcessTransition()
    {

        if (machine.IsJumpTrigger())
        {
            machine.ChangeState(PlayerState.Jump);
        }

        if (machine.IsGrounded() || !machine.CanGrabLadder()) {
            machine.ChangeState(PlayerState.Idle);
        }

        //check if we have a flat space above to land
        isTopAvailable = Physics.Raycast(player.position + (player.up * raycastCheckHeight) + (player.forward * 0.5f), Vector3.down, out topHit, distanceToGround, groundLayer);
        if (isTopAvailable)
        {
             checkForTop = false;
             StartCoroutine(ExitToTop());

        }

    }


    public IEnumerator ExitToTop()
    {
        machine.PlayAnimation("HangToTop");


        // Compute clean top position to stand
        Vector3 targetPosition = player.position + (Mathf.Abs(topHit.point.y - player.position.y) * player.up) + player.up * 1.1f; // slight offset
        targetPosition += player.forward * 0.5f;
        Debug.Log("Moving to climb top position: " + targetPosition);

        // Option 1: Smooth transition
        float duration1 = 0.1f;
        float elapsed = 0f;
        Vector3 startPos = player.position;

        //player.position = targetPosition;

        while (elapsed < duration1)
        {
            player.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration1);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.8f);

        float duration2 = 0.2f;
        targetPosition += player.forward * 0.5f;
        elapsed = 0f;
        startPos = player.position;

        while (elapsed < duration2)
        {
            player.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration1);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.position = targetPosition;

        yield return new WaitForSeconds(0.1f); // optional pause

        machine.ChangeState(PlayerState.Idle);
    }


    public override void OnUpdate(float deltaTime)
    {
        if (!checkForTop)
        {

            return;
        }

        ProcessTransition();

        Vector2 input = machine.GetMoveInput();

        direction = (input.y * Vector3.up);
        delta = direction * moveSpeed * deltaTime;

        Vector3 newPos = rb.position + delta;
        rb.MovePosition(newPos);
        player.rotation = Quaternion.LookRotation(machine.GetLadderTranform().forward);
        machine.SetAnimationParam("LadderClimbMultiplier", Mathf.RoundToInt(input.y));
    }


    public override void OnPhysicUpdate(float fixedDeltaTime)
    {

        //rb.MoveRotation(Quaternion.LookRotation(direction));
    }

    public override void OnStateExit()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        //reset other values
    }
}
