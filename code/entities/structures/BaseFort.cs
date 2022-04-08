using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;


[Library( "Structure.Base.BaseFort" )]
public partial class BaseFort : BaseStructure
{

	public override string StructureName => "Base Fort";
	public override string StructureAlignment => "Base";
	public override float ModelScale => 1f;
	public override string StructureModel => "models/structures/base_fort.vmdl";
	public virtual float EntranceDistance => 45f; // The lanes won't begin at the center
	public virtual float UnitsPerSecond => 15f; // How many units are generated each second inside of this fort
	public virtual string UnitsType => "Unit.Human.Peasant"; // Which units it generates
	public virtual int StartingUnits => 40; // How many units are inside the castle that you need to defeat before capturing
	public virtual float GoldPerSecond => 1f; // How much gold it generates each second

	public override StructureType Type => StructureType.Outpost;

	[Net] public Client Holder { get; set; } = null; // Who's holding the fort
	[Net] public Dictionary<string, int> StoredUnits { get; set; } = new Dictionary<string, int>();
	[Net] public Dictionary<BaseFort, Path> AvailablePaths { get; set; } = new Dictionary<BaseFort, Path>();

	public BaseFort()
	{

		if ( StartingUnits > 0 )
		{

			AddUnits( UnitsType, StartingUnits );

		}

	}

	public void AddUnits( string unitType, int unitAmount )
	{

		if ( Library.GetType( unitType ).IsSubclassOf( typeof( BaseUnit ) ) )
		{

			if ( StoredUnits.ContainsKey( unitType ) )
			{

				StoredUnits[ unitType ] += unitAmount;
				return;

			}

			StoredUnits.Add( unitType, unitAmount );

		}

	}

	TimeSince lastUnitGenerated = 0;
	TimeSince lastGoldGenerated = 0;

	[Event.Tick]
	public void GenerateTimers()
	{

		if ( Holder.IsValid() )
		{

			if ( lastUnitGenerated >= 1 / UnitsPerSecond )
			{

				AddUnits( UnitsType, 1 );
				lastUnitGenerated = 0;

			}

			if ( lastGoldGenerated >= 1 / GoldPerSecond )
			{

				( Holder.Pawn as King ).Gold++;
				lastGoldGenerated = 0;

			}

		}

		DebugOverlay.Text( Position + Vector3.Up * 100f, $"Holder: {Holder?.Name}" );

		if ( IsClient ) { return; } // TODO: Stored units work also clientside

		int count = 0;
		foreach ( var unit in StoredUnits )
		{

			DebugOverlay.Text( Position + Vector3.Up * 80f - count * 10f, $"{unit.Key}: {unit.Value}" );
			count++;

		}

	}

	Dictionary<Path, List<BaseUnit>> firstUnits = new Dictionary<Path, List<BaseUnit>>();

	[Event("Kingdom_Turn_Forts")]
	public void HandleTurns()
	{

		if ( Holder != null && Holder.IsValid() )
		{

			foreach ( var path in AvailablePaths )
			{

				if ( !firstUnits.ContainsKey( path.Value ) )
				{

					firstUnits.Add( path.Value, new List<BaseUnit>() );

				}

				foreach ( var unit in firstUnits[path.Value] )
				{

					unit.IsFirst = false;

				}

				firstUnits[path.Value].Clear();
				firstUnits[path.Value] = path.Value.GetFirstUnits( this );

				foreach ( var unit in firstUnits[path.Value] )
				{

					unit.IsFirst = true;

				}

				foreach ( var lane in path.Value.Lanes )
				{

					foreach ( var waypoint in lane.Waypoints )
					{

						if( IsServer )
						{

							//DebugOverlay.Text( waypoint.Position, $"{waypoint.Status}: {waypoint.Unit}", Kingdom.TurnDuration );

							if ( waypoint.Unit == null )
							{

								waypoint.Status = WaypointStatus.Free; // Cheeky fix for odd bug, might fix later

							}

						}

					}

				}

				foreach ( var unit in StoredUnits )
				{

					for( int i = 0; i < 3; i++ )
					{

						if ( StoredUnits[unit.Key] > 0 )
						{

							Lane middleLane = path.Value.Lanes[Rand.Int( 0, path.Value.TotalLanes - 1 )];

							bool isBackwards = path.Value.FortFrom == this ? false : true;
							int targetWaypoint = isBackwards ? middleLane.Waypoints.Length - 1 : 0;

							if ( middleLane.Waypoints[targetWaypoint].Status != WaypointStatus.Taken )
							{

								CreateUnit( unit.Key, path.Value, middleLane );
								StoredUnits[unit.Key]--;
								middleLane.Waypoints[targetWaypoint].Status = WaypointStatus.Taken;

							}


						}

					}

				}

			}

		}

	}

	public void CreateUnit( string unitType, Path originalPath, Lane originalLane )
	{

		if ( Library.GetType( unitType ).IsSubclassOf( typeof( BaseUnit ) ) )
		{

			var unit = Library.Create<BaseUnit>( unitType );
			unit.SetupUnit( this, originalPath, originalLane );
			unit.Commander = Holder;
			unit.Spawn();

		}

	}

}
