using UnityEngine;

public class ProjectileEnemy : MonoBehaviour
{
    public GameObject projectile;
    public float projectileSpeed = 20f;
    public Transform spawnPoint; 
    public float shootInterval = 2f;
    
    private float shootTimer;

    private void Start()
    {
        projectile.SetActive(false);
    }

    private void Update()
    {
        shootTimer += Time.deltaTime;

        if (shootTimer >= shootInterval)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    public void Shoot()
    {
        projectile.transform.position = spawnPoint.position;
        projectile.transform.rotation = spawnPoint.rotation;

        Projectile projectileScript = projectile.GetComponent<Projectile>();

        Vector3 direction = spawnPoint.forward;
        direction.y = 0f;
        direction.Normalize();

        projectileScript.Fire(direction);
    }
}