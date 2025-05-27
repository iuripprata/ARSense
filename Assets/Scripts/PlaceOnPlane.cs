using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
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

        if (arCamera == null)
            arCamera = Camera.main;

        Debug.Log("Available animation names:");
        foreach (var name in animationNames)
        {
            Debug.Log(name);
        }
    }

    void Update()
    {
        Vector2 screenPosition;

#if UNITY_EDITOR
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        screenPosition = Mouse.current.position.ReadValue();
#else
        if (Touchscreen.current == null || Touchscreen.current.touches.Count == 0) return;

        var touch = Touchscreen.current.touches[0];
        if (!touch.press.wasPressedThisFrame) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue())) return;

        screenPosition = touch.position.ReadValue();
#endif

        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
                spawnedObject.SetActive(true);

                if (uiCanvas != null) uiCanvas.SetActive(true);

                animator = spawnedObject.GetComponent<Animator>();
                if (animator != null)
                {
                    Debug.Log("Prefab placed. Animator initialized.");
                }
                else
                {
                    Debug.LogWarning("Animator component not found on placed prefab.");
                }
            }
            else
            {
                spawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
    }

    public void PlayAnimation(string animName)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is null. Cannot play animation.");
            return;
        }

        if (!animator.HasState(0, Animator.StringToHash(animName)))
        {
            Debug.LogWarning($"Animator does not contain the state: {animName}");
            return;
        }

        Debug.Log($"Playing animation: {animName}");
        animator.Play(animName, 0);
    }

    public void NextAnimation()
    {
        if (animator == null || animationNames.Length == 0)
        {
            Debug.LogWarning("Animator not set or animation list is empty.");
            return;
        }

        currentAnimIndex = (currentAnimIndex + 1) % animationNames.Length;
        string animName = animationNames[currentAnimIndex];
        Debug.Log($"Next animation: {animName}");
        PlayAnimation(animName);
    }

    public void PrevAnimation()
    {
        if (animator == null || animationNames.Length == 0)
        {
            Debug.LogWarning("Animator not set or animation list is empty.");
            return;
        }

        currentAnimIndex = (currentAnimIndex - 1 + animationNames.Length) % animationNames.Length;
        string animName = animationNames[currentAnimIndex];
        Debug.Log($"Previous animation: {animName}");
        PlayAnimation(animName);
    }
}
