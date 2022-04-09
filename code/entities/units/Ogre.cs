using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

[Library("Unit.Human.Ogre")]
public partial class Ogre : BaseUnit
{

	public override string LibraryName => "Unit.Human.Ogre";
	public override string UnitName => "Ogre";
	public override string UnitAlignment => "Human";
	public override int MaxHP => 300;
	public override int AttackStrength => 100;
	public override string Outfit => "";
	public override float ModelScale => 0.5f;


}
