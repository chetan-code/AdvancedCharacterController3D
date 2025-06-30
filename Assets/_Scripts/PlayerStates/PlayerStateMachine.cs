using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Globalization;
using Unity.VisualScripting;




#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PlayerState { 
    Idle,
    Move,
    Jump,
    LadderClimb,
    Climb,
    InAir,
}


public class PlayerStateMachine : MonoBehaviour
{
    [SerializeField]
    private Rigidbody playerRb; 
    [SerializeField]
    private List<BaseState> states;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private LayerMask ladderLayer;
    [SerializeField]
    private LayerMask movableLayer;
    [SerializeField]
    private ClimbNode activeClimbNode;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float inAirDelay = 0.5f;
    private BaseState currentState;
    private Dictionary<PlayerState, BaseState> stateMap;
    private InputAction moveAction;
    private Vector2 moveValue;
    private InputAction jumpAction;
    private InputAction interactAction;
    private bool jumpTrigger;
    private bool interactTrigger;
    private bool isGrounded;
    private bool isInAir;

    private bool canGrabLadder;
    private Transform activeLadder;
    private Vector3 movableVelocity = Vector3.zero;

    Vector3 groundCheckOrigin;
    RaycastHit[] hit = new RaycastHit[1];

    private void Start()
    {
        stateMap = new Dictionary<PlayerState, BaseState>();
        foreach (var state in states) {
            stateMap.Add(state.state, state);
            state.SetStateMachine(state.state, this);
        }
        //setup input
        moveAction = InputSystem.actions.FindAction("Player/Move");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");
        interactAction = InputSystem.actions.FindAction("Player/Interact");

        ChangeState(PlayerState.Idle);
    }


    private void Update()
    { 
        ProcessInput();
        GroundCheck();
        LadderCheck();
        MovableCheck();
        InAirCheck();
        if (currentState == null) { return; }
        currentState.OnUpdate(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (currentState == null) { return; }
        currentState.OnPhysicUpdate(Time.fixedDeltaTime);
    }

    private void ProcessInput() {
        moveValue = moveAction.ReadValue<Vector2>();
        jumpTrigger = jumpAction.triggered;
        interactTrigger = interactAction.IsPressed();
        //just for check
        if (interactTrigger) {
            Debug.Log("Interact pressed");
        }
    }


    private void GroundCheck() {
        groundCheckOrigin = playerRb.transform.position - ( playerRb.transform.up * 0.5f);
        int hits = Physics.SphereCastNonAlloc(groundCheckOrigin, 0.25f, Vector3.down,hit,0.5f,groundLayer);
        for (int i = 0; i < hits; i++) {
            //Debug.Log(hit[i].collider.name);
        }
        if (hits > 0) { 
            isGrounded = true;
            return;
        }

        isGrounded = false;
    }


    Vector3 raycastOriginTop;
    Vector3 raycastOriginBottom;
    float maxDistance = 1f;
    bool topCheck, bottomCheck, moveableCheck;
    RaycastHit ladderHit;
    private void LadderCheck() {
        raycastOriginTop = playerRb.transform.position +(Vector3.up * 1f);
        raycastOriginBottom = playerRb.transform.position + (Vector3.down * 1f);

        topCheck = Physics.Raycast(raycastOriginTop, playerRb.transform.forward,out ladderHit, maxDistance, ladderLayer);
        if (ladderHit.transform != null) { 
           activeLadder = ladderHit.transform;
        }
        bottomCheck = Physics.Raycast(raycastOriginBottom, playerRb.transform.forward,out ladderHit, maxDistance, ladderLayer);
        if (ladderHit.transform != null)
        {
            activeLadder = ladderHit.transform;
        }


        canGrabLadder = topCheck || bottomCheck;
    }

    private float moveableMaxDistance = 1f;
    private void MovableCheck() {
        RaycastHit hit;
        Vector3 rayBottomPos = playerRb.transform.position + (Vector3.down * 0.8f);
        bottomCheck = Physics.Raycast(rayBottomPos, -playerRb.transform.up,out hit, moveableMaxDistance, movableLayer);
        if (hit.collider != null)
        {
            MovePlatform movePlatform = hit.transform.GetComponent<MovePlatform>();
            movableVelocity = movePlatform.GetMoveVelocity();
        }
        else { 
            movableVelocity = Vector3.zero;
        }
    }

    float inAirTime;
    private void InAirCheck() {
        if (!IsGrounded())
        {
            //a delay is added to avoid inair due to jump
            inAirTime += Time.fixedDeltaTime;
            if (inAirTime >= inAirDelay)
            {
                isInAir = true;
            }
        }
        else
        {
            isInAir = false;
            inAirTime = 0f;
        }
    }

    public void PlayAnimation(string stateName)
    {
        
        animator.CrossFade(stateName, 0.01f);
    }

    public void PlayInstantAnimation(string stateName) {
        animator.Play(stateName, 0, 0f);
    }


    public void SetAnimationParam(string name, float value) {
        animator.SetFloat(name, value);
    }


    public Vector2 GetMoveInput() {
        return moveValue;
    }

    public bool IsJumpTrigger() {
        return jumpTrigger;
    }

    public bool IsInteractionTrigger() { 
        return interactTrigger;
    }

    public bool IsGrounded() {
        return isGrounded;
    }

    public bool CanGrabLadder() { 
        return canGrabLadder;
    }

    public Transform GetLadderTranform() {
        return activeLadder;
    }

    public Vector3 GetMovableVelocity (){
        return movableVelocity;
    }

    public bool IsInAir() {
        return isInAir;    
    }


    public Rigidbody GetPlayerRB(){ 
        return playerRb;
    }

    public void ChangeState(PlayerState newState) {
        if (currentState?.state == newState) {
            return;
        }
        currentState?.OnStateExit();  //old state exit
        stateMap[newState].OnStateEnter(); //new state enter
        Debug.Log("[" + currentState?.state.ToString() + "->" + newState.ToString() + "]");
        currentState = stateMap[newState]; //new state update begin;
    }

    public void SetClimbNode(ClimbNode node) {
        activeClimbNode = node;
    }

    public bool HasClimbNode() { 
        return activeClimbNode != null;
    }

    public ClimbNode GetClimbNode() {
        return activeClimbNode;
    }

    private void OnDrawGizmos()
    {
        if (currentState == null) { return; }


    #if UNITY_EDITOR
            // Draw debug text above the object
            Vector3 labelPosition = playerRb.transform.position + Vector3.up * 1f;

            // Define a style with larger font size
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 32; // Increase this for larger text
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckOrigin, 0.5f);
            Handles.Label(labelPosition, currentState.state.ToString(),style);
            if (movableVelocity.magnitude > 0) { 
                Handles.Label(labelPosition + (Vector3.down * 0.15f), "[OnMovable]",style);
            }
            if (canGrabLadder && currentState.state != PlayerState.LadderClimb) { 
                Handles.Label(labelPosition + (Vector3.down * 0.15f), "Press [E] to grab ladder",style);
            }

            if (HasClimbNode()) { 
                Handles.Label(activeClimbNode.transform.position, "Press [Space] to climb",style);
            }
        Gizmos.color = topCheck? Color.green : Color.red;
            Gizmos.DrawLine(raycastOriginTop, raycastOriginTop + (playerRb.transform.forward * maxDistance));
            Gizmos.color = bottomCheck ? Color.green : Color.red;
            Gizmos.DrawLine(raycastOriginBottom, raycastOriginBottom + (playerRb.transform.forward * maxDistance));
#endif
    }

}
