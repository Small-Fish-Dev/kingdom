using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

[Library( "Unit.Human.Giant" )]
public partial class Giant : BaseUnit
{

	public override string LibraryName => "Unit.Human.Giant";
	public override string UnitName => "Giant";
	public override string UnitAlignment => "Human";
	public override int MaxHP => 3000;
	public override int AttackStrength => 200;
	public override string Outfit => "";
	public override float ModelScale => 0.8f;


}
