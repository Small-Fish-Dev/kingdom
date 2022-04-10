using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

[Library("Unit.Human.Knight")]
public partial class Knight : BaseUnit
{

	public override string LibraryName => "Unit.Human.Knight";
	public override string UnitName => "Knight";
	public override string UnitAlignment => "Human";
	public override int MaxHP => 3;
	public override int AttackStrength => 3;
	public override string Outfit => "models/outfits/outfit_knight.vmdl";
	public override float ModelScale => 0.3f;


}
