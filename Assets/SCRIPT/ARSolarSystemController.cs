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
    [SerializeField] private float selfRotationSpeed = 30f;
    [SerializeField] private float orbitSpeed = 10f;
    [SerializeField] private bool enableMotion = false;
    [SerializeField] private bool enableMoon = false;
    [SerializeField] private bool enableSatellite = false;

    [Header("Audio Reference")]
    public AudioManager audioManager;

    private Transform arCameraTransform;
    private Vector3 originalSystemPosition;
    private Quaternion originalSystemRotation;

    [Header("Solar System, Earth , Moon References and Satellite References")]
    [SerializeField] private Transform moonObject;
    [SerializeField] private Transform earthObject;
    [SerializeField] private Transform satelliteObject;

    private void Start()
    {
        if (Camera.main != null)
        {
            arCameraTransform = Camera.main.transform;
        }

        // Save original position & rotation
        originalSystemPosition = transform.position;
        originalSystemRotation = transform.rotation;

        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManager>();
        }

        StartCoroutine(FullTourSequence());
    }

    private void Update()
    {
        if (!enableMotion) return;

        // 1. Self-Rotation on Y-axis
        foreach (Transform celestialObject in planetTargets)
        {
            if (celestialObject != null)
            {
                celestialObject.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime, Space.Self);
            }
        }

        // 2. Planet Revolution (Orbit around Sun / Index 0)
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

        // ===================================================
        // PHASE 1: 10 Planets Tour (Moon Disabled)
        // ===================================================
        enableMotion = false;
        enableMoon = false;
        enableSatellite = false;
        if (moonObject != null)
        {
            moonObject.gameObject.SetActive(false);
        }
        if (satelliteObject != null)
        {
            satelliteObject.gameObject.SetActive(false);
        }

        for (int i = 0; i < planetTargets.Count; i++)
        {
            Transform targetPlanet = planetTargets[i];

            yield return StartCoroutine(MoveObjectToCamera(targetPlanet));

            if (audioManager != null)
            {
                audioManager.PlayPlanetAudio(i);
                yield return null;

                while (audioManager.IsPlaying())
                {
                    KeepObjectInFrontOfCamera(targetPlanet);
                    yield return null;
                }
            }

            float elapsedPause = 0f;
            while (elapsedPause < pauseAfterAudio)
            {
                elapsedPause += Time.deltaTime;
                KeepObjectInFrontOfCamera(targetPlanet);
                yield return null;
            }
        }

        // ===================================================
        // PHASE 2: Solar System Audio & Motion Enabled
        // ===================================================
        yield return StartCoroutine(ResetToOriginalPosition());

        enableMotion = true; // Enables planet revolution/orbits

        if (audioManager != null)
        {
            audioManager.PlaySolarSystemAudio(0);
            yield return null;

            while (audioManager.IsPlaying())
            {
                yield return null;
            }
        }
        

        yield return new WaitForSeconds(pauseAfterAudio);

        // ===================================================
        // PHASE 3: Focus Camera on Earth & Play Moon Audio
        // ===================================================
        enableMotion = false; // Disable revolution while explaining

        if (moonObject != null)
        {
            moonObject.gameObject.SetActive(true);
            enableMoon = true;
        }
        if (satelliteObject != null)
        {
            satelliteObject.gameObject.SetActive(true);
            enableSatellite = true;
        }

        // Target Earth if assigned; fallback to planetTargets[3] or moonObject if null
        Transform focusTarget = earthObject;
        if (focusTarget == null && planetTargets.Count > 3)
        {
            focusTarget = planetTargets[3]; // Earth is typically index 3
        }
        else if (focusTarget == null)
        {
            focusTarget = moonObject;
        }
        else if (focusTarget == null)
        {
            focusTarget = satelliteObject;
        }

        if (focusTarget != null)
        {
            // Move system so camera focuses directly on Earth
            yield return StartCoroutine(MoveObjectToCamera(focusTarget));

            if (audioManager != null)
            {
                audioManager.PlayMoonAudio(0);
                yield return null;

                while (audioManager.IsPlaying())
                {
                    KeepObjectInFrontOfCamera(focusTarget);
                    yield return null;
                }
            }
            if (audioManager != null)
            {
                audioManager.PlaySatelliteAudio(0);
                yield return null;

                while (audioManager.IsPlaying())
                {
                    KeepObjectInFrontOfCamera(focusTarget);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(pauseAfterAudio);
        }

        enableMotion = true; // Re-enable revolution after Moon audio finishes

        // Final reset back to default world position
        yield return StartCoroutine(ResetToOriginalPosition());
    }

    private IEnumerator MoveObjectToCamera(Transform target)
    {
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * transitionSpeed;

            Vector3 targetCameraPosition = arCameraTransform.position + (arCameraTransform.forward * distanceFromCamera);
            Vector3 offset = target.position - transform.position;
            Vector3 desiredSystemPosition = targetCameraPosition - offset;

            transform.position = Vector3.Lerp(transform.position, desiredSystemPosition, elapsed / duration);
            yield return null;
        }
    }

    private void KeepObjectInFrontOfCamera(Transform target)
    {
        Vector3 targetCameraPosition = arCameraTransform.position + (arCameraTransform.forward * distanceFromCamera);
        Vector3 offset = target.position - transform.position;
        transform.position = targetCameraPosition - offset;
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