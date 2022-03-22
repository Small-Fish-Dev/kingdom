using Sandbox;
using System;
using System.Linq;

partial class BaseUnit : AnimEntity
{


	public virtual string UnitName => "Base";
	public virtual string UnitType => "Base";
	public virtual int HitPoints => 1;
	public virtual float ModelScale => 0.3f;
	// TODO Add animation stuff
	public virtual float UnitRadius => 7f;
	public virtual int AttackStrength => 1;
	public virtual float AttackSpeed => 2f; // Seconds
	public virtual float AttackRange => 15f;
	public virtual bool AttackAoE => false;
	public virtual float AttackRadius => 0f; // Useless if AttackAoE isn't true
	public virtual string UnitModel => "models/kingdom_citizen/kingdom_citizen.vmdl";


	public enum CurrentState
	{

		Idling,
		Walking,
		Attacking,
		Dying

	}

	public override void Spawn()
	{

		base.Spawn();

		SetModel( UnitModel );

		Tags.Add( "Unit", $"{UnitName}", $"{UnitType}" );

		EnableDrawing = true;
		EnableShadowCasting = false;
		UseAnimGraph = false;
		PlaybackRate = 0f;
		Scale = ModelScale;

	}

	public override void ClientSpawn()
	{

		base.ClientSpawn();


	}

	TimeSince frameTime = 0f;
	float lastDistance = 0f;
	float nextFrame = 0f;

	[Event.Tick.Client]
	public void HandleAnimations()
	{

		CurrentSequence.Name = "Walk_N";
		float frameDelta = 1f / 30f;		// Full animation frames  ( 1 / { fps } )
		float minFps = 1 / 0.5f;			// Frames at max distance ( 1 / { fps } )
		float minDistanceFalloff = 200f;	// Minimum distance before the frames start to drop
		float maxDistanceFalloff = 1500f;	// Distance at which the frames reach {minFps}

		if ( frameTime >= nextFrame )
		{

			CurrentSequence.Time = ( CurrentSequence.Time + frameTime ) % CurrentSequence.Duration;
			lastDistance = Math.Max( CurrentView.Position.Distance( Position ) - minDistanceFalloff, 1f );
			nextFrame = MathX.LerpTo( frameDelta, minFps, lastDistance / maxDistanceFalloff );

			frameTime = 0f;

		}

	}

}
