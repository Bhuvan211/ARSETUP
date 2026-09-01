using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARSolarSystemController : MonoBehaviour
{
    public List<GameObject> planets;
    public Transform arCamera;

    [Header("Settings")]
    public float stationaryWaitTime = 3.0f;
    public float movementThreshold = 0.2f;
    public float spawnDistanceFromCamera = 1.2f;

    private int currentIndex = 0;
    private float timer = 0f;
    private Vector3 anchorCameraPos;
    private Vector3 lastFramePos;
    private bool inGuidedMode = false;

    // Cache original transform data
    private class TransformData
    {
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    private Dictionary<Transform, TransformData> originalTransforms = new Dictionary<Transform, TransformData>();

    void Start()
    {
        if (arCamera == null && Camera.main != null)
            arCamera = Camera.main.transform;

        if (arCamera != null)
        {
            anchorCameraPos = arCamera.position;
            lastFramePos = arCamera.position;
        }

        // Cache original hierarchy and transform properties
        foreach (GameObject p in planets)
        {
            if (p != null)
            {
                originalTransforms[p.transform] = new TransformData
                {
                    parent = p.transform.parent,
                    localPosition = p.transform.localPosition,
                    localRotation = p.transform.localRotation,
                    localScale = p.transform.localScale
                };
            }
        }

        ShowRegularAR();
    }

    void Update()
    {
        if (arCamera == null || planets.Count == 0) return;

        Vector3 currentCamPos = arCamera.position;

        if (!inGuidedMode)
        {
            float frameDelta = Vector3.Distance(currentCamPos, lastFramePos);

            if (frameDelta < 0.01f)
            {
                timer += Time.deltaTime;
                if (timer >= stationaryWaitTime)
                {
                    inGuidedMode = true;
                    anchorCameraPos = currentCamPos;
                    currentIndex = 0;
                    ShowSinglePlanet(currentIndex);
                }
            }
            else
            {
                timer = 0f;
            }
        }
        else
        {
            float totalDistMoved = Vector3.Distance(currentCamPos, anchorCameraPos);

            if (totalDistMoved > movementThreshold)
            {
                ShowRegularAR();
            }
            else
            {
                timer += Time.deltaTime;
                if (timer >= stationaryWaitTime)
                {
                    currentIndex = (currentIndex + 1) % planets.Count;
                    ShowSinglePlanet(currentIndex);
                }
            }
        }

        lastFramePos = currentCamPos;
    }

    void ShowSinglePlanet(int index)
    {
        timer = 0f;

        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i] == null) continue;

            bool isActive = (i == index);

            if (isActive)
            {
                // Unparent temporarily to avoid parent scale distortion (Solar System scale is 2,2,2)
                planets[i].transform.SetParent(null, true);
                planets[i].SetActive(true);

                // Position cleanly in front of camera view
                Vector3 targetPos = arCamera.position + (arCamera.forward * spawnDistanceFromCamera);
                planets[i].transform.position = targetPos;
                planets[i].transform.rotation = Quaternion.LookRotation(targetPos - arCamera.position);

                // Disable LOD Group and force top LOD renderers active
                LODGroup lod = planets[i].GetComponent<LODGroup>();
                if (lod != null)
                {
                    lod.enabled = false;
                }

                // Enable all child renderers explicitly
                Renderer[] renderers = planets[i].GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renderers)
                {
                    r.enabled = true;
                }
            }
            else
            {
                planets[i].SetActive(false);
            }
        }
    }

    void ShowRegularAR()
    {
        inGuidedMode = false;
        timer = 0f;

        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i] == null) continue;

            // Restore parent hierarchy and local transform data
            if (originalTransforms.ContainsKey(planets[i].transform))
            {
                TransformData data = originalTransforms[planets[i].transform];
                planets[i].transform.SetParent(data.parent, false);
                planets[i].transform.localPosition = data.localPosition;
                planets[i].transform.localRotation = data.localRotation;
                planets[i].transform.localScale = data.localScale;
            }

            planets[i].SetActive(true);

            // Re-enable LOD Group
            LODGroup lod = planets[i].GetComponent<LODGroup>();
            if (lod != null)
            {
                lod.enabled = true;
                lod.RecalculateBounds();
            }

            Renderer[] renderers = planets[i].GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }
        }
    }
}