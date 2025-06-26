using UnityEngine;

public class Jumpboard : MonoBehaviour
{
    public float jumpBoostMultiplier = 5.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<StarterAssets.FirstPersonController>();
            if (controller != null)
            {
                controller.JumpMultiplier = jumpBoostMultiplier;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<StarterAssets.FirstPersonController>();
            if (controller != null)
            {
                controller.JumpMultiplier = 1f; // reset to normal
            }
        }
    }
}
