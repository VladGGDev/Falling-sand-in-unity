using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;

public class ParticleLogic : MonoBehaviour
{
	public int simWidth = 10;
	public int simHeight = 10;
	public int chunkWidth = 25;
	public int chunkHeight = 25;
	int simChunkWidth;
	int simChunkHeight;
	public bool chunkDebug = false;
	public Particle[,] particles;
	public Chunk[,] chunks;
	ParticleObject[] particleObjects;

	[Space(10f)]
	public Texture2D texture;

	public Color backGroundColor = Color.white;

	public Color[] particleColors;
	public Renderer rend;

	private void Awake()
	{
		particles = new Particle[simWidth, simHeight];	//Instantiate the particle grid
		for (int y = 0; y < particles.GetLength(1); y++)
		{
			for (int x = 0; x < particles.GetLength(0); x++)
			{
				particles[x, y] = new Particle();
				particles[x, y].type = 0;
				particles[x, y].useSecondColor = Random.value > 0.5f;
				particles[x, y].fluidHVel = 0;
			}
		}

		if(simWidth % chunkWidth != 0 || simHeight % chunkHeight != 0)
		{
			Debug.LogErrorFormat("The simulation width (= {1}) or height (= {2}) of {0}" +
				" isn't divisible by the chunk width (= {3}) or height (= {4}). The script will be disabled.",
				gameObject.name, simWidth, simHeight, chunkWidth, chunkHeight);
			this.enabled = false;
		}
		chunks = new Chunk[simWidth / chunkWidth, simHeight / chunkHeight];	//Instanciate the chunk grid
		simChunkWidth = simWidth / chunkWidth;
		simChunkHeight = simHeight / chunkHeight;
		for (int y = 0; y < chunks.GetLength(1); y++)
		{
			for (int x = 0; x < chunks.GetLength(0); x++)
			{
				chunks[x, y] = new Chunk();
			}
		}

		particleObjects = new ParticleObject[ParticleSettings.particleObjects.Length]; //Instanciate the particle objects
		for (int i = 0; i < particleObjects.Length; i++)
		{
			particleObjects[i] = ScriptableObject.CreateInstance<ParticleObject>();
			particleObjects[i] = ParticleSettings.particleObjects[i];
		}

		texture = new Texture2D(simWidth, simHeight);	//Instanciate the texture
		particleColors = new Color[particles.Length];
		texture.filterMode = FilterMode.Point;
	}



	#region Chunk Functions
	public Vector2Int ChunkAtPosition(int x, int y)
	{
		return new Vector2Int(x / chunkWidth, y / chunkHeight);
	}

	public bool WasChunkUpdated(int x, int y)
	{
		Vector2Int chunk = ChunkAtPosition(x, y);
		return chunks[chunk.x, chunk.y].updatedLastFrame;
	}

	public void UpdateSurroundingChunks(int x, int y)
	{
		Vector2Int chunk = ChunkAtPosition(x, y);
		chunks[chunk.x, chunk.y].updated = true;
		chunks[Mathf.Clamp(chunk.x + 1, 0, simChunkWidth - 1), chunk.y].updated = true;
		chunks[Mathf.Clamp(chunk.x - 1, 0, simChunkWidth - 1), chunk.y].updated = true;
		chunks[chunk.x, Mathf.Clamp(chunk.y + 1, 0, simChunkHeight - 1)].updated = true;
		chunks[chunk.x, Mathf.Clamp(chunk.y - 1, 0, simChunkHeight - 1)].updated = true;
	}

	public void ChunkUpdateStep()
	{
		for (int y = 0; y < chunks.GetLength(1); y++)
		{
			for (int x = 0; x < chunks.GetLength(0); x++)
			{
				chunks[x, y].updatedLastFrame = chunks[x, y].updated;
				chunks[x, y].updated = false;
			}
		}
	}
	#endregion

	#region Helping Functions
	public bool CheckForParticle(byte type, Vector2Int atPos)
	{
		if (particles[atPos.x, atPos.y].type == type)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool CheckForAnyParticle(Vector2Int atPos)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);

		if (particles[atPos.x, atPos.y].type != 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public void DeleteParticle(Vector2Int atPos)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);

