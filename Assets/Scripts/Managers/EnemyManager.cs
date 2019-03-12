using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
	public PlayerMovement playerMovement;
    public GameObject enemy;
	public Rigidbody enemyRigidbody;
    //public float spawnTime = 3f;
    //public Transform[] spawnPoints;


    void Awake ()
    {
		enemyRigidbody = GetComponent<Rigidbody> ();
        //InvokeRepeating ("Spawn", spawnTime, spawnTime);
    }

	/*void FixedUpdate()
	{
		enemyRigidbody.MoveRotation (Quaternion.LookRotation(playerMovement.playerRigidbody.transform.position - transform.position));
	}*/

    /*void Spawn ()
    {
        if(playerHealth.currentHealth <= 0f)
        {
            return;
        }

        int spawnPointIndex = Random.Range (0, spawnPoints.Length);

        Instantiate (enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
    }*/
}
