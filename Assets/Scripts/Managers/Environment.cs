using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

public class Environment : MonoBehaviour
{
	// System Variables
	public static Environment Instance { get; set; }
	public MazeSpawner mazeSpawner;
	public float timer = 0.0f;
	static int episode = 0;
	const int MAX_EPISODE = 1000;
	const int TIME_CAP = 100;
	int frame = 0;

	// Agent Variables
	public PlayerHealth playerHealth;
	public PlayerMovement playerMovement;
	public PlayerShooting playerShooting;
	const int moveCost = 1;
	const int noCollisionReward = 10;
	const int explorationReward = 20;
	const int finalReward = 10000;
	int MOVEMENT_STEP_SIZE = 0;
	int x = 0;
	int y = 0;
	bool isNewStateExplored = false;

	// State Variables
	int N_STATES=0;
	int currentStateIndex=1;
	int nextStateIndex=1;
	List<int> exploredStates;

	//Action Variables
	const int ACTION_NUMBERS = 4;
	int actionIndex=-1;
	List<int> actionList = new List<int> {0,1,2,3};

	// Q-Learning Parameters
	static float[,] q_table;
	const float alpha = 0.8f; //learning rate
	const float gamma = 0.5f; //delay rate
	const float epsilon = 0.5f; //e-greedy

	// IO Variables
	int h = 0;
	int v = 0;
	public string information = "";
	string filename = "./qtable.txt";

	void Awake()
	{
		episode += 1;
		frame=0;
		Initialize();
		exploredStates = new List<int>();
		MOVEMENT_STEP_SIZE = (int)mazeSpawner.CellWidth;
		Instance = this;
	}
		
	void FixedUpdate()
	{
		timer += Time.deltaTime;
		frame += 1;
		TrackingPosition();

		if (MenuManager.Instance.GAME_MODE == 1)
		{
			if (!playerMovement.isInAction)
			{
				h = (int)Input.GetAxisRaw("Horizontal");
				v = (int)Input.GetAxisRaw("Vertical");
			}
			playerMovement.Move(h,v,MOVEMENT_STEP_SIZE);
			if (playerMovement.isTerminating)
			{
				SceneManager.LoadScene("Game");
			}
		}

		if (MenuManager.Instance.GAME_MODE == 2)
		{
			if (timer > TIME_CAP || playerMovement.isTerminating)
			{
				SceneManager.LoadScene("Game");
			}

			if (episode <= MAX_EPISODE)
			{
				if (!playerMovement.isInAction)
				{
					ChooseAction();
					playerMovement.CheckMovementValidation(h,v,MOVEMENT_STEP_SIZE);
					CheckStateIndex();
					Q_Learning();
					Q_Learning_Console();
				}
				playerMovement.Move(h,v,MOVEMENT_STEP_SIZE);
				playerMovement.Animating (h,v);
			}
			else
			{
				SceneManager.LoadScene("GameOver");
			}
		}
	}

	void Initialize()
	{
		N_STATES = mazeSpawner.Rows * mazeSpawner.Columns;
		if (episode == 1) q_table = new float[N_STATES+1,ACTION_NUMBERS+1];
	}

	void TrackingPosition()
	{
		Vector3 pos = playerMovement.playerRigidbody.position;
		x = (int)(pos.x/mazeSpawner.CellWidth) + 1;
		y = (int)(pos.z/mazeSpawner.CellWidth) + 1;
	}

	void CheckStateIndex()
	{
		//input: currentState, action
		//output: state
		TrackingPosition();
		currentStateIndex = (y-1)*mazeSpawner.Columns + x;
			
		if (actionIndex==0) //left
		{
			nextStateIndex = (y-1)*mazeSpawner.Columns + (x - 1);
		}
		else if (actionIndex==1) //top
		{
			nextStateIndex = y*mazeSpawner.Columns + x;
		}
		else if (actionIndex==2) //right
		{
			nextStateIndex = (y-1)*mazeSpawner.Columns + (x + 1);
		}
		else if (actionIndex==3) //down
		{
			nextStateIndex = (y-2)*mazeSpawner.Columns + x;
		}

		if (nextStateIndex < 1 || nextStateIndex > N_STATES)
		{
			isNewStateExplored = false;
		}
		else
		{
			if (exploredStates.Find(e => e == nextStateIndex) == 0) 
			{
				isNewStateExplored = true;
				exploredStates.Add(nextStateIndex);
			}
			else
			{
				isNewStateExplored = false;
			}
		}
	}
		
