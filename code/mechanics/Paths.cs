
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public enum WaypointStatus
{

	Free,
	Taken

}

public partial class Waypoint : BaseNetworkable
{

	public Vector3 Position { get; set; }
	public Lane Lane;
	[Net] public BaseUnit Unit { get; set; }
	[Net] public WaypointStatus Status { get; set; } = WaypointStatus.Free;

	public Waypoint( Vector3 position, Lane lane)
	{

		Position = position;
		Lane = lane;

	}

}

public class Lane : BaseNetworkable
{

	// Ideally it will never have anything above it or drops below, because that will screw this up

	public Waypoint[] Waypoints;
	public Path OriginPath;
	[Net] public Dictionary<BaseFort, BaseUnit[]> UnitLaneMap { get; set; } = new Dictionary<BaseFort, BaseUnit[]>();

	public Lane() { }

	public Lane( Path originPath, Vector3 from, Vector3 to, Vector3 offset, int totalWaypoints, float totalDistance )
	{

		OriginPath = originPath;
		Waypoints = new Waypoint[totalWaypoints + 1];

		Vector3 direction = ( to - from ).Normal;
		float entranceDistance = OriginPath.FortFrom.EntranceDistance;
		float waypointDistance = totalDistance / ( totalWaypoints );

		Waypoints[0] = new Waypoint( from, this ); // Just for the first waypoint
		
		for ( int i = 1; i < totalWaypoints; i++ )
		{

			Vector3 rayPosition = Waypoints[i - 1].Position + direction * waypointDistance + ( i == 1 ? offset : Vector3.Zero );

			TraceResult ray = Trace.Ray( rayPosition + Vector3.Up * 100f, rayPosition + Vector3.Down * 100f )
				.WorldOnly()
				.Run();

			// Entrance waypoint
			if ( i == 1 )
			{

				Vector3 entranceDirection = ( ray.EndPosition - from + direction * entranceDistance).Normal;
				Vector3 entrancePosition = from + entranceDirection * entranceDistance - direction * entranceDistance;
				Waypoints[0] = new Waypoint( entrancePosition, this );

				//DebugOverlay.Sphere( entrancePosition, 5f, Color.Red, true, float.PositiveInfinity );

			}
			else if ( i == totalWaypoints - 1 )
			{

				Vector3 exitDirection = ( ray.EndPosition - to - direction * entranceDistance).Normal;
				Vector3 exitPosition = to + exitDirection * entranceDistance + direction * entranceDistance;
				Waypoints[totalWaypoints] = new Waypoint( exitPosition, this );

				//DebugOverlay.Sphere( exitPosition, 5f, Color.Red, true, float.PositiveInfinity );

			}

			Waypoints[i] = new Waypoint( ray.EndPosition, this );

			//DebugOverlay.Sphere( ray.EndPosition, 2f, Color.Red, true, float.PositiveInfinity);

		}

		UnitLaneMap.Add( OriginPath.FortFrom, new BaseUnit[totalWaypoints + 1] );
		UnitLaneMap.Add( OriginPath.FortTo, new BaseUnit[totalWaypoints + 1] );

	}

}

public class Path : BaseNetworkable
{

	public int TotalLanes;
	public float LaneWidth;
	public float LaneDensity;
	public BaseFort FortFrom;
	public BaseFort FortTo;
	public Lane[] Lanes;

	public Path() { }

	public Path( BaseFort from, BaseFort to, int totalLanes, float laneWidth, float laneDensity = 15f )
	{

		FortFrom = from;
		FortTo = to;
		TotalLanes = totalLanes;
		LaneWidth = laneWidth;
		LaneDensity = laneDensity;
		Lanes = new Lane[totalLanes];

	}

	public void GenerateLanes()
	{

		Vector3 forwardDirection = (FortTo.Position - FortFrom.Position).Normal;
		Vector3 rightDirection = Vector3.Cross( forwardDirection, Vector3.Up );
		Vector3 startingPos = FortFrom.Position + forwardDirection * FortFrom.EntranceDistance;
		Vector3 endingPos = FortTo.Position - forwardDirection * FortTo.EntranceDistance;
		float distance = startingPos.Distance( endingPos );
		int totalWaypoints = (int)(distance / LaneDensity);

		for ( int i = 0; i < TotalLanes; i++ )
		{

			Vector3 offset = ( rightDirection * i - ( rightDirection * ( TotalLanes / 2 ) ) ) * LaneWidth;

			Lane resultLane = new Lane( this, startingPos, endingPos, offset, totalWaypoints, distance );

			Lanes[i] = resultLane;

		}

	}

	public List<BaseUnit> GetFirstUnits( BaseFort fort )
	{

		List<BaseUnit> result = new List<BaseUnit>();

		foreach ( var lanes in Lanes )
		{

			BaseUnit laneUnit = null;

			if ( fort == FortTo )
			{

				laneUnit = lanes.UnitLaneMap[fort].FirstOrDefault( unit => unit != null, null);

			}
			else
			{

				laneUnit = lanes.UnitLaneMap[fort].LastOrDefault( unit => unit != null, null );

			}

			if ( laneUnit != null )
			{

				result.Add( laneUnit );

			}

		}

		return result;

	}

}

public partial class Kingdom
{

	public static List<BaseFort> Forts = new List<BaseFort>();
	public static int LanesCount = 3;
	public static float LanesWidth = 15f;
	public static float LanesDensity = 15f;

	[ServerCmd( "generate_paths" )]
	public static void GeneratePaths( float maxDistance = 500f )
	{

		foreach ( BaseFort fortFrom in Kingdom.Forts )
		{

			foreach ( BaseFort fortTo in Kingdom.Forts )
			{

				if ( fortFrom != fortTo && !fortFrom.AvailablePaths.ContainsKey( fortTo ) )
				{

					if ( fortFrom.Position.Distance( fortTo.Position ) <= maxDistance )
					{

						var path = new Path( fortFrom, fortTo, LanesCount, LanesWidth, LanesDensity );
						path.GenerateLanes();

						fortFrom.AvailablePaths[ fortTo ] = path;
						fortTo.AvailablePaths[ fortFrom ] = path;

					}

				}

			}

		}

	}

	[ServerCmd( "delete_paths" )]
	public static void DeletePaths()
	{

		foreach ( BaseFort fort in Kingdom.Forts )
		{

			foreach ( var path in fort.AvailablePaths )
			{

				foreach ( Lane lane in path.Value.Lanes )
				{

					for ( int i = 0; i < lane.Waypoints.Length; i++ )
					{

						lane.Waypoints[i] = null;

					}

				}

			}

		}

	}

}
