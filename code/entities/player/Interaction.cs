using Sandbox;
using Sandbox.Component;
using System;
using System.Linq;

public partial class King : Player
{

	Entity selectedEntity = null;

	//////////// ALL OF THIS IS JUST FOR DEBUG, WILL BE REMOVED OR IRONED OUT LATER ///////////
	public void HandleInteractions()
	{

		//// Spawn forts or units

		if ( Input.Pressed( InputButton.Slot1 ) )
		{

			if ( IsClient ) { return; }

			TraceResult tr = Trace.Ray( Input.Cursor, 5000f )
				.Ignore( this )
				.Run();

			/*for ( int i = 0; i < 100; i++ )
			{

				var unit = new Peasant();
				unit.Position = tr.EndPosition + new Vector3( ( i % 10 - 5 ) * 15f, (int)( i / 10 - 5 ) * 15f, 0 );

			}*/

			var fort = new BaseFort();
			fort.Position = tr.EndPosition;
			Kingdom.Forts.Add( fort );

		}

		if ( Input.Pressed( InputButton.Slot2 ) )
		{

			if ( IsClient ) { return; }

			TraceResult tr = Trace.Ray( Input.Cursor, 5000f )
				.Ignore( this )
				.Run();

			/*for ( int i = 0; i < 100; i++ )
			{

				var unit = new Peasant();
				unit.Position = tr.EndPosition + new Vector3( ( i % 10 - 5 ) * 15f, (int)( i / 10 - 5 ) * 15f, 0 );

			}*/

			var fort = new OgreFort();
			fort.Position = tr.EndPosition;
			Kingdom.Forts.Add( fort );

		}

		//// Set yourself as the tower holder

		if ( Input.Pressed( InputButton.Use ) )
		{

			TraceResult sel = Trace.Ray( Input.Cursor, 5000f )
				.Ignore( this )
				.Run();


			if ( sel.Entity is BaseFort )
			{

				( sel.Entity as BaseFort ).Holder = Client;

			}

		}

		//// Make the towers glow

		if ( IsServer ) { return; }

		TraceResult trace = Trace.Ray( Input.Cursor, 5000f )
				.Ignore( this )
				.Run();


		if ( selectedEntity != null && trace.Entity != selectedEntity )
		{

			var glow = selectedEntity.Components.GetOrCreate<Glow>();
			glow.Active = false;
			selectedEntity = null;

		}

		if ( trace.Entity is BaseStructure )
		{

			selectedEntity = trace.Entity;

		}

		if ( selectedEntity != null )
		{

			var glow = trace.Entity.Components.GetOrCreate<Glow>();
			glow.Active = true;
			glow.Color = Color.White;

		}

	}

}
