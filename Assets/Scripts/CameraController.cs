using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

  [Header("Camera to Control")]
  [Tooltip("Reference to the Camera that we want to move.")]
  public Camera mainCamera;

  [Header("Offset Settings")]
  [Tooltip("How far away the camera should be from the ship.")]
  public Vector3 cameraOffset = new Vector3(0f, 0f, -10f);

  // We'll store the camera's original rotation so we can preserve it (i.e. no rotation changes).
  private Quaternion initialCameraRotation;

  void Start()
  {
    if (mainCamera != null)
    {
      // Record the camera's starting rotation
      initialCameraRotation = mainCamera.transform.rotation;
    }
    else
    {
      Debug.LogWarning("No camera assigned to CameraController.");
    }
  }

  void LateUpdate()
  {
    // If we have a camera to control, update its position to follow the ship
    if (mainCamera != null)
    {
      // The new camera position is the ship's position plus the offset
      Vector3 newPosition = transform.position + cameraOffset;
      mainCamera.transform.position = newPosition;

      // Keep the original camera rotation, ignoring the ship's rotation
      mainCamera.transform.rotation = initialCameraRotation;
    }
  }

}
