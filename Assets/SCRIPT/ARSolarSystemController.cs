using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ARSolarSystemController : MonoBehaviour
{
    [Header("Celestial Objects (Sun to Pluto)")]
    [SerializeField] private List<Transform> planetTargets = new List<Transform>();

    [Header("Tour Settings")]
    [SerializeField] private float pauseAfterAudio = 1.0f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float distanceFromCamera = 0.6f;

    [Header("Rotation & Revolution Settings")]
    [SerializeField] private float selfRotationSpeed = 30f; // Speed of planets spinning on their Y-axis
    [SerializeField] private float orbitSpeed = 10f;        // Speed of planets orbiting around the Sun
    [SerializeField] private bool enableMotion = true;       // Toggle motion on/off

    [Header("Audio Reference")]
    public AudioManager audioManager;

    private Transform arCameraTransform;
    private Vector3 originalSystemPosition;
    private Quaternion originalSystemRotation;

    private void Start()
    {
        if (Camera.main != null)
        {
            arCameraTransform = Camera.main.transform;
        }

        // Store original position and rotation of the Solar System root
        originalSystemPosition = transform.position;
        originalSystemRotation = transform.rotation;

        // Auto-find AudioManager if unassigned
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManager>();
        }

        StartCoroutine(StartTourRoutine());
    }

    private void Update()
    {
        if (!enableMotion) return;

        // 1. Self-Rotation (Spinning on Y-axis) for all targets
        foreach (Transform celestialObject in planetTargets)
        {
            if (celestialObject != null)
            {
                celestialObject.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime, Space.Self);
            }
        }

        // 2. Revolution (Orbiting around the Sun / Index 0)
        if (planetTargets.Count > 0 && planetTargets[0] != null)
        {
            Vector3 centerPoint = planetTargets[0].position; // Sun's position as orbit center

            for (int i = 1; i < planetTargets.Count; i++)
            {
                if (planetTargets[i] != null)
                {
                    float adjustedOrbitSpeed = orbitSpeed / Mathf.Sqrt(i);
                    planetTargets[i].RotateAround(centerPoint, Vector3.up, adjustedOrbitSpeed * Time.deltaTime);
                }
            }
        }
    }

    private IEnumerator StartTourRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < planetTargets.Count; i++)
        {
            Transform targetPlanet = planetTargets[i];

            // 1. Smoothly transition to position the planet in front of camera
            yield return StartCoroutine(MovePlanetToCamera(targetPlanet));

            // 2. Play corresponding audio clip
            if (audioManager != null)
            {
                audioManager.PlayPlanetAudio(i);

                yield return null; // Wait 1 frame for audio state update

                // 3. Continuously adjust system position while audio plays so camera tracks planet orbit
                while (audioManager.IsPlaying())
                {
                    KeepPlanetInFrontOfCamera(targetPlanet);
                    yield return null;
                }
            }

            // 4. Brief delay while continuing to track planet before moving to next target
            float elapsedPause = 0f;
            while (elapsedPause < pauseAfterAudio)
            {
                elapsedPause += Time.deltaTime;
                KeepPlanetInFrontOfCamera(targetPlanet);
                yield return null;
            }
        }

        // Tour completed: Smoothly return system to starting transform
        yield return StartCoroutine(ResetToOriginalPosition());
    }

    private IEnumerator MovePlanetToCamera(Transform targetPlanet)
    {
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * transitionSpeed;

            Vector3 targetCameraPosition = arCameraTransform.position + (arCameraTransform.forward * distanceFromCamera);
            Vector3 planetOffset = targetPlanet.position - transform.position;
            Vector3 desiredSystemPosition = targetCameraPosition - planetOffset;

            transform.position = Vector3.Lerp(transform.position, desiredSystemPosition, elapsed / duration);
            yield return null;
        }
    }

    private void KeepPlanetInFrontOfCamera(Transform targetPlanet)
    {
        Vector3 targetCameraPosition = arCameraTransform.position + (arCameraTransform.forward * distanceFromCamera);
        Vector3 planetOffset = targetPlanet.position - transform.position;
        transform.position = targetCameraPosition - planetOffset;
    }

    private IEnumerator ResetToOriginalPosition()
    {
        float elapsed = 0f;
        float duration = 2.0f;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPosition, originalSystemPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, originalSystemRotation, t);

            yield return null;
        }

        transform.position = originalSystemPosition;
        transform.rotation = originalSystemRotation;
    }
}