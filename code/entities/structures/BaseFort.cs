using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BaseFort : BaseStructure
{

	public override string StructureName => "Base Capturable";
	public override string StructureAlignment => "Base";
	public override float ModelScale => 1f;
	public override string StructureModel => "models/structures/base_fort.vmdl";
	public virtual float EntranceDistance => 45f; // The lanes will begin at this offset from the center, so units won't stay hidden inside
	public virtual float UnitsPerSecond => 0.3f; // How many units are generated each second inside of this fort
	public virtual Type UnitsType => typeof( Peasant ); // Which units it generates
	public virtual int StartingUnits => 10; // How many units are inside the castle that you need to defeat before capturing
	public virtual float GoldPerSecond => 0.5f; // How much gold it generates each second

	public override StructureType Type => StructureType.Outpost;

	public Client Holder; // Who's holding the fort
	public Dictionary<Type, int> StoredUnits = new Dictionary<Type, int>();
	public Dictionary<BaseFort, Path> AvailablePaths = new Dictionary<BaseFort, Path>();

	public override void Spawn()
	{

		base.Spawn();

		if ( StartingUnits > 0 )
		{

			

		}

	}

	public void AddUnits( Type unitType, int unitAmount )
	{

		if ( unitType.IsSubclassOf( typeof( BaseUnit ) ) )
		{

			if ( StoredUnits.ContainsKey( unitType ) )
			{

				StoredUnits[ unitType ] += unitAmount;
				return;

			}

			StoredUnits.Add( unitType, unitAmount );

		}

	}

	int gold = 0;//TODO: Remove this
	TimeSince lastUnitGenerated = 0;
	TimeSince lastGoldGenerated = 0;

	[Event.Tick]
	public void GenerateUnits()
	{

		if ( true )//Holder.IsValid() )
		{

			if ( lastUnitGenerated >= 1/UnitsPerSecond )
			{

				AddUnits( UnitsType, 1 );
				lastUnitGenerated = 0;

			}

			if ( lastGoldGenerated >= 1 / GoldPerSecond )
			{

				//( Holder.Pawn as King ).Gold++;
				gold++; //TODO: Remove this
				lastGoldGenerated = 0;

			}

		}

		DebugOverlay.Text( Position + Vector3.Up * 100f, $"Gold: {gold}" );
		DebugOverlay.Text( Position + Vector3.Up * 80f, $"Units: Peasant: {StoredUnits[typeof(Peasant)]}" );

	}

}
