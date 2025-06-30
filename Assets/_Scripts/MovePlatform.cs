using UnityEngine;

public class MovePlatform : MonoBehaviour
{
    [SerializeField] private Vector3 deltaPos;
    [SerializeField] private float speed = 1f;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 targetPos;


    private Vector3 moveVelocity;
    Vector3 lastPosition;

    bool paused;
    private void Start()
    {
        startPos = transform.position;
        endPos = startPos + deltaPos;
        targetPos = endPos;
    }


    private void FixedUpdate()
    {

        if (paused)
        {
            moveVelocity = Vector3.zero;
            return;
        }

        moveVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;

        // Move towards the current target position
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // If reached target, flip direction
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            targetPos = targetPos == endPos ? startPos : endPos;
        }
    }

    public Vector3 GetMoveVelocity()
    {

        return moveVelocity;
    }



    private void OnDrawGizmos()
    {
        Vector3 from = transform.position;
        Vector3 to = targetPos;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(from, to);
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.collider.name);
        if (collision.collider.CompareTag("Player"))
        {
            if (collision.transform.position.y < transform.position.y)
            {
                Debug.Log("Player tag detected");
                paused = true;
            }
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (paused)
        {
            if (collision.collider.CompareTag("Player"))
            {
                paused = false;
            }
        }
    }


}