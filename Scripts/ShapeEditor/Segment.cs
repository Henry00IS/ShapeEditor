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

        /// <summary>The parent shape of the segment (read only).</summary>
        [NonSerialized]
        public Shape shape;

        /// <summary>Gets or sets the position of the segment on the grid.</summary>
        public float2 position { get => _position; set => _position = value; }

        /// <summary>Gets or sets whether the segment is selected.</summary>
        public bool selected { get => _selected; set => _selected = value; }

        /// <summary>Gets the previous segment.</summary>
        [NonSerialized]
        public Segment previous;

        /// <summary>Gets the next segment.</summary>
        [NonSerialized]
        public Segment next;

        /// <summary>
        /// The segment generator with the type set to <see cref="SegmentGeneratorType.Linear"/> by
        /// default. Generators define how segments are generated and sometimes add selectable
        /// objects. Never set this to null.
        /// </summary>
        [SerializeField]
        public SegmentGenerator generator;

        /// <summary>General purpose editor variable available to the object with input focus.</summary>
        public float2 gpVector1 { get; set; }

        /// <summary>Initializes a new instance of the <see cref="Segment"/> class.</summary>
        /// <param name="shape">The parent shape of this segment.</param>
        /// <param name="x">The x-coordinate on the grid.</param>
        /// <param name="y">The y-coordinate on the grid.</param>
        public Segment(Shape shape, float x, float y)
        {
            this.shape = shape;
            _position = new float2(x, y);
            generator = new SegmentGenerator(this);
        }

        /// <summary>Initializes a new instance of the <see cref="Segment"/> class.</summary>
        /// <param name="shape">The parent shape of this segment.</param>
        /// <param name="position">The coordinate on the grid.</param>
        public Segment(Shape shape, float2 position)
        {
            this.shape = shape;
            _position = position;
            generator = new SegmentGenerator(this);
        }
    }
}

#endif