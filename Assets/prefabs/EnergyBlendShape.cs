using UnityEngine;

public class EnergyBlendShape : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your Player (the object with the Energy script) into this slot.")]
    public Energy playerEnergy;
    
    [Tooltip("The exact name of the BlendShape in your model.")]
    public string blendShapeName = "Energy Level";

    private SkinnedMeshRenderer skinnedMesh;
    private int blendShapeIndex = -1;

    void Start()
    {
        // 1. Find the SkinnedMeshRenderer on this object
        skinnedMesh = GetComponent<SkinnedMeshRenderer>();
        
        if (skinnedMesh == null)
        {
            Debug.LogError("🔴 EnergyBlendShape: This object does NOT have a SkinnedMeshRenderer! BlendShapes only work on Skinned Meshes.");
            return;
        }

        // 2. Find the BlendShape by name
        blendShapeIndex = skinnedMesh.sharedMesh.GetBlendShapeIndex(blendShapeName);
        
        if (blendShapeIndex == -1)
        {
            Debug.LogError($"🔴 EnergyBlendShape: Could not find a BlendShape named '{blendShapeName}' on this mesh! Check the spelling and capitalization in your 3D model.");
            return;
        }
        
        Debug.Log($"🟢 EnergyBlendShape: Found '{blendShapeName}' at index {blendShapeIndex}.");

        // 3. Auto-find the player's Energy script if you forgot to drag it in
        if (playerEnergy == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerEnergy = playerObj.GetComponent<Energy>();
            }
            
            if (playerEnergy == null)
            {
                Debug.LogError("🔴 EnergyBlendShape: Could not find the Energy script on the Player! Drag it in manually.");
            }
        }
    }

    void Update()
    {
        if (skinnedMesh == null || blendShapeIndex == -1 || playerEnergy == null) return;

        // Calculate the percentage of energy (0.0 to 1.0)
        float energyPercent = playerEnergy.currentEnergy / playerEnergy.maxEnergy;
        
        // Convert to BlendShape weight (0 to 100)
        float blendShapeWeight = energyPercent * 100f;
        
        // Apply the weight to the BlendShape
        skinnedMesh.SetBlendShapeWeight(blendShapeIndex, blendShapeWeight);
    }
}