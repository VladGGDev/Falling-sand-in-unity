using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLogic : MonoBehaviour
{
	public float pixelsPerUnit = 100f;
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

	public SpriteRenderer spriteRenderer;
	//public Renderer rend;
	//public Material material;

	public Color backGroundColor = Color.white;
	public FilterMode textureFilter = FilterMode.Point;

	public Color[] particleColors;

	float minStepTime = 0;

	private void Start()
	{
		//Instantiate the particle grid
		particles = new Particle[simWidth, simHeight];
		for (int y = 0; y < particles.GetLength(1); y++)
		{
			for (int x = 0; x < particles.GetLength(0); x++)
			{
				particles[x, y] = new Particle();
				particles[x, y].type = 0;
				particles[x, y].gradientColor = Random.value;
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
		//Instanciate the chunk grid
		chunks = new Chunk[simWidth / chunkWidth, simHeight / chunkHeight];
		simChunkWidth = simWidth / chunkWidth;
		simChunkHeight = simHeight / chunkHeight;
		for (int y = 0; y < chunks.GetLength(1); y++)
		{
			for (int x = 0; x < chunks.GetLength(0); x++)
			{
				chunks[x, y] = new Chunk();
			}
		}

		//Instanciate particle obj
		particleObjects = new ParticleObject[ParticleManager.instance.particleObjects.Length];
		for (int i = 0; i < particleObjects.Length; i++)
		{
			particleObjects[i] = ScriptableObject.CreateInstance<ParticleObject>();
			particleObjects[i] = ParticleManager.instance.particleObjects[i];
			if (particleObjects[i].texture != null && !particleObjects[i].texture.isReadable)
			{
				Debug.LogWarning("Make sure to set the Read/Write Enabled property of " 
					+ particleObjects[i].texture.name + " texture!");
			}
		}

		//Instanciate the texture
		texture = new Texture2D(simWidth, simHeight);
		particleColors = new Color[particles.Length];
		texture.filterMode = textureFilter;

		spriteRenderer.sprite = Sprite.Create(texture,
			new Rect(0, 0, simWidth, simHeight),
			new Vector2(0.5f, 0.5f), 
			pixelsPerUnit);

		Debug.Log(spriteRenderer.sprite.pixelsPerUnit);

		transform.localScale = new Vector2(
			transform.localScale.x / (simWidth * (1 / pixelsPerUnit)),
			transform.localScale.y / (simHeight * (1 / pixelsPerUnit))); 
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
		if (CheckForAnyParticle(x, y + 1)
			&& CheckForAnyParticle(x, y - 1)
			&& CheckForAnyParticle(x + 1, y)
			&& CheckForAnyParticle(x - 1, y))
		{
			return;
		}
		Vector2Int chunk = ChunkAtPosition(x, y);
		chunks[chunk.x, chunk.y].updated = true;

		if (x % chunkWidth == chunkWidth - 1)
		{
			chunks[Mathf.Clamp(chunk.x + 1, 0, simChunkWidth - 1), chunk.y].updated = true;
		}
		if(x % chunkWidth == 0)
		{
			chunks[Mathf.Clamp(chunk.x - 1, 0, simChunkWidth - 1), chunk.y].updated = true;
		}
		if(y % chunkHeight == chunkHeight - 1)
		{
			chunks[chunk.x, Mathf.Clamp(chunk.y + 1, 0, simChunkHeight - 1)].updated = true;
		}
		if(y % chunkHeight == 0)
		{
			chunks[chunk.x, Mathf.Clamp(chunk.y - 1, 0, simChunkHeight - 1)].updated = true;
		}
	}

	public void UpdateSurroundingChunksNoCheck(int x, int y)
	{
		Vector2Int chunk = ChunkAtPosition(x, y);
		chunks[chunk.x, chunk.y].updated = true;

		if (x % chunkWidth == chunkWidth - 1)
		{
			chunks[Mathf.Clamp(chunk.x + 1, 0, simChunkWidth - 1), chunk.y].updated = true;
		}
		if (x % chunkWidth == 0)
		{
			chunks[Mathf.Clamp(chunk.x - 1, 0, simChunkWidth - 1), chunk.y].updated = true;
		}
		if (y % chunkHeight == chunkHeight - 1)
		{
			chunks[chunk.x, Mathf.Clamp(chunk.y + 1, 0, simChunkHeight - 1)].updated = true;
		}
		if (y % chunkHeight == 0)
		{
			chunks[chunk.x, Mathf.Clamp(chunk.y - 1, 0, simChunkHeight - 1)].updated = true;
		}
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

		if (particles[x, y].type != 0)
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

		particles[x, y].type = 0;
		particles[x, y].framesWaited = 0;
		particles[x, y].freshSpread = 0;
		particles[x, y].lifeTime = 0;
		particles[x, y].totalLifeTime = 0;
		UpdateSurroundingChunksNoCheck(x, y);
	}

	public void CreateParticle(byte type, int x, int y)
	{
		if (x < 0 || x > simWidth - 1 || y < 0 || y > simHeight - 1)
		{
			return;
		}

		particles[x, y].type = type;
		UpdateSurroundingChunksNoCheck(x, y);
		ParticleObject particleObject = ParticleObjectFromIndex(x, y);
		particles[x, y].framesWaited = particleObject.waitFrames;
		particles[x, y].freshSpread = particleObject.spread.freshForFrames;
		particles[x, y].totalLifeTime = 
			Random.Range(particleObject.life.minLifeTime, particleObject.life.maxLifeTime + 1);
		particles[x, y].lifeTime = particles[x, y].totalLifeTime;
	}
	public void CreateParticle(byte type, int x, int y, bool wait)
	{
		if (x < 0 || x > simWidth - 1 || y < 0 || y > simHeight - 1)
		{
			return;
		}

		particles[x, y].type = type;
		UpdateSurroundingChunksNoCheck(x, y);
		ParticleObject particleObject = ParticleObjectFromIndex(x, y);
		particles[x, y].freshSpread = particleObject.spread.freshForFrames;
		particles[x, y].totalLifeTime =
			Random.Range(particleObject.life.minLifeTime, particleObject.life.maxLifeTime + 1);
		particles[x, y].lifeTime = particles[x, y].totalLifeTime;
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

		particles[x, y].hasBeenUpdated = status;
	}

	public ParticleObject ParticleObjectFromIndex(int x, int y)
	{
		if (x < 0 || x > simWidth - 1 || y < 0 || y > simHeight - 1)
		{
			return ParticleManager.instance.airParticleObject;
		}

		if (particles[x, y].type == 0)
		{
			return ParticleManager.instance.airParticleObject;
		}
		return particleObjects[particles[x, y].type - 1];
	}
	public ParticleObject ParticleObjectFromType(byte type)
	{
		if (type == 0)
		{
			return ParticleManager.instance.airParticleObject;
		}
		return particleObjects[type - 1];
	}
	#endregion



	private void Update()
	{
		if(ParticleManager.instance.minParticleStepTime <= 0.001f)
		{
			return;
		}

		minStepTime += Time.deltaTime;
		while (minStepTime >= ParticleManager.instance.minParticleStepTime)
		{
			minStepTime -= ParticleManager.instance.minParticleStepTime;
			ParticlePhysicsStep();
		}
	}

	private void ParticlePhysicsStep()
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

				CollideWithParticles(new Vector2Int(x, y), ParticleObjectFromIndex(x, y));

				ParticleObject currentParticle = ParticleObjectFromIndex(x, y);

				if (!currentParticle.life.liveForever && particles[x, y].lifeTime <= 0)	//Particle death
				{
					if(currentParticle.life.createOnDeath != null && Random.value < currentParticle.life.chance)
					{
						CreateParticle(currentParticle.life.createOnDeath.type, x, y);
					}
					else
					{
						DeleteParticle(x, y);
					}
					continue;
				}


				//Check for every movement check for this particle object
				for (int i = 0; i < currentParticle.moveChecks.Length; i++)
				{
					ParticleMoveChecks check = currentParticle.moveChecks[i];

					if (check.moveDirection == ParticleMoveChecks.MoveDirection.Down){
						if(!(check.movementChance > Random.value)){
							if (!check.continueIfFailed){
								UpdateSurroundingChunks(x, y);
								break;
							}
						}
						else{
							if (CheckParticleDown(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleRandomDownDiagonal(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleDownRight(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleDownLeft(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleTowardsVelocity(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleRandomVelocity(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleRight(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleLeft(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleUp(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleRandomUpDiagonal(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleUpRight(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleUpLeft(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleXRandom(new Vector2Int(x, y), currentParticle))
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
							if (CheckParticleEightRandom(new Vector2Int(x, y), currentParticle))
								break;
						}
					}
				}


				if (particles[x, y].lifeTime > 0 && !currentParticle.life.liveForever)
				{
					particles[x, y].lifeTime--;
					UpdateSurroundingChunksNoCheck(x, y);
				}
				//Spread the particle
				if (currentParticle.spread.strength > 0)
				{
					if (particles[x, y].freshSpread > 0)
					{
						UpdateSurroundingChunksNoCheck(x, y);
						particles[x, y].freshSpread--;
					}
					SpreadParticle(new Vector2Int(x, y), currentParticle);
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
			int x = i % simWidth;
			int y = i / simWidth;

			if (chunkDebug)
			{
				if(!WasChunkUpdated(x, y))
				{
					if(CheckForParticle(0, x, y))
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
					if (CheckForParticle(0, x, y))
					{
						particleColors[i] = backGroundColor;
						continue;
					}

				}
			}

			if (!WasChunkUpdated(x, y) && !chunkDebug)
			{
				i += chunkWidth - 1;
				continue;
			}

			ParticleObject particleObject = ParticleObjectFromIndex(x, y);

			if (CheckForParticle(0, x, y) && !chunkDebug)
			{
				if (particleObject.texture != null)
				{
					particleColors[i] = ColorAtPosition(x, y, particleObject);
				}
				else
				{
					particleColors[i] = backGroundColor;
				}
				continue;
			}

			SetParticleUpdateStatus(x, y, false);

			ParticleSpread spread = particleObject.spread;
			ParticleLife life = particleObject.life;

			if(particleObject.texture != null)
			{
				particleColors[i] = ColorAtPosition(x, y, particleObject);
				continue;	//Skips color code because this already sets the pixel color
			}

			Color particleColor = particleObject.colors.Evaluate(particles[x, y].gradientColor);
			
			if (particles[x, y].freshSpread > 0 && spread.strength > 0)
			{
				particleColors[i] =
					Color.Lerp(particleColor, spread.freshSpreadColor,
					spread.freshColorCurve.Evaluate((float)particles[x, y].freshSpread / (float)spread.freshForFrames));
			}
			else if (!particleObject.life.liveForever)
			{
				particleColors[i] =
					Color.Lerp(life.deathColor, particleColor,
					life.deathColorCurve.Evaluate((float)particles[x, y].lifeTime / (float)particles[x, y].totalLifeTime));
			}
			else
			{
				particleColors[i] = particleColor;
			}
		}
		texture.SetPixels(particleColors);
		texture.Apply();

		Color ColorAtPosition(int x, int y, ParticleObject particleObject)
		{
			return particleObject.texture.GetPixel(x, y);
		}
	}

	#region Movement Checks
	#region Movement Functions
	void MoveParticle(Vector2Int particlePos, byte type, Vector2Int dir)
	{
		Vector2Int newPos = new Vector2Int(
			Mathf.Clamp(particlePos.x + dir.x, 0, simWidth - 1),
			Mathf.Clamp(particlePos.y + dir.y, 0, simHeight - 1));
		ParticleObject particleObject = particleObjects[type - 1];
		CreateParticle(type, newPos.x, newPos.y);
		if (particleObject.spread.strength > 0)
		{
			particles[newPos.x, newPos.y].freshSpread = (particles[particlePos.x, particlePos.y].freshSpread - 1);
		}
		if (!particleObject.life.liveForever)
		{
			particles[newPos.x, newPos.y].lifeTime = (particles[particlePos.x, particlePos.y].lifeTime - 1);
		}
		DeleteParticle(particlePos.x, particlePos.y);
		SetParticleUpdateStatus(newPos.x, newPos.y, true);
	}
	void MoveLiquidParticle(Vector2Int particlePos, byte type, sbyte velocity)
	{
		int newX = Mathf.Clamp(particlePos.x + velocity, 0, simWidth - 1);
		ParticleObject particleObject = particleObjects[type - 1];
		CreateParticle(type, newX, particlePos.y);
		if (particleObject.spread.strength > 0)
		{
			particles[newX, particlePos.y].freshSpread = (particles[particlePos.x, particlePos.y].freshSpread - 1);
		}
		if (!particleObject.life.liveForever)
		{
			particles[newX, particlePos.y].lifeTime = (particles[particlePos.x, particlePos.y].lifeTime - 1);
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
	bool CorrodeParticle(Vector2Int particlePos, ParticleObject particleObject, Vector2Int dir, bool liquid)
	{
		if (particleObject.corrosion.strength > 0
			&& ParticleObjectFromIndex(particlePos.x + dir.x, particlePos.y + dir.y).type != particleObject.type)
		{
			ParticleCorrosion corrosion = particleObject.corrosion;
			float chance = (corrosion.strength -
				ParticleObjectFromIndex(particlePos.x + dir.x, particlePos.y + dir.y).corrosion.resistance) *
				corrosion.chance;
			if (Random.value < chance)
			{
				if (liquid)
				{
					MoveLiquidParticle(particlePos, particleObject.type, (sbyte)dir.x);
				}
				else
				{
					MoveParticle(particlePos, particleObject.type, dir);
				}
				return true;
			}
			UpdateSurroundingChunks(particlePos.x, particlePos.y);
		}
		return false;
	}
	bool CanPassThroughParticle(Vector2Int particlePos, ParticleObject particleObject, Vector2Int dir, bool liquid)
	{
		if (particleObject.passThorugh.strength > 0
			&& ParticleObjectFromIndex(particlePos.x + dir.x, particlePos.y + dir.y).type != particleObject.type)
		{
			ParticlePassThrough pass = particleObject.passThorugh;
			float chance = (pass.strength -
				ParticleObjectFromIndex(particlePos.x + dir.x, particlePos.y + dir.y).passThorugh.resistance) *
				pass.chance;
			if (Random.value < chance)
			{
				ParticleObject replaceParticle = ParticleObjectFromIndex(particlePos.x + dir.x, particlePos.y + dir.y);

				if (liquid)
				{
					MoveLiquidParticle(particlePos, particleObject.type, (sbyte)dir.x); //Deletes the particle at particlePos
					CreateParticle(replaceParticle.type, particlePos.x, particlePos.y); //This creates it again
				}
				else
				{
					MoveParticle(particlePos, particleObject.type, dir); //Deletes the particle at particlePos
					CreateParticle(replaceParticle.type, particlePos.x, particlePos.y);	//This creates it again
				}
				return true;
			}
			else //Didn't pass the check
			{
				UpdateSurroundingChunksNoCheck(particlePos.x, particlePos.y);
				SetParticleUpdateStatus(particlePos.x, particlePos.y, true);
				return false;
			}
		}
		return false;
	}
	#endregion

	bool CheckParticleDown(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(particlePos.y == 0)
		{
			return false;
		}

		if(CorrodeParticle(particlePos, particleObject, new Vector2Int(0, -1), false))
		{
			return true;
		}

		if(CanPassThroughParticle(particlePos, particleObject, new Vector2Int(0, -1), false))
		{
			return true;
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

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(randDir, -1), false))
		{
			return true;
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

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(1, -1), false))
		{
			return true;
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

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(-1, -1), false))
		{
			return true;
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

		if (CorrodeParticle(particlePos, particleObject, new Vector2Int(lastVelocity, 0), true))
		{
			return true;
		}

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(lastVelocity, 0), true))
		{
			return true;
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

		if (CorrodeParticle(particlePos, particleObject, new Vector2Int(randomVelocity, 0), true))
		{
			return true;
		}

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(randomVelocity, 0), true))
		{
			return true;
		}

		return DisperseParticle(particlePos, particleObject, randomVelocity);
	}
	bool CheckParticleRight(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(particlePos.x == simWidth - 1)
		{
			return false;
		}

		if (CorrodeParticle(particlePos, particleObject, new Vector2Int(1, 0), true))
		{
			return true;
		}

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(1, 0), true))
		{
			return true;
		}

		return DisperseParticle(particlePos, particleObject, 1);
	}
	bool CheckParticleLeft(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(particlePos.x == 0)
		{
			return false;
		}

		if (CorrodeParticle(particlePos, particleObject, new Vector2Int(-1, 0), true))
		{
			return true;
		}

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(-1, 0), true))
		{
			return true;
		}

		return DisperseParticle(particlePos, particleObject, -1);
	}
	bool CheckParticleUp(Vector2Int particlePos, ParticleObject particleObject)
	{
		if(particlePos.y == simHeight - 1)
		{
			return false;
		}

		if (CorrodeParticle(particlePos, particleObject, new Vector2Int(0, 1), false))
		{
			return true;
		}

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(0, 1), false))
		{
			return true;
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

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(dir, 1), false))
		{
			return true;
		}

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

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(1, 1), false))
		{
			return true;
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

		if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(-1, 1), false))
		{
			return true;
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
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(0, 1), false)){
					return true;
				}
				return CheckParticleUp(particlePos, particleObject);
			}
			else if (rand == 1)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(1, 0), true)){
					return true;
				}
				return CheckParticleRight(particlePos, particleObject);
			}
			else if (rand == 2)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(0, -1), false)){
					return true;
				}
				return CheckParticleDown(particlePos, particleObject);
			}
			else if (rand == 3)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(-1, 0), true)){
					return true;
				}
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
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(1, 1), false)){
					return true;
				}
				return CheckParticleUpRight(particlePos, particleObject);
			}
			else if (rand == 1)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(1, -1), false)){
					return true;
				}
				return CheckParticleDownRight(particlePos, particleObject);
			}
			else if (rand == 2)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(-1, -1), false)){
					return true;
				}
				return CheckParticleDownLeft(particlePos, particleObject);
			}
			else if (rand == 3)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(-1, 1), false)){
					return true;
				}
				return CheckParticleUpLeft(particlePos, particleObject);
			}
		}
		return false;
	}
	bool CheckParticleEightRandom(Vector2Int particlePos, ParticleObject particleObject)
	{
		if (CheckForAnyParticle(particlePos.x, particlePos.y + 1)
			&& CheckForAnyParticle(particlePos.x, particlePos.y - 1)
			&& CheckForAnyParticle(particlePos.x + 1, particlePos.y)
			&& CheckForAnyParticle(particlePos.x - 1, particlePos.y)
			&& CheckForAnyParticle(particlePos.x + 1, particlePos.y + 1)
			&& CheckForAnyParticle(particlePos.x - 1, particlePos.y + 1)
			&& CheckForAnyParticle(particlePos.x + 1, particlePos.y - 1)
			&& CheckForAnyParticle(particlePos.x - 1, particlePos.y - 1)) //Can't move in any direction
		{
			return false;
		}

		byte rand;

		for (byte i = 0; i < 4; i++)
		{
			rand = (byte)Random.Range(0, 8);
			if (rand == 0)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(0, 1), false)){
					return true;
				}
				return CheckParticleUp(particlePos, particleObject);
			}
			else if (rand == 1)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(1, 0), true)){
					return true;
				}
				return CheckParticleRight(particlePos, particleObject);
			}
			else if (rand == 2)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(0, -1), false)){
					return true;
				}
				return CheckParticleDown(particlePos, particleObject);
			}
			else if (rand == 3)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(-1, 0), true)){
					return true;
				}
				return CheckParticleLeft(particlePos, particleObject);
			}
			if (rand == 4)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(1, 1), false)){
					return true;
				}
				return CheckParticleUpRight(particlePos, particleObject);
			}
			else if (rand == 5)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(1, -1), false)){
					return true;
				}
				return CheckParticleDownRight(particlePos, particleObject);
			}
			else if (rand == 6)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(-1, -1), false)){
					return true;
				}
				return CheckParticleDownLeft(particlePos, particleObject);
			}
			else if (rand == 7)
			{
				if (CanPassThroughParticle(particlePos, particleObject, new Vector2Int(-1, 1), false)){
					return true;
				}
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

		for (byte i = 0; i < particleObject.spread.spreadChecks; i++)
		{
			if(particleObject.spread.spreadDirection == ParticleSpread.SpreadDirection.EightRandom)
			{
				rand = (byte)Random.Range(0, 8);
			}
			else if(particleObject.spread.spreadDirection == ParticleSpread.SpreadDirection.PlusRandom)
			{
				rand = (byte)Random.Range(0, 4);
			}
			else
			{
				rand = (byte)Random.Range(4, 8);
			}

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
				if (!particleObject.spread.canSpreadToAir
					&& CheckForParticle(0, newPosition.x, newPosition.y))
				{
					continue;
				}
			}

			ParticleSpread spread = particleObject.spread;
			float chance =
				(spread.strength - 
				ParticleObjectFromIndex(newPosition.x, newPosition.y).spread.resistance) *
				spread.chance;
			if (Random.value < chance)
			{
				CreateParticle(particleObject.type, newPosition.x, newPosition.y);
				SetParticleUpdateStatus(newPosition.x, newPosition.y, true);
			}

			if (particleObject.spread.spreadOnce)
			{
				return;
			}
		}
	}

	void CollideWithParticles(Vector2Int particlePos, ParticleObject particleObject)
	{
		if (CheckForParticle(particleObject.type, particlePos.x, particlePos.y + 1)
			&& CheckForParticle(particleObject.type, particlePos.x, particlePos.y - 1)
			&& CheckForParticle(particleObject.type, particlePos.x + 1, particlePos.y)
			&& CheckForParticle(particleObject.type, particlePos.x - 1, particlePos.y))
		{
			return;
		}

		ParticleCollision[] collisions = particleObject.collision;

		for (int i = 0; i < collisions.Length; i++)
		{
			bool delete = collisions[i].becomeParticle == null || collisions[i].becomeParticle.type == 0;

			byte particleToCheck;
			if (collisions[i].touchingParticle == null || collisions[i].touchingParticle.type == 0)
			{
				particleToCheck = 0;
			}
			else
			{
				particleToCheck = collisions[i].touchingParticle.type;
			}

			for (int j = 0; j < 4; j++)
			{
				switch (j)
				{
					case 0:
						if (CheckForParticle(particleToCheck, particlePos.x, particlePos.y - 1)
							&& particlePos.y > 0)
						{
							if(Random.value < collisions[i].chance)
							{
								if (!delete)
								{
									CreateParticle(collisions[i].becomeParticle.type, particlePos.x, particlePos.y);
								}
								else
								{
									DeleteParticle(particlePos.x, particlePos.y);
								}
							}
						}
						break;

					case 1:
						if (CheckForParticle(particleToCheck, particlePos.x + 1, particlePos.y)
							&& particlePos.x < simWidth - 1)
						{
							if (Random.value < collisions[i].chance)
							{
								if (!delete)
								{
									CreateParticle(collisions[i].becomeParticle.type, particlePos.x, particlePos.y);
								}
								else
								{
									DeleteParticle(particlePos.x, particlePos.y);
								}
							}
						}
						break;

					case 2:
						if (CheckForParticle(particleToCheck, particlePos.x - 1, particlePos.y)
							&& particlePos.x > 0)
						{
							if (Random.value < collisions[i].chance)
							{
								if (!delete)
								{
									CreateParticle(collisions[i].becomeParticle.type, particlePos.x, particlePos.y);
								}
								else
								{
									DeleteParticle(particlePos.x, particlePos.y);
								}
							}
						}
						break;

					case 3:
						if (CheckForParticle(particleToCheck, particlePos.x, particlePos.y + 1)
							&& particlePos.y < simHeight - 1)
						{
							if (Random.value < collisions[i].chance)
							{
								if (!delete)
								{
									CreateParticle(collisions[i].becomeParticle.type, particlePos.x, particlePos.y);
								}
								else
								{
									DeleteParticle(particlePos.x, particlePos.y);
								}
							}
						}
						break;
				}
			}
		}
	}
	#endregion
}