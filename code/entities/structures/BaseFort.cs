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

	[Net] public Client Holder { get; set; } = null; // Who's holding the fort
	[Net] public Dictionary<Type, int> StoredUnits { get; set; } = new Dictionary<Type, int>();
	[Net] public Dictionary<BaseFort, Path> AvailablePaths { get; set; } = new Dictionary<BaseFort, Path>();

	public BaseFort()
	{

		if ( StartingUnits > 0 )
		{

			AddUnits( UnitsType, StartingUnits );

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

	TimeSince lastUnitGenerated = 0;
	TimeSince lastGoldGenerated = 0;

	[Event.Tick]
	public void GenerateTimers()
	{

		if ( Holder.IsValid() )
		{

			if ( lastUnitGenerated >= 1 / UnitsPerSecond )
			{

				AddUnits( UnitsType, 1 );
				lastUnitGenerated = 0;

			}

			if ( lastGoldGenerated >= 1 / GoldPerSecond )
			{

				( Holder.Pawn as King ).Gold++;
				lastGoldGenerated = 0;

			}

		}

		DebugOverlay.Text( Position + Vector3.Up * 100f, $"Holder: {Holder?.Name}" );

		int count = 0;
		foreach ( var unit in StoredUnits )
		{

			DebugOverlay.Text( Position + Vector3.Up * 80f - count * 10f, $"{unit.Key.Name}: {unit.Value}" );
			count++;

		}

	}

	[Event("Kingdom_Next_Turn")]
	public void HandleTurns()
	{

		if ( IsClient ) { return; }

		if ( Holder != null && Holder.IsValid() )
		{

			foreach ( var path in AvailablePaths )
			{

				foreach ( var lane in path.Value.Lanes )
				{

					foreach ( var unit in StoredUnits )
					{

						if ( unit.Value > 0 )
						{

							CreateUnit( unit.Key, this, path.Value, path.Value.Lanes.IndexOf( lane ) );
							StoredUnits[unit.Key]--;

						}

					}

				}

			}

		}

	}

	public void CreateUnit( Type unitType, BaseFort originalFort, Path originalPath, int laneID )
	{

		if ( unitType.IsSubclassOf( typeof( BaseUnit ) ) )
		{

			Library.Create<BaseUnit>( unitType ).SetupUnit( originalFort, originalPath, laneID );

		}

	}

}
