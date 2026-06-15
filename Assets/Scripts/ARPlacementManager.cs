using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class ARPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;

    [Header("Prefab & UI")]
    public GameObject gemPrefab;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI scoreText;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> placedGems = new List<GameObject>();
    private int score = 0;

    void Start()
    {
        UpdateScoreText();
    }

    void Update()
    {
        UpdateInstructions();
        HandleTouch();
    }

    // Gives the player on-screen guidance based on what AR is currently doing
    void UpdateInstructions()
    {
        // AR session still starting up
        if (ARSession.state < ARSessionState.SessionInitializing)
        {
            instructionText.text = "Checking AR support...";
            return;
        }

        // Lost tracking 
        if (ARSession.state != ARSessionState.SessionTracking)
        {
            instructionText.text = "Tracking lost. Move slowly to a well-lit area with more detail.";
            return;
        }

        // No surfaces found yet
        if (planeManager.trackables.count == 0)
        {
            instructionText.text = "Move your phone slowly to scan a floor or table.";
            return;
        }

        instructionText.text = "Tap a surface to place a gem. Tap a gem to collect it!";
    }

    // Handles both placing new gems and collecting existing ones
    void HandleTouch()
    {
        bool isClicking = Input.GetMouseButtonDown(0);
        bool isTouching = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;

        if (!isClicking && !isTouching) return;

        Vector2 screenPosition = isTouching ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        // Did the player tap an already-placed gem? If so, collect it.
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            GameObject tapped = hitInfo.collider.gameObject;
            if (placedGems.Contains(tapped))
            {
                placedGems.Remove(tapped);
                Destroy(tapped);
                score++;
                UpdateScoreText();
                return;
            }
        }

        // Try to place a new gem on a detected surface.
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            GameObject gem = Instantiate(gemPrefab, hitPose.position, hitPose.rotation);
            placedGems.Add(gem);
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Gems collected: " + score;
    }
}
