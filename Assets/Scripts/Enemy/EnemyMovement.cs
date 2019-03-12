using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
	public Rigidbody enemyRigidbody;
    Transform player;
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    UnityEngine.AI.NavMeshAgent nav;


    void Awake ()
    {
        player = GameObject.FindGameObjectWithTag ("Player").transform;
        playerHealth = player.GetComponent <PlayerHealth> ();
        enemyHealth = GetComponent <EnemyHealth> ();
		enemyRigidbody = GetComponent<Rigidbody> ();
        //nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
    }


    void Update ()
    {
        if(enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
        {
			enemyRigidbody.MoveRotation (Quaternion.LookRotation(player.position - transform.position));
            //nav.SetDestination (player.position);
        }
        //else
        //{
        //   nav.enabled = false;
        //}
    }
}
