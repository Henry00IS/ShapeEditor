#if UNITY_EDITOR
// PolyBoolCS is a C# port of the polybooljs library
// polybooljs is (c) Copyright 2016, Sean Connelly (@voidqk), http://syntheti.cc
// MIT License


namespace AeternumGames.ShapeEditor.PolyBoolCS
{
	// contains source code from https://github.com/StagPoint/PolyBoolCS/ (see Licenses/PolyBoolCS.txt).

	using System;
	using System.Collections.Generic;

	/// <summary>
	/// this is the core work-horse
	/// </summary>
	public class Intersecter
	{
		#region Private fields

		private bool selfIntersection = false;
		private BuildLog buildLog = null;

		private EventLinkedList event_root = new EventLinkedList();
		private StatusLinkedList status_root;

		#endregion

		#region Constructor

		public Intersecter( bool selfIntersection, BuildLog buildLog = null )
		{
			this.selfIntersection = selfIntersection;
			this.buildLog = buildLog;
		}

		#endregion

		#region Segment creation

		public Segment segmentNew( Point start, Point end )
		{
			return new Segment()
			{
				id = buildLog != null ? buildLog.segmentId() : -1,
				start = start,
				end = end,
				myFill = new SegmentFill(),
				otherFill = null
			};
		}

		public Segment segmentCopy( Point start, Point end, Segment seg )
		{
			return new Segment()
			{
				id = buildLog != null ? buildLog.segmentId() : -1,
				start = start,
				end = end,
				myFill = new SegmentFill()
				{
					above = seg.myFill.above,
					below = seg.myFill.below
				},
				otherFill = null
			};
		}

		#endregion

		#region Event logic

		public void eventAdd( EventNode ev, Point other_pt )
		{
			event_root.insertBefore( ev, other_pt );
		}

		public EventNode eventAddSegmentStart( Segment seg, bool primary )
		{
			var ev_start = new EventNode()
			{
				isStart = true,
				pt = seg.start,
				seg = seg,
				primary = primary,
				other = null,
				status = null
			};

			eventAdd( ev_start, seg.end );

			return ev_start;
		}

		public EventNode eventAddSegmentEnd( EventNode ev_start, Segment seg, bool primary )
		{
			var ev_end = new EventNode()
			{
				isStart = false,
				pt = seg.end,
				seg = seg,
				primary = primary,
				other = ev_start,
				status = null
			};

			ev_start.other = ev_end;

			eventAdd( ev_end, ev_start.pt );

			return ev_end;
		}

		public EventNode eventAddSegment( Segment seg, bool primary )
		{
			var ev_start = eventAddSegmentStart( seg, primary );
			eventAddSegmentEnd( ev_start, seg, primary );

			return ev_start;
		}

		public void eventUpdateEnd( EventNode ev, Point end )
		{
			// slides an end backwards
			//   (start)------------(end)    to:
			//   (start)---(end)

			if( buildLog != null )
			{
				buildLog.segmentChop( ev.seg, end );
			}

			ev.other.remove();
			ev.seg.end = end;
			ev.other.pt = end;
			eventAdd( ev.other, ev.pt );
		}

		public EventNode eventDivide( EventNode ev, Point pt )
		{
			var ns = segmentCopy( pt, ev.seg.end, ev.seg );
			eventUpdateEnd( ev, pt );

			return eventAddSegment( ns, ev.primary );
		}

		#endregion

		#region Calculation

		public SegmentList calculate( bool inverted = false )
		{
			if( !selfIntersection )
			{
				throw new Exception( "This function is only intended to be called when selfIntersection = true" );
			}

			return calculate_INTERNAL( inverted, false );
		}

		public SegmentList calculate( SegmentList segments1, bool inverted1, SegmentList segments2, bool inverted2 )
		{
			if( selfIntersection )
			{
				throw new Exception( "This function is only intended to be called when selfIntersection = false" );
			}

			// segmentsX come from the self-intersection API, or this API
			// invertedX is whether we treat that list of segments as an inverted polygon or not
			// returns segments that can be used for further operations
			for( int i = 0; i < segments1.Count; i++ )
			{
				eventAddSegment( segments1[ i ], true );
			}
			for( int i = 0; i < segments2.Count; i++ )
			{
				eventAddSegment( segments2[ i ], false );
			}

			return calculate_INTERNAL( inverted1, inverted2 );
		}

		public void addRegion( PointList region )
		{
			if( !selfIntersection )
			{
				throw new Exception( "The addRegion() function is only intended for use when selfIntersection = false" );
			}

			// Ensure that the polygon is fully closed (the start point and end point are exactly the same)
			if( !Epsilon.pointsSame( region[ region.Count - 1 ], region[ 0 ] ) )
			{
				region.Add( region[ 0 ] );
			}

			// regions are a list of points:
			//  [ [0, 0], [100, 0], [50, 100] ]
			// you can add multiple regions before running calculate
			var pt1 = new Point();
			var pt2 = region[ region.Count - 1 ];

			for( var i = 0; i < region.Count; i++ )
			{
				pt1 = pt2;
				pt2 = region[ i ];

				var forward = Epsilon.pointsCompare( pt1, pt2 );
				if( forward == 0 ) // points are equal, so we have a zero-length segment
					continue; // just skip it

				eventAddSegment(
					segmentNew(
						forward < 0 ? pt1 : pt2,
						forward < 0 ? pt2 : pt1
					),
					true
				);
			}
		}

