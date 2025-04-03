using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
  // Keep track of the last applied offset.
  private Vector3 lastShakeOffset = Vector3.zero;
  private Coroutine shakeCoroutine;

  /// <summary>
  /// Starts a screen shake effect relative to the camera's current position.
  /// </summary>
  /// <param name="duration">Duration of the shake in seconds</param>
  /// <param name="intensityX">Maximum offset on the X axis</param>
  /// <param name="intensityY">Maximum offset on the Y axis</param>
  public void Shake(float duration, float intensityX, float intensityY)
  {
    // If a shake is already in progress, cancel it and remove the last offset.
    if (shakeCoroutine != null)
    {
      StopCoroutine(shakeCoroutine);
      transform.position -= lastShakeOffset;
      lastShakeOffset = Vector3.zero;
    }

    shakeCoroutine = StartCoroutine(DoShake(duration, intensityX, intensityY));
  }

  /// <summary>
  /// Coroutine that applies a relative shake offset to the camera.
  /// </summary>
  private IEnumerator DoShake(float duration, float intensityX, float intensityY)
  {
    float elapsed = 0f;
    while (elapsed < duration)
    {
      // Remove the previous shake offset.
      transform.position -= lastShakeOffset;

      // Calculate a new random offset.
      float offsetX = Random.Range(-intensityX, intensityX);
      float offsetY = Random.Range(-intensityY, intensityY);
      Vector3 newOffset = new Vector3(offsetX, offsetY, 0f);

      // Apply the new offset to the current camera position.
      transform.position += newOffset;
      lastShakeOffset = newOffset;

      elapsed += Time.unscaledDeltaTime;
      yield return null;
    }

    // Remove the shake offset after the shake duration.
    transform.position -= lastShakeOffset;
    lastShakeOffset = Vector3.zero;
    shakeCoroutine = null;
  }

}
