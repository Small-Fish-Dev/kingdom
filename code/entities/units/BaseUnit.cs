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
	public virtual int AttackSpeed => 1; // How many turns to attack
	public virtual int AttackRange => 1; // How far they can reach ( In waypoints )
	public virtual bool AttackAoE => false; // Area attacks
	public virtual int AttackRadius => 0; // Useless if AttackAoE isn't true, 1 is the spot in front
	/*	      2 is 4 spots	          3 is 9 spots           4 is 16 spots
	 *	 	[ ][ ][ ][ ][ ]			[ ][ ][ ][ ][ ]			[ ][ ][ ][ ][ ]
	 *		[ ][ ][ ][ ][ ]			[ ][ ][ ][ ][ ]			[ ][X][X][X][ ]
	 *		[ ][ ][ ][ ][ ]			[ ][ ][X][ ][ ]			[ ][X][X][X][ ]
	 *		[ ][ ][X][ ][ ]			[ ][X][X][X][ ]			[X][X][X][X][X]
	 *		[ ][X][X][X][ ]			[X][X][X][X][X]			[X][X][X][X][X]
	 */
	public virtual string UnitModel => "models/kingdom_citizen/kingdom_citizen.vmdl";

	public virtual float AnimationFrames => 1f / 24f;        // Full animation frames  ( 1 / { fps } )
	public virtual float MinimumFrames => 1 / 0.5f;            // Frames at max distance ( 1 / { fps } )
	public virtual float StartingDistance => 200f;    // Minimum distance before the frames start to drop
	public virtual float EndingDistance => 1500f;   // Distance at which the frames reach {minFps}

	public virtual Dictionary<UnitState, string> UnitAnimations => new Dictionary<UnitState, string>()
	{

		[ UnitState.Idle ] = "IdleLayer_01", // Idle
		[ UnitState.Walk ] = "Walk_N", // Walk
		[ UnitState.Attack ] = "Melee_Punch_Attack_Right", // Attack

	};

	public virtual float AttackKeyframe => 0.2f; // At which point of the attack animation damage is dealt ( Seconds )


	[Net] public UnitState State { get; set; } = UnitState.Idle;
	[Net] public Waypoint OldWaypoint { get; set; }
	[Net] public int CurrentWaypointID { get; set; } = 0;
	[Net] public Lane CurrentLane { get; set; }
	[Net] public int CurrentLaneID { get; set; } = 0;
	[Net] public Waypoint CurrentWaypoint { get; set; }
	[Net] public Client Commander { get; set; }
	[Net] public BaseFort OriginalFort { get; set; }
	[Net] public Path OriginalPath { get; set; }
	public bool IsBackwards { get; set; }

	public void SetupUnit( BaseFort originalFort, Path originalPath, int laneID )
	{

		OriginalFort = originalFort;
		CurrentLaneID = laneID;
		OriginalPath = originalPath;
		CurrentLane = OriginalPath.Lanes[CurrentLaneID];
		Commander = OriginalFort.Holder;
		IsBackwards = CurrentLane.OriginPath.FortFrom == OriginalFort;
		CurrentWaypointID = IsBackwards ? 0 : CurrentLane.Waypoints.Count - 1;
		CurrentWaypoint = CurrentLane.Waypoints[ CurrentWaypointID ];
		OldWaypoint = CurrentWaypoint;

	}

	public override void Spawn()
	{

		base.Spawn();

		SetModel( UnitModel );

		Rotation = Rotation.LookAt( IsBackwards ? CurrentLane.OriginPath.FortFrom.Position - CurrentLane.OriginPath.FortTo.Position : CurrentLane.OriginPath.FortTo.Position - CurrentLane.OriginPath.FortFrom.Position );

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

	public bool CanMoveTo( Waypoint destination )
	{

		if ( destination.Status != WaypointStatus.Taken )
		{

			return true;

		}

		return false;

	}

	public void MoveTo( Waypoint destination )
	{

		if ( CanMoveTo( destination ) )
		{

			CurrentWaypoint.Status = WaypointStatus.Free;
			destination.Status = WaypointStatus.Taken;

			OldWaypoint = CurrentWaypoint;
			CurrentWaypoint = destination;

			State = UnitState.Walk;

		}

	}

	[Event("Kingdom_Next_Turn")]
	public void HandleTurnsMovement()
	{

		// Don't walk if it's attacking
		if ( State == UnitState.Attack ) { return; }

		int moveDirection = IsBackwards ? -1 : 1;

		// Try moving forward
		var targetWaypoint = CurrentLane.Waypoints[CurrentWaypointID + moveDirection];

		if ( CanMoveTo( targetWaypoint ) )
		{

			MoveTo( targetWaypoint );
			return;

		}

		// Try moving right
		if ( OriginalPath.Lanes[CurrentLaneID + 1] != null )
		{

			targetWaypoint = OriginalPath.Lanes[CurrentLaneID + 1].Waypoints[CurrentWaypointID + moveDirection];

			if ( CanMoveTo( targetWaypoint ) )
			{

				MoveTo( targetWaypoint );
				return;

			}

		}

		// Try moving left
		if ( OriginalPath.Lanes[CurrentLaneID - 1] != null )
		{

			targetWaypoint = OriginalPath.Lanes[CurrentLaneID - 1].Waypoints[CurrentWaypointID + moveDirection];

			if ( CanMoveTo( targetWaypoint ) )
			{

				MoveTo( targetWaypoint );
				return;

			}

		}

		State = UnitState.Idle;

	}

	[Event.Tick]
	public void HandleMovement()
	{

		Position = Vector3.Lerp( OldWaypoint.Position, CurrentWaypoint.Position, 10f * Time.Delta / Kingdom.TurnDuration );

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

}
