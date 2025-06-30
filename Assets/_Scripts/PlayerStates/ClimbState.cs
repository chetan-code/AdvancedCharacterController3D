using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.XR;

public class ClimbState : BaseState
{
    [SerializeField]
    private float lerpFactor = 5f;    
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private float distanceToGround = 5f;

    private ClimbNode activeClimbNode;
    private Rigidbody rb;
    private Transform player;
    private bool isGroundAvailable;
    private bool isTopAvailable;
    [SerializeField]
    private bool checkForTop;

    private RaycastHit topHit;

    public override void OnStateEnter()
    {                                      
        rb = machine.GetPlayerRB();
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        player = rb.transform;
        activeClimbNode = machine.GetClimbNode();
        machine.PlayAnimation("IdleToHang");
        checkForTop = true;

        //transform.forward = activeClimbNode.transform.forward;

    }

    public override void ProcessTransition()
    {
        //check of we have ground below - in a range 
        isGroundAvailable = Physics.Raycast(player.position, Vector3.down, distanceToGround, groundLayer);

        if (isGroundAvailable && machine.IsJumpTrigger())
        {
            if (machine.GetMoveInput().y <= -0.7f && Mathf.Abs(machine.GetMoveInput().x) < 0.5f)
            {
                Debug.Log("climb down");
                //down input
                if (activeClimbNode.down == null)
                {
                    //we dont have any node to move to - but ground is available
                    machine.ChangeState(PlayerState.Idle);
                }
            }
        }



        //check if we have a flat space above to land
        isTopAvailable = Physics.Raycast(player.position + (player.up * 3) + (player.forward * 1f), Vector3.down, out topHit,distanceToGround, groundLayer);
        if (isTopAvailable && machine.IsJumpTrigger() && checkForTop)
        {
            if (machine.GetMoveInput().y >= 0.7f && Mathf.Abs(machine.GetMoveInput().x) < 0.5f)
            {
                Debug.Log("climb up");
                //up input
                if (activeClimbNode.top == null)
                {
                    checkForTop = false;
                    StartCoroutine(ExitToTop());
                }
            }
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



    private void OnDrawGizmos()
    {
        Gizmos.color = isGroundAvailable ? Color.green : Color.red;
        if(player)Gizmos.DrawLine(player.position, player.position + Vector3.down * distanceToGround);
        Gizmos.color = isTopAvailable ? Color.green : Color.red;
        if (player) Gizmos.DrawLine(player.position + (player.up * 3) + (player.forward * 1f), player.position + (player.up * 5) + (player.forward * 2f) + Vector3.down * distanceToGround);
        if (topHit.point != null) {
            Gizmos.DrawSphere(topHit.point, 0.5f);
        } 
    }


    public override void OnUpdate(float deltaTime)
    {

        if (!checkForTop)
        {

            return;
        }

        ProcessTransition();


        Vector3 targetPos = activeClimbNode.GetPlayerPoint();

        

        player.position = Vector3.MoveTowards(player.position, targetPos, deltaTime * lerpFactor);

        if (player.position != activeClimbNode.GetPlayerPoint()) {
            return;
        }

        Vector2 input = machine.GetMoveInput();
        if (machine.IsJumpTrigger())
        {
            if (input.y >= 0.7f && Mathf.Abs(input.x) < 0.5f)
            {
                Debug.Log("climb up");
                //up input
                if (activeClimbNode.top != null)
                {
                   machine.SetClimbNode(activeClimbNode.top);
                   activeClimbNode = activeClimbNode.top;
                   machine.PlayInstantAnimation("HangHopUp");
                }
            }

            if (input.y <= -0.7f && Mathf.Abs(input.x) < 0.5f)
            {
                Debug.Log("climb down");
                //down input
                if (activeClimbNode.down != null)
                {
                    machine.SetClimbNode(activeClimbNode.down);
                    activeClimbNode = activeClimbNode.down;
                    machine.PlayInstantAnimation("HangHopDown");
                }
            }

            if (input.x >= 0.7f && Mathf.Abs(input.y) < 0.5f) { 
                Debug.Log("climb right");
                if (activeClimbNode.right != null)
                {
                    machine.SetClimbNode(activeClimbNode.right);
                    activeClimbNode = activeClimbNode.right;
                    machine.PlayInstantAnimation("HangHopRight");
                }
            }

            if (input.x <= -0.7f && Mathf.Abs(input.y) < 0.5f)
            {
                Debug.Log("climb left");
                if (activeClimbNode.left != null)
                {
                    machine.SetClimbNode(activeClimbNode.left);
                    activeClimbNode = activeClimbNode.left;
                    machine.PlayInstantAnimation("HangHopLeft");
                }
            }
        }

        player.rotation = Quaternion.LookRotation(activeClimbNode.transform.forward);

    }


    public override void OnPhysicUpdate(float fixedDeltaTime)
    {

    }

    public override void OnStateExit()
    {

        rb.useGravity = true;
        rb.isKinematic = false;
        //reset other values
    }
}