	void ChooseAction()
	{
		//input: currentState, q_table
		//output: action
		float randomNumber = Random.Range(0.0f, 1.0f);
		if (randomNumber <= epsilon)
		{
			actionIndex = Argmax(currentStateIndex);
		}
		else
		{
			actionIndex = actionList[Random.Range(0, actionList.Count)];
		}
		if (actionIndex == 0)
		{
			h = -1;
			v = 0;
		}
		else if (actionIndex == 1)
		{
			h = 0;
			v = 1;
		}
		else if (actionIndex == 2)
		{
			h = 1;
			v = 0;
		}
		else if (actionIndex == 3)
		{
			h = 0;
			v = -1;
		}
	}

	int Argmax(int stateIndex)
	{
		List<float> row = new List<float>();
		List<int> rowIndex = new List<int>();

		/*Debug.Log("here:" + stateIndex);
		if (stateIndex < 1 || stateIndex > 100)
		{
			Debug.Log(x);
			Debug.Log(y);
			Time.timeScale = 0;
		}*/

		for (int i=0; i<ACTION_NUMBERS; i++)
		{
			row.Add(q_table[stateIndex, i]);
		}
		float max = row.Max();
		for (int i=0; i<ACTION_NUMBERS; i++)
		{
			if (row[i]==max)
			{
				rowIndex.Add(i);
			}
		}
		// for instance, rowIndex = {3, 5, 9}
		int randomIndex = Random.Range(0, rowIndex.Count); // 0, 1, 2
		return rowIndex[randomIndex];
	}

	/*void Learning()
	{
		CheckState();
		Q_Learning();
		Q_Learning_Console();
		stateIndex = nextStateIndex;
	}*/

	void Q_Learning()
	{
		float r = RewardFunction();
		float ExactReward = r + gamma*(Argmax(nextStateIndex));
		float EstimatedReward = q_table[currentStateIndex, actionIndex];
		q_table[currentStateIndex, actionIndex] = alpha*ExactReward + alpha*EstimatedReward;
		q_table[currentStateIndex, actionIndex] = Mathf.Round(q_table[currentStateIndex, actionIndex]);
		//Debug.Log("Q Table ( " + currentStateIndex + " , " + actionIndex + " , " + nextStateIndex + " )  is updated by " + q_table[currentStateIndex, actionIndex]);
	}

	void Q_Learning_Console()
	{
		information="";
		information+="=========== EPISODE " + episode + " ==========\n";
		information+="At iteration " + frame +"\n";
		for(int i=1;i<=N_STATES;i++)
		{
			information += "At state " + i + " : ";
			for(int j=0;j<ACTION_NUMBERS;j++)
			{
				information+=q_table[i,j] + " ";
			}
			information += "\n";
		}
	}

	float RewardFunction()
	{
		//int damageTaken = (int)(playerHealth.startingHealth - playerHealth.currentHealth)/10;
		float r = 0;
		//r += -timer/10;
		if (isNewStateExplored)
		{
			r += explorationReward;
			isNewStateExplored = false;
		}
		//else
		//{
		//	r -= explorationReward;
		//}
		if (!playerMovement.isHit) 
		{
			r += noCollisionReward;
			//Debug.Log("Won't hit! Reward++!");
		}
		//else
		//{
		//	r -= noCollisionReward*100;
			//Debug.Log("Will hit! No reward!");
		//}
		if (playerMovement.isTerminating) 
		{
			r += finalReward;
		}
		return r;
	}
}