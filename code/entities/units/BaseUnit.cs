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

[Library( "Unit.Base" )]
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


	[Net] public UnitState State { get; set; } = UnitState.Walk;
	public Waypoint OldWaypoint { get; set; }
	[Net] public int CurrentWaypointID { get; set; } = 0;
	public Lane CurrentLane { get; set; }
	[Net] public int CurrentLaneID { get; set; } = 0;
	public Waypoint CurrentWaypoint { get; set; }
	public Client Commander { get; set; }
	[Net] BaseUnit Target { get; set; }
	public bool IsFirst = false;
	public BaseFort OriginalFort { get; set; }
	public Path OriginalPath { get; set; }
	public bool IsBackwards { get; set; }
	public bool IsSetup { get; set; } = false;

	public void SetupUnit( BaseFort originalFort, Path originalPath, Lane originalLane )
	{

		OriginalFort = originalFort;
		Commander = OriginalFort.Holder;
		OriginalPath = originalPath;
		CurrentLane = originalLane;
		CurrentLaneID = Array.IndexOf( OriginalPath.Lanes, CurrentLane);
		IsBackwards = CurrentLane.OriginPath.FortTo == OriginalFort;
		CurrentWaypoint = IsBackwards ? CurrentLane.Waypoints.Last<Waypoint>() : CurrentLane.Waypoints[0];
		OldWaypoint = CurrentWaypoint;
		CurrentWaypointID = Array.IndexOf( CurrentLane.Waypoints, CurrentWaypoint);
		OldWaypoint.Status = WaypointStatus.Taken;
		CurrentWaypoint.Status = WaypointStatus.Taken;
		CurrentWaypoint.Unit = this;
		CurrentLane.UnitLaneMap[OriginalFort][CurrentWaypointID] = this;

		IsSetup = true;

		Position = CurrentWaypoint.Position;
		Rotation = Rotation.LookAt( IsBackwards ? CurrentLane.OriginPath.FortFrom.Position - CurrentLane.OriginPath.FortTo.Position : CurrentLane.OriginPath.FortTo.Position - CurrentLane.OriginPath.FortFrom.Position );

	}

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

	public bool IsValidWaypoint( int waypointID, int laneID )
	{

		if ( laneID >= 0 && laneID < OriginalPath.Lanes.Count() )
		{

			if ( waypointID >= 0 && waypointID < CurrentLane.Waypoints.Count() )
			{

				return true;

			}

		}

		return false;

	}

	public Waypoint FindWaypoint( int forward, int right )
	{

		Waypoint targetWaypoint = CurrentWaypoint;

		int moveDirection = IsBackwards ? -1 : 1;
		int waypointID = CurrentWaypointID + forward * moveDirection;
		int laneID = CurrentLaneID + right * moveDirection;

		if ( IsValidWaypoint( waypointID, laneID ) )
		{

			targetWaypoint = OriginalPath.Lanes[laneID].Waypoints[waypointID];

		}

		return targetWaypoint;

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

		if ( !IsSetup ) { return; }

		CurrentWaypoint.Status = WaypointStatus.Free;
		destination.Status = WaypointStatus.Taken;

		CurrentLane.UnitLaneMap[OriginalFort][CurrentWaypointID] = null;

		OldWaypoint = CurrentWaypoint;

		CurrentLane = destination.Lane;
		CurrentLaneID = Array.IndexOf( OriginalPath.Lanes, CurrentLane );
		CurrentWaypoint = destination;
		CurrentWaypointID = Array.IndexOf( CurrentLane.Waypoints, CurrentWaypoint );

		OldWaypoint.Unit = null;
		CurrentWaypoint.Unit = this;

		CurrentLane.UnitLaneMap[OriginalFort][CurrentWaypointID] = this;

		Rotation = Rotation.LookAt( (CurrentWaypoint.Position - OldWaypoint.Position).Normal, Vector3.Up );

		State = UnitState.Walk;

	}

	[Event("Kingdom_Next_Turn")]
	public void HandleTurns()
	{

		if ( !IsSetup ) { return; }

		if ( IsFirst )
		{

			for ( int y = 0; y < AttackRange; y++ )
			{

				for ( int x = 0; x < OriginalPath.TotalLanes; x++ )
				{

					Waypoint waypointCheck = FindWaypoint( y, x );
					BaseUnit unitFound = waypointCheck.Unit;

					if ( unitFound != null && unitFound.IsBackwards != IsBackwards && unitFound.Commander != Commander )
					{

						if ( unitFound.Target == null )
						{
							//TODO FINISH THIS
							//unitFound.Target = this;
							Target = unitFound;
							Target.Delete();
							Target = null;

						}

					}

				}

			}

		}

		if ( OldWaypoint == CurrentWaypoint && CurrentWaypointID == ( IsBackwards ? 0 : CurrentLane.Waypoints.Length - 1 ) )
		{

			Kill();

		}
		
		switch ( State )
		{

			case UnitState.Walk:
				{

					for ( int i = 1; i < 4; i++ )
					{

						//First try walking forward, then right, then left
						Waypoint targetWaypoint = FindWaypoint( 1, i % 3 - 1 );
						
						if ( targetWaypoint != CurrentWaypoint )
						{

							if ( CanMoveTo( targetWaypoint ) )
							{

								MoveTo( targetWaypoint );
								return;

							}

						}

					}

					State = UnitState.Idle;
					OldWaypoint = CurrentWaypoint;

					break;
				}

			case UnitState.Attack:
				{



					break;

				}

			case UnitState.Idle:
				{

					break;

				}

			default: break;

		}

	}

	Vector3 randomOffset = new Vector3( Rand.Float( -4f, 4f ), Rand.Float( -4f, 4f ), 0 );
	[Event.Tick]
	public void HandleMovement()
	{

		if ( !IsSetup ) { return; }

		if ( State == UnitState.Walk )
		{

			Position = Vector3.Lerp( OldWaypoint.Position, CurrentWaypoint.Position, Kingdom.LastTurn / Kingdom.TurnDuration ) + randomOffset;

		}

		if ( IsFirst )
		{

			DebugOverlay.Sphere( Position, 5f, Color.Green );

		}

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

			CurrentSequence.Time = ( CurrentSequence.Time + frameTime / Kingdom.TurnDuration ) % CurrentSequence.Duration;
			lastDistance = Math.Max( CurrentView.Position.Distance( Position ) - StartingDistance, 1f );
			nextFrame = MathX.LerpTo( AnimationFrames, MinimumFrames, lastDistance / EndingDistance );

			if ( CurrentSequence.Time >= AttackKeyframe )
			{
				
				if ( hasAttacked == false )
				{

					//DebugOverlay.Sphere( Position + Rotation.Forward * 5f, 5f, Color.Red, true, 0.3f );
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

	public void Kill( bool silent = false )
	{

		CurrentWaypoint.Status = WaypointStatus.Free;
		OldWaypoint.Status = WaypointStatus.Free;

		if ( !silent )
		{

			Particles.Create( "particles/dead_citizen.vpcf", this.Position );

		}

		Delete();

	}

}
