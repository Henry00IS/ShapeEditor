#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    [CreateAssetMenu(fileName = "ShapeEditorResources", menuName = "ScriptableObjects/ShapeEditorResources", order = 1)]
    public class ShapeEditorResources : ScriptableObject
    {
        private static ShapeEditorResources s_Instance;

        /// <summary>Gets the singleton shape editor resources instance or creates it.</summary>
        public static ShapeEditorResources Instance
        {
            get
            {
                // if known, immediately return the instance.
                if (s_Instance) return s_Instance;

                // load the shape editor resources from the resources directory.
                LoadResources();

                return s_Instance;
            }
        }

        /// <summary>
        /// Before the first scene loads, we access the instance property to load all resources.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void BeforeSceneLoad()
        {
            // load the shape editor resources from the resources directory.
            LoadResources();
        }

        /// <summary>Loads the shape editor resources from the resources directory.</summary>
        private static void LoadResources()
        {
            s_Instance = (ShapeEditorResources)Resources.Load("ShapeEditorResources");
        }

        public Texture2D shapeEditorAbout;
        public Texture2D shapeEditorAboutPatreon;
        public Texture2D shapeEditorCreatePolygon;
        public Texture2D shapeEditorCut;
        public Texture2D shapeEditorDelete;
        public Texture2D shapeEditorEdgeSelect;
        public Texture2D shapeEditorExtrudeBevel;
        public Texture2D shapeEditorExtrudeCurvedStaircase;
        public Texture2D shapeEditorExtrudeFixed;
        public Texture2D shapeEditorExtrudeLinearStaircase;
        public Texture2D shapeEditorExtrudePoint;
        public Texture2D shapeEditorExtrudeRevolve;
        public Texture2D shapeEditorExtrudeRevolveChopped;
        public Texture2D shapeEditorExtrudeShape;
        public Texture2D shapeEditorExtrudeSlope;
        public Texture2D shapeEditorExtrudeSpiral;
        public Texture2D shapeEditorExtrudeSpline;
        public Texture2D shapeEditorFaceSelect;
        public Texture2D shapeEditorFlipHorizontally;
        public Texture2D shapeEditorFlipVertically;
        public Texture2D shapeEditorHome;
        public Texture2D shapeEditorIcon;
        public Texture2D shapeEditorMouseCursorHand;
        public Texture2D shapeEditorMouseCursorScissors;
        public Texture2D shapeEditorNew;
        public Texture2D shapeEditorOpen;
        public Texture2D shapeEditorRestore;
        public Texture2D shapeEditorRotate;
        public Texture2D shapeEditorRotate90Left;
        public Texture2D shapeEditorRotate90Right;
        public Texture2D shapeEditorSave;
        public Texture2D shapeEditorScale;
        public Texture2D shapeEditorSegmentApply;
        public Texture2D shapeEditorSegmentArch;
        public Texture2D shapeEditorSegmentBezier;
        public Texture2D shapeEditorSegmentBezierDetail;
        public Texture2D shapeEditorSegmentDuplicateWarning;
        public Texture2D shapeEditorSegmentExtrude;
        public Texture2D shapeEditorSegmentInsert;
        public Texture2D shapeEditorSegmentLinear;
        public Texture2D shapeEditorSegmentRepeat;
        public Texture2D shapeEditorSegmentSine;
        public Texture2D shapeEditorSelectBox;
        public Texture2D shapeEditorShapeCreate;
        public Texture2D shapeEditorShapeDuplicate;
        public Texture2D shapeEditorSnapping;
        public Texture2D shapeEditorSnappingDisabled;
        public Texture2D shapeEditorTranslate;
        public Texture2D shapeEditorVertexSelect;
        public Texture2D shapeEditorZoomIn;
        public Texture2D shapeEditorZoomOut;

        public Texture2D shapeEditorArchSegmentRound;
        public Texture2D shapeEditorArchSegmentSegmental;
        public Texture2D shapeEditorArchSegmentHorseshoe;
        public Texture2D shapeEditorArchSegmentPointedHorseshoe;
        public Texture2D shapeEditorArchSegmentThreeCentered;
        public Texture2D shapeEditorArchSegmentPseudoThreeCentered;
        public Texture2D shapeEditorArchSegmentPseudoFourCentered;
        public Texture2D shapeEditorArchSegmentTriangular;
        public Texture2D shapeEditorArchSegmentRoundedHorseshoe;
        public Texture2D shapeEditorArchSegmentThreePointed;
        public Texture2D shapeEditorArchSegmentPointedSegmental;
        public Texture2D shapeEditorArchSegmentParabolic;
        public Texture2D shapeEditorArchSegmentInflexed;
        public Texture2D shapeEditorArchSegmentRoundRampant;
        public Texture2D shapeEditorArchSegmentRampant;
        public Texture2D shapeEditorArchSegmentFourCentered;
        public Texture2D shapeEditorArchSegmentKeyhole;
        public Texture2D shapeEditorArchSegmentReverseOgee;
        public Texture2D shapeEditorArchSegmentOgeeThreeCentered;
        public Texture2D shapeEditorArchSegmentOgeeFourCentered;
        public Texture2D shapeEditorArchSegmentOriental;
        public Texture2D shapeEditorArchSegmentRoundTrefoil;
        public Texture2D shapeEditorArchSegmentShouldered;
        public Texture2D shapeEditorArchSegmentPointedTrefoil;
        public Texture2D shapeEditorArchSegmentDraped;
        public Texture2D shapeEditorArchSegmentDoubleDraped;
        public Texture2D shapeEditorArchSegmentCinquefoil;
        public Texture2D shapeEditorArchSegmentPointedCinquefoil;
        public Texture2D shapeEditorArchSegmentMultifoil;
        public Texture2D shapeEditorArchSegmentRoundRampantMirrored;
        public Texture2D shapeEditorArchSegmentRampantMirrored;

        public TextAsset shapeEditorFontSegoeUI14Xml;
        public Texture2D shapeEditorFontSegoeUI14;

        public Material shapeEditorDefaultMaterial;
        public Material shapeEditorGuiMaterial;

        private static Material _temporaryGuiMaterial;

        public static Material temporaryGuiMaterial
        {
            get
            {
                if (!_temporaryGuiMaterial)
                    _temporaryGuiMaterial = new Material(Instance.shapeEditorGuiMaterial);
                return _temporaryGuiMaterial;
            }
        }

        private static GUIStyle _toolbarStyle;

        public static GUIStyle toolbarStyle
        {
            get
            {
                if (_toolbarStyle == null)
                    _toolbarStyle = new GUIStyle(EditorStyles.toolbar);
                return _toolbarStyle;
            }
        }

        private static GUIStyle _toolbarButtonStyle;

        public static GUIStyle toolbarButtonStyle
        {
            get
            {
                if (_toolbarButtonStyle == null)
                    _toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
                return _toolbarButtonStyle;
            }
        }

        private static BmFont _fontSegoeUI14;

        public static BmFont fontSegoeUI14
        {
            get
            {
                if (_fontSegoeUI14 == null)
                    _fontSegoeUI14 = new BmFont(Instance.shapeEditorFontSegoeUI14Xml, Instance.shapeEditorFontSegoeUI14);
                return _fontSegoeUI14;
            }
        }
    }
}

#endif