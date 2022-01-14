#if UNITY_EDITOR
// PolyBoolCS is a C# port of the polybooljs library
// polybooljs is (c) Copyright 2016, Sean Connelly (@voidqk), http://syntheti.cc
// MIT License

namespace AeternumGames.ShapeEditor.PolyBoolCS
{
	// contains source code from https://github.com/StagPoint/PolyBoolCS/ (see Licenses/PolyBoolCS.txt).

	using System;
	using System.Reflection;
	using System.Collections.Generic;

	using Newtonsoft.Json;

	public class BuildLog
	{
		#region Public properties

		public IList<Entry> Entries { get { return list; } }

		#endregion

		#region Private fields

		private List<Entry> list;
		private int nextSegmentId = 0;
		private object curVert = null;

		#endregion

		#region Constructor

		public BuildLog()
		{
			this.list = new List<Entry>();
		}

		#endregion

		#region Public functions

		public string ToJSON()
		{
			return JsonConvert.SerializeObject( list, Formatting.Indented );
		}

		public int segmentId()
		{
			return nextSegmentId++;
		}

		public void clear()
		{
			list.Clear();
			nextSegmentId = 0;
			curVert = null;
		}

		public BuildLog checkIntersection( object seg1, object seg2 )
		{
			return push( "check", new { seg1 = seg1, seg2 = seg2 } );
		}

		public BuildLog segmentChop( object seg, object end )
		{
			push( "div_seg", new { seg = seg, pt = end } );
			return push( "chop", new { seg = seg, pt = end } );
		}

		public BuildLog statusRemove( object seg )
		{
			return push( "pop_seg", new { seg = seg } );
		}

		public BuildLog segmentUpdate( object seg )
		{
			return push( "seg_update", new { seg = seg } );
		}

		public BuildLog segmentNew( object seg, object primary )
		{
			return push( "new_seg", new { seg = seg, primary = primary } );
		}

		public BuildLog segmentRemove( object seg )
		{
			return push( "rem_seg", new { seg = seg } );
		}

		public BuildLog tempStatus( object seg, object above, object below )
		{
			return push( "temp_status", new { seg = seg, above = above, below = below } );
		}

		public BuildLog rewind( object seg )
		{
			return push( "rewind", new { seg = seg } );
		}

		public BuildLog status( object seg, object above, object below )
		{
			return push( "status", new { seg = seg, above = above, below = below } );
		}

		public BuildLog vert( object x )
		{
			if( x.Equals( curVert ) )
				return this;

			curVert = x;

			return push( "vert", new { x = x } );
		}

		public BuildLog log( object data )
		{
			return push( "log", new { txt = data.ToString() } );
		}

		public BuildLog reset()
		{
			return push( "reset", null );
		}

		public BuildLog selected( object segs )
		{
			return push( "selected", new { segs = segs } );
		}

		public BuildLog chainStart( object seg )
		{
			return push( "chain_start", new { seg = seg } );
		}

		public BuildLog chainRemoveHead( object index, object pt )
		{
			return push( "chain_rem_head", new { index = index, pt = pt } );
		}

		public BuildLog chainRemoveTail( object index, object pt )
		{
			return push( "chain_rem_tail", new { index = index, pt = pt } );
		}

		public BuildLog chainNew( object pt1, object pt2 )
		{
			return push( "chain_new", new { pt1 = pt1, pt2 = pt2 } );
		}

		public BuildLog chainMatch( object index )
		{
			return push( "chain_match", new { index = index } );
		}

		public BuildLog chainClose( object index )
		{
			return push( "chain_close", new { index = index } );
		}

		public BuildLog chainAddHead( object index, object pt )
		{
			return push( "chain_add_head", new { index = index, pt = pt } );
		}

		public BuildLog chainAddTail( object index, object pt )
		{
			return push( "chain_add_tail", new { index = index, pt = pt, } );
		}

		public BuildLog chainConnect( object index1, object index2 )
		{
			return push( "chain_con", new { index1 = index1, index2 = index2 } );
		}

		public BuildLog chainReverse( object index )
		{
			return push( "chain_rev", new { index = index } );
		}

		public BuildLog chainJoin( object index1, object index2 )
		{
			return push( "chain_join", new { index1 = index1, index2 = index2 } );
		}

		public BuildLog done()
		{
			return push( "done", null );
		}

		#endregion

		#region Private utility functions

		private BuildLog push( string type, object data )
		{
			var entry = new Entry() { type = type, data = cloneData( data ) };
			list.Add( entry );

			return this;
		}

		private object cloneData( object data )
		{
			return JsonConvert.DeserializeObject( JsonConvert.SerializeObject( data ) );
		}

		#endregion

		#region Nested Types

		public struct Entry
		{
			public string type;
			public object data;
		}

		#endregion
	}
}
#endif