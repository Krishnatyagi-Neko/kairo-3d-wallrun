using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;

    private Vector3 direction;

    public void Fire(Vector3 fireDirection)
    {
        direction = fireDirection.normalized;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.LoadGameOver();
        }
    }
}