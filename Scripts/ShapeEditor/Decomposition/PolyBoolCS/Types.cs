#if UNITY_EDITOR
// PolyBoolCS is a C# port of the polybooljs library
// polybooljs is (c) Copyright 2016, Sean Connelly (@voidqk), http://syntheti.cc
// MIT License


namespace AeternumGames.ShapeEditor.PolyBoolCS
{
	// contains source code from https://github.com/StagPoint/PolyBoolCS/ (see Licenses/PolyBoolCS.txt).

	using System;
	using System.Collections.Generic;

	public struct Transition
	{
		public EventNode before;
		public EventNode after;

		public StatusNode prev;
		public StatusNode here;
	}

	public class Segment
	{
		public int id = -1;
		public Point start;
		public Point end;
		public SegmentFill myFill;
		public SegmentFill otherFill;

		public Segment()
		{
			myFill = new SegmentFill();
		}

		#region Debugging support

		public override string ToString()
		{
			return string.Format( "Start={0}, End={1}, Fill={2}", start, end, myFill );
		}

		#endregion 
	}

	public class SegmentFill
	{
		// NOTE: This is kind of asinine, but the original javascript code used (below === null) to determine that the edge had not 
		// yet been processed, and treated below as a standard true/false in every other case, necessitating the use of a nullable 
		// bool here.

		public bool above;
		public bool? below;

		#region Debugging support

		public override string ToString()
		{
			return string.Format( "[Above={0}, Below={1}]", above, below );
		}

		#endregion 
	}

	public struct Intersection
	{
		public static readonly Intersection Empty = new Intersection();

		//  alongA and alongB will each be one of: -2, -1, 0, 1, 2
		//
		//  with the following meaning:
		//
		//    -2   intersection point is before segment's first point
		//    -1   intersection point is directly on segment's first point
		//     0   intersection point is between segment's first and second points (exclusive)
		//     1   intersection point is directly on segment's second point
		//     2   intersection point is after segment's second point

		/// <summary>
		/// where the intersection point is at
		/// </summary>
		public Point pt;

		/// <summary>
		/// where intersection point is along A
		/// </summary>
		public float alongA;

		/// <summary>
		/// where intersection point is along B
		/// </summary>
		public float alongB;
	}

	[System.Diagnostics.DebuggerTypeProxy( typeof( Polygon.PolygonDebugProxy ) )]
	public class Polygon
	{
		public List<PointList> regions = null;
		public bool inverted = false;

		#region Debugging support

		public override string ToString()
		{
			return string.Format( "Regions={0}, Inverted={1}", regions.Count, inverted );
		}

		public sealed class PolygonDebugProxy
		{
			private readonly ICollection<PointList> list;

			[System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.RootHidden )]
			public PointList[] Regions
			{
				get
				{
					var items = new PointList[ list.Count ];
					list.CopyTo( items, 0 );
					return items;
				}
			}

			public PolygonDebugProxy( Polygon target )
			{
				this.list = target.regions;
			}
		}

		#endregion
	}

	public struct Point
	{
		public double x;
		public double y;

		public Point( double x, double y )
		{
			this.x = x;
			this.y = y;
		}

		#region Debugging support

		public override string ToString()
		{
			return string.Format( "[{0:F3}, {1:F3}]", x, y );
		}

		#endregion 
	}

	[System.Diagnostics.DebuggerTypeProxy( typeof( PointList.PointListDebugProxy ) )]
	public class PointList : List<Point>
	{
		public PointList()
			: base()
		{
		}

		public PointList( int capacity )
			: base( capacity )
		{
		}

		#region Debugging support

		public override string ToString()
		{
			return string.Format( "Count={0}", this.Count );
		}

		public sealed class PointListDebugProxy
		{
			private readonly ICollection<Point> list;

			[System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.RootHidden )]
			public Point[] Points
			{
				get
				{
					var items = new Point[ list.Count ];
					list.CopyTo( items, 0 );
					return items;
				}
			}

			public PointListDebugProxy( PointList target )
			{
				this.list = target;
			}
		}

		#endregion
	}

	[System.Diagnostics.DebuggerTypeProxy( typeof( CombinedSegmentLists.CombinedSegmentListDebugProxy ) )]
	public class CombinedSegmentLists
	{
		public SegmentList combined;
		public bool inverted1;
		public bool inverted2;

		#region Debugging support

		public override string ToString()
		{
			return string.Format( "Count={0}", this.combined.Count );
		}

		public sealed class CombinedSegmentListDebugProxy
		{
			private readonly ICollection<Segment> list;

			[System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.RootHidden )]
			public Segment[] Segments
			{
				get
				{
					var items = new Segment[ list.Count ];
					list.CopyTo( items, 0 );
					return items;
				}
			}

			public CombinedSegmentListDebugProxy( CombinedSegmentLists target )
			{
				this.list = target.combined;
			}
		}

		#endregion
	}

	[System.Diagnostics.DebuggerTypeProxy( typeof( SegmentList.SegmentListDebugProxy ) )]
	public class SegmentList : List<Segment>
	{
		public bool inverted = false;

		public SegmentList()
			: base()
		{
		}

		public SegmentList( int capacity )
			: base( capacity )
		{
		}

		#region Debugging support

		public override string ToString()
		{
			return string.Format( "Count={0}", this.Count );
		}

		public sealed class SegmentListDebugProxy
		{
			private readonly ICollection<Segment> list;

			[System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.RootHidden )]
			public Segment[] Items
			{
				get
				{
					var items = new Segment[ list.Count ];
					list.CopyTo( items, 0 );
					return items;
				}
			}

			public SegmentListDebugProxy( SegmentList target )
			{
				this.list = target;
			}
		}

		#endregion
	}
}
#endif