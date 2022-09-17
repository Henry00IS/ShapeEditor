#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The inspector window with context sensitive properties.</summary>
    public class ArchInspectorWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(170, 225);
        private GuiFloatTextbox archDetailTextbox;
        private GuiButton flipDirectionButton;
        private bool archFlipDirection;
        private int archDetail = 8;
        private GuiFloatTextbox archGridSnapSizeTextbox;
        private float archGridSnapSize = 0f;
        private SegmentGenerator.ArchMode archMode;

        public ArchInspectorWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetBottomRightPosition();

            Add(new GuiWindowTitle("Arch Inspector"));

            var layout = new GuiTableLayout(this, 5, 24);

            layout.Add(new GuiLabel("Arch Mode"));

            layout.NextRow(1);

            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Round), 20, ApplyRoundArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Segmental), 20, ApplySegmentalArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Horseshoe), 20, ApplyHorseshoeArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.PointedHorseshoe), 20, ApplyPointedHorseshoeArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.ThreeCentered), 20, ApplyThreeCenteredArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.PseudoThreeCentered), 20, ApplyPseudoThreeCenteredArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.PseudoFourCentered), 20, ApplyPseudoFourCenteredArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Triangular), 20, ApplyTriangularArchToSelectedEdges));
            layout.NextRow();
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.RoundedHorseshoe), 20, ApplyRoundedHorseshoeArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.ThreePointed), 20, ApplyThreePointedArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.PointedSegmental), 20, ApplyPointedSegmentalArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Inflexed), 20, ApplyInflexedArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.RoundRampant), 20, ApplyRoundRampantArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.RoundRampantMirrored), 20, ApplyRoundRampantMirroredArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Rampant), 20, ApplyRampantArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.RampantMirrored), 20, ApplyRampantMirroredArchToSelectedEdges));
            layout.NextRow();
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Parabolic), 20, ApplyParabolicArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.FourCentered), 20, ApplyFourCenteredArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Keyhole), 20, ApplyKeyholeArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.ReverseOgee), 20, ApplyReverseOgeeArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.OgeeThreeCentered), 20, ApplyOgeeThreeCenteredArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.OgeeFourCentered), 20, ApplyOgeeFourCenteredArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Oriental), 20, ApplyOrientalArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.RoundTrefoil), 20, ApplyRoundTrefoilArchToSelectedEdges));
            layout.NextRow();
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Shouldered), 20, ApplyShoulderedArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.PointedTrefoil), 20, ApplyPointedTrefoilArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Draped), 20, ApplyDrapedArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.DoubleDraped), 20, ApplyDoubleDrapedArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Cinquefoil), 20, ApplyCinquefoilArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.PointedCinquefoil), 20, ApplyPointedCinquefoilArchToSelectedEdges));
            layout.Add(new GuiButton(GetArchTexture(SegmentGenerator.ArchMode.Multifoil), 20, ApplyMultifoilArchToSelectedEdges));

            layout.NextRow(4);

            layout.Add(new GuiLabel("Flip Direction"));
            layout.Space(84);
            layout.Add(flipDirectionButton = new GuiButton(resources.shapeEditorFlipVertically, new float2(16f, 16f), ToggleFlipDirection));
            layout.Add(new GuiButton("Apply", new float2(40, 16), ApplyArchFlipDirectionToSelectedEdges));

            layout.NextRow(4);

            layout.Add(new GuiLabel("Detail Level"));
            layout.Space(60);
            layout.Add(archDetailTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, minValue = 1 });
            layout.Add(new GuiButton("Apply", new float2(40, 16), ApplyArchDetailToSelectedEdges));

            layout.NextRow(1);

            for (int y = 0; y < 2; y++)
            {
                for (int x = 1; x <= 8; x++)
                {
                    var i = (y * 8) + x;
                    layout.Add(new GuiButton(i.ToString(), 20, () =>
                    {
                        archDetail = i;
                        ApplyArchDetailToSelectedEdges();
                    }));
                }
                layout.NextRow();
            }

            layout.NextRow(4);

            layout.Add(new GuiLabel("Grid Snap Size"));
            layout.Space(60);
            layout.Add(archGridSnapSizeTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, minValue = 0f });
            layout.Add(new GuiButton("Apply", new float2(40, 16), ApplyGridSnapSizeToSelectedEdges));
        }

        private float2 GetBottomRightPosition()
        {
            return new float2(
                Mathf.RoundToInt(editor.position.width - windowSize.x - 20),
                Mathf.RoundToInt(editor.position.height - windowSize.y - 42)
            );
        }

        public override void OnRender()
        {
            flipDirectionButton.isChecked = archFlipDirection;
            archDetail = Mathf.RoundToInt(archDetailTextbox.UpdateValue(archDetail));
            archGridSnapSize = archGridSnapSizeTextbox.UpdateValue(archGridSnapSize);

            base.OnRender();
        }

        [Instructions(title: "Applies the specified arch detail level, which generates more vertices.")]
        private void ApplyArchDetailToSelectedEdges()
        {
            editor.RegisterUndo("Apply Arch Detail");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Arch)
                    segment.generator.archDetail = archDetail;
        }

        [Instructions(title: "Applies the specified grid size, which snaps all generated vertices to it (set to 0 to disable).")]
        private void ApplyGridSnapSizeToSelectedEdges()
        {
            editor.RegisterUndo("Apply Arch Grid Snap Size");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Arch)
                    segment.generator.archGridSnapSize = archGridSnapSize;
        }

        [Instructions(title: "Whether the arch will be flipped, which will extrude the arch in the other direction.")]
        private void ToggleFlipDirection()
        {
            archFlipDirection = !archFlipDirection;
        }

        [Instructions(title: "Applies the specified flip direction, which extrudes the arch in the other direction.")]
        private void ApplyArchFlipDirectionToSelectedEdges()
        {
            editor.RegisterUndo("Apply Arch Flip Direction");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Arch)
                    segment.generator.archFlipped = archFlipDirection;
        }

        private void ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode archMode)
        {
            editor.RegisterUndo("Apply Arch Mode");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Arch)
                    segment.generator.archMode = archMode;
        }

        [Instructions(title: "Round Arch", description: "An arch formed in a continuous curve, especially in a semicircle. The round arch was the foundation of Rome's architectural mastery and the enormous extent of building projects throughout the ancient world.")]
        private void ApplyRoundArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Round);

        [Instructions(title: "Segmental Arch", description: "This is considered a basic type of arch, and it is used for buildings where the center of the arch lies below the springing line. In a segmental arch, the thrust transfers in an inclined direction all the way to the abutment. Considered one of the strongest arches available, it is able to resist thrust. It also must have a rise equal to a minimum of one-eighth of the span's width in order to prevent failure.")]
        private void ApplySegmentalArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Segmental);

        [Instructions(title: "Horseshoe Arch", description: "Like its name suggests, this arch is in the shape of a horseshoe that is more curved than semi-circle. It is most often considered for architectural provisions.")]
        private void ApplyHorseshoeArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Horseshoe);

        [Instructions(title: "Pointed Horseshoe Arch", description: "Like its name suggests, this arch is in the shape of a horseshoe that is more curved than a semi-circle. It is most often considered for architectural provisions.")]
        private void ApplyPointedHorseshoeArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.PointedHorseshoe);

        [Instructions(title: "Three-Centered Arch", description: "A round arch whose inner curve is drawn with circles having three centers. They are strong and can be found in aqueducts and bridges. Their strength comes from the consistent shape of the voussoirs, because each one is identical in taper to the one next to it.")]
        private void ApplyThreeCenteredArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.ThreeCentered);

        [Instructions(title: "Pseudo Three-Centered Arch", description: "Similar to the three-centered arch except that the top has been flattened.")]
        private void ApplyPseudoThreeCenteredArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.PseudoThreeCentered);

        [Instructions(title: "Pseudo Four-Centered Arch", description: "A four-centred arch, also known as a depressed arch or Tudor arch, is a low, wide type of arch with a pointed apex. It is much wider than its height and gives the visual effect of having been flattened under pressure.")]
        private void ApplyPseudoFourCenteredArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.PseudoFourCentered);

        [Instructions(title: "Triangular Arch", description: "An arch often formed by two large diagonal stones that mutually support each other to span an opening, also called a miter arch. It occurs in Anglo-Saxon architecture and technically may not be an arch at all.")]
        private void ApplyTriangularArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Triangular);

        [Instructions(title: "Rounded Horseshoe Arch", description: "Like its name suggests, this arch is in the shape of a horseshoe that is more curved than a semi-circle. It is most often considered for architectural provisions.")]
        private void ApplyRoundedHorseshoeArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.RoundedHorseshoe);

        [Instructions(title: "Three-Pointed Arch", description: "A pointed arch, ogival arch, or Gothic arch is an arch with a pointed crown, whose two curving sides meet at a relatively sharp angle at the top of the arch.")]
        private void ApplyThreePointedArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.ThreePointed);

        [Instructions(title: "Pointed Segmental Arch", description: "The pointed segmental arch grants a sense of height and loftiness about it that can be useful to help structures appear taller and accentuate height.")]
        private void ApplyPointedSegmentalArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.PointedSegmental);

        [Instructions(title: "Parabolic Arch", description: "This arch is shaped like a parabola, their curve represents an efficient method of load and they are used in bridges, cathedrals and many other areas.")]
        private void ApplyParabolicArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Parabolic);

        [Instructions(title: "Inflexed Arch", description: "An arch that is formed by two arcs curving inwards.")]
        private void ApplyInflexedArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Inflexed);

        [Instructions(title: "Round Rampant Arch", description: "An arch whose support is higher on one side than on the other.")]
        private void ApplyRoundRampantArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.RoundRampant);

        [Instructions(title: "Rampant Arch", description: "A pointed arch whose support is higher on one side than on the other.")]
        private void ApplyRampantArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Rampant);

        [Instructions(title: "Four-Centered Arch", description: "A four-centered arch is a low, wide type of arch with a pointed apex and is widely used in Islamic architecture.")]
        private void ApplyFourCenteredArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.FourCentered);

        [Instructions(title: "Keyhole Arch", description: "Like its name suggests, this arch is in the shape of a horseshoe that is more curved than a semi-circle, it resembles the top part of a keyhole. It is most often considered for architectural provisions.")]
        private void ApplyKeyholeArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Keyhole);

        [Instructions(title: "Reverse Ogee Arch", description: "An arch composed of two opposing s-shaped curves, convex at the top and concave at the bottom.")]
        private void ApplyReverseOgeeArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.ReverseOgee);

        [Instructions(title: "Ogee Three-Centered Arch", description: "An arch composed of two opposing s-shaped curves, concave at the top with a pointed apex and convex at the bottom.")]
        private void ApplyOgeeThreeCenteredArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.OgeeThreeCentered);

        [Instructions(title: "Ogee Four-Centered Arch", description: "An arch composed of two opposing s-shaped curves, concave at the top with a pointed apex and convex at the bottom.")]
        private void ApplyOgeeFourCenteredArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.OgeeFourCentered);

        [Instructions(title: "Oriental Arch", description: "An arch composed of two opposing curves, first curving outwards and then have a straight line to a pointed apex, widely used in Asian architecture.")]
        private void ApplyOrientalArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Oriental);

        [Instructions(title: "Round Trefoil Arch", description: "This arch incorporates the shape or outline of a trefoil - three overlapping circles. Trefoil arches are common in Gothic architecture for portals and decoration.")]
        private void ApplyRoundTrefoilArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.RoundTrefoil);

        [Instructions(title: "Shouldered Arch", description: "The jack arch is a structural element used in masonry construction and provides support at the openings in the masonry. Unlike other arches, these arches are not semi-circular in form. They are flat in profile and used under the same circumstances as lintels.")]
        private void ApplyShoulderedArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Shouldered);

        [Instructions(title: "Pointed Trefoil Arch", description: "A pointed arch having cusps in the intrados on either side of the apex.")]
        private void ApplyPointedTrefoilArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.PointedTrefoil);

        [Instructions(title: "Draped Arch", description: "A flat top with a curve draping off once on both sides.")]
        private void ApplyDrapedArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Draped);

        [Instructions(title: "Double Draped Arch", description: "A flat top with two curves draping off on both sides.")]
        private void ApplyDoubleDrapedArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.DoubleDraped);

        [Instructions(title: "Cinquefoil Arch", description: "A design having five sides composed of converging arcs, usually used as a frame for glass or a panel.")]
        private void ApplyCinquefoilArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Cinquefoil);

        [Instructions(title: "Pointed Cinquefoil Arch", description: "A design having five sides composed of converging arcs with a pointed apex at the top, usually used as a frame for glass or a panel.")]
        private void ApplyPointedCinquefoilArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.PointedCinquefoil);

        [Instructions(title: "Multifoil Arch", description: "A multifoil arch is characterized by multiple circular arcs or leaf shapes that are cut into its interior profile or intrados. The term foil comes from the old French word for leaf.")]
        private void ApplyMultifoilArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.Multifoil);

        [Instructions(title: "Round Rampant Arch (Mirrored)", description: "An arch whose support is higher on one side than on the other.")]
        private void ApplyRoundRampantMirroredArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.RoundRampantMirrored);

        [Instructions(title: "Rampant Arch (Mirrored)", description: "A pointed arch whose support is higher on one side than on the other.")]
        private void ApplyRampantMirroredArchToSelectedEdges() => ApplyArchModeToSelectedEdges(SegmentGenerator.ArchMode.RampantMirrored);

        private Texture2D GetArchTexture(SegmentGenerator.ArchMode archMode)
        {
            switch (archMode)
            {
                case SegmentGenerator.ArchMode.Round:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentRound;

                case SegmentGenerator.ArchMode.Segmental:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentSegmental;

                case SegmentGenerator.ArchMode.Horseshoe:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentHorseshoe;

                case SegmentGenerator.ArchMode.PointedHorseshoe:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentPointedHorseshoe;

                case SegmentGenerator.ArchMode.ThreeCentered:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentThreeCentered;

                case SegmentGenerator.ArchMode.PseudoThreeCentered:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentPseudoThreeCentered;

                case SegmentGenerator.ArchMode.PseudoFourCentered:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentPseudoFourCentered;

                case SegmentGenerator.ArchMode.Triangular:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentTriangular;

                case SegmentGenerator.ArchMode.RoundedHorseshoe:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentRoundedHorseshoe;

                case SegmentGenerator.ArchMode.ThreePointed:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentThreePointed;

                case SegmentGenerator.ArchMode.PointedSegmental:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentPointedSegmental;

                case SegmentGenerator.ArchMode.Parabolic:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentParabolic;

                case SegmentGenerator.ArchMode.Inflexed:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentInflexed;

                case SegmentGenerator.ArchMode.RoundRampant:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentRoundRampant;

                case SegmentGenerator.ArchMode.Rampant:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentRampant;

                case SegmentGenerator.ArchMode.FourCentered:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentFourCentered;

                case SegmentGenerator.ArchMode.Keyhole:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentKeyhole;

                case SegmentGenerator.ArchMode.ReverseOgee:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentReverseOgee;

                case SegmentGenerator.ArchMode.OgeeThreeCentered:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentOgeeThreeCentered;

                case SegmentGenerator.ArchMode.OgeeFourCentered:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentOgeeFourCentered;

                case SegmentGenerator.ArchMode.Oriental:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentOriental;

                case SegmentGenerator.ArchMode.RoundTrefoil:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentRoundTrefoil;

                case SegmentGenerator.ArchMode.Shouldered:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentShouldered;

                case SegmentGenerator.ArchMode.PointedTrefoil:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentPointedTrefoil;

                case SegmentGenerator.ArchMode.Draped:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentDraped;

                case SegmentGenerator.ArchMode.DoubleDraped:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentDoubleDraped;

                case SegmentGenerator.ArchMode.Cinquefoil:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentCinquefoil;

                case SegmentGenerator.ArchMode.PointedCinquefoil:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentPointedCinquefoil;

                case SegmentGenerator.ArchMode.Multifoil:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentMultifoil;

                case SegmentGenerator.ArchMode.RoundRampantMirrored:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentRoundRampantMirrored;

                case SegmentGenerator.ArchMode.RampantMirrored:
                    return ShapeEditorResources.Instance.shapeEditorArchSegmentRampantMirrored;
            }

            return ShapeEditorResources.Instance.shapeEditorArchSegmentRound;
        }
    }
}

#endif