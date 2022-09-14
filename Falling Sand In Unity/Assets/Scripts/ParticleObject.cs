using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Particle", menuName = "Particles/Empty Particle", order = 0)]
public class ParticleObject : ScriptableObject
{
	[Tooltip("The type of the particle will be used in scripts.\n" +
		"Each particle must have a unique type (just like an id).")]
	public byte type = 1;
	[Tooltip("The colors of the particle. The gradient is used to change the proportion of colors")]
	public Gradient colors;
	[Tooltip("Instead of using a noise value to determine the color, the particle will use this texture instead.\n" +
		"Leave empty to use the color gradient from above.")]
	public Texture2D texture;

	[Space(15)]
	[Tooltip("How many frames should a particle wait before moving.")]
	public int waitFrames = 0;

	[Tooltip("The minimum distance the liquid will spread.\nA lower value than the max or simply 1 should be here.")]
	public int dispersionSpeedMin = 1;
	[Tooltip("The maximum distance the liquid will spread.\nHigher values will not form lumps.")]
	public int dispersionSpeedMax = 1;

	[Space(15)]
	public ParticleLife life;

	[Space(15)]
	[Tooltip("The particle will check in the order of the elements in this array if it can move.")]
	public ParticleMoveChecks[] moveChecks;

	[Space(15)]
	[Tooltip("The pass through settings.\nUseful for making particles like sand pass through water.")]
	public ParticlePassThrough passThorugh;

	[Space(15)]
	[Tooltip("The corrosion settings of the particle.\nUse this to make acid.")]
	public ParticleCorrosion corrosion;

	[Space(10)]
	[Tooltip("After the particle moves, it will try to spread around.")]
	public ParticleSpread spread;

	[Space(15)]
	[Tooltip("The collision settings of this particle.")]
	public ParticleCollision[] collision;

	private void OnValidate()
	{
		if (spread.freshForFrames < 1 && spread.strength > 0)
			spread.freshForFrames = 1;
	}
}

[System.Serializable]
public class ParticleMoveChecks
{
	public ParticleMoveChecks(MoveDirection direction, float chance)
	{
		moveDirection = direction;
		movementChance = chance;
		continueIfFailed = false;
	}
	public ParticleMoveChecks(MoveDirection direction, float chance, bool continueFailed)
	{
		moveDirection = direction;
		movementChance = chance;
		continueIfFailed = continueFailed;
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
	[Range(-1f, 1f)]
	[Tooltip("The resistance to corrosion of a particle.")]
	public float resistance = 0;
	[Range(0, 2f)]
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
	[Range(-1f, 1f)]
	[Tooltip("The resistance to spread of a particle.")]
	public float resistance = 0;
	[Range(0, 2f)]
	[Tooltip("The strength of the spread.\n" +
		"If the strength is less than the resistance of the other particle, it will not spread to it.")]
	public float strength = 0;
	[Range(0, 1f)]
	[Tooltip("The chance to spread.")]
	public float chance = 0;
	public enum SpreadDirection
	{
		PlusRandom,
		XRandom,
		EightRandom
	}
	[Tooltip("The direction that the particle will try to spread to.")]
	public SpreadDirection spreadDirection = SpreadDirection.EightRandom;
	[Tooltip("The particle will check this many times in a random direction if it can spread around.")]
	public byte spreadChecks = 4;
	[Tooltip("Should this particle check if it can spread again after a successful spread.")]
	public bool spreadOnce = true;
	[Tooltip("For how many frames this particle will be considered fresh.")]
	public int freshForFrames = 0;
	[Tooltip("The color this particle will have when it first spreads.")]
	public Color freshSpreadColor = Color.cyan;
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
	public int maxLifeTime = 400;
	[Tooltip("The color of the particle will start changing to this the closer it gets to death.")]
	public Color deathColor = Color.black;
	[Tooltip("The particle color will interpolate between the normal (at t 0) and the death color (at t 1)" +
		" using the value of the curve as the time component.\n Set all keys to value 0 to not use the death color.")]
	public AnimationCurve deathColorCurve = new AnimationCurve(
		new Keyframe[2] { new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0)});
	[Space(10f)]
	[Tooltip("What particle should be created in place of the one that dies.\nLeave empty to not create anything.")]
	public ParticleObject createOnDeath;
	[Range(0, 1f)]
	[Tooltip("The chance to create a particle on death")]
	public float chance = 0.5f;
}

