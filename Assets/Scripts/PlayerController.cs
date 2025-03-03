using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
  // -- References --
  private Rigidbody2D rb;  // We'll store our 2D rigidbody here

  // -- Movement Variables --
  [Header("Movement Settings")]
  public float maxForwardSpeed = 5f;      // Maximum forward speed
  public float forwardAcceleration = 2f;  // Acceleration when pressing W
  public float forwardJerk = 2f;          // How quickly we can change acceleration (derivative of accel)

  // We'll decelerate with at least double the acceleration and jerk
  public float decelerationMultiplier = 2f;

  private float currentAcceleration = 0f; // Tracks the 'actual' acceleration in the forward direction
  private Vector2 currentVelocity;        // We'll track velocity manually

  // -- Rotation Variables --
  [Header("Rotation Settings")]
  public float maxTurnSpeed = 180f;    // Max degrees per second (angular velocity)
  public float turnAcceleration = 90f; // How quickly we accelerate rotation
  public float turnJerk = 90f;         // How quickly rotation acceleration can change

  private float currentTurnAcceleration = 0f; // Actual rotational acceleration
  private float currentAngularVelocity = 0f;  // Track angular velocity manually

  // -- Size Handling --
  [Header("Size Settings")]
  public Vector3 smallScale = new Vector3(1f, 1f, 1f);
  public Vector3 mediumScale = new Vector3(2f, 2f, 2f);
  public Vector3 largeScale = new Vector3(4f, 4f, 4f);

  // Movement multipliers for each size 
  // (bigger ship => more sluggish, so we reduce these for bigger sizes)
  public float smallSpeedMultiplier = 1.0f;
  public float mediumSpeedMultiplier = 0.5f;
  public float largeSpeedMultiplier = 0.25f;

  private float sizeSpeedMultiplier;  // The current multiplier based on chosen size

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();

    // Default to small ship
    transform.localScale = smallScale;
    sizeSpeedMultiplier = smallSpeedMultiplier;

    // Lock rotation in Z if doing a typical top-down. If you want free 2D rotation, remove this.
    // rb.constraints = RigidbodyConstraints2D.FreezeRotation; 
  }

  void Update()
  {
    HandleSizeSwitch();
    HandleAccelerationInput();
    HandleTurnInput();
  }

  void FixedUpdate()
  {
    // Apply forward/backward movement in physics update
    UpdateForwardVelocity();

    // Apply turning/rotation in physics update
    UpdateAngularVelocity();

    // Finally, assign to the Rigidbody2D so Unity's physics engine uses it
    rb.velocity = currentVelocity;
    rb.rotation -= currentAngularVelocity * Time.fixedDeltaTime;
    // Note: We subtract because Unity's 2D 'rotation' is typically opposite direction from standard math
  }

  // ---------------------------
  //  Ship Size Switching
  // ---------------------------
  private void HandleSizeSwitch()
  {
    // Press '1' => small
    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
      transform.localScale = smallScale;
      sizeSpeedMultiplier = smallSpeedMultiplier;
    }
    // Press '2' => medium
    else if (Input.GetKeyDown(KeyCode.Alpha2))
    {
      transform.localScale = mediumScale;
      sizeSpeedMultiplier = mediumSpeedMultiplier;
    }
    // Press '3' => large
    else if (Input.GetKeyDown(KeyCode.Alpha3))
    {
      transform.localScale = largeScale;
      sizeSpeedMultiplier = largeSpeedMultiplier;
    }
  }

  // ---------------------------
  //  Forward/Backward Input
  // ---------------------------
  private void HandleAccelerationInput()
  {
    float inputVertical = Input.GetAxisRaw("Vertical");
    // Typically, W => +1, S => -1

    // We'll define a "targetAcceleration" based on input
    float targetAcc = 0f;

    if (inputVertical > 0f)
    {
      // Accelerate forward
      targetAcc = forwardAcceleration * sizeSpeedMultiplier;
    }
    else if (inputVertical < 0f)
    {
      // Decelerate or accelerate backward with a stronger factor
      targetAcc = -forwardAcceleration * decelerationMultiplier * sizeSpeedMultiplier;
    }

    // We'll smoothly move 'currentAcceleration' toward 'targetAcc' 
    //   respecting the "forwardJerk" or "decelerationMultiplier * forwardJerk."
    float actualJerk = (inputVertical < 0f)
        ? forwardJerk * decelerationMultiplier
        : forwardJerk;

    currentAcceleration = Mathf.MoveTowards(
        currentAcceleration,
        targetAcc,
        actualJerk * Time.deltaTime
    );
  }

  private void UpdateForwardVelocity()
  {
    // currentVelocity changes based on currentAcceleration
    // velocity(t + dt) = velocity(t) + acceleration(t) * dt
    // acceleration(t) changes at a limited rate by "jerk" in the method above
    Vector2 forwardDir = transform.up;
    // 'transform.up' in 2D is the "forward" direction of the sprite

    // Increment velocity by acceleration * dt
    currentVelocity += forwardDir * currentAcceleration * Time.fixedDeltaTime;

    // Then clamp to max forward speed (scaled by size)
    float finalMaxSpeed = maxForwardSpeed * sizeSpeedMultiplier;
    if (currentVelocity.magnitude > finalMaxSpeed)
    {
      currentVelocity = currentVelocity.normalized * finalMaxSpeed;
    }
  }

  // ---------------------------
  //  Turning Input
  // ---------------------------
  private void HandleTurnInput()
  {
    float inputHorizontal = Input.GetAxisRaw("Horizontal");
    // A => -1, D => +1

    float targetTurnAcc = 0f;
    if (inputHorizontal > 0f)
    {
      // Turn right
      targetTurnAcc = turnAcceleration * sizeSpeedMultiplier;
    }
    else if (inputHorizontal < 0f)
    {
      // Turn left
      // We could also do stronger rotation if reversing direction quickly
      targetTurnAcc = -turnAcceleration * sizeSpeedMultiplier;
    }

    // If sign of input changes vs sign of velocity, apply "double jerk"
    float actualTurnJerk = turnJerk;
    if (Mathf.Sign(inputHorizontal) != Mathf.Sign(currentAngularVelocity) &&
        Mathf.Abs(currentAngularVelocity) > 0.1f)
    {
      actualTurnJerk *= decelerationMultiplier;
    }

    // Move currentTurnAcceleration toward target at a rate of 'turnJerk'
    currentTurnAcceleration = Mathf.MoveTowards(
        currentTurnAcceleration,
        targetTurnAcc,
        actualTurnJerk * Time.deltaTime
    );
  }

  private void UpdateAngularVelocity()
  {
    // angularVelocity(t + dt) = angularVelocity(t) + turnAcceleration(t) * dt
    currentAngularVelocity += currentTurnAcceleration * Time.fixedDeltaTime;

    // Clamp the turn speed
    float finalMaxTurnSpeed = maxTurnSpeed * sizeSpeedMultiplier;
    currentAngularVelocity = Mathf.Clamp(currentAngularVelocity, -finalMaxTurnSpeed, finalMaxTurnSpeed);
  }

}
