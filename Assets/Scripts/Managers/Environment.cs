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
	const int collisionCost = 1;
	const int explorationReward = 10;
	const int finalReward = 100;
	int MOVEMENT_STEP_SIZE = 0;
	int x = 0;
	int y = 0;
	bool isNewStateExplored = false;

	// State Variables
	int N_STATES=0;
	int stateIndex=0;
	int nextStateIndex=0;
	List<int> exploredStates;

	//Action Variables
	const int ACTION_NUMBERS = 4;
	int actionIndex=-1;
	List<int> actionList = new List<int> {0,1,2,3};

	// Q-Learning Parameters
	static float[,] q_table;
	const float alpha = 0.8f; //learning rate
	const float gamma = 0.5f; //delay rate
	const float epsilon = 0.9f; //e-greedy

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
		IdentifyNextState();
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
					actionIndex = ChooseAction();
					Learning();
				}
				MakeMove();
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

	void IdentifyNextState()
	{
		//input: currentState, action
		//output: state
		TrackingPosition();
		nextStateIndex = (y-1)*mazeSpawner.Columns + x;
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
		
	int ChooseAction()
	{
		//input: currentState, q_table
		//output: action
		float randomNumber = Random.Range(0.0f, 1.0f);
		if (randomNumber <= epsilon)
		{
			actionIndex = Argmax(stateIndex);
		}
		else
		{
			actionIndex = actionList[Random.Range(0, actionList.Count)];
		}
		return actionIndex;
	}

	void MakeMove()
	{
		switch (actionIndex)
		{
		case 0:
			// move left
			playerMovement.Move(-1,0,MOVEMENT_STEP_SIZE);
			playerMovement.Animating (-1, 0);
			break;
		case 1:
			// move top
			playerMovement.Move(0,1,MOVEMENT_STEP_SIZE);
			playerMovement.Animating (0, 1);
			break;
		case 2:
			// move right
			playerMovement.Move(1,0,MOVEMENT_STEP_SIZE);
			playerMovement.Animating (1, 0);
			break;
		case 3:
			// move bottom
			playerMovement.Move(0,-1,MOVEMENT_STEP_SIZE);
			playerMovement.Animating (0, -1);
			break;
		}
	}

	int Argmax(int currentState)
	{
		List<float> row = new List<float>();
		List<int> rowIndex = new List<int>();
		//Debug.Log(currentState);
		for (int i=0; i<ACTION_NUMBERS; i++)
		{
			row.Add(q_table[currentState, i]);
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

	void Learning()
	{
		IdentifyNextState();
		Q_Learning();
		Q_Learning_Console();
		stateIndex = nextStateIndex;
	}

	void Q_Learning()
	{
		float r = RewardFunction();
		float ExactReward = r + gamma*(Argmax(nextStateIndex));
		float EstimatedReward = q_table[stateIndex, actionIndex];
		q_table[stateIndex, actionIndex] += alpha*(ExactReward - EstimatedReward);
		q_table[stateIndex, actionIndex] = Mathf.Round(q_table[stateIndex, actionIndex]);
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
		if (isNewStateExplored) r+= explorationReward;
		else r-= explorationReward;
		if (playerMovement.isHit) r -= collisionCost + moveCost;
		else r -= moveCost;
		if (playerMovement.isTerminating) r += finalReward;
		r -= timer;
		if (isNewStateExplored)
		{
			r += explorationReward;
			isNewStateExplored = false;
		}
		return r;
	}
}