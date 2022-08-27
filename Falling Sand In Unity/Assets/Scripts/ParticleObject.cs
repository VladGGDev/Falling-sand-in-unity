using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "New Particle", menuName = "Scriptable Objects/Particle")]
public class ParticleObject : ScriptableObject
{
	[Tooltip("The type of the particle will be used in scripts.\n" +
		"Each particle must have a unique type (just like an id).")]
	public byte type = 1;
	[Tooltip("The name is an optional alternative for the type\n")]
	public string particleName = "New Particle";
	public Color primaryColor = Color.black;
	[Tooltip("The secondary color is usually darker.")]
	public Color secondaryColor = Color.black;

	[Space(15)]
	[Tooltip("How many frames should a particle wait before moving.")]
	public ushort waitFrames = 0;
	//[Tooltip("If the density of a particle is bigger than the density of another, it will pass through.\n" +
	//	"If the densities are equal, the particles will interract normally.")]
	//public int density = 0;

	[Tooltip("The minimum distance the liquid will spread.\nA lower value than the max or simply 1 should be here.")]
	public int dispersionSpeedMin = 1;
	[Tooltip("The maximum distance the liquid will spread.\nHigher values will not form lumps.")]
	public int dispersionSpeedMax = 1;

	[Space(15)]
	public ParticleLife lifeSettings;

	[Space(15)]
	public ParticleCorrosion corrosionSettings;

	[Space(15)]
	[Tooltip("The particle will check in the order of the elements in this array if it can move.")]
	public ParticleMoveChecks[] moveChecks = new ParticleMoveChecks[] 
		{new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Down, 0.94f),
		new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomDownDiagonal, 0.84f),
		new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownRight, 0.6f),
		new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownLeft, 0.86f)};

	[Space(10)]
	[Tooltip("After the particle moves, it will try to spread around.")]
	public ParticleSpread spreadSettings;

	private void OnValidate()
	{
		if (spreadSettings.freshForFrames < 1 && spreadSettings.strength > 0)
			spreadSettings.freshForFrames = 1;
	}
}

[System.Serializable]
public class ParticleMoveChecks
{
	public ParticleMoveChecks(MoveDirection direction, float chance)
	{
		moveDirection = direction;
		movementChance = chance;
	}

	public enum MoveDirection
	{
		Down,
		RandomDownDiagonal,
		DownRight,
		DownLeft,
		TowardsHorisontalVelocity,
		RandomHorisontal,
		Right,
		Left,
		Up,
		RandomUpDiagonal,
		UpRight,
		UpLeft,
		PlusRandom,
		XRandom,
		EightRandom
	}

	[Tooltip("Choose the particle movement direction.")]
	public MoveDirection moveDirection;
	[Range(0, 1f)]
	[Tooltip("How likely is the particle to move this frame.\n" +
		"Choose 1 if the particle should always move.")]
	public float movementChance = 1f;
	[Tooltip("If the Movement Chance fails should this particle check if it can still move in another way?\n" +
		"Useful for making snow or gas.")]
	public bool continueIfFailed = false;
}

[System.Serializable]
public class ParticleCorrosion
{
	[Range(0, 1f)]
	[Tooltip("The resistance to corrosion of a particle.\n" +
		"Acids must have a resistance of 1 so that they don't corode themselves")]
	public float resistance = 0;
	[Range(0, 1f)]
	[Tooltip("The strength of the acid.\n" +
		"If the strength is less than the resistance of the other particle, it will not be coroded.")]
	public float strength = 0;
	[Range(0, 1f)]
	[Tooltip("The chance to corode.")]
	public float chance = 0;
}

[System.Serializable]
public class ParticleSpread
{
	[Range(0, 1f)]
	[Tooltip("The resistance to spread of a particle.")]
	public float resistance = 0;
	[Range(0, 1f)]
	[Tooltip("The strength of the spread.\n" +
		"If the strength is less than the resistance of the other particle, it will not spread to it.")]
	public float strength = 0;
	[Range(0, 1f)]
	[Tooltip("The chance to spread.")]
	public float chance = 0;
	[Tooltip("For how many frames this particle will be considered fresh.")]
	public int freshForFrames = 1;
	[Tooltip("The color this particle will have when it first spreads.")]
	public Color freshSpreadColor = Color.black;
	[Tooltip("The particle color will interpolate between the normal (at t 0) and the fresh color (at t 1)" +
		" using the value of the curve as the time component.\n Set all keys to value 0 to not use the fresh color.")]
	public AnimationCurve freshColorCurve = 
		new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });
	[Tooltip("Can this particle spread to air?")]
	public bool canSpreadToAir = false;
}

[System.Serializable]
public class ParticleLife
{
	[Tooltip("Will this particle live forever?")]
	public bool liveForever = true;
	[Tooltip("For how many frames should this particle be alive if the particle doesn't live forever.")]
	public int minLifeTime = 300;
	[Tooltip("For how many frames should this particle be alive if the particle doesn't live forever.")]
	public int maxLifeTime = 1000;
	[Tooltip("The color of the particle will start changing to this the closer it gets to death.")]
	public Color deathColor = Color.black;
	[Tooltip("The particle color will interpolate between the normal (at t 0) and the death color (at t 1)" +
		" using the value of the curve as the time component.\n Set all keys to value 0 to not use the death color.")]
	public AnimationCurve deathColorCurve = new AnimationCurve(
		new Keyframe[3] { new Keyframe(0, 0, 0, 0), new Keyframe(0.5f, 0, 0, 2f), new Keyframe(1f, 1f, 2f, 0) });
}