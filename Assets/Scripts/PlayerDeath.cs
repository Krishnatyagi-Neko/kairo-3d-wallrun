using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("COLLISION WITH: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            GameManager.Instance.LoadGameOver();
        }
    }
}