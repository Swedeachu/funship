using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  [Header("Camera to Control")]
  public Camera mainCamera;

  [Header("Target Settings")]
  public Transform target; // The player's ship

  [Header("Offset & Follow Settings")]
  public Vector3 cameraOffset = new Vector3(0f, 0f, -10f);
  public float followSpeed = 2f; // Adjusts how smoothly the camera follows

  [Header("Sway Settings")]
  public float maxSwayDistance = 4f; // How far the camera sways from the target
  public float swaySpeed = 0.5f; // How fast the sway adjusts

  [Header("Zoom Settings")]
  public float baseMinZoom = 12f;
  public float maxZoom = 16f;
  public float zoomSpeed = 2f;
  public float speedThreshold = 5f;

  private Rigidbody2D targetRb;
  private Quaternion initialCameraRotation;
  private Vector3 velocity = Vector3.zero; // Used for smooth damp
  private Vector3 swayOffset = Vector3.zero; // Camera sway effect

  void Start()
  {
    if (mainCamera == null)
    {
      Debug.LogWarning("No camera assigned to CameraController.");
      return;
    }

    if (target != null)
    {
      targetRb = target.GetComponent<Rigidbody2D>();
    }

    initialCameraRotation = mainCamera.transform.rotation;
  }

  void LateUpdate()
  {
    if (mainCamera == null || target == null) return;

    // Calculate target position with offset
    Vector3 targetPosition = target.position + cameraOffset;

    // Apply camera sway based on movement direction
    if (targetRb != null)
    {
      Vector3 movementDirection = targetRb.velocity.normalized;
      swayOffset = Vector3.Lerp(swayOffset, movementDirection * maxSwayDistance, swaySpeed * Time.deltaTime);
    }

    // Apply smooth follow effect
    Vector3 finalPosition = targetPosition + swayOffset;
    mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, finalPosition, ref velocity, followSpeed * Time.deltaTime);

    // Keep original camera rotation
    mainCamera.transform.rotation = initialCameraRotation;

    // Adjust zoom based on speed and ship size
    if (targetRb != null)
    {
      float speed = targetRb.velocity.magnitude;

      // Scale min zoom based on ship size
      float sizeScaleFactor = 1f + (PlayerController.sizeIndex * 0.5f); // Larger ships zoom out
      float minZoom = baseMinZoom * sizeScaleFactor;

      float targetZoom = Mathf.Lerp(minZoom, maxZoom, speed / speedThreshold);
      mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
    }
  }

}
