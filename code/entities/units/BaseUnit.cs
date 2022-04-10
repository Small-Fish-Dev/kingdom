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
public partial class BaseUnit : Entity
{

	public virtual string LibraryName => "Unit.Base"; // TODO: Find a way to get it from the type
	public virtual string UnitName => "Base";
	public virtual string UnitAlignment => "Base";
	public virtual int MaxHP => 1;
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
	public virtual string Outfit => "";

	public virtual float AnimationFrames => 1f / 30f;        // Full animation frames  ( 1 / { fps } )
	public virtual float MinimumFrames => 1 / 1f;            // Frames at max distance ( 1 / { fps } )
	public virtual float StartingDistance => 400f;    // Minimum distance before the frames start to drop
	public virtual float EndingDistance => 3000f;   // Distance at which the frames reach {minFps}

	public virtual Dictionary<UnitState, string> UnitAnimations => new Dictionary<UnitState, string>()
	{

		[ UnitState.Idle ] = "IdleLayer_01", // Idle
		[ UnitState.Walk ] = "Walk_N", // Walk
		[ UnitState.Attack ] = "Melee_Punch_Attack_Right", // Attack

	};

	public virtual float AttackKeyframe => 0.2f; // At which point of the attack animation damage is dealt ( Seconds )


	int hp { get; set; }
	public int HitPoints {
		get 
		{
			return hp;
		}
		set
		{
			hp = value;
			if ( hp <= 0 )
			{

				Kill();

			}

		}
	}
	[Net] public UnitState State { get; set; } = UnitState.Walk;
	public Waypoint OldWaypoint { get; set; }
	public int CurrentWaypointID { get; set; } = 0;
	public Lane CurrentLane { get; set; }
	public int CurrentLaneID { get; set; } = 0;
	public Waypoint CurrentWaypoint { get; set; }
	[Net] public Vector3 OldPosition { get; set; }
	public Client Commander { get; set; }
	[Net] BaseUnit Target { get; set; }
	public bool IsFirst = false;
	public BaseFort OriginalFort { get; set; }
	public Path OriginalPath { get; set; }
	public bool IsBackwards { get; set; }
	[Net] public bool IsSetup { get; set; } = false;
	[Net] Rotation wishAngle { get; set; } = new Rotation();
	AnimEntity clientModel { get; set; }

	public void SetClothing( string modelPath )
	{

		if ( modelPath != "none" )
		{

			var entity = new ModelEntity();

			entity.SetModel( modelPath );
			entity.SetParent( this.clientModel, true );
			entity.EnableShadowCasting = false;

		}

	}

	public void SetupUnit( BaseFort originalFort, Path originalPath, Lane originalLane )
	{

		hp = MaxHP;
		OriginalFort = originalFort;
		Commander = OriginalFort.Holder;
		OriginalPath = originalPath;
		CurrentLane = originalLane;
		CurrentLaneID = Array.IndexOf( OriginalPath.Lanes, CurrentLane);
		IsBackwards = CurrentLane.OriginPath.FortTo == OriginalFort;
		CurrentWaypoint = IsBackwards ? CurrentLane.Waypoints[CurrentLane.Waypoints.Length - 2] : CurrentLane.Waypoints[1];
		OldWaypoint = IsBackwards ? CurrentLane.Waypoints[CurrentLane.Waypoints.Length - 1] : CurrentLane.Waypoints[0];
		CurrentWaypointID = Array.IndexOf( CurrentLane.Waypoints, CurrentWaypoint);
		CurrentWaypoint.Status = WaypointStatus.Taken;
		CurrentWaypoint.Unit = this;
		CurrentLane.UnitLaneMap[OriginalFort][CurrentWaypointID] = this;

		IsSetup = true;

		Position = CurrentWaypoint.Position;
		Rotation = Rotation.LookAt( (CurrentWaypoint.Position - OldWaypoint.Position).Normal, Vector3.Up );
		wishAngle = Rotation.LookAt( (CurrentWaypoint.Position - OldWaypoint.Position).Normal, Vector3.Up );

	}

