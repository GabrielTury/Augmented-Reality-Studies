namespace AnythingWorld.Editor
{
    using AnythingWorld.Utilities;
    using AnythingWorld.Utilities.Data;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// This class provides functionality to draw a texture in the Unity Scene view. 
    /// It also allows the user to drag and drop the texture onto objects within the scene.
    /// </summary>
    public class SceneTextureDrawer : Editor
    {
        private bool isDragging = false;
        private bool loading = false;

        // Textures for the object and its loading state
        private Texture2D objTexture;
        private Texture2D objLoadingTexture;

        // To store the current position of the mouse within the scene view
        private Vector2 currentMousePosition;

        // Singleton instance to ensure only one drawer is active at a time
        private static SceneTextureDrawer instance;

        // Flag to control the activation state of the drawer
        private bool enabled = false;

        // Delegate definition for handling the drag event
        public delegate GameObject OnDrag(SearchResult result, Vector3 position, bool forceSerialize = false);
        public OnDrag onDrag;

        // Modifier key to continue creating until released
        private EventModifiers continueUntil = EventModifiers.Shift;

        SearchResult searchResult;

        // Store the position where the object will be placed
        private Vector3 objectPosition;

        // Store the generated game object after a successful drag and drop
        private GameObject generatedGameObject;

        /// <summary>
        /// Singleton instance of the SceneTextureDrawer.
        /// </summary>
        public static SceneTextureDrawer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = CreateInstance<SceneTextureDrawer>();
                }
                return instance;
            }
        }

        /// <summary>
        /// Called when the object is enabled. It subscribes to the scene GUI event.
        /// </summary>
        private void OnEnable()
        {
            Enable();
        }

        /// <summary>
        /// Public method to enable the SceneTextureDrawer, ensuring there is only one instance active.
        /// </summary>
        public void Enable()
        {
            if (instance != null && instance != this)
            {
                DestroyImmediate(instance);
            }
            instance = this;
            if (enabled)
            {
                return;
            }
            enabled = true;
            SceneView.duringSceneGui += OnSceneGUI;
            isDragging = false;
            loading = false;
        }

        /// <summary>
        /// Checks if the SceneTextureDrawer is currently enabled.
        /// </summary>
        /// <returns>True if enabled, false otherwise.</returns>
        public bool IsEnabled()
        {
            return enabled;
        }

        /// <summary>
        /// Called when the object is disabled. It unsubscribes from the scene GUI event to prevent memory leaks.
        /// </summary>
        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            enabled = false;
        }

        /// <summary>
        /// Sets up the callback for dragging, initializing necessary state variables.
        /// </summary>
        public void SetCallBack(Texture2D texture, OnDrag onDrag, SearchResult result, ref bool isDrag)
        {
            this.onDrag = onDrag;
            searchResult = result;
            objTexture = texture;
            isDragging = true;
            isDrag = true;
            loading = false;
        }
       //Cancel the drag
       public void CancelDrag()
        {
            isDragging = false;
            loading = false;
            if(onDrag != null)
            onDrag(null, Vector3.zero, false);
        }
        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.DragUpdated:
                    HandleDragUpdate(sceneView, e);
                    break;
                case EventType.DragPerform:
                    HandleDragExited(sceneView, e);
                    break;
                case EventType.MouseDown:
                    HandleMouseDown(sceneView, e);
                    break;
                case EventType.MouseLeaveWindow:
                    if (isDragging)
                    {
                        CancelDrag();
                    }
                    break;
            }
            if (loading && generatedGameObject != null)
            {
                HandleLoadingAnimation();
            }
            else
            {
                IconFloating(sceneView, e);
            }
        }
        //Handle DragUpdated event
        private void HandleDragUpdate(SceneView sceneView, Event e)
        {
            if (!isDragging) {
                //check if the dragged object is from the AnythingCreatorEditor
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is AnythingCreatorEditor)
                    {
                        isDragging = true;
                        loading = false;
                    }
                }
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
        }
        //Handle DragExited event
        private void HandleDragExited(SceneView sceneView, Event e)
        {
            if (isDragging)
            {
                //accept the drag and clear the drag buffer
                DragAndDrop.AcceptDrag();
                //call the delegate to create the object
                generatedGameObject = onDrag(searchResult, objectPosition, false);
                //look at the object
                if (TransformSettings.FollowCam)
                {
                    sceneView.LookAt(objectPosition);
                }
                //start the loading animation
                if (TransformSettings.LoadingImageActive)
                {
                    StartLoadingAnimation();
                }
                if (e.modifiers == continueUntil || TransformSettings.ContinueUntilCancelled) return;
                isDragging = false;
                e.Use();
            }
        }
        /// <summary>
        /// Handles the mouse down event loading the object into the scene.
        /// </summary>
        private void HandleMouseDown(SceneView sceneView, Event e)
        {
            if (e.button == 0 && e.isMouse)
            {
                if (!isDragging) return;
                //call the delegate to create the object
                generatedGameObject = onDrag(searchResult, objectPosition, false);

                if (TransformSettings.FollowCam)
                {
                    sceneView.LookAt(objectPosition);
                }

                if (TransformSettings.LoadingImageActive)
                {
                    StartLoadingAnimation();
                }

                e.Use();
                if (e.modifiers == continueUntil || TransformSettings.ContinueUntilCancelled) return;
                isDragging = false;
            }
            else if (isDragging)
            {
                CancelDrag();
            }
        }
        /// <summary>
        /// Starts the loading animation of the object.
        /// </summary>
        private void StartLoadingAnimation()
        {
            loading = true;
            objLoadingTexture = Tex2dUtils.ConvertToGrayscale(objTexture);
        }
        /// <summary>
        /// Handles the loading animation of the object.
        /// </summary>
        private void HandleLoadingAnimation()
        {
            if (objLoadingTexture == null) return;

            Vector2 guiPosition = HandleUtility.WorldToGUIPoint(objectPosition);
            Vector2 texsize = new Vector2(objLoadingTexture.width, objLoadingTexture.height) * 0.4f;

            Handles.BeginGUI();
            GUI.DrawTexture(new Rect(guiPosition.x - texsize.x / 2, guiPosition.y - texsize.y / 2, texsize.x, texsize.y), objLoadingTexture);
            Handles.EndGUI();

            if (generatedGameObject.transform.childCount > 0)
            {
                loading = false;
            }
        }
        /// <summary>
        /// handle the dragging of the texture in the scene view
        /// </summary>
        void IconFloating(SceneView sceneView, Event e)
        {
            if (!isDragging || objTexture == null) return;

            // Store the current mouse position
            currentMousePosition = e.mousePosition;

            // Convert mouse position to world ray
            Ray ray = HandleUtility.GUIPointToWorldRay(currentMousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                DrawTextureAtPoint(objTexture,hit.point, 0.5f);
                objectPosition = hit.point;
            }
            else
            {
                DrawTextureAtPoint(objTexture,currentMousePosition, 0.8f, true);
                objectPosition = ray.origin + ray.direction * 10;
            }

            // Ensure the scene view is updated
            sceneView.Repaint();
        }
        /// <summary>
        /// Draws a texture at a given point in the scene view.
        /// </summary>
        void DrawTextureAtPoint(Texture2D texture,Vector3 point, float scale, bool isGuiPoint = false)
        {
            Handles.BeginGUI();
            Vector2 texsize = new Vector2(texture.width, texture.height) * scale;
            Vector2 position = isGuiPoint ? (Vector2)point : HandleUtility.WorldToGUIPoint(point);
            GUI.DrawTexture(new Rect(position.x - texsize.x / 2, position.y - texsize.y / 2, texsize.x, texsize.y), texture);
            Handles.EndGUI();
        }
    }
}
