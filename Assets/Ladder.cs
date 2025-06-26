using UnityEngine;

public class Ladder : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<StarterAssets.FirstPersonController>();
            if (controller != null)
            {
                controller.EnterLadder(transform.up); // pass ladder direction
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
                controller.ExitLadder();
            }
        }
    }
}
