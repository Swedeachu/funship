using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunController : MonoBehaviour
{

  public GameObject bulletPrefab;
  public GameObject missilePrefab; // Missile prefab for homing missiles
  public Transform firePoint;
  public float buildUpTime = 3f;

  public float baseBulletSpeed = 15f;
  public float baseMinFireRate = 0.5f;
  public float baseMaxFireRate = 0.1f;
  public float baseBulletSize = 1f;

  public int missileCount = 5; // Number of missiles in the barrage
  public float missileSpreadAngle = 45f; // Spread angle for the missiles

  private float timeDown = 0f;
  private float nextFireTime = 0f;

  // Private variable to alternate shot offset (left/right)
  private bool nextShotLeft = true;
  public float baseBulletOffsetAngle = 2f;

  void Update()
  {
    // Hold down to shoot
    if (Input.GetKey(KeyCode.Space))
    {
      timeDown += Time.deltaTime;

      float sizeFactor = 1f + (PlayerController.sizeIndex * 1f);
      float minFireRate = baseMinFireRate / sizeFactor;
      float maxFireRate = baseMaxFireRate / sizeFactor;

      float fireRate = Mathf.Lerp(minFireRate, maxFireRate, timeDown / buildUpTime);

      if (Time.time >= nextFireTime)
      {
        Shoot(sizeFactor);
        nextFireTime = Time.time + fireRate;
      }
    }
    else
    {
      timeDown = 0f;
      nextFireTime = Time.time;
    }

    // Missile Barrage on ENTER Key
    if (Input.GetKeyDown(KeyCode.Return))
    {
      LaunchMissileBarrage();
    }
  }

  private void Shoot(float sizeFactor)
  {
    sizeFactor++; // Increase sizeFactor as per original logic

    // Calculate base offset angle scaled by ship size
    float maxOffset = baseBulletOffsetAngle * (PlayerController.sizeIndex + 1);

    // Generate a random offset within a range, ensuring the spread isn't too wild
    float randomOffset = Random.Range(0.5f * maxOffset, maxOffset); // Random spread variation

    // Alternate left and right each shot, applying random variation
    float angleOffset = nextShotLeft ? randomOffset : -randomOffset;
    nextShotLeft = !nextShotLeft; // Flip for next shot

    // Create a new rotation with the offset applied
    Quaternion offsetRotation = firePoint.rotation * Quaternion.Euler(0f, 0f, angleOffset);

    // Determine the spawn position (unchanged in this case)
    Vector3 spawnPosition = firePoint.position + (firePoint.up * 0.5f);
    GameObject bullet = Instantiate(bulletPrefab, spawnPosition, offsetRotation);

    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
      // Apply velocity using the new offset rotation
      rb.velocity = offsetRotation * Vector2.up * (baseBulletSpeed * sizeFactor);
    }

    bullet.transform.localScale = Vector3.one * (baseBulletSize * sizeFactor / 4);
  }

  private void LaunchMissileBarrage()
  {
    int mCount = missileCount * (PlayerController.sizeIndex + 1);
    float startAngle = -missileSpreadAngle * (mCount - 1) / 2f;
    float sizeFactor = 1f + (PlayerController.sizeIndex * 0.5f); // Bigger ships = faster missiles

    for (int i = 0; i < mCount; i++)
    {
      Quaternion spreadRotation = Quaternion.Euler(0, 0, startAngle + (i * missileSpreadAngle) + Random.Range(-5f, 5f));
      Vector3 spawnPosition = firePoint.position + (firePoint.up * Random.Range(0.3f, 1.0f)); // Random offset for swarm effect
      GameObject missile = Instantiate(missilePrefab, spawnPosition, firePoint.rotation * spreadRotation);

      Rigidbody2D rb = missile.GetComponent<Rigidbody2D>();
      if (rb != null)
      {
        rb.velocity = missile.transform.up * (5f * sizeFactor); // Launch speed; missiles will decelerate and then home
      }
    }
  }

}
