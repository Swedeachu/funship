using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
  [Header("Missile Physics Settings")]
  public float jerk = 40f;          // Rate at which acceleration increases during homing
  public float acceleration = 15f;  // Base acceleration (used during homing phase)
  public float maxSpeed = 120f;      // Maximum speed the missile can reach
  public float turnSpeed = 360f;    // Base turning speed in degrees per second during homing
  public float lifespan = 12f;       // How long the missile exists before self-destructing

  // Multiplier to forcefully steer the missile back if it overshoots
  public float bananaTurnMultiplier = 3f;

  [Header("Sliding Phase Settings")]
  public float slideTime = 1f;         // Duration (in seconds) of the initial sliding phase
  public float decelerationFactor = 2f; // How quickly the missile decelerates during sliding

  private Rigidbody2D rb;
  private Transform target;
  private float currentAcceleration = 0f; // Ramp-up acceleration used during homing phase
  private float slideTimer = 0f;          // Timer for tracking sliding phase

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    FindNearestTarget();
    Destroy(gameObject, lifespan);
  }

  void FixedUpdate()
  {
    // --- Sliding Phase ---
    if (slideTimer < slideTime)
    {
      slideTimer += Time.fixedDeltaTime;
      // Gradually slow the missile down so it "slides" to a stop
      rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, decelerationFactor * Time.fixedDeltaTime);
      return; // Skip homing logic until slide phase is over
    }

    FindNearestTarget();

    // --- Homing Phase ---
    if (target != null)
    {
      // Calculate the vector from the missile to the target
      Vector2 toTarget = (Vector2)(target.position - transform.position);
      float distance = toTarget.magnitude;
      Vector2 direction = toTarget.normalized;

      // Determine the target angle (adjusted by -90° for sprite alignment)
      float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
      float currentAngle = rb.rotation;
      float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

      // If the missile overshoots (angle difference > 90°), boost the turning rate
      float dynamicMultiplier = (Mathf.Abs(angleDiff) > 90f) ? bananaTurnMultiplier : 1f;
      // Increase turn speed as the missile gets closer to the target
      float dynamicTurnSpeed = turnSpeed * dynamicMultiplier * (1 + (1f / Mathf.Max(distance, 0.1f)));

      // Smoothly rotate toward the target angle using physics-based rotation
      float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, dynamicTurnSpeed * Time.fixedDeltaTime);
      rb.MoveRotation(newAngle);
    }

    // Increase homing acceleration gradually using jerk
    currentAcceleration = Mathf.MoveTowards(currentAcceleration, acceleration, jerk * Time.fixedDeltaTime);

    // Apply force in the missile's forward (up) direction for rocket-powered homing
    rb.AddForce(transform.up * currentAcceleration, ForceMode2D.Force);
    rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
  }

  // Finds the nearest target among all GameObjects tagged "Target"
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
