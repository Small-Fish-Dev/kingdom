
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
	Taken,
	Freeing

}

public class Waypoint
{

	public Vector3 Position;
	public WaypointStatus Status = WaypointStatus.Free;

	public Waypoint( Vector3 position )
	{

		Position = position;

	}

}

public class Lane
{

	// Ideally it will never have anything above it or drops below, because that will screw this up

	public List<Waypoint> Waypoints = new List<Waypoint>();

	public Lane( Vector3 from, Vector3 to, Vector3 offset, float totalWaypoints, float totalDistance )
	{

		Waypoints.Add( new Waypoint( from ) ); // First waypoints are inside the fort and then spread out

		Vector3 direction = ( to - from ).Normal;
		float waypointDistance = totalDistance / ( totalWaypoints );
		
		for ( int i = 0; i < totalWaypoints - 1; i++ )
		{

			Vector3 rayPosition = Waypoints[i].Position + direction * waypointDistance + ( i == 0 ? offset : Vector3.Zero );

			TraceResult ray = Trace.Ray( rayPosition + Vector3.Up * 100f, rayPosition + Vector3.Down * 100f )
				.WorldOnly()
				.Run();

			Waypoints.Add( new Waypoint( ray.EndPosition ) );

			DebugOverlay.Sphere( ray.EndPosition, 5f, Color.Red, false, 15f );

		}

		Waypoints.Add( new Waypoint( to ) ); // Last waypoints are inside the fort

		DebugOverlay.Sphere( from, 8f, Color.Red, false, 15f );
		DebugOverlay.Sphere( to, 8f, Color.Red, false, 15f );

	}

}

public class Path
{

	public int TotalLanes;
	public float LaneWidth;
	public float LaneDensity;
	public BaseFort FortFrom;
	public BaseFort FortTo;
	public List<Lane> Lanes = new List<Lane>();

	public Path( BaseFort from, BaseFort to, int totalLanes, float laneWidth, float laneDensity = 15f )
	{

		FortFrom = from;
		FortTo = to;
		TotalLanes = totalLanes;
		LaneWidth = laneWidth;
		LaneDensity = laneDensity;

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

			Lane resultLane = new Lane( startingPos, endingPos, offset, totalWaypoints, distance );

			Lanes.Add( resultLane );

		}

	}

}

public partial class Kingdom
{

	public static List<BaseFort> Forts = new List<BaseFort>();

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

						var path = new Path( fortFrom, fortTo, 5, 15f, 15f );
						path.GenerateLanes();

						fortFrom.AvailablePaths[ fortTo ] = path;
						fortTo.AvailablePaths[ fortFrom ] = path;

					}

				}

			}

		}

	}
	
}
