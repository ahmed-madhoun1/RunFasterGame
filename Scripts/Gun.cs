using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Gun : MonoBehaviour
{
    // Add some randomized bullet variants so we're not always shooting in a perfectly straight line
    [SerializeField]
    private bool AddBulletSpread = true;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    private ParticleSystem ShootingParticleSystem;
    // A point that we'll be doing a raycast from
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    // The visualization that we showed to the player of where their bullet would be going without actually spawing a bullet
    [SerializeField]
    private TrailRenderer BulletTrailRenderer;
    [SerializeField]
    private Transform playerCamera;
    [SerializeField]
    private float ShootDelay = 0.5f;
    // The layer that we can hit with our raycast bullets 
    [SerializeField]
    private LayerMask Mask;
    private Animator GunShootAnimator;
    private float LastShootTime;
    [SerializeField]
    public GunType gunType;
    [SerializeField]
    private ShootAnim shootAnim;
    

    public enum GunType
    {
        Pistol,
        MachineGun,
        Shotgun,
        GrapplingGun
    }
    private void Awake()
    {
        GunShootAnimator = GetComponent<Animator>();
    }
    public void Shoot()
    {
        if (LastShootTime + ShootDelay < Time.time)
        {

            shootAnim.RecoilFire();
            ShootingParticleSystem.Play();
            Vector3 direction = GetDirection();
            

            if (Physics.Raycast(playerCamera.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                // That way our trail starts exactly at the point that bullets would spawn from
                TrailRenderer trail = Instantiate(BulletTrailRenderer, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, hit));

                LastShootTime = Time.time;
            }
        }
    }

    // The direction that we're going to shoot this raycast
    private Vector3 GetDirection()
    {
        Vector3 direction = playerCamera.forward;

        if (AddBulletSpread)
        {
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit)
    {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;
        
        while (time < 1)
        {
            // So we're going to move the trail renderer from wherever it spawned to that hit point over some time
            Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }
        if (Hit.collider.gameObject.tag == "Enemy")
        {
            Hit.collider.gameObject.GetComponent<EnemyAi>().TakeDamage(Hit.collider.gameObject);
        }
        // Once we finally move that trail to that hit point
        Trail.transform.position = Hit.point;
        Instantiate(ImpactParticleSystem, Hit.point, Quaternion.LookRotation(Hit.normal));

        Destroy(Trail.gameObject, Trail.time);
    }
}