	public override void Spawn()
	{

		base.Spawn();

		Transmit = TransmitType.Always;
		Tags.Add( "Unit", $"{UnitName}", $"{UnitAlignment}" );

	}

	public override void ClientSpawn()
	{

		base.ClientSpawn();

		clientModel = new AnimEntity();
		clientModel.SetModel( UnitModel );
		clientModel.Position = Position;
		clientModel.Rotation = Rotation;
		clientModel.EnableDrawing = true;
		clientModel.EnableShadowCasting = false;
		clientModel.UseAnimGraph = false;
		clientModel.PlaybackRate = 0f;
		clientModel.Scale = ModelScale;

		SetClothing( Outfit );

	}

	public Waypoint FindWaypoint( int forward, int right )
	{

		int moveDirection = IsBackwards ? -1 : 1;
		int waypointID = CurrentWaypointID + forward * moveDirection;
		int laneID = CurrentLaneID + right * moveDirection;

		if ( Waypoint.IsValidWaypoint( this, waypointID, laneID ) )
		{

			return OriginalPath.Lanes[laneID].Waypoints[waypointID];

		}

		return CurrentWaypoint;

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
		if ( !IsValid ) { return; }

	//	Log.Info( $"[{Time.Now}] {this} MOVING FROM [{CurrentLaneID},{CurrentWaypointID}] ({CurrentWaypoint.Status}-{CurrentWaypoint.Unit})" );
		//Log.Info( $"[{Time.Now}] {this} TOWARDS [{Array.IndexOf( OriginalPath.Lanes, destination.Lane )},{Array.IndexOf( destination.Lane.Waypoints, destination )}] ({destination.Status}-{destination.Unit})" );

		CurrentWaypoint.Unit = null;
		CurrentWaypoint.Status = WaypointStatus.Free;
		CurrentLane.UnitLaneMap[OriginalFort][CurrentWaypointID] = null;

		//Log.Info( $"[{Time.Now}] {this} UPDATED [{CurrentLaneID},{CurrentWaypointID}] ({CurrentWaypoint.Status}-{CurrentWaypoint.Unit})" );


		CurrentWaypoint = destination;
		CurrentLane = CurrentWaypoint.Lane;
		CurrentWaypointID = Array.IndexOf( CurrentLane.Waypoints, CurrentWaypoint );
		CurrentLaneID = Array.IndexOf( OriginalPath.Lanes, CurrentLane );

		CurrentLane.UnitLaneMap[OriginalFort][CurrentWaypointID] = this;

		//Log.Info( $"[{Time.Now}] {this} CURRENTLY [{CurrentLaneID},{CurrentWaypointID}] IS ({CurrentWaypoint.Status}-{CurrentWaypoint.Unit})" );

		CurrentWaypoint.Unit = this;
		CurrentWaypoint.Status = WaypointStatus.Taken;

		//Log.Info( $"[{Time.Now}] {this} [{CurrentLaneID},{CurrentWaypointID}] UPDATED TO ({CurrentWaypoint.Status}-{CurrentWaypoint.Unit})" );

		wishAngle = Rotation.LookAt( (CurrentWaypoint.Position - OldWaypoint.Position).Normal, Vector3.Up );

		State = UnitState.Walk;

	//	Log.Info( $"[{Time.Now}] {this} MOVED TO WAYPOINT [{CurrentLaneID},{CurrentWaypointID}] ({CurrentWaypoint.Status}-{CurrentWaypoint.Unit})" );

	}

	public Waypoint SearchBestWaypoint()
	{

		for ( int i = 1; i < 4; i++ )
		{

			//First try walking forward, then right, then left
			Waypoint targetWaypoint = FindWaypoint( 1, i % 3 - 1 );

			if ( targetWaypoint != CurrentWaypoint )
			{

				if ( CanMoveTo( targetWaypoint ) )
				{

					return targetWaypoint;

				}

			}

		}

		return CurrentWaypoint;

	}

