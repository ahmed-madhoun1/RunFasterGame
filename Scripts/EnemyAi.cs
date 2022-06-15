
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform playerTransform;

    [SerializeField]
    private Animator enemyAnimator;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    public EnemyGun enemyGunScript;
    [SerializeField]
    private int enemyDamage = 5;

    private void Awake()
    {
        playerTransform = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;

        enemyAnimator.SetBool("Attack", false);
        enemyAnimator.SetBool("Chase", false);
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }
    private void ChasePlayer()
    {
        enemyAnimator.SetBool("Attack", false);
        enemyAnimator.SetBool("Chase", true);
        agent.SetDestination(playerTransform.position);
    }
    private void AttackPlayer()
    {
        enemyAnimator.SetBool("Attack", true);
        enemyAnimator.SetBool("Chase", false);
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);
        transform.LookAt(playerTransform);

        if (!alreadyAttacked)
        {
            ///Attack code here
            enemyGunScript.Shoot();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
  
    private void OnTriggerEnter(Collider collider)
    {
       if (collider.gameObject.tag == "Player" && collider.gameObject.GetComponent<PlayerMovementAdvanced>().isPlayerSliding)
        {
            // Make Enemy Dead
            Destroy(this.gameObject);
        }
    }

    public void TakeDamage(GameObject enemy)
    {
        health -= enemyDamage;

        if (health <= 0) {
            enemy.transform.Rotate(-90, transform.rotation.y, transform.rotation.z);
            this.enabled = false;
        };
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    
}
