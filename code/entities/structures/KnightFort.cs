using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;


[Library( "Structure.Base.KnightFort" )]
public partial class KnightFort : BaseFort
{

	public override string StructureName => "Knight Fort";
	public override string StructureAlignment => "Base";
	public override string UnitsType => "Unit.Human.Knight";
	public override int StartingUnits => 100;
	public override float UnitsPerTurn => 0.0f;

}
