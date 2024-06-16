using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnythingWorld
{
    [Obsolete("ParamBuilder has been deprecated, use new class RequestParameter")]
    public static class ParamBuilder{}

    /// <summary>
    /// Static class accepts parameter values and packages them in RequestParam classes and returns to user.
    /// Allows us to accept a variable sized array of parameters of different types from the user.
    /// Accepted values are accepted after being passed to Factory by packing them into an object, this class is then reset to base values.
    /// </summary>
    public static class RequestParameter
    {
        internal static Utilities.ScaleType _scaleType = Utilities.ScaleType.SetRealWorld;
        internal static Utilities.TransformSpace _transformSpace = Utilities.TransformSpace.Local;
        internal static DefaultBehaviourPreset _defaultBehaviourPreset = null;
        internal static bool _useLegacyAnimatorInEditor = false;
        internal static bool _addCollider = false;
        internal static bool _addRigidbody = false;
        internal static bool _clampDbScale = false;
        internal static bool _serializeAsset = false;
        internal static bool _animateModel = true;
        internal static bool _placeOnGrid = false;
        internal static bool _placeOnGround = false;
        internal static bool _useGridArea = false;
        internal static bool _setDefaultBehaviourPreset = true;
        internal static Vector3Param _clampDbScaleUpperBounds = new Vector3Param();
        internal static Vector3Param _clampDbScaleLowerBounds = new Vector3Param();
        internal static Vector3Param _position = new Vector3Param();
        internal static Quaternion _rotation = Quaternion.identity;
        internal static Vector3Param _scale = new Vector3Param();
        internal static float _scaleMultiplier = 1;
        internal static Transform _parentTransform = null;
        internal static Action _onSuccessAction = null;
        internal static Action _onFailAction = null;
        internal static Action<CallbackInfo> _onSuccessActionCallback = null;
        internal static Action<CallbackInfo> _onFailActionCallback = null;
        internal static Type[] _behaviours = null;
        internal static Dictionary<DefaultBehaviourType, Type> _categorizedBehaviours = null;

        /// <summary>
        /// Package user inputs into object and reset Request class.
        /// </summary>
        /// <returns>RequestParamObject holding user parameter inputs.</returns>
        internal static RequestParamObject Fetch()
        {
            var rPO = new RequestParamObject
            {
                scaleType = _scaleType,
                transformSpace = _transformSpace,
                defaultBehaviourPreset = _defaultBehaviourPreset,
                useLegacyAnimatorInEditor = _useLegacyAnimatorInEditor,
                addCollider = _addCollider,
                addRigidbody = _addRigidbody,
                clampDbScale = _clampDbScale,
                serializeAsset = _serializeAsset,
                animateModel = _animateModel,
                placeOnGrid = _placeOnGrid,
                placeOnGround = _placeOnGround,
                useGridArea = _useGridArea,
                setDefaultBehaviourPreset = _setDefaultBehaviourPreset,
                clampDbScaleUpperBounds = _clampDbScaleUpperBounds,
                clampDbScaleLowerBounds = _clampDbScaleLowerBounds,
                position = _position,
                rotation = _rotation,
                scale = _scale,
                scaleMultiplier = _scaleMultiplier,
                parentTransform = _parentTransform,
                onSuccessAction = _onSuccessAction,
                onFailAction = _onFailAction,
                onSuccessActionCallback = _onSuccessActionCallback,
                onFailActionCallback = _onFailActionCallback,
                behaviours = _behaviours,
                categorizedBehaviours = _categorizedBehaviours
            };
            Reset();
            return rPO;
        }

        /// <summary>
        /// Reset all fields in this class.
        /// </summary>
        internal static void Reset()
        {
            _scaleType = AnythingWorld.Utilities.ScaleType.SetRealWorld;
            _transformSpace = Utilities.TransformSpace.Local;
            _defaultBehaviourPreset = null;
            _useLegacyAnimatorInEditor = false;
            _addCollider = false;
            _addRigidbody = false;
            _clampDbScale = false;
            _serializeAsset = false;
            _animateModel = true;
            _placeOnGrid = false;
            _placeOnGround = false;
            _useGridArea = false;
            _setDefaultBehaviourPreset = true;
            _clampDbScaleUpperBounds = new Vector3Param();
            _clampDbScaleLowerBounds = new Vector3Param();
            _position = new Vector3Param();
            _rotation = Quaternion.identity;
            _scale = new Vector3Param();
            _scaleMultiplier = 1;
            _parentTransform = null;
            _onSuccessAction = null;
            _onFailAction = null;
            _onSuccessActionCallback = null;
            _onFailActionCallback = null;
            _behaviours = null;
            _setDefaultBehaviourPreset = true;
            _categorizedBehaviours = null;
        }

        /// <summary>
        /// Set type of scaling operation to apply to model.
        /// </summary>
        /// <param name="scaleType"></param>
        /// <returns></returns>
        public static RequestParameterOption ScaleType(Utilities.ScaleType scaleType)
        {
            return new ScaleType(scaleType);
        }


        public static RequestParameterOption UseLegacyAnimatorInEditor(bool value)
        {
            return new LegacyAnimatorInEditorOption(value);
        }
        /// <summary>
        /// If using scales from DB, clamp the scale between certain values. Defaults to unclamped.
        /// </summary>
        /// <returns></returns>
        public static RequestParameterOption ClampDatabaseScale(Vector3 lowerBound, Vector3 upperBound)
        {
            return new ClampScale(lowerBound, upperBound);
        }
        /// <summary>
        /// If using scales from DB, clamp the scale between certain values. Defaults to unclamped.
        /// </summary>
        /// <returns></returns>
        public static RequestParameterOption ClampDatabaseScale(Vector3 upperBound)
        {
            return new ClampScale(Vector3.zero, upperBound);
        }
        /// <summary>
        /// Finds first instance of RuntimeDefaultBehaviours and applies to model.
        /// </summary>
        /// <returns></returns>
        public static RequestParameterOption SetDefaultBehaviour(bool addDefault = true)
        {
            return new DefaultBehaviour(addDefault);
        }
        /// <summary>
        /// Specify a RuntimeDefaultBehaviour instance to apply to this model.
        /// </summary>
        /// <param name="behaviourPreset"></param>
        /// <returns></returns>
        public static RequestParameterOption SetDefaultBehaviourPreset(DefaultBehaviourPreset behaviourPreset)
        {
            return new DefaultBehaviour(behaviourPreset);
        }
        /// <summary>
        /// Specify which transform space input rotation and position will be applied to.
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public static RequestParameterOption TransformSpace(Utilities.TransformSpace space)
        {
            return new TransformSpace(space);
        }

        /// <summary>
        /// Parent the model to a parent transform.
        /// </summary>
        /// <param name="parentTransform">Transform that model will be parented to.</param>
        public static RequestParameterOption Parent(Transform parentTransform)
        {
            return new ParentTransform(parentTransform);
        }
        /// <summary>
        /// Request model with animation system if available.
        /// </summary>
        public static RequestParameterOption IsAnimated(bool value)
        {
            return new AnimateModel(value);
        }
        /// <summary>
        /// Add collider around object that encloses object mesh(es).
        /// </summary>
        /// <returns></returns>
        public static AddCollider AddCollider()
        {
            return new AddCollider();
        }
        /// <summary>
        /// Add collider around object that encloses object mesh(es).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AddCollider AddCollider(bool value)
        {
            return new AddCollider(value);
        }
        /// <summary>
        /// Add Rigidbody to object.
        /// </summary>
        /// <returns></returns>
        public static AddRigidbody AddRigidbody()
        {
            return new AddRigidbody();
        }
        /// <summary>
        /// Add Rigidbody to object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AddRigidbody AddRigidbody(bool value)
        {
            return new AddRigidbody(value);
        }

        /// <summary>
        /// Specify an array of behaviour scripts to be added to model on completion.
        /// </summary>
        /// <param name="types">Must be script deriving from Monobehaviour.</param>
        /// <returns></returns>
        public static RequestParameterOption AddScripts(params Type[] types)
        {
            var monoBehaviourType = typeof(MonoBehaviour);

            foreach (var type in types)
            {
                if (type.IsSubclassOf(monoBehaviourType)) continue;

                Debug.LogError($"The script of type {type.Name} provided in AnythingMaker.Make() " +
                               "doesn't inherit from MonoBehaviour. User scripts will not be added");
                return null;
            }
            return new AdditionalScripts(types);
        }
        /// <summary>
        /// Specify an array of behaviour scripts to be added to model on completion.
        /// </summary>
        /// <param name="monobehaviours"></param>
        /// <returns></returns>
        public static RequestParameterOption AddScripts(params MonoBehaviour[] monobehaviours)
        {
            return new AdditionalScripts(monobehaviours);
        }

        /// <summary>
        /// Specify if model should be spawned on a grid.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ///
        public static RequestParameterOption PlaceOnGrid(bool value)
        {
            return new PlaceOnGrid(value);
        }

        /// <summary>
        /// Specify if the grid alignement uses an area from an object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ///
        public static RequestParameterOption UseGridArea(bool value)
        {
            return new UseGridArea(value);
        }
        /// <summary>
        /// Specify if model should be spawned on a grid.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RequestParameterOption PlaceOnGround(bool value)
        {
            return new PlaceOnGround(value);
        }


        /// <summary>
        /// Set rotation of created model with Quaternion.
        /// </summary>
        /// <param name="quaternionRotation">Quaternion rotation value to be assigned to model rotation.</param>
        public static RequestParameterOption Rotation(Quaternion quaternionRotation)
        {
            return new Rotation(quaternionRotation);
        }

        /// <summary>
        /// Set rotation of created model with euler angles.
        /// </summary>
        /// <param name="eulerRotation">Euler rotation to apply to model.</param>
        /// <returns></returns>
        public static RequestParameterOption Rotation(Vector3 eulerRotation)
        {
            return new Rotation(eulerRotation);
        }

        /// <summary>
        /// Set rotation of created model with euler angles.
        /// </summary>
        /// <param name="x">Euler angle for x axis.</param>
        /// <param name="y">Euler angle for y axis.</param>
        /// <param name="z">Euler angle for z axis.</param>
        public static RequestParameterOption Rotation(int x, int y, int z)
        {
            return new Rotation(x, y, z);
        }
        /// <summary>
        /// Set model scale with vector.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RequestParameterOption Scale(Vector3 value)
        {
            return new Scale(value);
        }

        /// <summary>
        /// Set model scale with integers.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static RequestParameterOption Scale(int x, int y, int z)
        {
            return new Scale(x, y, z);
        }
        /// <summary>
        /// Multiply the default or defined scale value by this value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RequestParameterOption ScaleMultiplier(float value)
        {
            return new ScaleMultiplier(value);
        }


        /// <summary>
        /// Position of created mode.
        /// </summary>
        /// <param name="positionVector">Vector3 value that will be set to object transform position.</param>
        /// <returns></returns>
        public static RequestParameterOption Position(Vector3 positionVector)
        {
            return new Position(positionVector);
        }
        /// <summary>
        /// Position of created mode.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static RequestParameterOption Position(int x, int y, int z)
        {
            return new Position(x, y, z);
        }
        /// <summary>
        /// Serialize model assets to database on loading completion.
        /// </summary>
        /// <returns></returns>
        public static RequestParameterOption SerializeAsset()
        {
            return new SerializeAsset();
        }
        /// <summary>
        /// Serialize model assets to database on loading completion.
        /// </summary>
        /// <returns></returns>
        public static RequestParameterOption SerializeAsset(bool value)
        {
            return new SerializeAsset(value);
        }
        /// <summary>
        /// Action that will be called on successful model creation.
        /// </summary>
        /// <param name="action">Function to be invoked.</param>
        /// <returns></returns>
        public static RequestParameterOption OnSuccessAction(Action action)
        {
            return new OnSuccessAction(action);
        }
        /// <summary>
        /// Action that will be called on successful model creation that is passed CallbackInfo
        /// object as a parameter.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static RequestParameterOption OnSuccessAction(Action<CallbackInfo> action)
        {
            return new OnSuccessAction(action);
        }
        /// <summary>
        /// Action called on failed model creation.
        /// </summary>
        /// <param name="action">Function to be invoked.</param>
        /// <returns></returns>
        public static RequestParameterOption OnFailAction(Action action)
        {
            return new OnFailureAction(action);
        }
        /// <summary>
        /// Action that will be called on failed model creation that is passed CallbackInfo
        /// object as a parameter.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static RequestParameterOption OnFailAction(Action<CallbackInfo> action)
        {
            return new OnFailureAction(action);
        }
    }

    [Obsolete("Class renamed to RequestParameterOption.")]
    public class RequestParam { }

    public class RequestParameterOption { }

    /// <summary>
    /// Clamp database scale between given values.
    /// </summary>
    public class ClampScale : RequestParameterOption
    {
        public ClampScale(Vector3 lowerBound, Vector3 upperBound)
        {
            RequestParameter._clampDbScale = true;
            RequestParameter._clampDbScaleLowerBounds = new Vector3Param(lowerBound);
            RequestParameter._clampDbScaleUpperBounds = new Vector3Param(upperBound);
        }
    }

    public class LegacyAnimatorInEditorOption : RequestParameterOption
    {
        public LegacyAnimatorInEditorOption(bool value)
        {
            RequestParameter._useLegacyAnimatorInEditor = value;
        }
    }

    /// <summary>
    /// Add default behaviour from default or given preset.
    /// </summary>
    public class DefaultBehaviour : RequestParameterOption
    {

        public DefaultBehaviour()
        {
            RequestParameter._defaultBehaviourPreset = new DefaultBehaviourPreset();
            RequestParameter._setDefaultBehaviourPreset = true;
        }
        public DefaultBehaviour(DefaultBehaviourPreset behaviourPreset)
        {
            RequestParameter._defaultBehaviourPreset = behaviourPreset;
            RequestParameter._setDefaultBehaviourPreset = true;
        }
        public DefaultBehaviour(bool _bool)
        {
            RequestParameter._defaultBehaviourPreset = new DefaultBehaviourPreset();
            RequestParameter._setDefaultBehaviourPreset = _bool;
        }
    }
    /// <summary>
    /// Set origin of object to the centre bottom of mesh bounds,
    /// effectively placing object on top of spawn position.
    /// </summary>
    public class PlaceOnGround : RequestParameterOption
    {
        public PlaceOnGround()
        {
            RequestParameter._placeOnGround = true;
        }
        public PlaceOnGround(bool value)
        {
            RequestParameter._placeOnGround = value;
        }
    }
    /// <summary>
    /// Add collider around object that encloses object mesh(es).
    /// </summary>
    public class AddCollider : RequestParameterOption
    {
        public AddCollider()
        {
            RequestParameter._addCollider = true;
        }
        public AddCollider(bool value)
        {
            RequestParameter._addCollider = value;
        }
    }
    /// <summary>
    /// Add Rigidbody to object.
    /// </summary>
    public class AddRigidbody : RequestParameterOption
    {
        public AddRigidbody()
        {
            RequestParameter._addRigidbody = true;
        }
        public AddRigidbody(bool value)
        {
            RequestParameter._addRigidbody = value;
        }
    }
    /// <summary>
    /// Serialize Asset
    /// </summary>
    public class SerializeAsset : RequestParameterOption
    {
        public SerializeAsset()
        {
            RequestParameter._serializeAsset = true;
        }
        public SerializeAsset(bool value)
        {
            RequestParameter._serializeAsset = value;
        }
    }
    public class PlaceOnGrid : RequestParameterOption
    {
        public PlaceOnGrid(bool value)
        {
            RequestParameter._placeOnGrid = value;
        }
    }
    public class UseGridArea : RequestParameterOption
    {
        public UseGridArea(bool value)
        {
            RequestParameter._useGridArea = value;
        }
    }
    public class TransformSpace : RequestParameterOption
    {
        public TransformSpace(Utilities.TransformSpace space)
        {
            RequestParameter._transformSpace = space;
        }
    }
    public class ParentTransform : RequestParameterOption
    {
        public ParentTransform(GameObject gameObject)
        {
            RequestParameter._parentTransform = gameObject.transform;
        }

        public ParentTransform(Transform transform)
        {
            RequestParameter._parentTransform = transform;
        }
    }
    public class AdditionalScripts : RequestParameterOption
    {
        public AdditionalScripts(Dictionary<DefaultBehaviourType, Type> behaviours)
        {
            var monoBehaviourType = typeof(MonoBehaviour);

            foreach (var behaviour in behaviours.Values)
            {
                if (behaviour.IsSubclassOf(monoBehaviourType)) continue;

                Debug.LogError($"The script of type {behaviour.Name} provided in AnythingMaker.Make() " +
                               "doesn't inherit from MonoBehaviour. User scripts will not be added");
                return;
            }
            RequestParameter._categorizedBehaviours = behaviours;
        }

        public AdditionalScripts(Type[] behaviours)
        {
            RequestParameter._behaviours = behaviours;
        }

        public AdditionalScripts(MonoBehaviour[] behaviours)
        {
            //Convert array of monobehaviours to their type values
            var typeArray = behaviours.Select(x => x.GetType()).ToArray();
            RequestParameter._behaviours = typeArray;
        }
    }

    /// <summary>
    /// Request model with animation system if available.
    /// </summary>
    public class AnimateModel : RequestParameterOption
    {
        public AnimateModel(bool value)
        {
            RequestParameter._animateModel = value;
        }
    }

    /// <summary>
    /// Rotation of created object.
    /// </summary>
    public class Rotation : RequestParameterOption
    {
        public Rotation(Quaternion quaternionRotation)
        {
            RequestParameter._rotation = quaternionRotation;
        }

        public Rotation(Vector3 eulerRotation)
        {
            RequestParameter._rotation = Quaternion.Euler(eulerRotation);
        }

        /// <summary>
        /// Set rotation using Euler angles.
        /// </summary>
        /// <param name="x">Euler angle for x axis.</param>
        /// <param name="y">Euler angle for y axis.</param>
        /// <param name="z">Euler angle for z axis.</param>
        public Rotation(int x, int y, int z)
        {
            RequestParameter._rotation = Quaternion.Euler(x, y, z);
        }
    }

    /// <summary>
    /// Position of created object.
    /// </summary>
    public class Position : RequestParameterOption
    {
        public Position(Vector3 value)
        {
            RequestParameter._position = new Vector3Param(value);
        }

        public Position(int x, int y, int z)
        {
            RequestParameter._position = new Vector3Param(new Vector3(x, y, z));
        }
    }

    public class Scale : RequestParameterOption
    {
        public Scale(Vector3 value)
        {
            RequestParameter._scale = new Vector3Param(value);
        }

        public Scale(int x, int y, int z)
        {
            RequestParameter._scale = new Vector3Param(new Vector3(x, y, z));
        }
        public Scale(float x, float y, float z)
        {
            RequestParameter._scale = new Vector3Param(new Vector3(x, y, z));
        }

    }
    public class ScaleMultiplier : RequestParameterOption
    {
        public ScaleMultiplier(float scalar)
        {
            RequestParameter._scaleMultiplier = scalar;
        }
    }

    public class ScaleType : RequestParameterOption
    {
        public ScaleType(Utilities.ScaleType value)
        {
            RequestParameter._scaleType = value;
        }
    }

    /// <summary>
    /// Action called on successful model creation.
    /// </summary>
    public class OnSuccessAction : RequestParameterOption
    {
        public OnSuccessAction(Action value)
        {
            RequestParameter._onSuccessAction = value;
        }

        /// Returns callback info from model (guid, linked object, message)
        public OnSuccessAction(Action<CallbackInfo> value)
        {
            RequestParameter._onSuccessActionCallback = value;
        }
    }

    /// <summary>
    /// Action called on model creation failure.
    /// </summary>
    public class OnFailureAction : RequestParameterOption
    {
        public OnFailureAction(Action value)
        {
            RequestParameter._onFailAction = value;
        }

        /// <summary>
        /// Returns callback info from model (guid, linked object, message)
        /// </summary>
        /// <param name="value"></param>
        public OnFailureAction(Action<CallbackInfo> value)
        {
            RequestParameter._onFailActionCallback = value;
        }
    }
}
