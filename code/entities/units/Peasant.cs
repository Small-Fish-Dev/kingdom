using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

[Library("Unit.Human.Peasant")]
public partial class Peasant : BaseUnit
{

	public override string LibraryName => "Unit.Human.Peasant";
	public override string UnitName => "Peasant";
	public override string UnitAlignment => "Human";
	public override string Outfit => "models/outfits/outfit_thief.vmdl";


}
