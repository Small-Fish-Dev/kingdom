using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;


public partial class Kingdom : Sandbox.Game
{

	public static int[] SpiralPattern1D( int pos, int max )
	{

		int[] result = new int[max + 1];

		result[0] = 0; // Always check in front first

		for ( int i = 1; i <= max * 2; i++ )
		{

			int curX = (int)Math.Ceiling( i / 2d );
			int value = curX * ( i % 2 == 0 ? -1 : 1 );
			int offset = value + pos;

			if ( offset >= 0 && offset <= max )
			{

				result[curX] = value;

			}

		}

		return result;

	}

	/*
	 
		A = Position of the unit

		0 1 2 3 4	Would return	X:			0 1 2 3 4
		    A			--->		Result:		0 1 -1 2 -2

		0 1 2 3 4	Would return	X:			0 1 2 3 4
		A				--->		Result:		0 1 2 3 4

		0 1 2 3 4	Would return	X:			0 1 2 3 4
		        A		--->		Result:		0 -1 -2 -3 -4

	*/
}

