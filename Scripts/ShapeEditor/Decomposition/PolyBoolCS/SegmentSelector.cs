#if UNITY_EDITOR
// PolyBoolCS is a C# port of the polybooljs library
// polybooljs is (c) Copyright 2016, Sean Connelly (@voidqk), http://syntheti.cc
// MIT License


namespace AeternumGames.ShapeEditor.PolyBoolCS
{
	// contains source code from https://github.com/StagPoint/PolyBoolCS/ (see Licenses/PolyBoolCS.txt).

	using System;
	using System.Collections.Generic;

	public class SegmentSelector
	{
		#region Selection tables

		// primary | secondary
		// above1 below1 above2 below2    Keep?               Value
		//    0      0      0      0   =>   no                  0
		//    0      0      0      1   =>   yes filled below    2
		//    0      0      1      0   =>   yes filled above    1
		//    0      0      1      1   =>   no                  0
		//    0      1      0      0   =>   yes filled below    2
		//    0      1      0      1   =>   yes filled below    2
		//    0      1      1      0   =>   no                  0
		//    0      1      1      1   =>   no                  0
		//    1      0      0      0   =>   yes filled above    1
		//    1      0      0      1   =>   no                  0
		//    1      0      1      0   =>   yes filled above    1
		//    1      0      1      1   =>   no                  0
		//    1      1      0      0   =>   no                  0
		//    1      1      0      1   =>   no                  0
		//    1      1      1      0   =>   no                  0
		//    1      1      1      1   =>   no                  0
		private static readonly int[] union_select_table = {
			0, 2, 1, 0,
			2, 2, 0, 0,
			1, 0, 1, 0,
			0, 0, 0, 0
		};

		// primary & secondary
		// above1 below1 above2 below2    Keep?               Value
		//    0      0      0      0   =>   no                  0
		//    0      0      0      1   =>   no                  0
		//    0      0      1      0   =>   no                  0
		//    0      0      1      1   =>   no                  0
		//    0      1      0      0   =>   no                  0
		//    0      1      0      1   =>   yes filled below    2
		//    0      1      1      0   =>   no                  0
		//    0      1      1      1   =>   yes filled below    2
		//    1      0      0      0   =>   no                  0
		//    1      0      0      1   =>   no                  0
		//    1      0      1      0   =>   yes filled above    1
		//    1      0      1      1   =>   yes filled above    1
		//    1      1      0      0   =>   no                  0
		//    1      1      0      1   =>   yes filled below    2
		//    1      1      1      0   =>   yes filled above    1
		//    1      1      1      1   =>   no                  0
		private static readonly int[] intersect_select_table = {
			0, 0, 0, 0,
			0, 2, 0, 2,
			0, 0, 1, 1,
			0, 2, 1, 0
		};

		// primary - secondary
		// above1 below1 above2 below2    Keep?               Value
		//    0      0      0      0   =>   no                  0
		//    0      0      0      1   =>   no                  0
		//    0      0      1      0   =>   no                  0
		//    0      0      1      1   =>   no                  0
		//    0      1      0      0   =>   yes filled below    2
		//    0      1      0      1   =>   no                  0
		//    0      1      1      0   =>   yes filled below    2
		//    0      1      1      1   =>   no                  0
		//    1      0      0      0   =>   yes filled above    1
		//    1      0      0      1   =>   yes filled above    1
		//    1      0      1      0   =>   no                  0
		//    1      0      1      1   =>   no                  0
		//    1      1      0      0   =>   no                  0
		//    1      1      0      1   =>   yes filled above    1
		//    1      1      1      0   =>   yes filled below    2
		//    1      1      1      1   =>   no                  0
		private static readonly int[] difference_select_table = {
			0, 0, 0, 0,
			2, 0, 2, 0,
			1, 1, 0, 0,
			0, 1, 2, 0
		};

