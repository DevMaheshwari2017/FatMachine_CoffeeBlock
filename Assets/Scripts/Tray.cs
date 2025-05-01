using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class Tray : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Camera cam;
    private Vector3 offset;
    private float objectZ;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool isDragging = false;
    private float maxMoveDistance = 0.5f;
    private List<Vector3> collisionNormals = new List<Vector3>();

    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        rb.isKinematic = false;
        isDragging = true;
        objectZ = cam.WorldToScreenPoint(transform.position).z;
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = objectZ;
        offset = transform.position - cam.ScreenToWorldPoint(mousePoint);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = objectZ;
        Vector3 desiredPosition = cam.ScreenToWorldPoint(mousePoint) + offset;

        desiredPosition.y = transform.position.y;

        Vector3 desiredMove = desiredPosition - transform.position;

        foreach (Vector3 normal in collisionNormals)
        {
            float dot = Vector3.Dot(desiredMove, normal);
            if (dot < 0f)
            {
                desiredMove -= Vector3.Project(desiredMove, normal);
            }
        }

        targetPosition = transform.position + desiredMove;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        rb.linearVelocity = Vector3.zero;
        collisionNormals.Clear();
        rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        if (isDragging)
        {
            Vector3 moveDelta = targetPosition - rb.position;
            if (moveDelta.magnitude > maxMoveDistance)
            {
                moveDelta = moveDelta.normalized * maxMoveDistance;
            }
            rb.MovePosition(rb.position + moveDelta);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isDragging) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 normal = contact.normal;

            bool alreadyStored = false;
            foreach (Vector3 stored in collisionNormals)
            {
                if (Vector3.Angle(stored, normal) < 5f)
                {
                    alreadyStored = true;
                    break;
                }
            }

            if (!alreadyStored)
                collisionNormals.Add(normal);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!isDragging) return;


        collisionNormals.Clear();
    }
}
