#if UNITY_EDITOR

using System;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A 2D Shape Editor Segment.</summary>
    [Serializable]
    public partial class Segment : ISelectable
    {
        /// <summary>The position of the segment on the grid.</summary>
        [SerializeField]
        private float2 _position;

        /// <summary>Whether the segment is selected.</summary>
        [NonSerialized]
        private bool _selected;

        /// <summary>The parent shape of the segment.</summary>
        [NonSerialized]
        private Shape _shape;

        /// <summary>Gets or sets the position of the segment on the grid.</summary>
        public float2 position { get => _position; set => _position = value; }

        /// <summary>Gets the parent shape of the segment.</summary>
        public Shape shape { get => _shape; }

        /// <summary>Gets or sets whether the segment is selected.</summary>
        public bool selected { get => _selected; set => _selected = value; }

        /// <summary>The segment type.</summary>
        [SerializeField]
        public SegmentType type = SegmentType.Linear;

        /// <summary>The first bezier pivot (see <see cref="SegmentType.Bezier"/>).</summary>
        [SerializeField]
        public Pivot bezierPivot1 = new Pivot();

        /// <summary>The second bezier pivot (see <see cref="SegmentType.Bezier"/>).</summary>
        [SerializeField]
        public Pivot bezierPivot2 = new Pivot();

        [SerializeField]
        public int bezierDetail = 3;

        /// <summary>Initializes a new instance of the <see cref="Segment"/> class.</summary>
        /// <param name="shape">The parent shape of this segment.</param>
        /// <param name="x">The x-coordinate on the grid.</param>
        /// <param name="y">The y-coordinate on the grid.</param>
        public Segment(Shape shape, float x, float y)
        {
            _shape = shape;
            _position = new float2(x, y);
        }

        /// <summary>Initializes a new instance of the <see cref="Segment"/> class.</summary>
        /// <param name="shape">The parent shape of this segment.</param>
        /// <param name="position">The coordinate on the grid.</param>
        public Segment(Shape shape, float2 position)
        {
            _shape = shape;
            _position = position;
        }
    }
}

#endif