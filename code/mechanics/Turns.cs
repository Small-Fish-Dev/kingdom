
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
	[Net, Predicted] public static TimeSince LastTurn { get; set; } = 0;

	[Event.Tick]
	public void HandleTurns()
	{

		if ( LastTurn >= TurnDuration )
		{

			if ( IsServer )
			{

				// Units start first
				Event.Run( "Kingdom_Turn_Units" );
				Event.Run( "Kingdom_Turn_Forts" );

			}

			LastTurn = 0;

		}

	}

}

