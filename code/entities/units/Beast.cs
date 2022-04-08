using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

[Library("Unit.Human.Beast")]
public partial class Beast : BaseUnit
{

	public override string UnitName => "Beast";
	public override string UnitAlignment => "Human";
	public override int MaxHP => 300;
	public override string Outfit => "";
	public override float ModelScale => 0.5f;


}
