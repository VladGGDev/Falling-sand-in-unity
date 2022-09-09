using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public float minParticleStepTime = 0.02f;
    public ParticleObject airParticleObject;
	[Tooltip("Pretend this is a button")]
    public bool sortParticlesButton = false;
    public ParticleObject[] particleObjects;
    public static ParticleManager instance;

	private void Awake()
	{
        if(instance == null)
		{
            instance = this;
            DontDestroyOnLoad(gameObject);
		}
		else
		{
            Destroy(gameObject);
            return;
		}

        SortParticleObjectTypes(particleObjects);
        MakeParticleTypesConsecutive(particleObjects);
    }

	private void OnValidate()
	{
		if (sortParticlesButton)
		{
			SortParticleObjectTypes(particleObjects);
            MakeParticleTypesConsecutive(particleObjects);
            sortParticlesButton = false;
        }
	}

	public static void SortParticleObjectTypes(ParticleObject[] particleObjects)
    {
        for (int i = 0; i < particleObjects.Length - 1; i++)
        {
            // Find the minimum element in unsorted array
            int min_idx = i;
            for (int j = i + 1; j < particleObjects.Length; j++)
                if (particleObjects[j].type < particleObjects[min_idx].type)
                    min_idx = j;

            // Swap the found minimum element with the first element
            ParticleObject temp = particleObjects[min_idx];
            particleObjects[min_idx] = particleObjects[i];
            particleObjects[i] = temp;
        }
    }

    public static void MakeParticleTypesConsecutive(ParticleObject[] particleObjects)
    {
        particleObjects[0].type = 1;

        for (byte i = 0; i < particleObjects.Length - 1; i++)
        {
            if (particleObjects[i].type + 1 != particleObjects[i + 1].type)
            {
                particleObjects[i + 1].type = (byte)(particleObjects[i].type + 1);
            }
        }
    }
}
