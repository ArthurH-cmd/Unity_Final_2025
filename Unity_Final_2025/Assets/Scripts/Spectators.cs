using UnityEngine;
using System.Collections;

public class Spectators : MonoBehaviour
{
    [Header("Spectator Prefab")]
    public GameObject spectatorPrefab;

    [Header("Spawn Settings")]
    public int spectatorCount = 50;
    public float spawnRadius = 10f;
    public float innerRadius = 5f; // Keep spectators away from ring center

    [Header("Jump Settings")]
    public float minJumpInterval = 0.5f;
    public float maxJumpInterval = 3f;
    public float jumpHeight = 0.5f;
    public float jumpDuration = 0.5f;

    [Header("Wave Settings")]
    public bool enableWaves = true;
    public float waveInterval = 5f;
    public float waveSpeed = 2f;

    [Header("Excitement")]
    public bool reactToHits = true;
    public float excitementRadius = 3f; // How close to react to hits

    private Transform[] spectators;
    private Vector3[] spectatorBasePositions;
    private Coroutine[] jumpCoroutines;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnSpectators();

        if (enableWaves)
        {
            StartCoroutine(WaveRoutine());
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnSpectators()
    {
        spectators = new Transform[spectatorCount];
        spectatorBasePositions = new Vector3[spectatorCount];
        jumpCoroutines = new Coroutine[spectatorCount];

        for (int i = 0; i < spectatorCount; i++)
        {
            // Spawn in a ring around the center
            float angle = (i / (float)spectatorCount) * 360f * Mathf.Deg2Rad;
            float distance = Random.Range(innerRadius, spawnRadius);

            Vector3 spawnPos = new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            GameObject spectator = Instantiate(spectatorPrefab, transform.position + spawnPos, Quaternion.identity, transform);

            // Make them face the center
            spectator.transform.LookAt(transform.position);

            spectators[i] = spectator.transform;
            spectatorBasePositions[i] = spectator.transform.position;

            // Start random jumping
            jumpCoroutines[i] = StartCoroutine(RandomJumpRoutine(i));
        }
    }

    IEnumerator RandomJumpRoutine(int spectatorIndex)
    {
        while (true)
        {
            float waitTime = Random.Range(minJumpInterval, maxJumpInterval);
            yield return new WaitForSeconds(waitTime);

            yield return JumpSpectator(spectatorIndex);
        }
    }

    IEnumerator JumpSpectator(int spectatorIndex)
    {
        if (spectators[spectatorIndex] == null) yield break;

        Transform spectator = spectators[spectatorIndex];
        Vector3 startPos = spectator.position;
        Vector3 targetPos = startPos + Vector3.up * jumpHeight;

        float elapsed = 0f;

        // Jump up
        while (elapsed < jumpDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (jumpDuration / 2f);
            spectator.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        elapsed = 0f;

        // Jump down
        while (elapsed < jumpDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (jumpDuration / 2f);
            spectator.position = Vector3.Lerp(targetPos, startPos, t);
            yield return null;
        }

        spectator.position = startPos;
    }

    IEnumerator WaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(waveInterval);

            // Create a wave effect
            for (int i = 0; i < spectatorCount; i++)
            {
                StartCoroutine(JumpSpectator(i));
                yield return new WaitForSeconds(1f / waveSpeed / spectatorCount);
            }
        }
    }

    // Call this from your combat system when a hit occurs
    public void OnHitOccurred(Vector3 hitPosition, bool isHeavyHit)
    {
        if (!reactToHits) return;

        float radius = isHeavyHit ? excitementRadius * 1.5f : excitementRadius;

        for (int i = 0; i < spectatorCount; i++)
        {
            if (spectators[i] == null) continue;

            float distance = Vector3.Distance(spectators[i].position, hitPosition);

            if (distance < radius)
            {
                // Make nearby spectators jump in excitement
                if (jumpCoroutines[i] != null)
                {
                    StopCoroutine(jumpCoroutines[i]);
                }
                StartCoroutine(JumpSpectator(i));
                jumpCoroutines[i] = StartCoroutine(RandomJumpRoutine(i));
            }
        }
    }

    // Call this when a player is defeated for dramatic effect
    public void OnPlayerDefeated()
    {
        StartCoroutine(AllJumpRoutine());
    }

    IEnumerator AllJumpRoutine()
    {
        // Make everyone jump multiple times
        for (int wave = 0; wave < 3; wave++)
        {
            for (int i = 0; i < spectatorCount; i++)
            {
                StartCoroutine(JumpSpectator(i));
            }
            yield return new WaitForSeconds(jumpDuration * 1.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize spawn area
        Gizmos.color = Color.green;
        DrawCircle(transform.position, spawnRadius, 32);

        Gizmos.color = Color.red;
        DrawCircle(transform.position, innerRadius, 32);
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}