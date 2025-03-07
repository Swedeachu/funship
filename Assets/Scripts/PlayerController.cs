using UnityEngine;

public class PlayerController : MonoBehaviour
{
  [Header("Movement Settings")]
  public float baseAcceleration = 5f;
  public float jerk = 10f;
  public float decelMultiplier = 2f;
  public float maxSpeed = 5f;
  public float linearFriction = 3f;

  [Header("Rotation Settings")]
  public float baseRotAcceleration = 100f;
  public float rotJerk = 200f;
  public float rotDecelMultiplier = 2f;
  public float maxRotSpeed = 180f;
  public float rotationalFriction = 10f;

  [Header("Gunship Sizes")]
  public Vector3[] scales = { new Vector3(1, 1, 1), new Vector3(2, 2, 1), new Vector3(4, 4, 1) };
  public float[] speedFactors = { 1f, 0.5f, 0.25f };
  private int sizeIndex = 0;

  private Rigidbody2D rb;
  private float currentAcceleration;
  private float currentSpeed;
  private float currentRotAcceleration;
  private float currentRotSpeed;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    ApplySize();
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Alpha1)) { sizeIndex = 0; ApplySize(); }
    if (Input.GetKeyDown(KeyCode.Alpha2)) { sizeIndex = 1; ApplySize(); }
    if (Input.GetKeyDown(KeyCode.Alpha3)) { sizeIndex = 2; ApplySize(); }
  }

  void FixedUpdate()
  {
    // Linear movement
    float vertical = Input.GetAxisRaw("Vertical");

    // determine which way to decel or accel towards using jerk
    bool decel = (vertical != 0 && currentSpeed != 0 && Mathf.Sign(currentSpeed) != Mathf.Sign(vertical));
    float targetAccel = vertical * baseAcceleration * speedFactors[sizeIndex] * (decel ? decelMultiplier : 1f);
    currentAcceleration = Mathf.MoveTowards(currentAcceleration, targetAccel, jerk * Time.fixedDeltaTime);

    // apply velocity and acceleration
    currentSpeed += currentAcceleration * Time.fixedDeltaTime;
    currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * speedFactors[sizeIndex], maxSpeed * speedFactors[sizeIndex]);
    if (vertical == 0) { currentSpeed = Mathf.MoveTowards(currentSpeed, 0, linearFriction * Time.fixedDeltaTime); }

    // Rotational movement 
    float horizontal = -Input.GetAxisRaw("Horizontal");

    // determine whcih way to decel or accel rotation towards using jerk
    bool rotDecel = (horizontal != 0 && currentRotSpeed != 0 && Mathf.Sign(currentRotSpeed) != Mathf.Sign(horizontal));
    float targetRotAccel = horizontal * baseRotAcceleration * speedFactors[sizeIndex] * (rotDecel ? rotDecelMultiplier : 1f);

    // apply velocity and acceleration
    currentRotAcceleration = Mathf.MoveTowards(currentRotAcceleration, targetRotAccel, rotJerk * Time.fixedDeltaTime);
    currentRotSpeed += currentRotAcceleration * Time.fixedDeltaTime;
    currentRotSpeed = Mathf.Clamp(currentRotSpeed, -maxRotSpeed * speedFactors[sizeIndex], maxRotSpeed * speedFactors[sizeIndex]);
    if (horizontal == 0) { currentRotSpeed = Mathf.MoveTowards(currentRotSpeed, 0, rotationalFriction * Time.fixedDeltaTime); }

    rb.velocity = transform.up * currentSpeed;
    rb.MoveRotation(rb.rotation + currentRotSpeed * Time.fixedDeltaTime);
  }

  void ApplySize()
  {
    transform.localScale = scales[sizeIndex];
    currentSpeed = 0f;
    currentAcceleration = 0f;
    currentRotSpeed = 0f;
    currentRotAcceleration = 0f;
  }

}
