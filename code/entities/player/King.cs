using Sandbox;
using System;
using System.Linq;

partial class King : Player
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

			for ( int i = 0; i < 9; i++ )
			{

				BaseUnit unit = new BaseUnit();
				unit.Position = tr.EndPosition + new Vector3( ( i % 3 ) * 20f, (int)( i / 3 ) * 20f, 0 );

			}

		}

	}

}
