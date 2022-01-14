#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor.PolyBoolCS
{
    // contains source code from https://github.com/StagPoint/PolyBoolCS/ (see Licenses/PolyBoolCS.txt).
    // contains source code from https://github.com/velipso/polybooljs (see Licenses/PolyBoolJS.txt).

    using System;

    public class PolyBool
    {
        #region Public properties

        public BuildLog BuildLog
        {
            get { return _log; }
            set { _log = value; }
        }

        #endregion Public properties

        #region Private fields

        private BuildLog _log;

        #endregion Private fields

        #region Core api

        public SegmentList segments(Polygon poly)
        {
            var i = new Intersecter(true, _log);

            foreach (var region in poly.regions)
            {
                i.addRegion(region);
            }

            var result = i.calculate(poly.inverted);
            result.inverted = poly.inverted;

            return result;
        }

        public CombinedSegmentLists combine(SegmentList segments1, SegmentList segments2)
        {
            var i = new Intersecter(false, _log);

            return new CombinedSegmentLists()
            {
                combined = i.calculate(
                    segments1, segments1.inverted,
                    segments2, segments2.inverted
                ),
                inverted1 = segments1.inverted,
                inverted2 = segments2.inverted
            };
        }

        public SegmentList selectUnion(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.union(combined.combined, _log);
            result.inverted = combined.inverted1 || combined.inverted2;

            return result;
        }

        public SegmentList selectIntersect(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.intersect(combined.combined, _log);
            result.inverted = combined.inverted1 && combined.inverted2;

            return result;
        }

        public SegmentList selectDifference(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.difference(combined.combined, _log);
            result.inverted = combined.inverted1 && !combined.inverted2;

            return result;
        }

        public SegmentList selectDifferenceRev(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.differenceRev(combined.combined, _log);
            result.inverted = !combined.inverted1 && combined.inverted2;

            return result;
        }

        public SegmentList selectXor(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.xor(combined.combined, _log);
            result.inverted = combined.inverted1 != combined.inverted2;

            return result;
        }

        public Polygon polygon(SegmentList segments)
        {
            var chain = new SegmentChainer().chain(segments, _log);

            return new Polygon()
            {
                regions = chain,
                inverted = segments.inverted
            };
        }

        #endregion Core api

        #region Helper functions for common operations

        public Polygon union(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectUnion);
        }

        public Polygon intersect(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectIntersect);
        }

        public Polygon difference(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectDifference);
        }

        public Polygon differenceRev(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectDifferenceRev);
        }

        public Polygon xor(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectXor);
        }

        #endregion Helper functions for common operations

        #region Private utility functions

        private Polygon operate(Polygon poly1, Polygon poly2, Func<CombinedSegmentLists, SegmentList> selector)
        {
            var seg1 = segments(poly1);
            var seg2 = segments(poly2);
            var comb = combine(seg1, seg2);

            var seg3 = selector(comb);

            return polygon(seg3);
        }

        #endregion Private utility functions
    }
}

#endif