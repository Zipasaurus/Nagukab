using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnableObject : MonoBehaviour
{
    private Health health;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private List<GameObject> spawnedDebris = new List<GameObject>();

    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        health = GetComponent<Health>();
    }

    public void RegisterDebris(GameObject debris) { spawnedDebris.Add(debris); }

    public void OnObjectDestroyed()
    {
        if (RespawnManager.Instance != null) RespawnManager.Instance.RegisterDestroyed(this);
        gameObject.SetActive(false); // Hide the object instead of destroying it
    }

    public void RespawnObject()
    {
        gameObject.SetActive(true); // Un-hide the object
        transform.position = startPosition;
        transform.rotation = startRotation;
        
        if (health != null) health.ResetHealth();
        
        // Reset Animator to default state
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.Rebind();

        // Destroy any debris (like broken crate pieces) that was spawned when it broke
        foreach (var debris in spawnedDebris)
        {
            if (debris != null) Destroy(debris);
        }
        spawnedDebris.Clear();
    }
}