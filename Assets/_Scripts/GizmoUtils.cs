#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class GizmoUtils
{
#if UNITY_EDITOR
    public static void DrawCapsule(Vector3 start, Vector3 end, float radius, Color color)
    {
        Handles.color = color;

        

        // Direction and length
        Vector3 up = (end - start).normalized;
        float height = Vector3.Distance(start, end);

        // Rotation to align with capsule direction
        Quaternion rotation = Quaternion.LookRotation(up);

        // Draw cylinder part
        Handles.DrawWireDisc(start, up, radius);
        Handles.DrawWireDisc(end, up, radius);

        Vector3 right = Vector3.Cross(up, Vector3.right).normalized * radius;
        Vector3 forward = Vector3.Cross(up, Vector3.forward).normalized * radius;

        Handles.DrawLine(start + right, end + right);
        Handles.DrawLine(start - right, end - right);
        Handles.DrawLine(start + forward, end + forward);
        Handles.DrawLine(start - forward, end - forward);

        // Draw hemispheres
        Handles.DrawWireArc(start, right, forward, 360f, radius);
        Handles.DrawWireArc(end, right, forward, 360f, radius);
        Handles.DrawWireArc(start, forward, right, 360f, radius);
        Handles.DrawWireArc(end, forward, right, 360f, radius);
    }
#endif
}
