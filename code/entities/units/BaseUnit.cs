using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public enum UnitState
{

	Idle,
	Walk,
	Attack

}

public partial class BaseUnit : AnimEntity
{

	public virtual string UnitName => "Base";
	public virtual string UnitAlignment => "Base";
	public virtual int HitPoints => 1;
	public virtual float ModelScale => 0.3f;
	public virtual int UnitWidth => 1; // How many lanes are taken up
	public virtual int AttackStrength => 1; // How many hitpoints they deal
	public virtual float AttackSpeed => 2f; // Seconds
	public virtual float AttackRange => 15f; // How far they can reach
	public virtual bool AttackAoE => false; // Area attacks
	public virtual float AttackRadius => 0f; // Useless if AttackAoE isn't true
	public virtual string UnitModel => "models/kingdom_citizen/kingdom_citizen.vmdl";

	public virtual float AnimationFrames => 1f / 30f;        // Full animation frames  ( 1 / { fps } )
	public virtual float MinimumFrames => 1 / 0.5f;            // Frames at max distance ( 1 / { fps } )
	public virtual float StartingDistance => 200f;    // Minimum distance before the frames start to drop
	public virtual float EndingDistance => 1500f;   // Distance at which the frames reach {minFps}

	public virtual Dictionary<UnitState, string> UnitAnimations => new Dictionary<UnitState, string>()
	{

		[ UnitState.Idle ] = "Melee_Punch_Idle_Standing_01", // Idle
		[ UnitState.Walk ] = "Walk_N", // Walk
		[ UnitState.Attack ] = "Melee_Punch_Attack_Right", // Attack

	};

	public virtual float AttackKeyframe => 0.2f; // At which point of the attack animation damage is dealt ( Seconds )


	public virtual UnitState State => UnitState.Idle;

	public override void Spawn()
	{

		base.Spawn();

		SetModel( UnitModel );

		Tags.Add( "Unit", $"{UnitName}", $"{UnitAlignment}" );

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
	bool hasAttacked = false;

	[Event.Tick.Client]
	public void HandleAnimations()
	{

		if ( frameTime >= nextFrame )
		{

			CurrentSequence.Name = UnitAnimations[ State ];

			CurrentSequence.Time = ( CurrentSequence.Time + frameTime ) % CurrentSequence.Duration;
			lastDistance = Math.Max( CurrentView.Position.Distance( Position ) - StartingDistance, 1f );
			nextFrame = MathX.LerpTo( AnimationFrames, MinimumFrames, lastDistance / EndingDistance );

			if ( CurrentSequence.Time >= AttackKeyframe )
			{
				
				if ( hasAttacked == false )
				{

					DebugOverlay.Sphere( Position + Rotation.Forward * 5f, 5f, Color.Red, true, 0.3f );
					hasAttacked = true;

				}

			}
			else
			{

				hasAttacked = false;

			}

			frameTime = 0f;

		}

	}

	[Event.Tick]
	public void HandleMovement()
	{

		if ( State == UnitState.Walk )
		{

			Position = Position + Vector3.Forward * 0.4f;

		}

	}

}
