using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Particle Settings", menuName = "tempSettings")]
public class ParticleSettings : ScriptableObject
{
    public bool sortParticles = false;
	[SerializeField]
    ParticleObject[] allParticleObjects;
    public static ParticleObject[] particleObjects;

	private void OnEnable()
	{
		SortParticleObjectTypes();
		MakeParticleTypesConsecutive();
		MakeParticleObjectsStatic();
	}

	private void Awake()
	{
		SortParticleObjectTypes();
		MakeParticleTypesConsecutive();
		MakeParticleObjectsStatic();
	}

	private void OnValidate()
	{
        if (sortParticles)
        {
            SortParticleObjectTypes();
            MakeParticleTypesConsecutive();
            MakeParticleObjectsStatic();

            sortParticles = false;
            Debug.Log("Sorted particle objects.");
        }
    }

	void SortParticleObjectTypes()
	{
        for (int i = 0; i < allParticleObjects.Length - 1; i++)
        {
            // Find the minimum element in unsorted array
            int min_idx = i;
            for (int j = i + 1; j < allParticleObjects.Length; j++)
                if (allParticleObjects[j].type < allParticleObjects[min_idx].type)
                    min_idx = j;

            // Swap the found minimum element with the first element
            ParticleObject temp = allParticleObjects[min_idx];
            allParticleObjects[min_idx] = allParticleObjects[i];
            allParticleObjects[i] = temp;
        }
    }

    void MakeParticleTypesConsecutive()
	{
        allParticleObjects[0].type = 1;

		for (byte i = 0; i < allParticleObjects.Length; i++)
		{
            if (i == allParticleObjects.Length - 1)
                break;

			if (allParticleObjects[i].type + 1 != allParticleObjects[i + 1].type)
			{
                allParticleObjects[i + 1].type = (byte)(allParticleObjects[i].type + 1);
			}
		}
	}

    void MakeParticleObjectsStatic()
	{
        particleObjects = new ParticleObject[allParticleObjects.Length];
        
        for (byte i = 0; i < allParticleObjects.Length; i++)
		{
            particleObjects[i] = ScriptableObject.CreateInstance<ParticleObject>();
            particleObjects[i] = allParticleObjects[i];
		}
	}
}