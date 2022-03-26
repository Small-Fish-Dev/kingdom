using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BaseTower : BaseStructure
{

	public override string StructureName => "Base Tower";
	public override string StructureAlignment => "Base";
	public override float ModelScale => 1f;
	public override string StructureModel => "models/structures/base_tower.vmdl";

	public override StructureType Type => StructureType.Tower;

}
