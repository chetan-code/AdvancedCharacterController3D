using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ClimbNode : MonoBehaviour
{

    public ClimbNode top, down, left, right, forward, back;

    [SerializeField]
    Vector3 playerPosOffset;


    public Vector3 GetPlayerPoint() {
        return transform.TransformPoint(playerPosOffset); 
    }


    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        //for visual debug only
        List<Transform> list = new List<Transform>();
        if (top != null) { list.Add(top.transform); };
        if (down != null) { list.Add(down.transform); };
        if (left != null) { list.Add(left.transform); };
        if (right != null) { list.Add(right.transform); };
        if (forward != null) { list.Add(forward.transform); };
        if (back != null) { list.Add(back.transform); };

        if (list == null || list.Count == 0) return;
        foreach (var node in list) {
            Handles.color = Color.blue;
            Vector3[] points = { transform.position, node.position };
            Handles.DrawAAPolyLine(10f,points);
        }

        Gizmos.DrawWireCube(GetPlayerPoint(), Vector3.one * 0.12f);
#endif

    }

}
