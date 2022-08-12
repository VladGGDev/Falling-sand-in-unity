using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;

public class ParticleLogic : MonoBehaviour
{
	public int simWidth = 10;
	public int simHeight = 10;
	public Particle[,] particles;
	public ParticleObject[] particleObjects;

	[Space(10f)]
	public Texture2D texture;

	public Color backGroundColor = Color.white;
	public byte backGroundType = 0;

	public Color[] particleColors;
	public Renderer rend;

	private void Awake()
	{
		particles = new Particle[simWidth, simHeight];
		for (int y = 0; y < particles.GetLength(1); y++)
		{
			for (int x = 0; x < particles.GetLength(0); x++)
			{
				particles[x, y] = new Particle();
				particles[x, y].type = 0;
				particles[x, y].useSecondColor = Random.value > 0.5f;
				particles[x, y].fluidHVel = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;
			}
		}
		texture = new Texture2D(simWidth, simHeight);
		particleColors = new Color[particles.Length];
		texture.filterMode = FilterMode.Point;
	}



	#region Helping Functions
	//int PositionToIndex (int xPos, int yPos)
	//{
	//	xPos = Mathf.Clamp(xPos, 0, simWidth -1);
	//	yPos = Mathf.Clamp(yPos, 0, simHeight - 1);
	//  return yPos * simWidth + xPos;
	//}

	//Vector2Int IndexToPosition(int index)
	//{
	//	return new Vector2Int(index % simWidth, index / simWidth);
	//}

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
		//if (atatIndexPos < 0 || atIndex >= particles.Length)
		//{
		//	Debug.LogWarning("In the DeleteParticle() function, atIndex (= " + atIndex + ") int is outside of array bounds");
		//	return;
		//}

		particles[atPos.x, atPos.y].type = 0;
	}

	public void CreateParticle(byte type, Vector2Int atPos)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);
		//if (atPos.y < 0 || atPos.y >= particles.Length)
		//{
		//	Debug.LogWarning("In the CreateParticle() function, atIndex (= " + atPos + ") int is outside of array bounds");
		//	return;
		//}

		particles[atPos.x, atPos.y].type = type;
	}

	public void SetParticleUpdateStatus(Vector2Int atPos, bool status)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);

		particles[atPos.x, atPos.y].hasBeenUpdated = status;
	}
	#endregion



	private void FixedUpdate()      //Runs at 50 FPS by default
	{
		for (int y = 0; y < simHeight; y++)
		{
			for (int x = 0; x < simWidth; x++)
			{
				if (particles[x, y].hasBeenUpdated && particles[x, y].type != 0)
				{
					continue;
				}

				for (int i = 0; i < particleObjects.Length; i++)    //Check for every particle object
				{
					if (CheckForParticle(particleObjects[i].type, new Vector2Int(x, y)))
					{
						//Check for every movement check for this particle object
						foreach (ParticleMoveChecks check in particleObjects[i].moveChecks)
						{
							if (check.moveDirection == ParticleMoveChecks.MoveDirection.Down)
							{
								if (CheckParticleDown(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.RandomDownDiagonal)
							{
								if (CheckParticleRandomDownDiagonal(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.DownRight)
							{
								if (CheckParticleDownRight(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.DownLeft)
							{
								if (CheckParticleDownLeft(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.TowardsHorisontalVelocity)
							{
								if (CheckParticleTowardsVelocity(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.RandomHorisontal)
							{
								if (CheckParticleRandomVelocity(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.Right)
							{
								if (CheckParticleRight(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.Left)
							{
								if (CheckParticleLeft(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.Up)
							{
								if (CheckParticleUp(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.RandomUpDiagonal)
							{
								if (CheckParticleRandomUpDiagonal(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.UpRight)
							{
								if (CheckParticleUpRight(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
							else if (check.moveDirection == ParticleMoveChecks.MoveDirection.UpLeft)
							{
								if (CheckParticleUpLeft(new Vector2Int(x, y), particleObjects[i].type))
									break;
							}
						}
					}
				}
				//if (CheckForParticle(1, new Vector2Int(x, y)))     //Sand physics
				//{
				//	SandPhysics(new Vector2Int(x, y));
				//}
				//else if (CheckForParticle(2, new Vector2Int(x, y)))     //Water physics
				//{
				//	WaterPhysics(new Vector2Int(x, y));
				//}
				//else if (CheckForParticle(3, new Vector2Int(x, y)))     //Solid physics
				//{
				//	continue;
				//}
			}
		}
		DrawParticles();
	}

	public void DrawParticles()    //	!!!!! ADD MORE IFs WHEN YOU ADD MORE PARTICLE TYPES !!!!!!
	{
		for (int i = 0; i < particles.Length; i++)
		{
			int ix = i % simWidth;
			int iy = i / simWidth;

			particles[ix, iy].hasBeenUpdated = false;   //Reset update status for every particle

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
				else if (CheckForParticle(backGroundType, new Vector2Int(ix, iy)))
				{
					particleColors[i] = backGroundColor;
				}
			}
			//Color particleColor = Color.clear;

			//if (CheckForParticle(0, new Vector2Int(ix, iy)))
			//{
			//	particleColors[i] = airColor;	//The air shouldn't have color variations. That would be unhealthy.
			//	continue;
			//}
			//else if (CheckForParticle(1, new Vector2Int(ix, iy)))
			//{
			//	particleColor = sandColor;
			//}
			//else if (CheckForParticle(2, new Vector2Int(ix, iy)))
			//{
			//	particleColor = waterColor;
			//}
			//else if (CheckForParticle(3, new Vector2Int(ix, iy)))
			//{
			//	particleColor = solidColor;
			//}

			//if (particles[ix, iy].useSecondColor)
			//{
			//	particleColor = Color.Lerp(particleColor, Color.black, colorDiffrence);
			//}

			//particleColors[i] = particleColor;
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

		sbyte dir = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x + dir, particlePos.y - 1)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + dir, particlePos.y - 1), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x + dir, particlePos.y - 1));
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

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x + lastVelocity, particlePos.y)))
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + lastVelocity, particlePos.y), true);
			CreateParticle(particleType, new Vector2Int(particlePos.x + lastVelocity, particlePos.y));
			particles[particlePos.x + lastVelocity, particlePos.y].fluidHVel = lastVelocity;
			particles[particlePos.x, particlePos.y].fluidHVel = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;
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