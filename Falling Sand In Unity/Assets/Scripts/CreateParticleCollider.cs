using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class CreateParticleCollider : MonoBehaviour
{
    public ParticleLogic particleLogic;
    public ParticleColliderGroup[] groups;
    [Space(10)]
    [Tooltip("How many physics frames should the script skip before generating the collider.")]
    public int skipPhysicsFrames = 0;
    int framesSkiped = 0;

    private void Start()
    {
        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].outputParticles = new byte[particleLogic.simWidth, particleLogic.simHeight];
        }
    }

    private void FixedUpdate()
    {
        framesSkiped--;
        if(framesSkiped < 0)
        {
            framesSkiped = skipPhysicsFrames;
        }
        else
        {
            return;
        }

        //After checking if it should skip, generate the collider
        for (int i = 0; i < groups.Length; i++)
        {
            IgnoreParticles(groups[i]);
        }
    }
    void IgnoreParticles(ParticleColliderGroup g)
    {
		if(g.minNeighbors == 0)
		{
			for (int y = 0; y < particleLogic.simHeight; y++)
			{
				for (int x = 0; x < particleLogic.simWidth; x++)
				{
					if (particleLogic.particles[x, y].type != 0)
					g.outputParticles[x, y] = particleLogic.particles[x, y].type;
				}
			}
		}

        int neighbors = 0;
        bool[,] keepParticleH = new bool[particleLogic.simWidth, particleLogic.simHeight];
        bool[,] keepParticleV = new bool[particleLogic.simWidth, particleLogic.simHeight];

        //Horisontal neighbor check
		for (int y = 0; y < particleLogic.simHeight; y++){
			for (int x = 0; x < particleLogic.simWidth; x++){   //Repeats for every particle

                if (particleLogic.particles[x, y].type == 0
                    || !CheckForParticleInGroup(x, y))
                {
                    continue;
                }

                //Checks for neighbors
                int ii = 1;
                while (true)
                {
					if (x + ii >= particleLogic.simWidth)
					{
						//neighbors = ii - 1;
						keepParticleH[x, y] = true;
						break;
					}

					if (CheckForParticleInGroup(x + ii, y))
					{
						neighbors++;
					}
					else
					{
						if(neighbors < g.minNeighbors)
						{
							if (x == 0)
							{
								keepParticleH[x, y] = true;
							}
							else
							{
								keepParticleH[x, y] = false;
							}
						}
						else
						{
							keepParticleH[x, y] = true;
						}
						break;
					}
					ii++;
                }

				if (keepParticleH[x, y])
                {
                    for (int i = 1; i <= neighbors; i++)
                    {
                        keepParticleH[x + i, y] = true;
                    }
                    x += neighbors + 1;
                }
                neighbors = 0;
			}
		}

		//Vertical neighbor check
		for (int x = 0; x < particleLogic.simWidth; x++)
		{
			for (int y = 0; y < particleLogic.simHeight; y++)
			{   //Repeats for every particle

				if (particleLogic.particles[x, y].type == 0
                    || !CheckForParticleInGroup(x, y))
				{
					continue;
				}

				//Checks for neighbors
				int ii = 1;
				while (true)
				{
					if (y + ii >= particleLogic.simHeight)
					{
						//neighbors = ii - 1;
						keepParticleV[x, y] = true;
						break;
					}

					if (CheckForParticleInGroup(x, y + ii))
					{
						neighbors++;
					}
					else
					{
						if (neighbors < g.minNeighbors)
						{
							if (y == 0)
							{
								keepParticleV[x, y] = true;
							}
							else
							{
								keepParticleV[x, y] = false;
							}
						}
						else
						{
							keepParticleV[x, y] = true;
						}
						break;
					}
					ii++;
				}

				if (keepParticleV[x, y])
				{
					for (int i = 1; i <= neighbors; i++)
					{
						keepParticleV[x, y + i] = true;
					}
					y += neighbors + 1;
				}
				neighbors = 0;
			}
		}

		//keepParticle array to Particle array
		for (int y = 0; y < particleLogic.simHeight; y++)
		{
			for (int x = 0; x < particleLogic.simWidth; x++)
			{
                if (keepParticleH[x, y] && keepParticleV[x, y])
                {
                    g.outputParticles[x, y] = particleLogic.particles[x, y].type;
                }
                else
                {
                    g.outputParticles[x, y] = 0;
				}
			}
		}

		bool CheckForParticleInGroup(int x, int y)
        {
			for (int i = 0; i < g.particleObjects.Length; i++)  //Checks for every particle object
			{
				if (particleLogic.particles[x, y].type == g.particleObjects[i].type)
				{
					return true;
				}
			}
            return false;
		}
	}
}