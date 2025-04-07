using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeConstraint2D : MonoBehaviour
{
  [SerializeField]
  private Transform anchor = null;

  [SerializeField]
  private float maxRopeLength = 5f;

  [SerializeField]
  private float ropeStiffness = 10f;

  [SerializeField]
  private float damping = 1f;

  private Rigidbody2D rb;
  private LineRenderer lineRenderer;

  private void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    lineRenderer = GetComponent<LineRenderer>();

    if (lineRenderer != null)
    {
      lineRenderer.positionCount = 2;
    }
  }

  private void FixedUpdate()
  {
    // If the anchor is destroyed, stop rendering and optionally destroy the rope behavior
    if (anchor == null || rb == null)
    {
      if (lineRenderer != null)
      {
        lineRenderer.enabled = false; // Just disable rendering
        // Destroy(lineRenderer); // If we want to remove it entirely
        // Destroy(this);         // If the whole script should stop
      }

      return;
    }

    Vector2 direction = rb.position - (Vector2)anchor.position;
    float distance = direction.magnitude;

    if (distance > maxRopeLength)
    {
      Vector2 directionNormalized = direction.normalized;
      float stretch = distance - maxRopeLength;
      Vector2 correctiveForce = -directionNormalized * stretch * ropeStiffness;
      Vector2 dampingForce = -rb.velocity * damping;
      Vector2 totalForce = correctiveForce + dampingForce;
      rb.AddForce(totalForce, ForceMode2D.Force);
    }

    // Update line renderer if active
    if (lineRenderer != null && lineRenderer.enabled)
    {
      lineRenderer.SetPosition(0, anchor.position);
      lineRenderer.SetPosition(1, transform.position);
    }
  }

  public void BreakRope()
  {
    anchor = null;
  }

}
