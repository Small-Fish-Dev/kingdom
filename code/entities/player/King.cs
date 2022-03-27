using Sandbox;
using Sandbox.Component;
using System;
using System.Linq;

public partial class King : Player
{

	public Clothing.Container Clothing = new();
	[Net] public int Gold { get; set; } = 0;

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

		HandleInteractions();

	}

}
