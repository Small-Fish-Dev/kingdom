using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BaseFort : BaseStructure
{

	public override string StructureName => "Base Capturable";
	public override string StructureAlignment => "Base";
	public override float ModelScale => 10f;
	public override string StructureModel => "models/sbox_props/burger_box/burger_box.vmdl";
	public virtual float UnitsPerSecond => 0.5f; // How many units are generated each second inside of this fort
	public virtual Type UnitsType => typeof( Peasant ); // Which units it generates
	public virtual float GoldPerSecond => 1f; // How much gold it generates each second

	public override StructureType Type => StructureType.Castle;


	public Dictionary<BaseFort, Path> AvailablePaths = new Dictionary<BaseFort, Path>();


}
