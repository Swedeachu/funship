using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
  [Header("Missile Physics Settings")]
  public float jerk = 40f;          // Rate at which acceleration increases
  public float acceleration = 15f;  // Base acceleration value
  public float maxSpeed = 60f;      // Maximum speed
  public float turnSpeed = 360f;    // Base turning speed in degrees per second
  public float lifespan = 6f;       // How long the missile exists before self-destructing

  // Multiplier to aggressively correct when overshooting
  public float bananaTurnMultiplier = 3f;

  private Rigidbody2D rb;
  private Transform target;
  private float currentAcceleration = 0f; // Current acceleration ramping up using jerk

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    FindNearestTarget();
    Destroy(gameObject, lifespan);
  }

  void FixedUpdate()
  {
    // Continuously look for a target so that if one is lost, we may pick a new one.
    FindNearestTarget();

    if (target != null)
    {
      // Calculate the direction and distance to the target
      Vector2 toTarget = (Vector2)(target.position - transform.position);
      float distance = toTarget.magnitude;
      Vector2 direction = toTarget.normalized;

      // Determine the target angle (adjusted by -90 degrees due to sprite orientation)
      float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

      // Get the current rotation from the Rigidbody2D (in degrees)
      float currentAngle = rb.rotation;
      // Calculate the smallest angle difference between the current and target angles
      float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

      // If the missile overshoots (angle difference > 90 degrees), apply a stronger turning multiplier
      float dynamicMultiplier = (Mathf.Abs(angleDiff) > 90f) ? bananaTurnMultiplier : 1f;

      // Also adjust turning speed based on distance (closer means more aggressive turning)
      float dynamicTurnSpeed = turnSpeed * dynamicMultiplier * (1 + (1f / Mathf.Max(distance, 0.1f)));

      // Smoothly adjust the missile's rotation towards the target angle using the dynamic turn speed
      float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, dynamicTurnSpeed * Time.fixedDeltaTime);
      rb.MoveRotation(newAngle);
    }

    // Increase the missile's acceleration gradually using jerk (for a smoother physics-based feel)
    currentAcceleration = Mathf.MoveTowards(currentAcceleration, acceleration, jerk * Time.fixedDeltaTime);

    // Apply force in the missile's forward (up) direction using the current acceleration
    rb.AddForce(transform.up * currentAcceleration, ForceMode2D.Force);
    // Clamp the missile's velocity so it doesn't exceed the maximum speed
    rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
  }

  // Finds the nearest target by comparing distances to all GameObjects tagged "Target"
  private void FindNearestTarget()
  {
    GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
    float minDistance = Mathf.Infinity;
    Transform closestTarget = null;

    foreach (GameObject potentialTarget in targets)
    {
      float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
      if (distance < minDistance)
      {
        minDistance = distance;
        closestTarget = potentialTarget.transform;
      }
    }

    if (closestTarget != null)
    {
      target = closestTarget;
    }
  }
}
