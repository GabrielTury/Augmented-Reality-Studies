using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnythingWorld
{
    /// <summary>
    /// This class is used to store the settings for the transform tool.
    /// </summary>
 #if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class TransformSettings : ScriptableObject
    {
        /// <summary>
        /// Returns the instance of the TransformSettings asset.
        /// </summary>
        private static TransformSettings Instance;
        private static bool InstanceInCreation;

        /// <summary>
        /// Checks if the instance of the TransformSettings asset exists, if not it creates a new one.
        /// </summary>
       public static TransformSettings GetInstance()
       {
            if (Instance == null && !InstanceInCreation)
            {
                TransformSettings instance = null;
                string path = "Assets/AnythingWorld/AnythingEditor/Editor/Settings/TransformSettings.asset";
#if UNITY_EDITOR
                instance = AssetDatabase.LoadAssetAtPath<TransformSettings>(path);
#endif
                Instance = instance;
                if (instance == null)
                {
                    Debug.Log("Instance is null, making new TransformSettings file");
                    InstanceInCreation = true;
                    var asset = CreateInstance<TransformSettings>();
#if UNITY_EDITOR
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
#endif
                    Instance = asset;
                    return asset;
                }
            }
            InstanceInCreation = false;
            return Instance;
        }

        [SerializeField] private bool animateModel = true;
        [SerializeField] private bool placeOnGround = true;
        [SerializeField] private bool clickInPlacementLocation = false;
        [SerializeField] private bool continueUntilCancelled = false;
        [SerializeField] private bool loadingImageActive = true;
        [SerializeField] private bool followCam = false;
        [SerializeField] private bool loadingMessage = true;

        [SerializeField] private bool placeOnGrid = true;
        [SerializeField] private bool serializeAsset = false;
        [SerializeField] private bool addCollider = true;
        [SerializeField] private bool addRigidbody = true;
        [SerializeField] private bool showGridHandles = false;
        [SerializeField] private bool repositioning = false;

        [SerializeField] private bool customParentTransformEnabled = false;
        [SerializeField] private bool customPositionFieldEnabled = false;
        [SerializeField] private bool customRotationFieldEnabled = false;
        [SerializeField] private bool customScaleMultiplierEnabled = false;

        [SerializeField] private bool defaultRiggedBehaviourEnabled = true;
        [SerializeField] private bool defaultVehicleBehaviourEnabled = true;
        [SerializeField] private bool defaultFlyingVehicleBehaviourEnabled = true;
        [SerializeField] private bool defaultFlyingAnimalBehaviourEnabled = true;
        [SerializeField] private bool defaultShaderBehaviourEnabled = false;
        [SerializeField] private bool defaultStaticBehaviourEnabled = false;
        [SerializeField] private bool defaultSwimmingAnimalBehaviourEnabled = false;

        [SerializeField] private Vector3 objectPosition;
        [SerializeField] private Vector3 objectRotation;
        [SerializeField] private float objectScaleMultiplier = 1f;
#if UNITY_EDITOR
        [SerializeField] private MonoScript defaultRiggedScript;
        [SerializeField] private MonoScript defaultVehicleScript;
        [SerializeField] private MonoScript defaultFlyingVehicleScript;
        [SerializeField] private MonoScript defaultFlyingAnimalScript;
        [SerializeField] private MonoScript defaultStaticScript;
        [SerializeField] private MonoScript defaultShaderScript;
        [SerializeField] private MonoScript defaultSwimmingAnimalScript;

        [SerializeField] private Vector3 dragPosition;
#endif

        [SerializeField] private Vector3 gridOrigin;
        [SerializeField] private int gridCellCount;
        [SerializeField] private float gridCellWidth;

        [SerializeField] private bool gridAreaEnabled = false;
        [SerializeField] private Vector3 gridAreaOrigin;
        [SerializeField] private Vector2 gridAreaSize;
        [SerializeField] private Vector3 gridAreaForward;
        [SerializeField] private Vector3 gridAreaRight;
        [SerializeField] private Vector3 gridAreaObjectsDistance;
        [SerializeField] private int gridAreaObjectsPerRow;
        [SerializeField] private bool gridAreaFitMode = true;
        [SerializeField] private bool gridAreaClipMode = false;
        [SerializeField] private bool gridAreaCanGrow = false;
        [SerializeField] private bool gridAreaShowPositions = true;
        [SerializeField] private bool gridAreaRandomOffset = false;
        [SerializeField] private bool gridAreaIgnoreCollision = false;

        public static bool AnimateModel { get => GetInstance().animateModel; set => GetInstance().animateModel = value; }
        public static bool PlaceOnGround { get => GetInstance().placeOnGround; set => GetInstance().placeOnGround = value; }
        public static bool PlaceOnGrid { get => GetInstance().placeOnGrid; set => GetInstance().placeOnGrid = value; }
        public static bool ClickInPlacementLocation { get => GetInstance().clickInPlacementLocation; set => GetInstance().clickInPlacementLocation = value; }
        public static bool ContinueUntilCancelled { get => GetInstance().continueUntilCancelled; set => GetInstance().continueUntilCancelled = value; }
        public static bool LoadingImageActive { get => GetInstance().loadingImageActive; set => GetInstance().loadingImageActive = value; }
        public static bool FollowCam { get => GetInstance().followCam; set => GetInstance().followCam = value; }
        public static bool LoadingMessage { get => GetInstance().loadingMessage; set => GetInstance().loadingMessage = value; }


        public static bool SerializeAsset { get => GetInstance().serializeAsset; set => GetInstance().serializeAsset = value; }
        public static bool AddCollider { get => GetInstance().addCollider; set => GetInstance().addCollider = value; }
        public static bool AddRigidbody { get => GetInstance().addRigidbody; set => GetInstance().addRigidbody = value; }
        public static bool ShowGridHandles { get => GetInstance().showGridHandles; set => GetInstance().showGridHandles = value; }
        public static bool Repositioning { get => GetInstance().repositioning; set => GetInstance().repositioning = value; }

        public static bool ParentFieldEnabled { get => GetInstance().customParentTransformEnabled; set => GetInstance().customParentTransformEnabled = value; }
        public static bool PositionFieldEnabled { get => GetInstance().customPositionFieldEnabled; set => GetInstance().customPositionFieldEnabled = value; }
        public static bool RotationFieldEnabled { get => GetInstance().customRotationFieldEnabled; set => GetInstance().customRotationFieldEnabled = value; }
        public static bool ScaleFieldEnabled { get => GetInstance().customScaleMultiplierEnabled; set => GetInstance().customScaleMultiplierEnabled = value; }

        public static bool RiggedBehaviourFieldEnabled { get => GetInstance().defaultRiggedBehaviourEnabled; set => GetInstance().defaultRiggedBehaviourEnabled = value; }
        public static bool VehicleBehaviourFieldEnabled { get => GetInstance().defaultVehicleBehaviourEnabled; set => GetInstance().defaultVehicleBehaviourEnabled = value; }
        public static bool FlyingVehicleBehaviourFieldEnabled { get => GetInstance().defaultFlyingVehicleBehaviourEnabled; set => GetInstance().defaultFlyingVehicleBehaviourEnabled = value; }
        public static bool ShaderBehaviourFieldEnabled { get => GetInstance().defaultShaderBehaviourEnabled; set => GetInstance().defaultShaderBehaviourEnabled = value; }
        public static bool StaticBehaviourFieldEnabled { get => GetInstance().defaultStaticBehaviourEnabled; set => GetInstance().defaultStaticBehaviourEnabled = value; }
        public static bool FlyingAnimalBehaviourFieldEnabled { get => GetInstance().defaultFlyingAnimalBehaviourEnabled; set => GetInstance().defaultFlyingAnimalBehaviourEnabled = value; }
        public static bool SwimmingAnimalBehaviourFieldEnabled { get => GetInstance().defaultSwimmingAnimalBehaviourEnabled; set => GetInstance().defaultSwimmingAnimalBehaviourEnabled = value; }

        public static Vector3 PositionField { get => GetInstance().objectPosition; set => GetInstance().objectPosition = value; }
        public static Vector3 RotationField { get => GetInstance().objectRotation; set => GetInstance().objectRotation = value; }
        public static float ScaleField { get => GetInstance().objectScaleMultiplier; set => GetInstance().objectScaleMultiplier = value; }

#if UNITY_EDITOR
        public static MonoScript RiggedAnimalScript { get => GetInstance().defaultRiggedScript; set => GetInstance().defaultRiggedScript = value; }
        public static MonoScript GroundVehicleScript { get => GetInstance().defaultVehicleScript; set => GetInstance().defaultVehicleScript = value; }
        public static MonoScript FlyingVehicleScript { get => GetInstance().defaultFlyingVehicleScript; set => GetInstance().defaultFlyingVehicleScript = value; }
        public static MonoScript ShaderBehaviourScript { get => GetInstance().defaultShaderScript; set => GetInstance().defaultShaderScript = value; }
        public static MonoScript StaticBehaviourScript { get => GetInstance().defaultStaticScript; set => GetInstance().defaultStaticScript = value; }
        public static MonoScript FlyingAnimalScript { get => GetInstance().defaultFlyingAnimalScript; set => GetInstance().defaultFlyingAnimalScript = value; }
        public static MonoScript SwimmingAnimalScript { get => GetInstance().defaultSwimmingAnimalScript; set => GetInstance().defaultSwimmingAnimalScript = value; }

        public static Vector3 DragPosition { get => GetInstance().dragPosition; set => GetInstance().dragPosition = value; }
#endif

        public static Vector3 GridOrigin { get => GetInstance().gridOrigin; set => GetInstance().gridOrigin = value; }
        public static int GridCellCount { get => GetInstance().gridCellCount; set => GetInstance().gridCellCount = value; }
        public static float GridCellWidth { get => GetInstance().gridCellWidth; set => GetInstance().gridCellWidth = value; }

        public static bool GridAreaEnabled { get => GetInstance().gridAreaEnabled; set => GetInstance().gridAreaEnabled = value; }
        public static Vector3 GridAreaOrigin { get => GetInstance().gridAreaOrigin; set => GetInstance().gridAreaOrigin = value; }
        public static Vector2 GridAreaSize { get => GetInstance().gridAreaSize; set => GetInstance().gridAreaSize = value; }
        public static Vector3 GridAreaForward { get => GetInstance().gridAreaForward; set => GetInstance().gridAreaForward = value; }
        public static Vector3 GridAreaRight { get => GetInstance().gridAreaRight; set => GetInstance().gridAreaRight = value; }
        public static Vector3 GridAreaObjectsDistance { get => GetInstance().gridAreaObjectsDistance; set => GetInstance().gridAreaObjectsDistance = value; }
        public static int GridAreaObjectsPerRow { get => GetInstance().gridAreaObjectsPerRow; set => GetInstance().gridAreaObjectsPerRow = value; }
        public static bool GridAreaFitMode { get => GetInstance().gridAreaFitMode; set => GetInstance().gridAreaFitMode = value; }
        public static bool GridAreaClipMode { get => GetInstance().gridAreaClipMode; set => GetInstance().gridAreaClipMode = value; }
        public static bool GridAreaCanGrow { get => GetInstance().gridAreaCanGrow; set => GetInstance().gridAreaCanGrow = value; }
        public static bool GridAreaShowPositions { get => GetInstance().gridAreaShowPositions; set => GetInstance().gridAreaShowPositions = value; }
        public static bool GridAreaRandomOffset { get => GetInstance().gridAreaRandomOffset; set => GetInstance().gridAreaRandomOffset = value; }
        public static bool GridAreaIgnoreCollision { get => GetInstance().gridAreaIgnoreCollision; set => GetInstance().gridAreaIgnoreCollision = value; }

        /// <summary>
        /// Resets the settings to their default values.
        /// </summary>
        public static void ResetSettings()
        {
            PositionField = Vector3.zero;
            RotationField = Vector3.zero;
            ScaleField = 1f;
            ContinueUntilCancelled = false;
            ClickInPlacementLocation = true;
            LoadingImageActive = true;
            FollowCam = false;
            LoadingMessage = true;
            
            ParentFieldEnabled = false;
            PositionFieldEnabled = false;
            RotationFieldEnabled = false;
            ScaleFieldEnabled = false;

            AnimateModel = true;
            PlaceOnGround = true;
            PlaceOnGrid = false;
            SerializeAsset = false;
            AddCollider = true;
            AddRigidbody = true;
            ShowGridHandles = false;
            Repositioning = true;
#if UNITY_EDITOR
            GroundVehicleScript = null;
            FlyingVehicleScript = null;
            RiggedAnimalScript = null;
            StaticBehaviourScript = null;
            ShaderBehaviourScript = null;
            DragPosition = Vector3.zero;
#endif
            VehicleBehaviourFieldEnabled = false;
            FlyingVehicleBehaviourFieldEnabled = false;
            RiggedBehaviourFieldEnabled = false;
            StaticBehaviourFieldEnabled = false;
            ShaderBehaviourFieldEnabled = false;
            FlyingAnimalBehaviourFieldEnabled = false;

            GridOrigin = Vector3.zero;
            GridCellCount = 10;
            GridCellWidth = 1f;

            GridAreaEnabled = false;
            GridAreaOrigin = Vector3.zero;
            GridAreaSize = Vector2.one;
            GridAreaForward = new Vector3(0, 0, 1);
            GridAreaRight = new Vector3(1, 0, 0);
            GridAreaObjectsDistance = new Vector3(1, 0, 1);
            GridAreaObjectsPerRow = 10;
            GridAreaFitMode = true;
            GridAreaClipMode = false;
            GridAreaCanGrow = false;
            GridAreaShowPositions = false;
            GridAreaRandomOffset = false;
            GridAreaIgnoreCollision = false;
         }
    }
}
