using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour
{
    [Header("Objects")]
    public GameObject leftClickObject;
    public GameObject rightClickObject;

    [Header("Audio")]
    public AudioSource leftAudio;
    public AudioSource rightAudio;

    [Header("BlendShapes")]
    public SkinnedMeshRenderer leftMesh;
    public int leftBlendShapeIndex = 0;

    public SkinnedMeshRenderer rightMesh;
    public int rightBlendShapeIndex = 0;

    [Header("Timing")]
    public float maxHoldTime = 3f;
    public float rechargeTime = 5f;

    float leftCharge;
    float rightCharge;

    bool leftCoolingDown;
    bool rightCoolingDown;

    void Start()
    {
        leftCharge = maxHoldTime;
        rightCharge = maxHoldTime;

        UpdateBlend(leftMesh, leftBlendShapeIndex, 1f);
        UpdateBlend(rightMesh, rightBlendShapeIndex, 1f);

        DisableAll(leftClickObject, leftAudio);
        DisableAll(rightClickObject, rightAudio);
    }

    void Update()
    {
        HandleLeft();
        HandleRight();
    }

    void HandleLeft()
    {
        if (leftCoolingDown)
            return;

        if (Input.GetMouseButton(0) && leftCharge > 0f)
        {
            leftCharge -= Time.deltaTime;
            leftCharge = Mathf.Max(0f, leftCharge);

            EnableAll(leftClickObject, leftAudio);
            UpdateBlend(leftMesh, leftBlendShapeIndex, leftCharge / maxHoldTime);

            if (leftCharge <= 0f)
                StartCoroutine(RechargeLeft());
        }
        else
        {
            DisableAll(leftClickObject, leftAudio);
        }
    }

    void HandleRight()
    {
        if (rightCoolingDown)
            return;

        if (Input.GetMouseButton(1) && rightCharge > 0f)
        {
            rightCharge -= Time.deltaTime;
            rightCharge = Mathf.Max(0f, rightCharge);

            EnableAll(rightClickObject, rightAudio);
            UpdateBlend(rightMesh, rightBlendShapeIndex, rightCharge / maxHoldTime);

            if (rightCharge <= 0f)
                StartCoroutine(RechargeRight());
        }
        else
        {
            DisableAll(rightClickObject, rightAudio);
        }
    }

    IEnumerator RechargeLeft()
    {
        leftCoolingDown = true;
        DisableAll(leftClickObject, leftAudio);

        float t = 0f;
        while (t < rechargeTime)
        {
            t += Time.deltaTime;
            UpdateBlend(leftMesh, leftBlendShapeIndex, t / rechargeTime);
            yield return null;
        }

        leftCharge = maxHoldTime;
        UpdateBlend(leftMesh, leftBlendShapeIndex, 1f);
        leftCoolingDown = false;
    }

    IEnumerator RechargeRight()
    {
        rightCoolingDown = true;
        DisableAll(rightClickObject, rightAudio);

        float t = 0f;
        while (t < rechargeTime)
        {
            t += Time.deltaTime;
            UpdateBlend(rightMesh, rightBlendShapeIndex, t / rechargeTime);
            yield return null;
        }

        rightCharge = maxHoldTime;
        UpdateBlend(rightMesh, rightBlendShapeIndex, 1f);
        rightCoolingDown = false;
    }

    void EnableAll(GameObject obj, AudioSource audio)
    {
        if (obj && !obj.activeSelf)
            obj.SetActive(true);

        if (audio && !audio.isPlaying)
            audio.Play();
    }

    void DisableAll(GameObject obj, AudioSource audio)
    {
        if (obj && obj.activeSelf)
            obj.SetActive(false);

        if (audio && audio.isPlaying)
            audio.Stop();
    }

    void UpdateBlend(SkinnedMeshRenderer mesh, int index, float normalized)
    {
        if (!mesh) return;
        mesh.SetBlendShapeWeight(index, normalized * 100f);
    }
}