		private Transition statusFindSurrounding( EventNode ev )
		{
			return status_root.findTransition( ev );
		}

		private EventNode checkIntersection( EventNode ev1, EventNode ev2 )
		{
			// returns the segment equal to ev1, or false if nothing equal

			var seg1 = ev1.seg;
			var seg2 = ev2.seg;
			var a1 = seg1.start;
			var a2 = seg1.end;
			var b1 = seg2.start;
			var b2 = seg2.end;

			if( buildLog != null )
				buildLog.checkIntersection( seg1, seg2 );

			Intersection intersect;
			if( !Epsilon.linesIntersect( a1, a2, b1, b2, out intersect ) )
			{
				// segments are parallel or coincident

				// if points aren't collinear, then the segments are parallel, so no intersections
				if( !Epsilon.pointsCollinear( a1, a2, b1 ) )
					return null;

				// otherwise, segments are on top of each other somehow (aka coincident)

				if( Epsilon.pointsSame( a1, b2 ) || Epsilon.pointsSame( a2, b1 ) )
					return null; // segments touch at endpoints... no intersection

				var a1_equ_b1 = Epsilon.pointsSame( a1, b1 );
				var a2_equ_b2 = Epsilon.pointsSame( a2, b2 );

				if( a1_equ_b1 && a2_equ_b2 )
					return ev2; // segments are exactly equal

				var a1_between = !a1_equ_b1 && Epsilon.pointBetween( a1, b1, b2 );
				var a2_between = !a2_equ_b2 && Epsilon.pointBetween( a2, b1, b2 );

				// handy for debugging:
				// buildLog.log({
				//	a1_equ_b1: a1_equ_b1,
				//	a2_equ_b2: a2_equ_b2,
				//	a1_between: a1_between,
				//	a2_between: a2_between
				// });

				if( a1_equ_b1 )
				{
					if( a2_between )
					{
						//  (a1)---(a2)
						//  (b1)----------(b2)
						eventDivide( ev2, a2 );
					}
					else
					{
						//  (a1)----------(a2)
						//  (b1)---(b2)
						eventDivide( ev1, b2 );
					}

					return ev2;
				}
				else if( a1_between )
				{
					if( !a2_equ_b2 )
					{
						// make a2 equal to b2
						if( a2_between )
						{
							//         (a1)---(a2)
							//  (b1)-----------------(b2)
							eventDivide( ev2, a2 );
						}
						else
						{
							//         (a1)----------(a2)
							//  (b1)----------(b2)
							eventDivide( ev1, b2 );
						}
					}

					//         (a1)---(a2)
					//  (b1)----------(b2)
					eventDivide( ev2, a1 );
				}
			}
			else
			{
				// otherwise, lines intersect at i.pt, which may or may not be between the endpoints

				// is A divided between its endpoints? (exclusive)
				if( intersect.alongA == 0 )
				{
					if( intersect.alongB == -1 ) // yes, at exactly b1
						eventDivide( ev1, b1 );
					else if( intersect.alongB == 0 ) // yes, somewhere between B's endpoints
						eventDivide( ev1, intersect.pt );
					else if( intersect.alongB == 1 ) // yes, at exactly b2
						eventDivide( ev1, b2 );
				}

				// is B divided between its endpoints? (exclusive)
				if( intersect.alongB == 0 )
				{
					if( intersect.alongA == -1 ) // yes, at exactly a1
						eventDivide( ev2, a1 );
					else if( intersect.alongA == 0 ) // yes, somewhere between A's endpoints (exclusive)
						eventDivide( ev2, intersect.pt );
					else if( intersect.alongA == 1 ) // yes, at exactly a2
						eventDivide( ev2, a2 );
				}
			}

			return null;
		}

		private EventNode checkBothIntersections( EventNode ev, EventNode above, EventNode below )
		{
			if( above != null )
			{
				var eve = checkIntersection( ev, above );
				if( eve != null )
					return eve;
			}

			if( below != null )
			{
				return checkIntersection( ev, below );
			}

			return null;
		}