		particles[atPos.x, atPos.y].type = 0;
		particles[atPos.x, atPos.y].framesWaited = 0;
		UpdateSurroundingChunks(atPos.x, atPos.y);
	}

	public void CreateParticle(byte type, Vector2Int atPos)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);

		particles[atPos.x, atPos.y].type = type;
		UpdateSurroundingChunks(atPos.x, atPos.y);

		particles[atPos.x, atPos.y].framesWaited = particleObjects[type - 1].waitFrames; //type - 1 = the index of the particleObject
	}
	public void CreateParticle(byte type, Vector2Int atPos, bool wait)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);

		particles[atPos.x, atPos.y].type = type;
		UpdateSurroundingChunks(atPos.x, atPos.y);

		if (!wait)
			return;
		particles[atPos.x, atPos.y].framesWaited = particleObjects[type - 1].waitFrames; //type - 1 = the index of the particleObject
	}

	public void SetParticleUpdateStatus(Vector2Int atPos, bool status)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);

		particles[atPos.x, atPos.y].hasBeenUpdated = status;
	}

	public byte TypeFromIndex(int x, int y)
	{
		return particleObjects[particles[x, y].type - 1].type;
	}
	#endregion



	private void FixedUpdate()      //Runs at 50 FPS by default
	{
		for (int y = 0; y < simHeight; y++)
		{
			for (int x = 0; x < simWidth; x++)
			{
				if (particles[x, y].hasBeenUpdated)
				{
					continue;
				}
				if (particles[x, y].type == 0)
				{
					continue;
				}
				if (particles[x, y].framesWaited != 0)
				{
					particles[x, y].framesWaited--;
					continue;
				}
				if(!WasChunkUpdated(x, y))
				{
					continue;
				}

				//Check for every movement check for this particle object
				foreach (ParticleMoveChecks check in particleObjects[particles[x, y].type - 1].moveChecks)
				{
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.Down){
						if(!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleDown(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}	
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.RandomDownDiagonal){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleRandomDownDiagonal(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.DownRight){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleDownRight(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.DownLeft){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleDownLeft(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.TowardsHorisontalVelocity){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleTowardsVelocity(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.RandomHorisontal){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleRandomVelocity(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.Right){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleRight(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.Left){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleLeft(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.Up){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleUp(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.RandomUpDiagonal){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleRandomUpDiagonal(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.UpRight){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleUpRight(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.UpLeft){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								break;
							}
						}
						else{
							if (CheckParticleUpLeft(new Vector2Int(x, y), TypeFromIndex(x, y)))
								break;
						}
					}
				}
			}
		}
		DrawParticles();
		ChunkUpdateStep();
	}

	public void DrawParticles()    //	!!!!! ADD MORE IFs WHEN YOU ADD MORE PARTICLE TYPES !!!!!!
	{
		for (int i = 0; i < particles.Length; i++)
		{
			int ix = i % simWidth;
			int iy = i / simWidth;

			particles[ix, iy].hasBeenUpdated = false;   //Reset update status for every particle

			if (!WasChunkUpdated(ix, iy))
			{
				if (chunkDebug)
				{
					if(CheckForParticle(0, new Vector2Int(ix, iy)))
					{
						particleColors[i] = Color.grey;
					}
					else
					{
						particleColors[i] = Color.black;
					}
				}
				continue;
			}
			if (CheckForParticle(0, new Vector2Int(ix, iy)))
			{
				particleColors[i] = backGroundColor;
				continue;
			}

			for (int j = 0; j < particleObjects.Length; j++)    //Check for every particle object
			{
				if (CheckForParticle(particleObjects[j].type, new Vector2Int(ix, iy)))
				{
					if (particles[ix, iy].useSecondColor)
					{
						particleColors[i] = particleObjects[j].secondaryColor;
					}
					else
					{
						particleColors[i] = particleObjects[j].primaryColor;
					}
				}
			}
		}

		texture.SetPixels(particleColors);
		texture.Apply();
		rend.material.mainTexture = texture;
	}

	void SandPhysics(Vector2Int particlePos)
	{
		if (particlePos.y == 0)
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y), true);
			return;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x, particlePos.y - 1)))    //Check for particle down
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y - 1), true);
			CreateParticle(1, new Vector2Int(particlePos.x, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x + 1, particlePos.y - 1)))   //Check for particle down right
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + 1, particlePos.y - 1), true);
			CreateParticle(1, new Vector2Int(particlePos.x + 1, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x - 1, particlePos.y - 1)))   //Check for particle down left
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x - 1, particlePos.y - 1), true);
			CreateParticle(1, new Vector2Int(particlePos.x - 1, particlePos.y - 1));
		}
		else
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y), true);
			return;
		}

		DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));   //Remove the initial position particle
	}

	void WaterPhysics(Vector2Int particlePos)
	{
		sbyte lastVelocity = particles[particlePos.x, particlePos.y].fluidHVel;

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x, particlePos.y - 1)) && particlePos.y != 0)    //Check for particle down
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y - 1), true);
			CreateParticle(2, new Vector2Int(particlePos.x, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x + 1, particlePos.y - 1)) && particlePos.y != 0)   //Check for particle down right
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + 1, particlePos.y - 1), true);
			CreateParticle(2, new Vector2Int(particlePos.x + 1, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x - 1, particlePos.y - 1)) && particlePos.y != 0)   //Check for particle down left
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x - 1, particlePos.y - 1), true);
			CreateParticle(2, new Vector2Int(particlePos.x - 1, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x + lastVelocity, particlePos.y))) //Check towards velocity
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + lastVelocity, particlePos.y), true);
			CreateParticle(2, new Vector2Int(particlePos.x + lastVelocity, particlePos.y));
			particles[particlePos.x + lastVelocity, particlePos.y].fluidHVel = lastVelocity;
			particles[particlePos.x, particlePos.y].fluidHVel = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x + 1, particlePos.y)))        //Check for particle right
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + 1, particlePos.y), true);
			CreateParticle(2, new Vector2Int(particlePos.x + 1, particlePos.y));
			particles[particlePos.x + 1, particlePos.y].fluidHVel = 1;
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x - 1, particlePos.y)))       //Check for particle left
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x - 1, particlePos.y), true);
			CreateParticle(2, new Vector2Int(particlePos.x - 1, particlePos.y));
			particles[particlePos.x - 1, particlePos.y].fluidHVel = -1;
		}
		else
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y), true);
			return;
		}

		DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));   //Remove the initial position particle
	}

	#region Movement Checks
	bool CheckParticleDown(Vector2Int particlePos, byte particleType)
	{
		if (!CheckForAnyParticle(new Vector2Int(particlePos.x, particlePos.y - 1)) && particlePos.y != 0)
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y - 1), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x, particlePos.y - 1));
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleRandomDownDiagonal(Vector2Int particlePos, byte particleType)
	{
		if (particlePos.y == 0)
		{
			return false;
		}
		if (particlePos.x == simWidth - 1)
		{
			return false;
		}
		if (particlePos.x == 0)
		{
			return false;
		}

		sbyte randDir = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x + randDir, particlePos.y - 1)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + randDir, particlePos.y - 1), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x + randDir, particlePos.y - 1));
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleDownRight(Vector2Int particlePos, byte particleType)
	{
		if (particlePos.y == 0)
		{
			return false;
		}
		if (particlePos.x == simWidth - 1)
		{
			return false;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x + 1, particlePos.y - 1)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + 1, particlePos.y - 1), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x + 1, particlePos.y - 1));
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleDownLeft(Vector2Int particlePos, byte particleType)
	{
		if(particlePos.y == 0)
		{
			return false;
		}
		if(particlePos.x == 0)
		{
			return false;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x - 1, particlePos.y - 1)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x - 1, particlePos.y - 1), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x - 1, particlePos.y - 1));
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleTowardsVelocity(Vector2Int particlePos, byte particleType)
	{
		if(particlePos.x == 0)
		{
			return false;
		}
		if(particlePos.x == simWidth - 1)
		{
			return false;
		}

		sbyte lastVelocity = particles[particlePos.x, particlePos.y].fluidHVel;

		if (lastVelocity == 0)
		{
			return false;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x + lastVelocity, particlePos.y)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + lastVelocity, particlePos.y), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x + lastVelocity, particlePos.y));
			particles[particlePos.x + lastVelocity, particlePos.y].fluidHVel = lastVelocity;
			particles[particlePos.x, particlePos.y].fluidHVel = 0;
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleRandomVelocity(Vector2Int particlePos, byte particleType)
	{
		if (particlePos.x == 0)
		{
			return false;
		}
		if(particlePos.x == simWidth - 1)
		{
			return false;
		}

		sbyte randomVelocity = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x + randomVelocity, particlePos.y)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + randomVelocity, particlePos.y), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x + randomVelocity, particlePos.y));
			particles[particlePos.x + randomVelocity, particlePos.y].fluidHVel = randomVelocity;
			particles[particlePos.x, particlePos.y].fluidHVel = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleRight(Vector2Int particlePos, byte particleType)
	{
		if(particlePos.x == simWidth - 1)
		{
			return false;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x + 1, particlePos.y)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + 1, particlePos.y), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x + 1, particlePos.y));
			particles[particlePos.x + 1, particlePos.y].fluidHVel = 1;
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleLeft(Vector2Int particlePos, byte particleType)
	{
		if(particlePos.x == 0)
		{
			return false;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x - 1, particlePos.y)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x - 1, particlePos.y), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x - 1, particlePos.y));
			particles[particlePos.x - 1, particlePos.y].fluidHVel = -1;
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleUp(Vector2Int particlePos, byte particleType)
	{
		if(particlePos.y == simHeight - 1)
		{
			return false;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x, particlePos.y + 1)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y + 1), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x, particlePos.y + 1));
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleRandomUpDiagonal(Vector2Int particlePos, byte particleType)
	{
		if(particlePos.y == simHeight - 1)
		{
			return false;
		}
		if(particlePos.x == simWidth - 1)
		{
			return false;
		}
		if(particlePos.x == 0)
		{
			return false;
		}

		sbyte dir = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x + dir, particlePos.y + 1)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + dir, particlePos.y + 1), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x + dir, particlePos.y + 1));
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleUpRight(Vector2Int particlePos, byte particleType)
	{
		if (particlePos.y == simHeight - 1)
		{
			return false;
		}
		if (particlePos.x == simWidth - 1)
		{
			return false;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x + 1, particlePos.y + 1)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + 1, particlePos.y + 1), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x + 1, particlePos.y + 1));
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	bool CheckParticleUpLeft(Vector2Int particlePos, byte particleType)
	{
		if (particlePos.y == simHeight - 1)
		{
			return false;
		}
		if(particlePos.x == 0)
		{
			return false;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x - 1, particlePos.y + 1)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x - 1, particlePos.y + 1), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x - 1, particlePos.y + 1));
			DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));
			return true;
		}
		return false;
	}
	#endregion
}