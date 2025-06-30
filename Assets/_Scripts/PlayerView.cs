using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField]
    private PlayerStateMachine playerStateMachine;
    ClimbNode node;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ONtrigger enter: " + other);
        if (other != null) {
            if (other.TryGetComponent<ClimbNode>(out node)) { 
                 playerStateMachine.SetClimbNode(node);
                Debug.Log("set node " + node);

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null) {
            ClimbNode exitNode;
            other.TryGetComponent<ClimbNode>(out exitNode);
            if (node == exitNode)
            {
                playerStateMachine.SetClimbNode(null); 
                node = null;
            }
        }
    }

}
