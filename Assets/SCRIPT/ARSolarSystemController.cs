using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARSolarSystemController : MonoBehaviour
{
    [Header("Celestial Objects (Sun to Pluto)")]
    [SerializeField] private List<Transform> planetTargets = new List<Transform>();

    [Header("Tour Settings")]
    [SerializeField] private float pauseAfterAudio = 1.0f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float distanceFromCamera = 0.6f;

    [Header("Rotation & Revolution Settings")]
    [SerializeField] private float selfRotationSpeed = 30f;
    [SerializeField] private float orbitSpeed = 10f;
    [SerializeField] private bool enableMotion = false;
    [SerializeField] private bool enableMoon = false;
    [SerializeField] private bool enableSatellite = false;

    [Header("Audio Reference")]
    public AudioManager audioManager;

    [Header("Solar System, Earth, Moon & Satellite References")]
    [SerializeField] private Transform moonObject;
    [SerializeField] private Transform earthObject;
    [SerializeField] private Transform satelliteObject;

    [Header("Satellite Orbit Animation Parameters")]
    [SerializeField] private float lowOrbitRadius = 0.15f;
    [SerializeField] private float mediumOrbitRadius = 0.3f;
    [SerializeField] private float geoOrbitRadius = 0.5f;
    [SerializeField] private float polarOrbitRadius = 0.2f;

    [SerializeField] private float satelliteOrbitSpeed = 80f;

    private Transform arCameraTransform;
    private Vector3 originalSystemPosition;
    private Quaternion originalSystemRotation;

    // Track active orbit animation
    private Coroutine activeSatelliteOrbitCoroutine;

    private void Start()
    {
        if (Camera.main != null)
            arCameraTransform = Camera.main.transform;

        originalSystemPosition = transform.position;
        originalSystemRotation = transform.rotation;

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        StartCoroutine(FullTourSequence());
    }

    private void Update()
    {
        if (!enableMotion) return;

        // 1. Self-Rotation on Y-axis
        foreach (Transform celestialObject in planetTargets)
        {
            if (celestialObject != null)
                celestialObject.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime, Space.Self);
        }

        // 2. Planet Revolution around Sun (Index 0)
        if (planetTargets.Count > 0 && planetTargets[0] != null)
        {
            Vector3 centerPoint = planetTargets[0].position;

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

    private IEnumerator FullTourSequence()
    {
        yield return new WaitForSeconds(1.5f);

        // PHASE 1: Planets Tour (Moon & Satellite Disabled)
        SetObjectsState(false);

        for (int i = 0; i < planetTargets.Count; i++)
        {
            Transform targetPlanet = planetTargets[i];

            yield return StartCoroutine(MoveObjectToCamera(targetPlanet));

            if (audioManager != null)
            {
                audioManager.PlayPlanetAudio(i);
                yield return StartCoroutine(WaitForAudioAndMaintainPosition(targetPlanet));
            }

            yield return StartCoroutine(PauseAndMaintainPosition(targetPlanet));
        }

        // PHASE 2: Solar System Overview
        yield return StartCoroutine(ResetToOriginalPosition());
        enableMotion = true;

        if (audioManager != null)
        {
            audioManager.PlaySolarSystemAudio(0);
            yield return new WaitWhile(() => audioManager.IsPlaying());
        }

        yield return new WaitForSeconds(pauseAfterAudio);

        // PHASE 3: Focus on Earth & Moon / Satellite / Orbit Explanations
        enableMotion = false;
        SetObjectsState(true);

        Transform focusTarget = GetFocusTarget();

        if (focusTarget != null)
        {
            yield return StartCoroutine(MoveObjectToCamera(focusTarget));

            if (audioManager != null)
            {
                // 1. Moon Audio
                audioManager.PlayMoonAudio(0);
                yield return StartCoroutine(WaitForAudioAndMaintainPosition(focusTarget));

                // 2. Satellite Audio
                audioManager.PlaySatelliteAudio(0);
                yield return StartCoroutine(WaitForAudioAndMaintainPosition(focusTarget));

                // 3. Satellite Orbit Audios & Synchronized Animations
                // Index 0: Low Orbit, 1: Medium Orbit, 2: Geostationary Orbit, 3: Polar Orbit
                int orbitAudioCount = audioManager.GetSatelliteOrbitAudioCount();
                for (int j = 0; j < orbitAudioCount; j++)
                {
                    // Start specific orbit animation for current orbit type
                    StartSatelliteOrbitAnimation(j);

                    audioManager.PlaySatelliteOrbitAudio(j);
                    yield return StartCoroutine(WaitForAudioAndMaintainPosition(focusTarget));
                    yield return StartCoroutine(PauseAndMaintainPosition(focusTarget));

                    StopSatelliteOrbitAnimation();
                }
            }

            yield return StartCoroutine(PauseAndMaintainPosition(focusTarget));
        }

        enableMotion = true;
        yield return StartCoroutine(ResetToOriginalPosition());
    }

    // --- Satellite Orbit Animation Logic ---

    private void StartSatelliteOrbitAnimation(int orbitTypeIndex)
    {
        StopSatelliteOrbitAnimation();

        if (satelliteObject == null || earthObject == null) return;

        float radius = lowOrbitRadius;
        Vector3 rotationAxis = Vector3.up; // Default equatorial orbit axis

        switch (orbitTypeIndex)
        {
            case 0: // Lower Earth Orbit (LEO) - Fast, close orbit, slight inclination
                radius = lowOrbitRadius;
                rotationAxis = Quaternion.Euler(15f, 0f, 0f) * Vector3.up;
                break;

            case 1: // Medium Earth Orbit (MEO) - Mid-range distance, 55 degree inclination
                radius = mediumOrbitRadius;
                rotationAxis = Quaternion.Euler(55f, 0f, 0f) * Vector3.up;
                break;

            case 2: // Geostationary Orbit (GEO) - Distant, equatorial orbit (0 degree tilt)
                radius = geoOrbitRadius;
                rotationAxis = Vector3.up;
                break;

            case 3: // Polar Orbit - Passes directly over poles (90 degree tilt)
                radius = polarOrbitRadius;
                rotationAxis = Vector3.right;
                break;
        }

        activeSatelliteOrbitCoroutine = StartCoroutine(AnimateSatelliteOrbit(radius, rotationAxis));
    }

    private void StopSatelliteOrbitAnimation()
    {
        if (activeSatelliteOrbitCoroutine != null)
        {
            StopCoroutine(activeSatelliteOrbitCoroutine);
            activeSatelliteOrbitCoroutine = null;
        }
    }

    private IEnumerator AnimateSatelliteOrbit(float radius, Vector3 rotationAxis)
    {
        float angle = 0f;

        while (true)
        {
            if (earthObject != null && satelliteObject != null)
            {
                angle += satelliteOrbitSpeed * Time.deltaTime;

                // Calculate position relative to Earth based on tilt axis and radius
                Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis);
                Vector3 orbitOffset = rotation * (Vector3.forward * radius);

                satelliteObject.position = earthObject.position + orbitOffset;
                satelliteObject.LookAt(earthObject); // Keep satellite oriented toward Earth
            }

            yield return null;
        }
    }

    // --- Helper Methods ---

    private void SetObjectsState(bool active)
    {
        enableMotion = active;
        enableMoon = active;
        enableSatellite = active;

        if (moonObject != null) moonObject.gameObject.SetActive(active);
        if (satelliteObject != null) satelliteObject.gameObject.SetActive(active);
    }

    private Transform GetFocusTarget()
    {
        if (earthObject != null) return earthObject;
        if (planetTargets.Count > 3 && planetTargets[3] != null) return planetTargets[3];
        if (moonObject != null) return moonObject;
        return satelliteObject;
    }

    private IEnumerator MoveObjectToCamera(Transform target)
    {
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            Vector3 desiredPosition = GetCameraTargetPosition(target);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, elapsed / duration);
            yield return null;
        }
    }

    private void KeepObjectInFrontOfCamera(Transform target)
    {
        transform.position = GetCameraTargetPosition(target);
    }

    private Vector3 GetCameraTargetPosition(Transform target)
    {
        Vector3 targetCameraPosition = arCameraTransform.position + (arCameraTransform.forward * distanceFromCamera);
        Vector3 offset = target.position - transform.position;
        return targetCameraPosition - offset;
    }

    private IEnumerator WaitForAudioAndMaintainPosition(Transform target)
    {
        yield return null;
        while (audioManager.IsPlaying())
        {
            KeepObjectInFrontOfCamera(target);
            yield return null;
        }
    }

    private IEnumerator PauseAndMaintainPosition(Transform target)
    {
        float elapsed = 0f;
        while (elapsed < pauseAfterAudio)
        {
            elapsed += Time.deltaTime;
            KeepObjectInFrontOfCamera(target);
            yield return null;
        }
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