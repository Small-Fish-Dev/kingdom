using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;


[Library( "Structure.Base.GiantFort" )]
public partial class GiantFort : BaseFort
{

	public override string StructureName => "Giant Fort";
	public override string StructureAlignment => "Base";
	public override string UnitsType => "Unit.Human.Giant";
	public override float ModelScale => base.ModelScale * 2f;
	public override float EntranceDistance => base.EntranceDistance * 2f;
	public override int StartingUnits => 1;
	public override float UnitsPerTurn => 0.001f;

}
