using Sandbox;
using System;
using System.Linq;

public partial class King : Player
{

	public Clothing.Container Clothing = new();

	public King() { }
	public King( Client cl )
	{

		Clothing.LoadFromClient( cl );

	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		CameraMode = new FirstPersonCamera();

		base.Respawn();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//TickPlayerUse();
		//SimulateActiveChild( cl, ActiveChild );

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( CameraMode is ThirdPersonCamera )
			{
				CameraMode = new FirstPersonCamera();
			}
			else
			{
				CameraMode = new ThirdPersonCamera();
			}
		}

		if ( Input.Pressed( InputButton.Attack1 ) )
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

	}

}
