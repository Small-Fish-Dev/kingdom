using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BaseTower : BaseStructure
{

	public override string StructureName => "Base Tower";
	public override string StructureAlignment => "Base";
	public override float ModelScale => 6f;
	public override string StructureModel => "models/sbox_props/burger_box/burger_box.vmdl";

	public override StructureType Type => StructureType.Tower;

}
