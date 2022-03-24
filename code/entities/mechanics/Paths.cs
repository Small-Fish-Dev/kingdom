
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

		}

		Waypoints.Add( to );

	}

}

public class Path
{

	public int TotalLanes;
	public BaseFort FortFrom;
	public BaseFort FortTo;

	public Path( BaseFort from, BaseFort to, int totalLanes)
	{

		FortFrom = from;
		FortTo = to;
		TotalLanes = totalLanes;

	}

	

}

public partial class Kingdom
{

	public List<BaseFort> Forts = new List<BaseFort>();

	

	[ServerCmd("generate_paths")]
	public static void GeneratePaths()
	{



	}
	
}
