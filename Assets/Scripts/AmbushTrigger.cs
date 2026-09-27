using UnityEngine;

public class AmbushTrigger : MonoBehaviour
{
    public AmbusherEnemy ambusher;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ambusher.Activate();
        }
    }
}