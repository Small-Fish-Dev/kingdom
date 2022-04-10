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


			var fort = new KnightFort();
			fort.Position = tr.EndPosition;
			Kingdom.Forts.Add( fort );

		}

		if ( Input.Pressed( InputButton.Slot3 ) )
		{

			if ( IsClient ) { return; }

			TraceResult tr = Trace.Ray( Input.Cursor, 5000f )
				.Ignore( this )
				.Run();


			var fort = new OgreFort();
			fort.Position = tr.EndPosition;
			Kingdom.Forts.Add( fort );

		}

		if ( Input.Pressed( InputButton.Slot4 ) )
		{

			if ( IsClient ) { return; }

			TraceResult tr = Trace.Ray( Input.Cursor, 5000f )
				.Ignore( this )
				.Run();

			var fort = new GiantFort();
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