		private SegmentList calculate_INTERNAL( bool primaryPolyInverted, bool secondaryPolyInverted )
		{
			//
			// main event loop
			//
			var segments = new SegmentList();
			status_root = new StatusLinkedList();

			while( !event_root.isEmpty )
			{
				var ev = (EventNode)event_root.head;

				if( buildLog != null )
					buildLog.vert( ev.pt.x );

				if( ev.isStart )
				{
					if( buildLog != null )
					{
						buildLog.segmentNew( ev.seg, ev.primary );
					}

					var surrounding = statusFindSurrounding( ev );
					var above = surrounding.before != null ? surrounding.before : null;
					var below = surrounding.after != null ? surrounding.after : null;

					if( buildLog != null )
					{
						buildLog.tempStatus(
							ev.seg,
							above != null ? above.seg : (object)false,
							below != null ? below.seg : (object)false
						);
					}

					var eve = checkBothIntersections( ev, above, below );
					if( eve != null )
					{
						// ev and eve are equal
						// we'll keep eve and throw away ev

						// merge ev.seg's fill information into eve.seg

						if( selfIntersection )
						{
							var toggle = false; // are we a toggling edge?
							if( ev.seg.myFill.below == null )
								toggle = true;
							else
								toggle = ev.seg.myFill.above != ev.seg.myFill.below;

							// merge two segments that belong to the same polygon
							// think of this as sandwiching two segments together, where `eve.seg` is
							// the bottom -- this will cause the above fill flag to toggle
							if( toggle )
							{
								eve.seg.myFill.above = !eve.seg.myFill.above;
							}
						}
						else
						{
							// merge two segments that belong to different polygons
							// each segment has distinct knowledge, so no special logic is needed
							// note that this can only happen once per segment in this phase, because we
							// are guaranteed that all self-intersections are gone
							eve.seg.otherFill = ev.seg.myFill;
						}

						if( buildLog != null )
						{
							buildLog.segmentUpdate( eve.seg );
						}

						ev.other.remove();
						ev.remove();
					}

					if( event_root.head != ev )
					{
						// something was inserted before us in the event queue, so loop back around and
						// process it before continuing
						if( buildLog != null )
						{
							buildLog.rewind( ev.seg );
						}

						continue;
					}

					//
					// calculate fill flags
					//
					if( selfIntersection )
					{
						var toggle = false; // are we a toggling edge?

						// if we are a new segment...
						if( ev.seg.myFill.below == null ) 
							// then we toggle
							toggle = true; 
						else 
							// we are a segment that has previous knowledge from a division
							toggle = ev.seg.myFill.above != ev.seg.myFill.below; // calculate toggle

						// next, calculate whether we are filled below us
						if( below == null )
						{
							// if nothing is below us...
							// we are filled below us if the polygon is inverted
							ev.seg.myFill.below = primaryPolyInverted;
						}
						else
						{
							// otherwise, we know the answer -- it's the same if whatever is below
							// us is filled above it
							ev.seg.myFill.below = below.seg.myFill.above;
						}

						// since now we know if we're filled below us, we can calculate whether
						// we're filled above us by applying toggle to whatever is below us
						if( toggle )
							ev.seg.myFill.above = !ev.seg.myFill.below.Value;
						else
							ev.seg.myFill.above = ev.seg.myFill.below.Value;
					}
					else
					{
						// now we fill in any missing transition information, since we are all-knowing
						// at this point

						if( ev.seg.otherFill == null )
						{
							// if we don't have other information, then we need to figure out if we're
							// inside the other polygon
							var inside = false;
							if( below == null )
							{
								// if nothing is below us, then we're inside if the other polygon is
								// inverted
								inside = ev.primary ? secondaryPolyInverted : primaryPolyInverted;
							}
							else
							{ 
								// otherwise, something is below us
								// so copy the below segment's other polygon's above
								if( ev.primary == below.primary )
									inside = below.seg.otherFill.above;
								else
									inside = below.seg.myFill.above;
							}

							ev.seg.otherFill = new SegmentFill()
							{
								above = inside,
								below = inside
							};
						}
					}

					if( buildLog != null )
					{
						buildLog.status(
							ev.seg,
							above != null ? above.seg : (object)false,
							below != null ? below.seg : (object)false
						);
					}

					// insert the status and remember it for later removal
					ev.other.status = status_root.insert( surrounding, ev );
				}
				else
				{
					var st = ev.status;

					if( st == null )
					{
						throw new Exception( "PolyBool: Zero-length segment detected; your epsilon is probably too small or too large" );
					}

					// removing the status will create two new adjacent edges, so we'll need to check
					// for those
					if( status_root.exists( st.prev ) && status_root.exists( st.next ) )
						checkIntersection( st.prev.ev, st.next.ev );

					if( buildLog != null )
						buildLog.statusRemove( st.ev.seg );

					// remove the status
					st.remove();

					// if we've reached this point, we've calculated everything there is to know, so
					// save the segment for reporting
					if( !ev.primary )
					{
						// make sure `seg.myFill` actually points to the primary polygon though
						var s = ev.seg.myFill;
						ev.seg.myFill = ev.seg.otherFill;
						ev.seg.otherFill = s;
					}

					segments.Add( ev.seg );
				}

				// remove the event and continue
				event_root.head.remove();
			}

			if( buildLog != null )
			{
				buildLog.done();
			}

			return segments;
		}

		#endregion
	}
}
#endif