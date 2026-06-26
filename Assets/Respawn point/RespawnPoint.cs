using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    // This triggers when the Player walks into the door's collider
    private void OnTriggerEnter(Collider other)
    {
        // Make sure the thing walking through is the Player
        if (other.CompareTag("Player"))
        {
            // Tell the RespawnManager to update the checkpoint to THIS door
            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.SetRespawnPoint(transform);
            }
        }
    }
}