	public Waypoint MoveTowards( Waypoint destination ) // TODO: DEBUG!
	{

		var destLane = destination.Lane;
		var destPath = destLane.OriginPath;
		Vector2 destPosition = new Vector2( Array.IndexOf( destLane.Waypoints, destination ), Array.IndexOf( destPath.Lanes, destLane ) );

		var bestOption = CurrentWaypoint;
		float bestDistance = Vector2.DistanceBetween( destPosition, new Vector2( CurrentWaypointID, CurrentLaneID ) );

		for ( int forward = -1; forward <= 1; forward++ )
		{

			for ( int right = -1; right <= 1; right++ )
			{

				Vector2 currentOption = new Vector2( CurrentWaypointID + forward * ( IsBackwards ? -1 : 1 ), CurrentLaneID + right * (IsBackwards ? -1 : 1) );
				float distance = Vector2.DistanceBetween( destPosition, currentOption );

				var localDestination = FindWaypoint( forward, right );

				if ( CanMoveTo( localDestination ) )
				{


					if ( distance < bestDistance )
					{

						bestOption = localDestination;
						bestDistance = distance;

					}

				}

			}

		}

		return bestOption;

	}

	public virtual bool ComputeWalk()
	{

		if ( Target != null )
		{

			var destination = MoveTowards( Target.CurrentWaypoint );

			if ( destination != CurrentWaypoint )
			{

				MoveTo( destination );
				return true;

			}
			else
			{

				State = UnitState.Idle;

			}

		}
		else
		{

			Waypoint targetWaypoint = SearchBestWaypoint();

			if ( targetWaypoint != CurrentWaypoint )
			{

				MoveTo( targetWaypoint );
				return true;

			}
			else
			{

				State = UnitState.Idle;

			}

		}

		

		return false;

	}

	public virtual bool FindEnemy()
	{

		for ( int y = 0; y <= AttackRange; y++ )
		{

			int[] searchPattern = Kingdom.SpiralPattern1D( CurrentLaneID, OriginalPath.TotalLanes, IsBackwards );

			for ( int x = 0; x < OriginalPath.TotalLanes; x++ )
			{

				int depthCheck = (y + 1) % (AttackRange + 1); // Check the same row last
				Waypoint waypointCheck = FindWaypoint( depthCheck, searchPattern[x] );
				BaseUnit unitFound = waypointCheck.Unit;

				if ( unitFound != null && unitFound != this )
				{

					if ( unitFound.IsBackwards != IsBackwards )//( unitFound.Commander != Commander )
					{

						Target = unitFound;
						return true;

					}

				}

			}

		}

		return false;

	}

	public bool IsInRange( BaseUnit target )
	{

		int waypointPos = target.CurrentWaypointID;
		int lanePos = target.CurrentLaneID;

		if ( waypointPos <= CurrentWaypointID + AttackRange && waypointPos >= CurrentWaypointID - AttackRange )
		{

			if ( lanePos <= CurrentLaneID + AttackRange && lanePos >= CurrentLaneID - AttackRange )
			{

				return true;

			}

		}

		return false;

	}

	public bool IsNearToFort()
	{

		if ( CurrentWaypointID == (IsBackwards ? 0 : CurrentLane.Waypoints.Length - 1) )
		{

			return true;

		}

		return false;

	}

	public virtual void ComputeInvasion()
	{

		if ( IsNearToFort() )
		{

			Kill( true );

			if ( IsBackwards )
			{

				OriginalPath.FortFrom.AddUnits( LibraryName, 1 );

			}
			else
			{

				OriginalPath.FortTo.AddUnits( LibraryName, 1 );

			}

		}

	}

