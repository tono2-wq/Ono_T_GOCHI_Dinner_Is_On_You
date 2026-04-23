using UnityEngine;
using System.Collections;

public class LastPlaceDroneController : MonoBehaviour
{
    public static LastPlaceDroneController instance;

    public Transform drone; // LastPlaceDrone_0 transform
    public AudioSource audioSource;
    public AudioClip lastPlaceTimpaniRoll;

    void Awake()
    {
        instance = this;
    }

    public IEnumerator StartDroneSequence(PlayerData targetPlayer, bool hasPitari, PlayerData secondPlace, PlayerData firstPlace)
    {
        if (drone == null)
        {
            Debug.LogError("Drone Transform not assigned!");
            yield break;
        }

        // Initial position
        drone.localPosition = new Vector3(-266.42f, -178.14f, drone.localPosition.z);

        // Determine target X
        float targetX = -246.82f; // default "You"
        if (targetPlayer.playerName == "Yabe") targetX = -239.36f;
        if (targetPlayer.playerName == "Okamura") targetX = -233.35f;

        // Random starting player
        PlayerData startPlayer = null;
        if (hasPitari)
        {
            startPlayer = Random.value < 0.5f ? firstPlace : targetPlayer;
        }
        else
        {
            startPlayer = Random.value < 0.5f ? secondPlace : targetPlayer;
        }

        // Set X start based on player
        float startX = drone.localPosition.x;
        if (startPlayer.playerName == "You") startX = -246.82f;
        else if (startPlayer.playerName == "Yabe") startX = -239.36f;
        else if (startPlayer.playerName == "Okamura") startX = -233.35f;

        drone.localPosition = new Vector3(startX, -178.14f, drone.localPosition.z);

        // Random laps: 0.5–2
        float totalLaps = Random.Range(0.5f, 2f);
        int halfLaps = Mathf.CeilToInt(totalLaps / 0.5f);

        for (int i = 0; i < halfLaps; i++)
        {
            // Glide X toward target
            float progress = (i + 1f) / halfLaps;
            float newX = Mathf.Lerp(startX, targetX, progress);
            float durationX = 2f; // you can randomize a bit
            StartCoroutine(GlideX(drone, drone.localPosition.x, newX, durationX));

            // 35% chance drone dips down
            if (Random.value < 0.35f)
            {
                yield return StartCoroutine(GlideY(drone, drone.localPosition.y, -180.76f, 2f));
                yield return StartCoroutine(GlideY(drone, -180.76f, -178.14f, Random.Range(0.5f, 1f)));
            }

            yield return new WaitForSeconds(durationX);
        }

        // Final 0.5 lap: glide X to exact target & Y down to -181
        float finalDuration = 2f;
        StartCoroutine(GlideX(drone, drone.localPosition.x, targetX, finalDuration));
        yield return StartCoroutine(GlideY(drone, drone.localPosition.y, -181f, 2f));

        // Stop timpani roll SFX
        if (audioSource != null && lastPlaceTimpaniRoll != null)
            audioSource.Stop();
    }

    IEnumerator GlideX(Transform t, float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float x = Mathf.Lerp(from, to, timer / duration);
            t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
            yield return null;
        }
        t.localPosition = new Vector3(to, t.localPosition.y, t.localPosition.z);
    }


    IEnumerator GlideY(Transform t, float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float y = Mathf.Lerp(from, to, timer / duration);
            t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
            yield return null;
        }
        t.localPosition = new Vector3(t.localPosition.x, to, t.localPosition.z);
    }
}