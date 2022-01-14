#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor.PolyBoolCS
{
    // contains source code from https://github.com/StagPoint/PolyBoolCS/ (see Licenses/PolyBoolCS.txt).
    // contains source code from https://github.com/velipso/polybooljs (see Licenses/PolyBoolJS.txt).

    public class EventNode
    {
        public bool isStart;
        public Point pt;
        public Segment seg;
        public bool primary;
        public EventNode other;
        public StatusNode status;

        #region Linked List Node

        public EventNode next;
        public EventNode prev;

        public void remove()
        {
            prev.next = next;

            if (next != null)
            {
                next.prev = prev;
            }

            prev = null;
            next = null;
        }

        #endregion Linked List Node

        #region Debugging support

        public override string ToString()
        {
            return string.Format("Start={0}, Point={1}, Segment={2}", isStart, pt, seg);
        }

        #endregion Debugging support
    }

    public class StatusNode
    {
        public EventNode ev;

        #region Linked List Node

        public StatusNode next;
        public StatusNode prev;

        public void remove()
        {
            prev.next = next;

            if (next != null)
            {
                next.prev = prev;
            }

            prev = null;
            next = null;
        }

        #endregion Linked List Node
    }

    public class StatusLinkedList
    {
        #region Fields

        private StatusNode root = new StatusNode();

        #endregion Fields

        #region Public properties

        public bool isEmpty
        { get { return root.next == null; } }

        public StatusNode head
        { get { return root.next; } }

        #endregion Public properties

        #region Public functions

        public bool exists(StatusNode node)
        {
            if (node == null || node == root)
                return false;

            return true;
        }

        public Transition findTransition(EventNode ev)
        {
            var prev = root;
            var here = root.next;

            while (here != null)
            {
                if (findTransitionPredicate(ev, here))
                    break;

                prev = here;
                here = here.next;
            }

            return new Transition()
            {
                before = prev == root ? null : prev.ev,
                after = here != null ? here.ev : null,
                here = here,
                prev = prev
            };
        }

        public StatusNode insert(Transition surrounding, EventNode ev)
        {
            var prev = surrounding.prev;
            var here = surrounding.here;

            var node = new StatusNode() { ev = ev };

            node.prev = prev;
            node.next = here;
            prev.next = node;

            if (here != null)
            {
                here.prev = node;
            }

            return node;
        }

        #endregion Public functions

        #region Private utilty functions

        private bool findTransitionPredicate(EventNode ev, StatusNode here)
        {
            var comp = statusCompare(ev, here.ev);
            return comp > 0;
        }

        private int statusCompare(EventNode ev1, EventNode ev2)
        {
            var a1 = ev1.seg.start;
            var a2 = ev1.seg.end;
            var b1 = ev2.seg.start;
            var b2 = ev2.seg.end;

            if (Epsilon.pointsCollinear(a1, b1, b2))
            {
                if (Epsilon.pointsCollinear(a2, b1, b2))
                    return 1;//eventCompare(true, a1, a2, true, b1, b2);

                return Epsilon.pointAboveOrOnLine(a2, b1, b2) ? 1 : -1;
            }

            return Epsilon.pointAboveOrOnLine(a1, b1, b2) ? 1 : -1;
        }

        #endregion Private utilty functions
    }

    public class EventLinkedList
    {
        #region Fields

        private EventNode root = new EventNode();

        #endregion Fields

        #region Public properties

        public bool isEmpty
        { get { return root.next == null; } }

        public EventNode head
        { get { return root.next; } }

        #endregion Public properties

        #region Public functions

        public void insertBefore(EventNode node, Point other_pt)
        {
            var last = root;
            var here = root.next;

            while (here != null)
            {
                if (insertBeforePredicate(here, node, ref other_pt))
                {
                    node.prev = here.prev;
                    node.next = here;
                    here.prev.next = node;
                    here.prev = node;

                    return;
                }

                last = here;
                here = here.next;
            }

            last.next = node;
            node.prev = last;
            node.next = null;
        }

        #endregion Public functions

        #region Private utility functions

        private bool insertBeforePredicate(EventNode here, EventNode ev, ref Point other_pt)
        {
            // should ev be inserted before here?
            var comp = eventCompare(
                ev.isStart,
                ref ev.pt,
                ref other_pt,
                here.isStart,
                ref here.pt,
                ref here.other.pt
            );

            return comp < 0;
        }

        private int eventCompare(bool p1_isStart, ref Point p1_1, ref Point p1_2, bool p2_isStart, ref Point p2_1, ref Point p2_2)
        {
            // compare the selected points first
            var comp = Epsilon.pointsCompare(p1_1, p2_1);
            if (comp != 0)
                return comp;

            // the selected points are the same

            if (Epsilon.pointsSame(p1_2, p2_2)) // if the non-selected points are the same too...
                return 0; // then the segments are equal

            if (p1_isStart != p2_isStart) // if one is a start and the other isn't...
                return p1_isStart ? 1 : -1; // favor the one that isn't the start

            // otherwise, we'll have to calculate which one is below the other manually
            return Epsilon.pointAboveOrOnLine(
                p1_2,
                p2_isStart ? p2_1 : p2_2, // order matters
                p2_isStart ? p2_2 : p2_1
            ) ? 1 : -1;
        }

        #endregion Private utility functions
    }
}

#endif