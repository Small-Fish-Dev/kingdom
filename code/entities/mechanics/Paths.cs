
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Lane
{

	// Ideally it will never have anything above it or drops below, because that will screw this up

	public List<Vector3> Waypoints = new List<Vector3>();

	public Lane( Vector3 from, Vector3 to, float totalWaypoints, float totalDistance )
	{

		Waypoints.Add( from );

		Vector3 direction = ( to - from ).Normal;
		float waypointDistance = totalDistance / ( totalWaypoints - 1 ); // Last waypoint is "to"
		
		for ( int i = 0; i < totalWaypoints; i++ )
		{

			Vector3 rayPosition = Waypoints[i] + direction * waypointDistance;

			TraceResult ray = Trace.Ray( rayPosition + Vector3.Up * 100f, rayPosition + Vector3.Down * 100f )
				.WorldOnly()
				.Run();

			Waypoints.Add( ray.EndPosition );

			DebugOverlay.Sphere( ray.EndPosition, 5f, Color.Red, true, 5f );

		}

		Waypoints.Add( to );

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

		Vector3 startingPos = FortFrom.Position;
		Vector3 endingPos = FortTo.Position;
		float distance = startingPos.Distance( endingPos );
		int totalWaypoints = (int)(distance / LaneDensity);
		Vector3 forwardDirection = (endingPos - startingPos).Normal;
		Vector3 rightDirection = Vector3.Cross( forwardDirection, Vector3.Up );

		for ( int i = 0; i < TotalLanes; i++ )
		{

			Vector3 offset = ( rightDirection * i - ( rightDirection * ( TotalLanes / 2 ) ) ) * LaneWidth;

			Lane resultLane = new Lane( startingPos + offset, endingPos + offset, totalWaypoints, distance );

			Lanes.Add( resultLane );

		}

	}

}

public partial class Kingdom
{

	public static List<BaseFort> Forts = new List<BaseFort>();

	[ServerCmd("generate_paths")]
	public static void GeneratePaths()
	{

		new Path( Kingdom.Forts[0], Kingdom.Forts[1], 5, 15f, 20f )
			.GenerateLanes();

		new Path( Kingdom.Forts[0], Kingdom.Forts[2], 5, 15f, 20f )
			.GenerateLanes();

		new Path( Kingdom.Forts[0], Kingdom.Forts[3], 5, 15f, 20f )
			.GenerateLanes();

		new Path( Kingdom.Forts[1], Kingdom.Forts[2], 5, 15f, 20f )
			.GenerateLanes();

		new Path( Kingdom.Forts[1], Kingdom.Forts[3], 5, 15f, 20f )
			.GenerateLanes();

		new Path( Kingdom.Forts[2], Kingdom.Forts[3], 5, 15f, 20f )
			.GenerateLanes();

	}
	
}
