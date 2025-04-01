using UnityEngine;

public class BulletUnscaledMover : MonoBehaviour
{
  public float speed = 20f;
  private Rigidbody2D rb;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();

    // Optional: if bullet was fired with velocity already
    if (rb != null)
    {
      rb.velocity = transform.up * speed;
    }
  }

  void Update()
  {
    // Override physics-based motion with manual movement
    transform.position += transform.up * speed * Time.unscaledDeltaTime;
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    // Check if the other object's name contains "barrier"
    if (collision.gameObject.name.ToLower().Contains("barrier"))
    {
      Destroy(gameObject);
    }
  }

}
