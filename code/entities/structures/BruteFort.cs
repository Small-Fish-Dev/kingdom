using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;


[Library( "Structure.Base.BruteFort" )]
public partial class BruteFort : BaseFort
{

	public override string StructureName => "Brute Fort";
	public override string StructureAlignment => "Base";
	public override string UnitsType => "Unit.Human.Beast";
	public override int StartingUnits => 1;
	public override float UnitsPerSecond => 0.2f;

}
