using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    [Header("Battery Settings")]
    [Tooltip("How much energy this battery gives to the player.")]
    public float energyAmount = 25f;
    
    [Header("Pickup Delay")]
    [Tooltip("How many seconds the battery must wait before it can be picked up.")]
    public float pickupDelay = 1.5f;

    private Collider batteryCollider;

    void Start()
    {
        // Find the collider on this battery
        batteryCollider = GetComponent<Collider>();
        
        // If we have a pickup delay, disable the collider so it can't be picked up yet
        if (pickupDelay > 0f && batteryCollider != null)
        {
            batteryCollider.enabled = false;
            
            // Re-enable the collider after the delay
            Invoke("EnablePickup", pickupDelay);
        }
    }

    void EnablePickup()
    {
        if (batteryCollider != null)
        {
            batteryCollider.enabled = true;
            Debug.Log("🔋 Battery is now pickupable!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Energy playerEnergy = other.GetComponent<Energy>();
            
            if (playerEnergy != null)
            {
                playerEnergy.currentEnergy += energyAmount;
                playerEnergy.currentEnergy = Mathf.Clamp(playerEnergy.currentEnergy, 0, playerEnergy.maxEnergy);
                
                Debug.Log("🔋 Picked up battery! +" + energyAmount + " Energy.");
            }
            
            Destroy(gameObject);
        }
    }
}