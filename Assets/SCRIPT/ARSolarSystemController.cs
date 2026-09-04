using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ARSolarSystemController : MonoBehaviour
{
    public List<GameObject> planets;
    public AudioSource solarAudio;
    public List<AudioClip> clips;

    public float spacing = 0.4f;
    public float closeUpDist = 0.4f;
    public float moveSpeed = 2f;

    private Vector3[] defaultPos;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;

        defaultPos = new Vector3[planets.Count];
        Vector3 center = cam.position + cam.forward * 1f;
        Vector3 right = cam.right;

        for (int i = 0; i < planets.Count; i++)
        {
            float offset = (i - (planets.Count - 1) / 2f) * spacing;
            defaultPos[i] = center + right * offset;
            planets[i].transform.position = defaultPos[i];
        }

        StartCoroutine(Tour());
    }

    IEnumerator Tour()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < planets.Count; i++)
        {
            Vector3 target = cam.position + cam.forward * closeUpDist;
            while (Vector3.Distance(planets[i].transform.position, target) > 0.01f)
            {
                planets[i].transform.position = Vector3.MoveTowards(
                    planets[i].transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            if (i < clips.Count)
            {
                solarAudio.clip = clips[i];
                solarAudio.Play();
                yield return new WaitUntil(() => !solarAudio.isPlaying);
            }

            yield return new WaitForSeconds(0.5f);
        }

        for (int i = 0; i < planets.Count; i++)
        {
            StartCoroutine(ReturnPlanet(planets[i], defaultPos[i]));
        }
    }

    IEnumerator ReturnPlanet(GameObject planet, Vector3 target)
    {
        while (Vector3.Distance(planet.transform.position, target) > 0.01f)
        {
            planet.transform.position = Vector3.MoveTowards(
                planet.transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}