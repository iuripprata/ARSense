using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;  // New Input System
using System.Collections.Generic;

public class PlaceOnPlane : MonoBehaviour
{
    [SerializeField] private GameObject placedPrefab;
    [SerializeField] private GameObject uiCanvas;
    [SerializeField] private Camera arCamera;

    private ARRaycastManager raycastManager;
    private GameObject spawnedObject;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Animator animator;

    public string[] animationNames;
    private int currentAnimIndex = 0;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        if (uiCanvas != null)
            uiCanvas.SetActive(false);

        // Use main camera if not assigned
        if (arCamera == null)
            arCamera = Camera.main;
    }

    void Update()
    {
        Vector2 screenPosition;

        // Determine input type: mouse click or touch
#if UNITY_EDITOR
        // Simulate AR interaction with mouse in Editor
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        screenPosition = Mouse.current.position.ReadValue();
#else
        if (Touchscreen.current == null || Touchscreen.current.touches.Count == 0) return;

        var touch = Touchscreen.current.touches[0];
        if (!touch.press.wasPressedThisFrame) return;
        screenPosition = touch.position.ReadValue();
#endif

        // Perform raycast against detected planes
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
                if (uiCanvas != null) uiCanvas.SetActive(true);
                animator = spawnedObject.GetComponent<Animator>();
            }
            else
            {
                spawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
    }

    public void PlayAnimation(string animName)
    {
        if (animator != null)
            animator.Play(animName);
    }

    public void NextAnimation()
    {
        if (animator == null || animationNames.Length == 0) return;
        currentAnimIndex = (currentAnimIndex + 1) % animationNames.Length;
        animator.Play(animationNames[currentAnimIndex]);
    }

    public void PrevAnimation()
    {
        if (animator == null || animationNames.Length == 0) return;
        currentAnimIndex = (currentAnimIndex - 1 + animationNames.Length) % animationNames.Length;
        animator.Play(animationNames[currentAnimIndex]);
    }
}