	[Event("Kingdom_Turn_Units")]
	public virtual void HandleTurns()
	{

		if ( !IsSetup ) { return; }
		if ( !IsValid ) { return; }

		if ( IsFirst )
		{

			

		}

		OldWaypoint = CurrentWaypoint;
		ComputeInvasion();
		
		switch ( State )
		{

			case UnitState.Walk:
				{

					ComputeWalk();

					if ( Target != null )
					{

						if ( IsInRange( Target ) )
						{

							wishAngle = Rotation.LookAt( Target.Position - Position, Vector3.Up );
							State = UnitState.Attack;
							break;

						}

					}
					else
					{

						FindEnemy();

					}

					break;

				}

			case UnitState.Attack:
				{


					if ( Target != null && IsInRange( Target ) )
					{

						wishAngle = Rotation.LookAt( Target.Position - Position, Vector3.Up );
						Target.HitPoints -= AttackStrength;
						FindEnemy(); // In case it killed the last one

					}
					else
					{

						ComputeWalk();

					}

					break;

				}

			case UnitState.Idle:
				{

					ComputeWalk();

					if ( Target != null )
					{

						if ( IsInRange( Target ) )
						{

							wishAngle = Rotation.LookAt( Target.Position - Position, Vector3.Up );
							State = UnitState.Attack;
							break;

						}

					}
					else
					{

						FindEnemy();

					}

					break;

				}

			default: break;

		}

	}

	Vector3 randomOffset = new Vector3( Rand.Float( -0f, 0f ), Rand.Float( -0f, 0f ), 0 ); //TODO put back -4 4
	[Event.Tick]
	public void HandleMovement()
	{

		if ( !IsSetup ) { return; }
		if ( !IsValid ) { return; }
		if ( IsServer )
		{

			OldPosition = OldWaypoint.Position;
			Position = CurrentWaypoint.Position;

			return;

		}
		if ( !clientModel.IsValid ) { return; }


		clientModel.Position = Vector3.Lerp( OldPosition, Position, Kingdom.LastTurn / Kingdom.TurnDuration ) + randomOffset;

		if ( IsFirst )
		{

			//DebugOverlay.Sphere( Position, 5f, Color.Green );

		}

		clientModel.Rotation = Rotation.Lerp( clientModel.Rotation, wishAngle, Time.Delta * 5f / Kingdom.TurnDuration );

	}

	TimeSince frameTime = 0f;
	float lastDistance = 0f;
	float nextFrame = 0f;
	bool hasAttacked = false;

	[Event.Tick]
	public void HandleAnimations()
	{

		if ( !IsSetup ) { return; }
		if ( !IsValid ) { return; }
		if ( IsServer ) { return; }

		if ( !clientModel.IsValid ) { return; }

		if ( frameTime >= nextFrame )
		{

			clientModel.CurrentSequence.Name = UnitAnimations[ State ];

			clientModel.CurrentSequence.Time = (clientModel.CurrentSequence.Time + frameTime / Kingdom.TurnDuration ) % clientModel.CurrentSequence.Duration;
			lastDistance = Math.Max( CurrentView.Position.Distance( Position ) - StartingDistance, 1f );
			nextFrame = MathX.LerpTo( AnimationFrames, MinimumFrames, lastDistance / EndingDistance );

			if ( clientModel.CurrentSequence.Time >= AttackKeyframe )
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

		if ( !IsSetup ) { return; }
		if ( !IsValid ) { return; }

		CurrentLane.UnitLaneMap[OriginalFort][CurrentWaypointID] = null;
		CurrentWaypoint.Status = WaypointStatus.Free;
		CurrentWaypoint.Unit = null;

		if ( !silent )
		{

			CreateClientParticle( "particles/dead_citizen.vpcf" );

		}

		DeleteClientModel();
		Delete();

	}

	[ClientRpc]
	public void DeleteClientModel()
	{

		clientModel.Delete();

	}

	[ClientRpc]
	public void CreateClientParticle( string name )
	{

		Particles.Create( name, clientModel.Position );

	}

}
