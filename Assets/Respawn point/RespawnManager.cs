using UnityEngine;
using System.Collections.Generic;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Drag the Player object here.")]
    public Transform player;

    private Transform currentRespawnPoint;
    private List<RespawnableObject> destroyedObjects = new List<RespawnableObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetRespawnPoint(Transform door)
    {
        currentRespawnPoint = door;
        Debug.Log("🚪 Respawn point updated to: " + door.name);
    }

    public void RegisterDestroyed(RespawnableObject obj)
    {
        if (!destroyedObjects.Contains(obj)) destroyedObjects.Add(obj);
    }

    public void RespawnPlayer()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        // 1. Move Player to Respawn Point
        if (currentRespawnPoint != null)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = Vector3.zero; // Stop falling/moving

            player.position = currentRespawnPoint.position;
            player.rotation = currentRespawnPoint.rotation;
            Debug.Log("🔄 Player respawned at " + currentRespawnPoint.name);
        }

        // 2. Reset Player Health and Energy
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null) playerHealth.ResetHealth();

        Energy playerEnergy = player.GetComponent<Energy>();
        if (playerEnergy != null) playerEnergy.currentEnergy = playerEnergy.maxEnergy;

        // 3. Reset all destroyed objects
        foreach (var obj in destroyedObjects)
        {
            if (obj != null) obj.RespawnObject();
        }
        destroyedObjects.Clear();
        Debug.Log("🔄 All destroyed objects have been reset.");
    }
}