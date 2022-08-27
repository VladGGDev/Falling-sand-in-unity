using System.Collections;
using System.Collections.Generic;
using TMPro;
//using System;
using UnityEngine;
using UnityEngine.XR;

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
	public FilterMode textureFilter = FilterMode.Point;

	Color[] particleColors;
	public Renderer rend;

	private void Start()
	{
		particles = new Particle[simWidth, simHeight];	//Instantiate the particle grid
		for (int y = 0; y < particles.GetLength(1); y++)
		{
			for (int x = 0; x < particles.GetLength(0); x++)
			{
				particles[x, y] = new Particle();
				particles[x, y].type = 0;
				particles[x, y].useSecondColor = Random.value > 0.499f;
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

		particleObjects = new ParticleObject[ParticleManager.instance.particleObjects.Length]; //Instanciate the particle objects
		for (int i = 0; i < particleObjects.Length; i++)
		{
			particleObjects[i] = ScriptableObject.CreateInstance<ParticleObject>();
			particleObjects[i] = ParticleManager.instance.particleObjects[i];
		}

		texture = new Texture2D(simWidth, simHeight);	//Instanciate the texture
		particleColors = new Color[particles.Length];
		texture.filterMode = textureFilter;
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
		if(CheckFor8Particles(x, y))
		{
			return;
		}
		Vector2Int chunk = ChunkAtPosition(x, y);
		chunks[chunk.x, chunk.y].updated = true;
		chunks[Mathf.Clamp(chunk.x + 1, 0, simChunkWidth - 1), chunk.y].updated = true;
		chunks[Mathf.Clamp(chunk.x - 1, 0, simChunkWidth - 1), chunk.y].updated = true;
		chunks[chunk.x, Mathf.Clamp(chunk.y + 1, 0, simChunkHeight - 1)].updated = true;
		chunks[chunk.x, Mathf.Clamp(chunk.y - 1, 0, simChunkHeight - 1)].updated = true;
	}

	public void UpdateSurroundingChunksNoCheck(int x, int y)
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
	public bool CheckForParticle(byte type, int x, int y)
	{
		if (x < 0 || x > simWidth - 1 || y < 0 || y > simHeight - 1)
		{
			return true;
		}

		if (particles[x, y].type == type)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool CheckForAnyParticle(int x, int y)
	{
		if(x < 0 || x > simWidth - 1 || y < 0 || y > simHeight - 1)
		{
			return true;
		}
		//x = Mathf.Clamp(x, 0, simWidth - 1);
		//y = Mathf.Clamp(y, 0, simHeight - 1);

		if (particles[x, y].type != 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool CheckFor8Particles(int x, int y)
	{
		if (CheckForAnyParticle(x, y + 1)
			&& CheckForAnyParticle(x, y - 1)
			&& CheckForAnyParticle(x + 1, y)
			&& CheckForAnyParticle(x - 1, y)
			&& CheckForAnyParticle(x + 1, y + 1)
			&& CheckForAnyParticle(x - 1, y + 1)
			&& CheckForAnyParticle(x + 1, y - 1)
			&& CheckForAnyParticle(x - 1, y - 1))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public void DeleteParticle(int x, int y)
	{
		if (x < 0 || x > simWidth - 1 || y < 0 || y > simHeight - 1)
		{
			return;
		}
		//x = Mathf.Clamp(x, 0, simWidth - 1);
		//y = Mathf.Clamp(y, 0, simHeight - 1);

		particles[x, y].type = 0;
		particles[x, y].framesWaited = 0;
		particles[x, y].freshSpread = 0;
		particles[x, y].lifeTime = 0;
		UpdateSurroundingChunksNoCheck(x, y);
	}

	public void CreateParticle(byte type, int x, int y)
	{
		if (x < 0 || x > simWidth - 1 || y < 0 || y > simHeight - 1)
		{
			return;
		}
		//x = Mathf.Clamp(x, 0, simWidth - 1);
		//y = Mathf.Clamp(y, 0, simHeight - 1);

		particles[x, y].type = type;
		UpdateSurroundingChunksNoCheck(x, y);
		ParticleObject particleObject = ParticleObjectFromIndex(x, y);
		particles[x, y].framesWaited = particleObject.waitFrames;
		particles[x, y].freshSpread = particleObject.spreadSettings.freshForFrames;
		particles[x, y].lifeTime = 
			Random.Range(particleObject.lifeSettings.minLifeTime, particleObject.lifeSettings.maxLifeTime + 1);
	}
	public void CreateParticle(byte type, int x, int y, bool wait)
	{
		if (x < 0 || x > simWidth - 1 || y < 0 || y > simHeight - 1)
		{
			return;
		}
		//x = Mathf.Clamp(x, 0, simWidth - 1);
		//y = Mathf.Clamp(y, 0, simHeight - 1);

		particles[x, y].type = type;
		UpdateSurroundingChunksNoCheck(x, y);
		ParticleObject particleObject = ParticleObjectFromIndex(x, y);
		particles[x, y].freshSpread = particleObject.spreadSettings.freshForFrames;
		particles[x, y].lifeTime =
			Random.Range(particleObject.lifeSettings.minLifeTime, particleObject.lifeSettings.maxLifeTime + 1);
		if (!wait)
			return;
		particles[x, y].framesWaited = particleObject.waitFrames;
	}

	public void SetParticleUpdateStatus(int x, int y, bool status)
	{
		if (x < 0 || x > simWidth - 1 || y < 0 || y > simHeight - 1)
		{
			return;
		}
		//x = Mathf.Clamp(x, 0, simWidth - 1);
		//y = Mathf.Clamp(y, 0, simHeight - 1);

		particles[x, y].hasBeenUpdated = status;
	}

	public ParticleObject ParticleObjectFromIndex(int x, int y)
	{
		if(particles[x, y].type == 0)
		{
			return ParticleManager.instance.airParticleObject;
		}
		return particleObjects[particles[x, y].type - 1];
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
				if(!WasChunkUpdated(x, y))
				{
					x += chunkWidth - 1;
					continue;
				}
				if (particles[x, y].framesWaited > 0)
				{
					particles[x, y].framesWaited--;
					UpdateSurroundingChunksNoCheck(x, y);
					continue;
				}
				ParticleObject currentParticleObject = ParticleObjectFromIndex(x, y);
				if (!currentParticleObject.lifeSettings.liveForever && particles[x, y].lifeTime <= 0)
				{
					DeleteParticle(x, y);
					continue;
				}
				if (particles[x, y].lifeTime > 0 && !currentParticleObject.lifeSettings.liveForever)
				{
					particles[x, y].lifeTime--;
					UpdateSurroundingChunksNoCheck(x, y);
				}


				//Check for every movement check for this particle object
				foreach (ParticleMoveChecks check in particleObjects[particles[x, y].type - 1].moveChecks)
				{
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.Down){
						if(!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleDown(new Vector2Int(x, y), currentParticleObject))
								break;
						}	
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.RandomDownDiagonal){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleRandomDownDiagonal(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.DownRight){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleDownRight(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.DownLeft){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleDownLeft(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.TowardsHorisontalVelocity){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleTowardsVelocity(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.RandomHorisontal){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleRandomVelocity(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.Right){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleRight(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.Left){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleLeft(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.Up){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleUp(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.RandomUpDiagonal){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleRandomUpDiagonal(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.UpRight){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleUpRight(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.UpLeft){
						if (!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleUpLeft(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.PlusRandom)
					{
						if (!(check.movementChance > Random.value))
						{
							if (!check.continueIfFailed)
							{
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else
						{
							if (CheckParticlePlusRandom(new Vector2Int(x, y), ParticleObjectFromIndex(x, y)))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.XRandom)
					{
						if (!(check.movementChance > Random.value))
						{
							if (!check.continueIfFailed)
							{
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else
						{
							if (CheckParticleXRandom(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
					if (check.moveDirection == ParticleMoveChecks.MoveDirection.EightRandom)
					{
						if (!(check.movementChance > Random.value))
						{
							if (!check.continueIfFailed)
							{
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else
						{
							if (CheckParticleEightRandom(new Vector2Int(x, y), currentParticleObject))
								break;
						}
					}
				}

				//Spread the particle
				if(currentParticleObject.spreadSettings.strength > 0)
				{
					if (particles[x, y].freshSpread > 0)
					{
						UpdateSurroundingChunksNoCheck(x, y);
						particles[x, y].freshSpread--;
					}
					SpreadParticle(new Vector2Int(x, y), currentParticleObject);
				}
			}
		}
		DrawParticles();
		ChunkUpdateStep();
	}

	public void DrawParticles()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			int ix = i % simWidth;
			int iy = i / simWidth;

			if (chunkDebug)
			{
				if(!WasChunkUpdated(ix, iy))
				{
					if(CheckForParticle(0, ix, iy))
					{
						particleColors[i] = Color.grey;
						continue;
					}
					else
					{
						particleColors[i] = Color.black;
						continue;
					}
				}
				else
				{
					if (CheckForParticle(0, ix, iy))
					{
						particleColors[i] = backGroundColor;
						continue;
					}
					else
					{
						particleColors[i] = ParticleObjectFromIndex(ix, iy).primaryColor;
					}

				}
			}

			if (!WasChunkUpdated(ix, iy) && !chunkDebug)
			{
				i += chunkWidth - 1;
				continue;
			}

			if (CheckForParticle(0, ix, iy) && !chunkDebug)
			{
				particleColors[i] = backGroundColor;
				continue;
			}

			SetParticleUpdateStatus(ix, iy, false);

			ParticleObject particleObject = ParticleObjectFromIndex(ix, iy);
			ParticleSpread spread = particleObject.spreadSettings;
			if (particles[ix, iy].useSecondColor)
			{
				if (particles[ix, iy].freshSpread > 0)
				{
					particleColors[i] =
						Color.Lerp(particleObject.secondaryColor, spread.freshSpreadColor,
						spread.freshColorCurve.Evaluate((float)particles[ix, iy].freshSpread / (float)spread.freshForFrames));
				}
				else
				{
					particleColors[i] = particleObject.secondaryColor;
				}
			}
			else
			{
				if (particles[ix, iy].freshSpread > 0)
				{
					particleColors[i] =
						Color.Lerp(particleObject.primaryColor, spread.freshSpreadColor,
						spread.freshColorCurve.Evaluate((float)particles[ix, iy].freshSpread / (float)spread.freshForFrames));
				}
				else
				{
					particleColors[i] = particleObject.primaryColor;
				}
			}
		}
		StartCoroutine(ApplyTextureToMaterial());
	}

	public IEnumerator ApplyTextureToMaterial()
	{
		yield return new WaitForEndOfFrame();
		texture.SetPixels(particleColors);
		texture.Apply();
		rend.material.mainTexture = texture;
	}

	#region Movement Checks
	#region Movement Functions
	void MoveParticle(Vector2Int particlePos, byte type, Vector2Int dir)
	{
		Vector2Int newPos = new Vector2Int(particlePos.x + dir.x, particlePos.y + dir.y);
		CreateParticle(type, newPos.x, newPos.y);
		if (particleObjects[type - 1].spreadSettings.strength > 0)
		{
			particles[newPos.x, newPos.y].freshSpread =
				particles[particlePos.x, particlePos.y].freshSpread - 1;
		}
		if (!particleObjects[type - 1].lifeSettings.liveForever)
		{
			particles[newPos.x, newPos.y].lifeTime = particles[particlePos.x, particlePos.y].lifeTime - 1;
		}
		DeleteParticle(particlePos.x, particlePos.y);
		SetParticleUpdateStatus(newPos.x, newPos.y, true);
	}
	void MoveLiquidParticle(Vector2Int particlePos, byte type, sbyte velocity)
	{
		int newX = particlePos.x + velocity;
		CreateParticle(type, newX, particlePos.y);
		if (particleObjects[type - 1].spreadSettings.strength > 0)
		{
			particles[newX, particlePos.y].freshSpread =
				particles[particlePos.x, particlePos.y].freshSpread - 1;
		}
		if (!particleObjects[type - 1].lifeSettings.liveForever)
		{
			particles[newX, particlePos.y].lifeTime = particles[particlePos.x, particlePos.y].lifeTime - 1;
		}
		DeleteParticle(particlePos.x, particlePos.y);
		SetParticleUpdateStatus(newX, particlePos.y, true);
		particles[Mathf.Clamp(newX, 0, simWidth - 1), particlePos.y].fluidHVel =
			(sbyte)System.Math.Sign(velocity);
		particles[particlePos.x, particlePos.y].fluidHVel = 0;
	}
	bool DisperseParticle(Vector2Int particlePos, ParticleObject particleObject, sbyte dir)
	{
		sbyte i = 1;
		for (i = 1; i <= Random.Range(particleObject.dispersionSpeedMin, particleObject.dispersionSpeedMax + 1); i++)
		{
			if (!CheckForAnyParticle(particlePos.x + (i * dir), particlePos.y)
				&& particlePos.x + (i * dir) < simWidth
				&& particlePos.x + (i * dir) >= 0)
			{
				continue;
			}
			else
			{
				if (i == 1)
				{
					return false;
				}
				MoveLiquidParticle(particlePos, particleObject.type, (sbyte)(dir * (i - 1)));
				return true;
			}
		}

		if (!CheckForAnyParticle(particlePos.x + (i - 1) * dir, particlePos.y))
		{
			MoveLiquidParticle(particlePos, particleObject.type, (sbyte)(dir * (i - 1)));
			return true;
		}
		else
		{
			return false;
		}
	}
	#endregion

	bool CheckParticleDown(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(particlePos.y == 0)
		{
			return false;
		}

		if(particleObject.corrosionSettings.strength != 0)
		{
			ParticleCorrosion corrosion = particleObject.corrosionSettings;
			float chance = (corrosion.strength - 
				ParticleObjectFromIndex(particlePos.x, particlePos.y - 1).corrosionSettings.resistance) * 
				corrosion.chance;
			if (Random.value < chance)
			{
				MoveParticle(particlePos, particleObject.type, new Vector2Int(0, -1));
				return true;
			}
			UpdateSurroundingChunks(particlePos.x, particlePos.y);
		}

		if (!CheckForAnyParticle(particlePos.x, particlePos.y - 1))
		{
			MoveParticle(particlePos, particleObject.type, new Vector2Int(0, -1));
			return true;
		}
		return false;
	}
	bool CheckParticleRandomDownDiagonal(Vector2Int particlePos, ParticleObject particleObject)
	{
		if (particlePos.y == 0)
		{
			return false;
		}
		sbyte randDir = Random.value > 0.499f ? (sbyte)1 : (sbyte)-1;
		if (particlePos.x == simWidth - 1 && randDir > 0)
		{
			return false;
		}
		if (particlePos.x == 0 && randDir < 0)
		{
			return false;
		}

		if (!CheckForAnyParticle(particlePos.x + randDir, particlePos.y - 1))
		{
			MoveParticle(particlePos, particleObject.type, new Vector2Int(randDir, -1));
			return true;
		}
		return false;
	}
	bool CheckParticleDownRight(Vector2Int particlePos, ParticleObject particleObject)
	{
		if (particlePos.y == 0)
		{
			return false;
		}
		if (particlePos.x == simWidth - 1)
		{
			return false;
		}

		if (!CheckForAnyParticle(particlePos.x + 1, particlePos.y - 1))
		{
			MoveParticle(particlePos, particleObject.type, new Vector2Int(1, -1));
			return true;
		}
		return false;
	}
	bool CheckParticleDownLeft(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(particlePos.y == 0)
		{
			return false;
		}
		if(particlePos.x == 0)
		{
			return false;
		}

		if (!CheckForAnyParticle(particlePos.x - 1, particlePos.y - 1))
		{
			MoveParticle(particlePos, particleObject.type, new Vector2Int(-1, -1));
			return true;
		}
		return false;
	}
	bool CheckParticleTowardsVelocity(Vector2Int particlePos, ParticleObject particleObject)
	{
		sbyte lastVelocity = particles[particlePos.x, particlePos.y].fluidHVel;

		if (particlePos.x == 0 && lastVelocity < 0)
		{
			return false;
		}
		if (particlePos.x == simWidth - 1 && lastVelocity > 0)
		{
			return false;
		}

		if (lastVelocity == 0)
		{
			return false;
		}

		if (particleObject.corrosionSettings.strength != 0)
		{
			ParticleCorrosion corrosion = particleObject.corrosionSettings;
			float chance = (corrosion.strength - 
				ParticleObjectFromIndex(particlePos.x + lastVelocity, particlePos.y).corrosionSettings.resistance) *
				corrosion.chance;
			if (Random.value < chance)
			{
				MoveLiquidParticle(particlePos, particleObject.type, lastVelocity);
				return true;
			}
			UpdateSurroundingChunks(particlePos.x, particlePos.y);
		}

		return DisperseParticle(particlePos, particleObject, lastVelocity);
	}
	bool CheckParticleRandomVelocity(Vector2Int particlePos, ParticleObject particleObject)
	{
		sbyte randomVelocity = Random.value > 0.499f ? (sbyte)1 : (sbyte)-1;

		if (particlePos.x == 0 && randomVelocity < 0)
		{
			return false;
		}
		if(particlePos.x == simWidth - 1 && randomVelocity > 0)
		{
			return false;
		}

		if (particleObject.corrosionSettings.strength != 0)
		{
			ParticleCorrosion corrosion = particleObject.corrosionSettings;
			float chance = (corrosion.strength - 
				ParticleObjectFromIndex(particlePos.x + randomVelocity, particlePos.y).corrosionSettings.resistance) * 
				corrosion.chance;
			if (Random.value < chance)
			{
				MoveLiquidParticle(particlePos, particleObject.type, randomVelocity);
				return true;
			}
			UpdateSurroundingChunks(particlePos.x, particlePos.y);
		}
		
		return DisperseParticle(particlePos, particleObject, randomVelocity);
	}
	bool CheckParticleRight(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(particlePos.x == simWidth - 1)
		{
			return false;
		}

		if (particleObject.corrosionSettings.strength != 0)
		{
			ParticleCorrosion corrosion = particleObject.corrosionSettings;
			float chance = (corrosion.strength -
				ParticleObjectFromIndex(particlePos.x + 1, particlePos.y).corrosionSettings.resistance) *
				corrosion.chance;
			if (Random.value < chance)
			{
				MoveLiquidParticle(particlePos, particleObject.type, 1);
				return true;
			}
		}
		
		return DisperseParticle(particlePos, particleObject, 1);
	}
	bool CheckParticleLeft(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(particlePos.x == 0)
		{
			return false;
		}

		if (particleObject.corrosionSettings.strength != 0)
		{
			ParticleCorrosion corrosion = particleObject.corrosionSettings;
			float chance = (corrosion.strength -
				ParticleObjectFromIndex(particlePos.x - 1, particlePos.y).corrosionSettings.resistance) *
				corrosion.chance;
			if (Random.value < chance)
			{
				MoveLiquidParticle(particlePos, particleObject.type, -1);
				return true;
			}
		}

		return DisperseParticle(particlePos, particleObject, -1);
	}
	bool CheckParticleUp(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(particlePos.y == simHeight - 1)
		{
			return false;
		}

		if (particleObject.corrosionSettings.strength != 0)
		{
			ParticleCorrosion corrosion = particleObject.corrosionSettings;
			float chance = (corrosion.strength - 
				ParticleObjectFromIndex(particlePos.x, particlePos.y + 1).corrosionSettings.resistance) *
				corrosion.chance;
			if (Random.value < chance)
			{
				MoveParticle(particlePos, particleObject.type, new Vector2Int(0, 1));
				return true;
			}
			UpdateSurroundingChunks(particlePos.x, particlePos.y);
		}

		if (!CheckForAnyParticle(particlePos.x, particlePos.y + 1))
		{
			MoveParticle(particlePos, particleObject.type, new Vector2Int(0, 1));
			return true;
		}
		return false;
	}
	bool CheckParticleRandomUpDiagonal(Vector2Int particlePos, ParticleObject particleObject)
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

		sbyte dir = Random.value > 0.499f ? (sbyte)1 : (sbyte)-1;

		if (!CheckForAnyParticle(particlePos.x + dir, particlePos.y + 1))
		{
			MoveParticle(particlePos, particleObject.type, new Vector2Int(dir, 1));
			return true;
		}
		return false;
	}
	bool CheckParticleUpRight(Vector2Int particlePos, ParticleObject particleObject)
	{
		if (particlePos.y == simHeight - 1)
		{
			return false;
		}
		if (particlePos.x == simWidth - 1)
		{
			return false;
		}

		if (!CheckForAnyParticle(particlePos.x + 1, particlePos.y + 1))
		{
			MoveParticle(particlePos, particleObject.type, new Vector2Int(1, 1));
			return true;
		}
		return false;
	}
	bool CheckParticleUpLeft(Vector2Int particlePos, ParticleObject particleObject)
	{
		if (particlePos.y == simHeight - 1)
		{
			return false;
		}
		if(particlePos.x == 0)
		{
			return false;
		}

		if (!CheckForAnyParticle(particlePos.x - 1, particlePos.y + 1))
		{
			MoveParticle(particlePos, particleObject.type, new Vector2Int(-1, 1));
			return true;
		}
		return false;
	}
	bool CheckParticlePlusRandom(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(CheckForAnyParticle(particlePos.x, particlePos.y + 1)
			&& CheckForAnyParticle(particlePos.x, particlePos.y - 1)
			&& CheckForAnyParticle(particlePos.x + 1, particlePos.y)
			&& CheckForAnyParticle(particlePos.x - 1, particlePos.y)) //Can't move in any direction
		{
			return false;
		}
		
		byte rand;

		for (byte i = 0; i < 4; i++)
		{
			rand = (byte)Random.Range(0, 4);

			if(rand == 0)
			{
				return CheckParticleUp(particlePos, particleObject);
			}
			else if (rand == 1)
			{
				return CheckParticleRight(particlePos, particleObject);
			}
			else if (rand == 2)
			{
				return CheckParticleDown(particlePos, particleObject);
			}
			else if (rand == 3)
			{
				return CheckParticleLeft(particlePos, particleObject);
			}
		}
		return false;
	}
	bool CheckParticleXRandom(Vector2Int particlePos, ParticleObject particleObject)
	{
		if (CheckForAnyParticle(particlePos.x + 1, particlePos.y + 1)
			&& CheckForAnyParticle(particlePos.x + 1, particlePos.y - 1)
			&& CheckForAnyParticle(particlePos.x - 1, particlePos.y - 1)
			&& CheckForAnyParticle(particlePos.x - 1, particlePos.y + 1)) //Can't move in any direction
		{
			return false;
		}

		byte rand;

		for(byte i = 0; i < 4; i++)
		{
			rand = (byte)Random.Range(0, 4);
			if (rand == 0)
			{
				return CheckParticleUpRight(particlePos, particleObject);
			}
			else if (rand == 1)
			{
				return CheckParticleDownRight(particlePos, particleObject);
			}
			else if (rand == 2)
			{
				return CheckParticleDownLeft(particlePos, particleObject);
			}
			else if (rand == 3)
			{
				return CheckParticleUpLeft(particlePos, particleObject);
			}
		}
		return false;
	}
	bool CheckParticleEightRandom(Vector2Int particlePos, ParticleObject particleObject)
	{
		if (CheckFor8Particles(particlePos.x, particlePos.y)) //Can't move in any direction
		{
			return false;
		}

		byte rand;

		for (byte i = 0; i < 4; i++)
		{
			rand = (byte)Random.Range(0, 8);
			if (rand == 0)
			{
				return CheckParticleUp(particlePos, particleObject);
			}
			else if (rand == 1)
			{
				return CheckParticleRight(particlePos, particleObject);
			}
			else if (rand == 2)
			{
				return CheckParticleDown(particlePos, particleObject);
			}
			else if (rand == 3)
			{
				return CheckParticleLeft(particlePos, particleObject);
			}
			if (rand == 4)
			{
				return CheckParticleUpRight(particlePos, particleObject);
			}
			else if (rand == 5)
			{
				return CheckParticleDownRight(particlePos, particleObject);
			}
			else if (rand == 6)
			{
				return CheckParticleDownLeft(particlePos, particleObject);
			}
			else if (rand == 7)
			{
				return CheckParticleUpLeft(particlePos, particleObject);
			}
		}
		return false;
	}
	void SpreadParticle(Vector2Int particlePos, ParticleObject particleObject)
	{
		if (CheckForParticle(particleObject.type, particlePos.x, particlePos.y + 1)
			&& CheckForParticle(particleObject.type, particlePos.x, particlePos.y - 1)
			&& CheckForParticle(particleObject.type, particlePos.x + 1, particlePos.y)
			&& CheckForParticle(particleObject.type, particlePos.x - 1, particlePos.y)
			&& CheckForParticle(particleObject.type, particlePos.x + 1, particlePos.y + 1)
			&& CheckForParticle(particleObject.type, particlePos.x - 1, particlePos.y + 1)
			&& CheckForParticle(particleObject.type, particlePos.x + 1, particlePos.y - 1)
			&& CheckForParticle(particleObject.type, particlePos.x - 1, particlePos.y - 1))
		{
			return;
		}

		UpdateSurroundingChunks(particlePos.x, particlePos.y);

		byte rand;
		Vector2Int dir = Vector2Int.zero;

		for (byte i = 0; i < 4; i++)
		{
			rand = (byte)Random.Range(0, 8);

			if (rand == 0){
				dir = new Vector2Int(0, 1);
			}
			else if (rand == 1){
				dir = new Vector2Int(0, -1);
			}
			else if (rand == 2){
				dir = new Vector2Int(1, 0);
			}
			else if (rand == 3){
				dir = new Vector2Int(-1, 0);
			}
			if (rand == 4)
			{
				dir = new Vector2Int(1, 1);
			}
			else if (rand == 5)
			{
				dir = new Vector2Int(1, -1);
			}
			else if (rand == 6)
			{
				dir = new Vector2Int(-1, 1);
			}
			else if (rand == 7)
			{
				dir = new Vector2Int(-1, -1);
			}

			Vector2Int newPosition = new Vector2Int(Mathf.Clamp(particlePos.x + dir.x, 0, simWidth - 1),
					Mathf.Clamp(particlePos.y + dir.y, 0, simHeight - 1));

			if (CheckForParticle(particleObject.type, newPosition.x, newPosition.y))
			{
				continue;
			}
			else
			{
				if (!particleObject.spreadSettings.canSpreadToAir
					&& CheckForParticle(0, newPosition.x, newPosition.y))
				{
					continue;
				}
			}

			ParticleSpread spread = particleObject.spreadSettings;
			float chance =
				(spread.strength - 
				ParticleObjectFromIndex(newPosition.x, newPosition.y).spreadSettings.resistance) *
				spread.chance;
			if (Random.value < chance)
			{
				CreateParticle(particleObject.type, newPosition.x, newPosition.y);
			}
			return;
		}
	}
	#endregion
}