		// secondary - primary
		// above1 below1 above2 below2    Keep?               Value
		//    0      0      0      0   =>   no                  0
		//    0      0      0      1   =>   yes filled below    2
		//    0      0      1      0   =>   yes filled above    1
		//    0      0      1      1   =>   no                  0
		//    0      1      0      0   =>   no                  0
		//    0      1      0      1   =>   no                  0
		//    0      1      1      0   =>   yes filled above    1
		//    0      1      1      1   =>   yes filled above    1
		//    1      0      0      0   =>   no                  0
		//    1      0      0      1   =>   yes filled below    2
		//    1      0      1      0   =>   no                  0
		//    1      0      1      1   =>   yes filled below    2
		//    1      1      0      0   =>   no                  0
		//    1      1      0      1   =>   no                  0
		//    1      1      1      0   =>   no                  0
		//    1      1      1      1   =>   no                  0
		private static readonly int[] differenceRev_select_table = {
			0, 2, 1, 0,
			0, 0, 1, 1,
			0, 2, 0, 2,
			0, 0, 0, 0
		};

		// primary ^ secondary
		// above1 below1 above2 below2    Keep?               Value
		//    0      0      0      0   =>   no                  0
		//    0      0      0      1   =>   yes filled below    2
		//    0      0      1      0   =>   yes filled above    1
		//    0      0      1      1   =>   no                  0
		//    0      1      0      0   =>   yes filled below    2
		//    0      1      0      1   =>   no                  0
		//    0      1      1      0   =>   no                  0
		//    0      1      1      1   =>   yes filled above    1
		//    1      0      0      0   =>   yes filled above    1
		//    1      0      0      1   =>   no                  0
		//    1      0      1      0   =>   no                  0
		//    1      0      1      1   =>   yes filled below    2
		//    1      1      0      0   =>   no                  0
		//    1      1      0      1   =>   yes filled above    1
		//    1      1      1      0   =>   yes filled below    2
		//    1      1      1      1   =>   no                  0
		private static readonly int[] xor_select_table = {
			0, 2, 1, 0,
			2, 0, 0, 1,
			1, 0, 0, 2,
			0, 1, 2, 0
		};

		#endregion

		#region Public functions

		public static SegmentList union( SegmentList segments, BuildLog buildLog )
		{
			return select( segments, union_select_table, buildLog );
		}

		public static SegmentList intersect( SegmentList segments, BuildLog buildLog )
		{
			return select( segments, intersect_select_table, buildLog );
		}

		public static SegmentList difference( SegmentList segments, BuildLog buildLog )
		{
			return select( segments, difference_select_table, buildLog );
		}

		public static SegmentList differenceRev( SegmentList segments, BuildLog buildLog )
		{
			return select( segments, differenceRev_select_table, buildLog );
		}

		public static SegmentList xor( SegmentList segments, BuildLog buildLog )
		{
			return select( segments, xor_select_table, buildLog );
		}

		#endregion

		#region Private functions

		private static SegmentList select( SegmentList segments, int[] selection, BuildLog buildLog )
		{
			var result = new SegmentList();

			foreach( var seg in segments )
			{
				var index =
					( seg.myFill.above ? 8 : 0 ) +
					( seg.myFill.below.Value ? 4 : 0 ) +
					( ( seg.otherFill != null && seg.otherFill.above ) ? 2 : 0 ) +
					( ( seg.otherFill != null && seg.otherFill.below.Value ) ? 1 : 0 );

				if( selection[ index ] != 0 )
				{
					// copy the segment to the results, while also calculating the fill status
					result.Add( new Segment()
					{
						id = buildLog != null ? buildLog.segmentId() : -1,
						start = seg.start,
						end = seg.end,
						myFill = new SegmentFill()
						{
							above = selection[ index ] == 1, // 1 if filled above
							below = selection[ index ] == 2  // 2 if filled below
						},
						otherFill = null
					} );
				}
			};

			if( buildLog != null )
			{
				buildLog.selected( result );
			}

			return result;
		}

		#endregion
	}
}
#endif