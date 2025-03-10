using UnityEngine;

public class SelfDestruct : MonoBehaviour
{

  public float lifespan = 3f; // Bullet disappears after 3 seconds

  void Start()
  {
    Destroy(gameObject, lifespan);
  }

}
