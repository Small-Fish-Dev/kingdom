using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public enum StructureType
{

	Castle,
	Outpost,
	Tower

}

[Library( "Structure.Base" )]
public partial class BaseStructure : ModelEntity
{

	public virtual string StructureName => "Base";
	public virtual string StructureAlignment => "Base";
	public virtual float ModelScale => 2f;
	public virtual string StructureModel => "models/sbox_props/burger_box/burger_box.vmdl";

	public virtual StructureType Type => StructureType.Castle;

	public override void Spawn()
	{

		base.Spawn();

		SetModel( StructureModel );
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		Tags.Add( "Structure", $"{StructureName}", $"{StructureAlignment}" );

		EnableDrawing = true;
		EnableShadowCasting = true;
		Scale = ModelScale;

	}

	public override void ClientSpawn()
	{

		base.ClientSpawn();

	}

}
