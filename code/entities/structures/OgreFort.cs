using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;


[Library( "Structure.Base.OgreFort" )]
public partial class OgreFort : BaseFort
{

	public override string StructureName => "Ogre Fort";
	public override string StructureAlignment => "Base";
	public override string UnitsType => "Unit.Human.Ogre";
	public override float ModelScale => base.ModelScale * 1.5f;
	public override float EntranceDistance => base.EntranceDistance * 1.5f;
	public override int StartingUnits => 2;
	public override float UnitsPerTurn => 0.0f;

}
