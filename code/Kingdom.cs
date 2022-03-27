
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class Kingdom : Sandbox.Game
{
	public Kingdom()
	{

		Precache.Add( "models/kingdom_citizen/kingdom_citizen.vmdl" );

	}

	public override void ClientJoined( Client client )
	{

		base.ClientJoined( client );

		var pawn = new King(client);
		client.Pawn = pawn;
		pawn.Respawn();

		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( randomSpawnPoint != null )
		{

			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;

		}

	}
	
}