[System.Serializable]
public class ParticlePassThrough
{
	[Range(-1f, 1f)]
	[Tooltip("The resistance to pass through of a particle.")]
	public float resistance = 0;
	[Range(0, 2f)]
	[Tooltip("The strength of the pass thorugh.\n" +
		"If the strength is less than the resistance of the other particle, it will not be coroded.")]
	public float strength = 0;
	[Range(0, 1f)]
	[Tooltip("The chance to pass thorugh.")]
	public float chance = 0;
}

[System.Serializable]
public class ParticleCollision
{
	[Tooltip("The particle that this particle touches.\nLeave empty for air.")]
	public ParticleObject touchingParticle;
	[Tooltip("What particle should replace this one.\nLeave empty for air.")]
	public ParticleObject becomeParticle;
	[Range(0, 1f)]
	[Tooltip("The chance to create a particle on collision")]
	public float chance = 0.5f;
}


[System.Serializable]
static class ParticleObjectDefaults
{
	[MenuItem("Assets/Create/Particles/Defaults/Sand")]
	static void CreateSandParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 0;
		asset.dispersionSpeedMin = 1;
		asset.dispersionSpeedMax = 1;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(new Color(1f, 0.87f, 0.137f), 0.333f),
			new GradientColorKey(new Color(1f, 0.803f, 0.16f), 0.666f)
		};

		asset.life = new ParticleLife();
		asset.life.liveForever = true;
		asset.life.minLifeTime = 500;
		asset.life.maxLifeTime = 600;
		asset.life.deathColor = Color.black;
		asset.life.deathColorCurve = new AnimationCurve(
			new Keyframe[2] { new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0) }); ;
		asset.life.createOnDeath = null;
		asset.life.chance = 0.5f;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 0.2f;
		asset.corrosion.strength = 0;
		asset.corrosion.chance = 0;

		asset.moveChecks = new ParticleMoveChecks[]
		{
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Down, 0.94f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomDownDiagonal, 0.84f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownRight, 0.6f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownLeft, 0.86f)
		};

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 0.6f;
		asset.spread.strength = 0;
		asset.spread.chance = 0.5f;
		asset.spread.canSpreadToAir = false;
		asset.spread.freshForFrames = 1;
		asset.spread.freshSpreadColor = Color.cyan;
		asset.spread.spreadChecks = 4;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.EightRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		path += "/Sand.asset";

		ProjectWindowUtil.CreateAsset(asset, path);
	}

	[MenuItem("Assets/Create/Particles/Defaults/Water")]
	static void CreateWaterParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 0;
		asset.dispersionSpeedMin = 1;
		asset.dispersionSpeedMax = 16;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(new Color(0.105f, 0.592f, 1f), 0),
			new GradientColorKey(new Color(0.098f, 0.541f, 1f), 1f)
		};

		asset.life = new ParticleLife();
		asset.life.liveForever = true;
		asset.life.minLifeTime = 500;
		asset.life.maxLifeTime = 600;
		asset.life.deathColor = Color.black;
		asset.life.deathColorCurve = new AnimationCurve(
			new Keyframe[2] { new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0) }); ;
		asset.life.createOnDeath = null;
		asset.life.chance = 0.5f;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 1f;
		asset.corrosion.strength = 0;
		asset.corrosion.chance = 0;

		asset.moveChecks = new ParticleMoveChecks[8]
		{
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Down, 0.963f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomDownDiagonal, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownRight, 0.7f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownLeft, 0.8f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.TowardsHorisontalVelocity, 0.739f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomHorisontal, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Right, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Left, 0.165f),
		};

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 0.6f;
		asset.spread.strength = 0;
		asset.spread.chance = 0.5f;
		asset.spread.canSpreadToAir = false;
		asset.spread.freshForFrames = 1;
		asset.spread.freshSpreadColor = Color.cyan;
		asset.spread.spreadChecks = 4;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.EightRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		path += "/Water.asset";

		ProjectWindowUtil.CreateAsset(asset, path);
	}

	[MenuItem("Assets/Create/Particles/Defaults/Solid")]
	static void CreateSolidParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 0;
		asset.dispersionSpeedMin = 1;
		asset.dispersionSpeedMax = 1;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[3]
		{
			new GradientColorKey(new Color(0.717f, 0, 0), 0),
			new GradientColorKey(new Color(0.6f, 0, 0), 0.5f),
			new GradientColorKey(new Color(0.62f, 0, 0), 1f)
		};

		asset.life = new ParticleLife();
		asset.life.liveForever = true;
		asset.life.minLifeTime = 500;
		asset.life.maxLifeTime = 600;
		asset.life.deathColor = Color.black;
		asset.life.deathColorCurve = new AnimationCurve(
			new Keyframe[2] { new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0) }); ;
		asset.life.createOnDeath = null;
		asset.life.chance = 0.5f;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 0f;
		asset.corrosion.strength = 0;
		asset.corrosion.chance = 0;

		asset.moveChecks = new ParticleMoveChecks[0];

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 0f;
		asset.spread.strength = 0;
		asset.spread.chance = 0.5f;
		asset.spread.canSpreadToAir = false;
		asset.spread.freshForFrames = 1;
		asset.spread.freshSpreadColor = Color.cyan;
		asset.spread.spreadChecks = 4;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.EightRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		path += "/Solid.asset";

		ProjectWindowUtil.CreateAsset(asset, path);
		Debug.Log("Modify the corrosion and spread resistance for materials similar to wood, concrete, ceramic etc.");
	}

	[MenuItem("Assets/Create/Particles/Defaults/Gas")]
	static void CreateGasParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 1;
		asset.dispersionSpeedMin = 1;
		asset.dispersionSpeedMax = 2;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(new Color(0.423f, 0.631f, 0.27f), 0.13f),
			new GradientColorKey(new Color(0.38f, 0.568f, 0.243f), 0.88f)
		};

		asset.life = new ParticleLife();
		asset.life.liveForever = true;
		asset.life.minLifeTime = 500;
		asset.life.maxLifeTime = 600;
		asset.life.deathColor = Color.black;
		asset.life.deathColorCurve = new AnimationCurve(
			new Keyframe[2] { new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0) }); ;
		asset.life.createOnDeath = null;
		asset.life.chance = 0.5f;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 0;
		asset.corrosion.strength = 0;
		asset.corrosion.chance = 0;

		asset.moveChecks = new ParticleMoveChecks[8]
		{
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Up, 0.08f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomUpDiagonal, 0.133f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.UpRight, 0.34f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.UpLeft, 0.375f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.TowardsHorisontalVelocity, 1f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomHorisontal, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Right, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Left, 0.222f),
		};

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 0.0f;
		asset.spread.strength = 0;
		asset.spread.chance = 0.5f;
		asset.spread.canSpreadToAir = false;
		asset.spread.freshForFrames = 1;
		asset.spread.freshSpreadColor = Color.cyan;
		asset.spread.spreadChecks = 4;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.EightRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		path += "/Gas.asset";

		ProjectWindowUtil.CreateAsset(asset, path);
	}

	[MenuItem("Assets/Create/Particles/Defaults/Snow")]
	static void CreateSnowParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 1;
		asset.dispersionSpeedMin = 1;
		asset.dispersionSpeedMax = 1;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(new Color(0.941f, 0.941f, 0.941f), 0.5f),
			new GradientColorKey(new Color(0.909f, 0.909f, 0.909f), 1f)
		};
		asset.colors.mode = GradientMode.Fixed;

		asset.life = new ParticleLife();
		asset.life.liveForever = true;
		asset.life.minLifeTime = 500;
		asset.life.maxLifeTime = 600;
		asset.life.deathColor = Color.black;
		asset.life.deathColorCurve = new AnimationCurve(
			new Keyframe[2] { new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0) }); ;
		asset.life.createOnDeath = null;
		asset.life.chance = 0.5f;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 0;
		asset.corrosion.strength = 0;
		asset.corrosion.chance = 0;

		asset.moveChecks = new ParticleMoveChecks[4]
		{
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Down, 0.08f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomDownDiagonal, 0.302f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownRight, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownLeft, 1f),
		};

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 0.6f;
		asset.spread.strength = 0;
		asset.spread.chance = 0.5f;
		asset.spread.canSpreadToAir = false;
		asset.spread.freshForFrames = 1;
		asset.spread.freshSpreadColor = Color.cyan;
		asset.spread.spreadChecks = 4;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.EightRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		path += "/Snow.asset";

		ProjectWindowUtil.CreateAsset(asset, path);
	}

	[MenuItem("Assets/Create/Particles/Defaults/Acid")]
	static void CreateAcidParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 0;
		asset.dispersionSpeedMin = 1;
		asset.dispersionSpeedMax = 10;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(new Color(0.372f, 1f, 0f), 0.333f),
			new GradientColorKey(new Color(0.066f, 1f, 0f), 0.666f)
		};

		asset.life = new ParticleLife();
		asset.life.liveForever = true;
		asset.life.minLifeTime = 500;
		asset.life.maxLifeTime = 600;
		asset.life.deathColor = Color.black;
		asset.life.deathColorCurve = new AnimationCurve(
			new Keyframe[2] { new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0) }); ;
		asset.life.createOnDeath = null;
		asset.life.chance = 0.5f;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 0;
		asset.corrosion.strength = 0.5f;
		asset.corrosion.chance = 0.75f;

		asset.moveChecks = new ParticleMoveChecks[8]
		{
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Down, 0.963f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomDownDiagonal, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownRight, 0.7f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.DownLeft, 0.8f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.TowardsHorisontalVelocity, 0.739f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomHorisontal, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Right, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Left, 0.165f),
		};

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 0.6f;
		asset.spread.strength = 0;
		asset.spread.chance = 0.5f;
		asset.spread.canSpreadToAir = false;
		asset.spread.freshForFrames = 1;
		asset.spread.freshSpreadColor = Color.cyan;
		asset.spread.spreadChecks = 4;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.EightRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		path += "/Acid.asset";

		ProjectWindowUtil.CreateAsset(asset, path);
	}

	[MenuItem("Assets/Create/Particles/Defaults/Smoke")]
	static ParticleObject CreateSmokeParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 1;
		asset.dispersionSpeedMin = 1;
		asset.dispersionSpeedMax = 1;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(new Color(0.509f, 0.509f, 0.509f), 0.33f),
			new GradientColorKey(new Color(0.564f, 0.564f, 0.564f), 0.66f)
		};

		asset.life = new ParticleLife();
		asset.life.liveForever = false;
		asset.life.minLifeTime = 100;
		asset.life.maxLifeTime = 200;
		asset.life.deathColor = Color.white;
		asset.life.deathColorCurve = new AnimationCurve(new Keyframe[3]
		{
			new Keyframe(0, 0, 0, 2f), new Keyframe(0.5f, 1f, 0, 0, 2f, 2f), new Keyframe(1f, 0, 2f, 0)
		});
		asset.life.createOnDeath = null;
		asset.life.chance = 0.5f;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 0;
		asset.corrosion.strength = 0;
		asset.corrosion.chance = 0;

		asset.moveChecks = new ParticleMoveChecks[8]
		{
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Up, 0.08f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomUpDiagonal, 0.133f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.UpRight, 0.34f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.UpLeft, 0.375f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.TowardsHorisontalVelocity, 1f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.RandomHorisontal, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Right, 1f),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Left, 0.222f),
		};

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 0.6f;
		asset.spread.strength = 0;
		asset.spread.chance = 0.5f;
		asset.spread.canSpreadToAir = false;
		asset.spread.freshForFrames = 1;
		asset.spread.freshSpreadColor = Color.cyan;
		asset.spread.spreadChecks = 4;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.EightRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		path += "/Smoke.asset";

		ProjectWindowUtil.CreateAsset(asset, path);
		return asset;
	}

	[MenuItem("Assets/Create/Particles/Defaults/Corruption")]
	static void CreateCorruptionParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 0;
		asset.dispersionSpeedMin = 1;
		asset.dispersionSpeedMax = 1;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(new Color(0.192f, 0.745f, 0.886f), 0.5f),
			new GradientColorKey(new Color(0.149f, 0.717f, 0.909f), 1f)
		};
		asset.colors.mode = GradientMode.Fixed;

		asset.life = new ParticleLife();
		asset.life.liveForever = true;
		asset.life.minLifeTime = 100;
		asset.life.maxLifeTime = 200;
		asset.life.deathColor = Color.white;
		asset.life.deathColorCurve = new AnimationCurve(new Keyframe[2] {
			new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0)
		});
		asset.life.createOnDeath = null;
		asset.life.chance = 0.5f;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 0;
		asset.corrosion.strength = 0;
		asset.corrosion.chance = 0;

		asset.moveChecks = new ParticleMoveChecks[0];

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 0;
		asset.spread.strength = 1;
		asset.spread.chance = 0.083f;
		asset.spread.canSpreadToAir = true;
		asset.spread.freshForFrames = 100;
		asset.spread.freshSpreadColor = new Color(1f, 0, 0.92f, 1f);
		asset.spread.spreadChecks = 4;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.EightRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		path += "/Corruption.asset";

		ProjectWindowUtil.CreateAsset(asset, path);
		Debug.Log("Set the 'spread.spreadOnce' value to false for a cool oscilating reaction.\n" +
			"You can also make the particle not spread to air.");
	}

	[MenuItem("Assets/Create/Particles/Defaults/Fire")]
	static void CreateFireParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 0;
		asset.dispersionSpeedMin = 1;
		asset.dispersionSpeedMax = 1;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[3]
		{
			new GradientColorKey(new Color(1f, 0.87f, 0.211f), 0.4f),
			new GradientColorKey(new Color(1f, 0.585f, 0.211f), 0.5f),
			new GradientColorKey(new Color(1f, 0.721f, 0.211f), 0.6f)
		};

		asset.life = new ParticleLife();
		asset.life.liveForever = false;
		asset.life.minLifeTime = 25;
		asset.life.maxLifeTime = 50;
		asset.life.deathColor = new Color(1f, 0.967f, 0.55f);
		asset.life.deathColorCurve = new AnimationCurve(new Keyframe[2] {
			new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0)
		});

		asset.life.chance = 0.088f;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 1f;
		asset.corrosion.strength = 0;
		asset.corrosion.chance = 0;

		asset.moveChecks = new ParticleMoveChecks[2]
		{
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.Up, 0.18f, true),
			new ParticleMoveChecks(ParticleMoveChecks.MoveDirection.EightRandom, 0.611f)
		};

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 1f;
		asset.spread.strength = 0.59f;
		asset.spread.chance = 1f;
		asset.spread.canSpreadToAir = false;
		asset.spread.freshForFrames = 10;
		asset.spread.freshSpreadColor = new Color(1f, 0.05f, 0.05f, 1f);
		asset.spread.spreadChecks = 4;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.XRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (AssetDatabase.FindAssets("Smoke t:ParticleObject", new string[1] { path }).Length == 0)
		{
			asset.life.createOnDeath = CreateSmokeParticle();
			Debug.Log("A smoke particle object was created and added for you to the 'life.createOnDeath' " +
				"variable of the fire particle object");
		}
		else
		{
			asset.life.createOnDeath = null;
			Debug.Log("You can add the 'Smoke' particle object to the 'life.createOnDeath' variable for a smoke effect.");
		}
		path += "/Fire.asset";


		ProjectWindowUtil.CreateAsset(asset, path);
	}

	[MenuItem("Assets/Create/Particles/Defaults/Air")]
	static void CreateAirParticle()
	{
		ParticleObject asset = ScriptableObject.CreateInstance<ParticleObject>();

		asset.type = 1;

		asset.waitFrames = 0;
		asset.dispersionSpeedMin = 0;
		asset.dispersionSpeedMax = 0;

		asset.colors = new Gradient();
		asset.colors.colorKeys = new GradientColorKey[1]
		{
			new GradientColorKey(new Color(0, 0, 0, 0), 0)
		};

		asset.life = new ParticleLife();
		asset.life.liveForever = true;
		asset.life.minLifeTime = 0;
		asset.life.maxLifeTime = 0;
		asset.life.deathColor = Color.clear;
		asset.life.deathColorCurve = new AnimationCurve(new Keyframe[2] {
			new Keyframe(0, 0, 0, 2f), new Keyframe(1f, 1f, 0, 0)
		});
		asset.life.createOnDeath = null;
		asset.life.chance = 0;

		asset.corrosion = new ParticleCorrosion();
		asset.corrosion.resistance = 0;
		asset.corrosion.strength = 0;
		asset.corrosion.chance = 0;

		asset.moveChecks = new ParticleMoveChecks[0];

		asset.spread = new ParticleSpread();
		asset.spread.resistance = 0;
		asset.spread.strength = 0;
		asset.spread.chance = 0;
		asset.spread.canSpreadToAir = false;
		asset.spread.freshForFrames = 0;
		asset.spread.freshSpreadColor = Color.clear;
		asset.spread.spreadChecks = 0;
		asset.spread.spreadDirection = ParticleSpread.SpreadDirection.EightRandom;
		asset.spread.spreadOnce = true;
		asset.spread.freshColorCurve =
			new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0) });


		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		path += "/Air.asset";

		ProjectWindowUtil.CreateAsset(asset, path);
		Debug.Log("This is a empty air particle object. Place in the Air Particle Object variable of the Particle Manager.");
	}
}