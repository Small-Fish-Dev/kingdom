
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;


public partial class Kingdom : Sandbox.Game
{

	public const float TurnDuration = 1f; // How long a turn lasts in seconds

	[Event( "Kingdom_Next_Turn" ), ClientRpc]
	public static void Test()
	{

		Event.Run( "Kingdrom_Next_Turn" );

	}

	TimeSince lastTurn = 0;

	[Event.Tick.Server]
	public void HandleTurns()
	{

		if ( lastTurn >= TurnDuration )
		{

			Event.Run( "Kingdom_Next_Turn" );
			lastTurn = 0;

		}

	}

}

