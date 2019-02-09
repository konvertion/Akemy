// Include basic namespaces
using UnityEngine;
using System.Collections;

// Include for Lists and Dictionaries
using System.Collections.Generic;

//Include these namespaces to use BinaryFormatter
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//Include for Unity UI
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelEditor : MonoBehaviour {

	// The instance of the LevelEditor
	public static LevelEditor instance = null;
	// Whether this script is enabled (false, if the user closes the window)
	private bool scriptEnabled = true;
	// Counter for errors
	private int errorCounter = 0;

	// Define empty tile for map
	const int EMPTY = -1;

	// The X,Y and Z value of the map
	public int HEIGHT = 14;
	public int WIDTH = 16;
	public int LAYERS = 10;

	// The internal representation of the level (int values) and gameObjects (transforms)
	private int[, ,] level;
	private Transform[, ,] gameObjects;

	// The list of tiles the user can use to create maps
	// Public so the user can add all user-created prefabs
	public List<Transform> tiles;

	// Used to store the currently selected tile index and layer
	private int selectedTileIndex = EMPTY;
	private int selectedLayer = 0;

	// GameObject as the parent for all the layers (to keep the Hierarchy window clean)
	private GameObject tileLevelParent;
	// Dictionary as the parent for all the GameObjects per layer
	private Dictionary<int, GameObject> layerParents = new Dictionary<int, GameObject>();

	// GameObject as the parent for all the GameObject in the tiles selection
	private GameObject prefabParent;
	// Button Prefab used to create tile selection buttons for each GameObjects.
	public GameObject buttonPrefab;

	// Dimension used to set the width of the GameObject tile container object
	// Represented using a 0-500 slider in the editor
	[Range(0.0f, 500.0f)]
	public float PrefabsContainerWidth = 300f;
	// Dimensions used for the representation of the GameObject tile selection buttons
	// Represented using a 0-200 slider in the editor
	[Range(0.0f, 200.0f)]
	public float buttonSize = 100;

	// GameObject used to show the currently selected tile
	private GameObject selectedTile;
	// Dimensions used for the representation of the selected tile game object
	// Represented using a 0-200 slider in the editor
	[Range(0.0f, 200.0f)]
	public float selectedTileSize = 100;

	// Scale of the images in regards to the total image rectangle size
	public float buttonImageScale = 0.8f;

	// Image to indicate the currently selected tile
	private Image selectedTileImage;
	// Sprite to indicate no tile is currently selected
	public Sprite noSelectedTileImage;

	// File extension used to save and load the levels
	public string fileExtension = "lvl";

	// Boolean to determine whether to show all layers or only the current one
	private bool onlyShowCurrentLayer = false;
	// UI objects to toggle onlyShowCurrentLayer
	private Toggle onlyShowCurrentLayerToggleComponent;
	private GameObject layerEyeImage;
	private GameObject layerClosedEyeImage;

	// UI objects to toggle the grid
	private GameObject gridEyeImage;
	private GameObject gridClosedEyeImage;
	private Toggle gridEyeToggleComponent;

	// The parent object of the Level Editor UI as prefab
	public GameObject levelEditorUIPrefab;
	// Text used to represent the currently selected layer
	private Text layerText;
	// Open button to reopen the level editor after closing it
	private GameObject openButton;
	// The UI panel used to store the Level Editor options
	private GameObject levelEditorPanel;

	// Transform used to preview selected tile on map
	private Transform previewTile;

	// Stacks to keep track for undo and redo feature
	private FiniteStack<int[,,]> undoStack;
	private FiniteStack<int[,,]> redoStack;

	// Main camera and components for zoom feature
	private GameObject mainCamera;
	private Camera mainCameraComponent;
	private float mainCameraInitialSize;

	// Boolean to determine whether to use fill mode or pencil mode
	private bool fillMode = false;
	// UI objects to display pencil/fill mode
	private Image pencilModeButtonImage;
	private Image fillModeButtonImage;
	public Texture2D fillCursor;
	private static Color32 DisabledColor = new Color32 (150, 150, 150, 255);

	// FileBrowser Prefab to open Save- and LoadFilePanel
	public GameObject fileBrowserPrefab;
	// Temporary variable to save level before getting the path using the FileBrowser
	private string levelToSave;
	// Temporary variable to save state of level editor before opening file browser and restore it after save/load
	private bool preFileBrowserState = true;

	// Method to Instantiate the LevelEditor instance and keep it from destroying
	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if(instance != this){
			Destroy(gameObject);
		}
	}

	// Method to instantiate the dependencies and variables
 	void Start()
	{
		// Check the start values to prevent errors
		CheckStartValues ();

		// Define the level sizes as the sizes for the grid
		GridOverlay.instance.SetGridSizeX (WIDTH);
		GridOverlay.instance.SetGridSizeY (HEIGHT);

		// Find the camera, position it in the middle of our level and store initial zoom level
		mainCamera = GameObject.FindGameObjectWithTag ("MainCamera");
		if (mainCamera != null) {
			mainCamera.transform.position = new Vector3 (WIDTH / 2, HEIGHT / 2, mainCamera.transform.position.z);
			//Store initial zoom level
			mainCameraComponent = mainCamera.GetComponent<Camera> ();
			if (mainCameraComponent.orthographic) {
				mainCameraInitialSize = mainCameraComponent.orthographicSize;
			} else {
				mainCameraInitialSize = mainCameraComponent.fieldOfView;
			}
		} else {
			errorCounter++;
			Debug.LogError ("Object with tag MainCamera not found");
		}

		// Get or create the tileLevelParent object so we can make it our newly created objects' parent
		tileLevelParent = GameObject.Find ("TileLevel");
		if (tileLevelParent == null) {
			tileLevelParent = new GameObject ("TileLevel");
		}

		// Instantiate the level and gameObject to an empty level and empty Transform
		level = CreateEmptyLevel ();
		gameObjects = new Transform[WIDTH, HEIGHT, LAYERS];

		// Instantiate the undo and redo stack
		undoStack = new FiniteStack<int[,,]> ();
		redoStack = new FiniteStack<int[,,]> ();

		SetupUI ();
	}

	// Method that checks public variables values and sets them to valid defaults when necessary
	private void CheckStartValues()
	{
		WIDTH = Mathf.Clamp (WIDTH, 1, WIDTH);
		HEIGHT = Mathf.Clamp (HEIGHT, 1, HEIGHT);
		LAYERS = Mathf.Clamp (LAYERS, 1, LAYERS);
		buttonSize = Mathf.Clamp (buttonSize, 1, buttonSize);
		buttonImageScale = Mathf.Clamp01 (buttonImageScale);
		fileExtension = fileExtension.Trim () == "" ? "lvl" : fileExtension;
	}

	private void SetupUI(){

		//------ UI ---------

		// Instantiate the LevelEditorUI
		GameObject canvas = GameObject.Find("Canvas");
		if (canvas == null) {
			errorCounter++;
			Debug.LogError ("Make sure there is a canvas GameObject present in the Hierarcy (Create UI/Canvas)");
		}
		GameObject levelEditorUI = Instantiate (levelEditorUIPrefab, canvas.transform);
		levelEditorUI.name = ("LevelEditorUI");

		// Instantiate the LevelEditorPanel
		levelEditorPanel = GameObject.Find ("LevelEditorPanel");
		if (levelEditorPanel == null) {
			errorCounter++;
			Debug.LogError ("Make sure LevelEditorPanel is present");
		}
		SetupSaveLoadButton ();
		SetupUndoRedoButton ();
		SetupModeButtons ();
		SetupZoomButtons ();
		SetupLayerButtons ();
		SetupGridButtons ();
		SetupOpenCloseButton ();
		SetupSelectedTile ();
		SetupPrefabsButtons ();
	}

	// Finds and returns a game object by name or prints and error and increments error counter
	private GameObject FindGameObjectOrError(string name){
		GameObject gameObject = GameObject.Find (name);
		if (gameObject == null) {
			errorCounter++;
			Debug.LogError ("Make sure " + name + " is present");
			return null;
		} else {
			return gameObject;
		}
	}

	private void SetupSaveLoadButton(){
		// Hook up SaveLevel method to SaveButton
		GameObject saveButton = FindGameObjectOrError ("SaveButton");
		saveButton.GetComponent<Button> ().onClick.AddListener (SaveLevel);

		// Hook up LoadLevel method to LoadButton
		GameObject loadButton = FindGameObjectOrError ("LoadButton");
		loadButton.GetComponent<Button> ().onClick.AddListener (LoadLevel);
	}

	private void SetupUndoRedoButton(){
		// Hook up Undo method to UndoButton
		GameObject undoButton = FindGameObjectOrError ("UndoButton");
		undoButton.GetComponent<Button> ().onClick.AddListener (Undo);

		// Hook up Redo method to RedoButton
		GameObject redoButton = FindGameObjectOrError ("RedoButton");
		redoButton.GetComponent<Button> ().onClick.AddListener (Redo);
	}

	private void SetupModeButtons(){
		// Hook up EnablePencilMode method to PencilButton
		GameObject pencilModeButton = FindGameObjectOrError ("PencilButton");
		pencilModeButton.GetComponent<Button> ().onClick.AddListener (DisableFillMode);
		pencilModeButtonImage = pencilModeButton.GetComponent<Image> ();

		// Hook up EnableFillMode method to FillButton
		GameObject fillModeButton = FindGameObjectOrError ("FillButton");
		fillModeButton.GetComponent<Button> ().onClick.AddListener (EnableFillMode);
		fillModeButtonImage = fillModeButton.GetComponent<Image> ();

		DisableFillMode ();
	}

	private void SetupZoomButtons(){
		// Hook up ZoomIn method to ZoomInButton
		GameObject zoomInButton = FindGameObjectOrError ("ZoomInButton");
		zoomInButton.GetComponent<Button> ().onClick.AddListener (ZoomIn);

		// Hook up ZoomOut method to ZoomOutButton
		GameObject zoomOutButton = FindGameObjectOrError ("ZoomOutButton");
		zoomOutButton.GetComponent<Button> ().onClick.AddListener (ZoomOut);

		// Hook up ZoomDefault method to ZoomDefaultButton
		GameObject zoomDefaultButton = FindGameObjectOrError ("ZoomDefaultButton");
		zoomDefaultButton.GetComponent<Button> ().onClick.AddListener (ZoomDefault);
	}

	private void SetupLayerButtons(){
		// Hook up LayerUp method to +LayerButton
		GameObject plusLayerButton = FindGameObjectOrError ("+LayerButton");
		plusLayerButton.GetComponent<Button> ().onClick.AddListener (LayerUp);

		// Hook up LayerDown method to -LayerButton
		GameObject minusLayerButton = FindGameObjectOrError ("-LayerButton");
		minusLayerButton.GetComponent<Button> ().onClick.AddListener (LayerDown);

		// Hook up ToggleOnlyShowCurrentLayer method to OnlyShowCurrentLayerToggle
		GameObject onlyShowCurrentLayerToggle = FindGameObjectOrError ("OnlyShowCurrentLayerToggle");
		layerEyeImage = GameObject.Find ("LayerEyeImage");
		layerClosedEyeImage = GameObject.Find ("LayerClosedEyeImage");
		onlyShowCurrentLayerToggleComponent = onlyShowCurrentLayerToggle.GetComponent<Toggle> ();
		onlyShowCurrentLayerToggleComponent.onValueChanged.AddListener (ToggleOnlyShowCurrentLayer);

		// Instantiate the LayerText game object to display the current layer
		layerText = GameObject.Find ("LayerText").GetComponent<Text> ();
		if (layerText == null) {
			errorCounter++;
			Debug.LogError ("Make sure LayerText is present");
		}
	}

	private void SetupGridButtons(){
		// Hook up ToggleGrid method to GridToggle
		GameObject gridEyeToggle = FindGameObjectOrError ("GridEyeToggle");
		gridEyeImage = GameObject.Find ("GridEyeImage");
		gridClosedEyeImage = GameObject.Find ("GridClosedEyeImage");
		gridEyeToggleComponent = gridEyeToggle.GetComponent<Toggle> ();
		gridEyeToggleComponent.onValueChanged.AddListener (ToggleGrid);
		ToggleGrid (true);

		// Hook up GridSizeUp method to GridSizeUpButton
		GameObject gridSizeUpButton = FindGameObjectOrError ("GridSizeUpButton");
		gridSizeUpButton.GetComponent<Button> ().onClick.AddListener (GridOverlay.instance.GridSizeUp);

		// Hook up GridSizeDown method to GridSizeDownButton
		GameObject gridSizeDownButton = FindGameObjectOrError ("GridSizeDownButton");
		gridSizeDownButton.GetComponent<Button> ().onClick.AddListener (GridOverlay.instance.GridSizeDown);

		// Hook up GridUp method to GridUpButton
		GameObject gridUpButton = FindGameObjectOrError ("GridUpButton");
		gridUpButton.GetComponent<Button> ().onClick.AddListener (GridOverlay.instance.GridUp);

		// Hook up GridDown method to GridDownButton
		GameObject gridDownButton = FindGameObjectOrError ("GridDownButton");
		gridDownButton.GetComponent<Button> ().onClick.AddListener (GridOverlay.instance.GridDown);

		// Hook up GridLeft method to GridLeftButton
		GameObject gridLeftButton = FindGameObjectOrError ("GridLeftButton");
		gridLeftButton.GetComponent<Button> ().onClick.AddListener (GridOverlay.instance.GridLeft);

		// Hook up GridRight method to GridRightButton
		GameObject gridRightButton = FindGameObjectOrError ("GridRightButton");
		gridRightButton.GetComponent<Button> ().onClick.AddListener (GridOverlay.instance.GridRight);
	}

	private void SetupOpenCloseButton ()
	{
		// Hook up CloseLevelEditorPanel method to CloseButton
		GameObject closeButton = FindGameObjectOrError ("CloseButton");
		closeButton.GetComponent<Button> ().onClick.AddListener (CloseLevelEditorPanel);

		// Instantiate the OpenButton
		openButton = FindGameObjectOrError ("OpenButton");
		openButton.GetComponent<Button> ().onClick.AddListener (OpenLevelEditorPanel);
		openButton.SetActive (false);
	}

	private void SetupSelectedTile(){
		selectedTile = FindGameObjectOrError("SelectedTile");
		// Find the image component of the SelectedTileImage GameObject
		selectedTileImage = FindGameObjectOrError ("SelectedTileImage").GetComponent<Image>();
		// Set the SelectedTile to Empty (-1) and update the selectedTileImage
		SetSelectedTile (EMPTY);
	}

	private void SetupPrefabsButtons(){
		// Find the prefabParent object and set the cellSize for the tile selection buttons
		prefabParent = GameObject.Find ("Prefabs");
		if (prefabParent == null || prefabParent.GetComponent<GridLayoutGroup> () == null) {
			errorCounter++;
			Debug.LogError ("Make sure prefabParent is present and has a GridLayoutGroup component");
		} else {
			// Update the size, to scale the buttons
			UpdatePrefabButtonsSize ();
		}
		// Set the width of the prefabParent object, using the initially set value
		UpdatePrefabParentWidth();

		// Counter to determine which tile button is pressed
		int tileCounter = 0;
		//Create a button for each tile in tiles
		foreach (Transform tile in tiles) {
			int index = tileCounter;
			GameObject button = Instantiate (buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			button.name = tile.name;
			button.GetComponent<Image> ().sprite = tile.gameObject.GetComponent<SpriteRenderer> ().sprite;
			button.transform.SetParent (prefabParent.transform, false);
			button.transform.localScale = new Vector3 (buttonImageScale, buttonImageScale, buttonImageScale);
			// Add a click handler to the button
			button.GetComponent<Button> ().onClick.AddListener (() => {
				ButtonClick (index);
			});
			tileCounter++;
		}
	}

	// Method to set the selectedTile variable and the selectedTileImage
	private void SetSelectedTile (int tileIndex){
		// Update selectedTile variable
		selectedTileIndex = tileIndex;
		// If EMPTY, set selectedTileImage to noSelectedTileImage else to the corresponding Prefab tile image
		if (tileIndex == EMPTY) {
			selectedTileImage.sprite = noSelectedTileImage;
		} else {
			selectedTileImage.sprite = tiles [tileIndex].gameObject.GetComponent<SpriteRenderer> ().sprite;
		}
	}
		
	// Method to switch selectedTile on tile selection
	private void ButtonClick (int tileIndex)
	{
		SetSelectedTile (tileIndex);
		if (previewTile != null) {
			DestroyImmediate (previewTile.gameObject);
		}
		previewTile = Instantiate (tiles [selectedTileIndex], new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100), Quaternion.identity) as Transform;
		foreach (Collider2D c in previewTile.GetComponents<Collider2D>()) {
			c.enabled = false;
		}
	}

	// Method to create an empty level by looping through the Height, Width and Layers 
	// and setting the value to EMPTY (-1)
	private int[, ,] CreateEmptyLevel()
	{
		int[,,] emptyLevel = new int[WIDTH, HEIGHT, LAYERS];
		for (int x = 0; x < WIDTH; x++) {
			for (int y = 0; y < HEIGHT; y++) {
				for (int z = 0; z < LAYERS; z++) {
					emptyLevel [x, y, z] = EMPTY;
				}
			}
		}
		return emptyLevel;
	}

	// Method to determine for a given x, y, z, whether the position is valid (within Width, Height and Layers)
	private bool ValidPosition(int x, int y, int z)
	{
		if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT || z < 0 || z >= LAYERS) {
			return false;
		} else {
			return true;
		}
	}

	// Method that creates a GameObject on click
	private void CreateBlock(int value, int xPos, int yPos, int zPos)
	{
		// The transform to create
		Transform toCreate = null;
		// Return on invalid positions
		if (!ValidPosition (xPos, yPos, zPos)) {
			return;
		}
		// Set the value for the internal level representation
		level [xPos, yPos, zPos] = value;
		// If the value is not empty, set it to the correct tile
		if (value != EMPTY) {
			toCreate = tiles [value];
		}
		if (toCreate != null) {
			//Create the object we want to create
			Transform newObject = Instantiate (toCreate, new Vector3 (xPos, yPos, toCreate.position.z), Quaternion.identity) as Transform;
			//Give the new object the same name as our tile prefab
			newObject.name = toCreate.name;
			// Set the object's parent to the layer parent variable so it doesn't clutter our Hierarchy
			newObject.parent = GetLayerParent (zPos).transform;
			// Add the new object to the gameObjects array for correct administration
			gameObjects [xPos, yPos, zPos] = newObject;
		}
	}

	// Rebuild the level (e.g. after using undo/redo)
	// Reset the Transforms and Layer, then loop trough level array and create blocks
	private void RebuildLevel(){
		ResetTransformsAndLayers ();
		for (int x = 0; x < WIDTH; x++) {
			for (int y = 0; y < HEIGHT; y++) {
				for (int z = 0; z < LAYERS; z++) {
					CreateBlock (level [x, y, z], x, y, z);
				}
			}
		}
	}

	// Load last saved level from undo stack and rebuild level
	private void Undo(){
		// See if there is anything on the undo stack
		if (undoStack.Count > 0) {
			// If so, push it to the redo stack
			redoStack.Push (level);
		}
		// Get the last level entry
		int[,,] undoLevel = undoStack.Pop ();
		if (undoLevel != null) {
			// Set level and rebuild the level
			level = undoLevel;
			RebuildLevel ();
		}
	}

	// Load last saved level from redo tack and rebuild level
	private void Redo() {
		// See if there is anything on the redo stack
		if (redoStack.Count > 0){
			// If so, push it to the redo stack
			undoStack.Push(level);
		}
		// Get the last level entry
		int[,,] redoLevel = redoStack.Pop();
		if (redoLevel != null) {
			// Set level and rebuild the level
			level = redoLevel;
			RebuildLevel ();
		}
	}

	// Increment the orthographic size or field of view of the camera, thereby zooming in
	private void ZoomIn(){
		if (mainCameraComponent.orthographic) {
			mainCameraComponent.orthographicSize = Mathf.Max (mainCameraComponent.orthographicSize - 1, 1);
		} else {
			mainCameraComponent.fieldOfView = Mathf.Max (mainCameraComponent.fieldOfView - 1, 1);
		}
	}

	// Decrement the orthographic size or field of view of the camera, thereby zooming out
	private void ZoomOut(){
		if (mainCameraComponent.orthographic) {
			mainCameraComponent.orthographicSize += 1;
		} else {
			mainCameraComponent.fieldOfView += 1;
		}
	}

	// Resets the orthographic size or field of view of the camera, thereby resetting the zoom level
	private void ZoomDefault(){
		if (mainCameraComponent.orthographic) {
			mainCameraComponent.orthographicSize = mainCameraInitialSize;
		}else {
			mainCameraComponent.fieldOfView = mainCameraInitialSize;
		}

	}

	// Clicked on position, so check if it is the same, and (destroy and) build if neccesary
	private void ClickedPosition(int posX, int posY){
		// If it's the same, just keep the previous one and do nothing, else (destroy and) build
		if (level [posX, posY, selectedLayer] != selectedTileIndex) {
			// Push level on undoStack since it is going to change
			undoStack.Push (level.Clone () as int[,,]);
			// If the position is not empty, destroy the the current element (using gameObjects array)
			if (level [posX, posY, selectedLayer] != EMPTY) {
				DestroyImmediate (gameObjects [posX, posY, selectedLayer].gameObject);
			}
			// Create the new game object
			CreateBlock (selectedTileIndex, posX, posY, selectedLayer);
		}
	}

	// Fill from position recursively. Only fill if the position is valid and empty
	private void Fill(int posX, int posY, bool undoPush){
		// Check valid and empty
		if (ValidPosition (posX, posY, selectedLayer) && level [posX, posY, selectedLayer] == EMPTY) {
			if (undoPush) {
				// Push level on undoStack since it is going to change
				undoStack.Push (level.Clone () as int[,,]);
			}
			// Create a block on the position
			CreateBlock (selectedTileIndex, posX, posY, selectedLayer);
			// Fill x+1, x-1, y+1, y-1
			Fill (posX + 1, posY, false);
			Fill (posX - 1, posY, false);
			Fill (posX, posY + 1, false);
			Fill (posX, posY - 1, false);
		}
	}

	// Toggle fill mode (between fill and pencil mode)
	private void ToggleFillMode(){
		if(fillMode){
			DisableFillMode ();
		}
		else{
			EnableFillMode ();
		}
	}

	// Enable fill mode and update UI
	private void EnableFillMode(){
		fillMode = true;
		fillModeButtonImage.GetComponent<Image>().color = Color.black;
		pencilModeButtonImage.GetComponent<Image>().color = DisabledColor;
	}

	// Disable fill mode and update UI and cursor
	private void DisableFillMode(){
		fillMode = false;
		Cursor.SetCursor (null, Vector2.zero , CursorMode.Auto);
		pencilModeButtonImage.GetComponent<Image>().color = Color.black;
		fillModeButtonImage.GetComponent<Image>().color = DisabledColor;
	}

	// Method that updates the UI and handles creation and deletion on click
	void Update()
	{
		// Only continue if the script is enabled (level editor is open) and there are no errors
		if (scriptEnabled && errorCounter == 0) {
			// Update the width to scale at runtime
			UpdatePrefabParentWidth ();
			// Update the button size to scale at runtime
			UpdatePrefabButtonsSize();
			// Update the selected tile game object to scale at runtime
			UpdateSelectedTileSize();

			// Save the world point were the mouse clicked
			Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			// Update fill mode cursor
			UpdateFillModeCursor (worldMousePosition);
			// Update preview tile position
			UpdatePreviewTilePosition (worldMousePosition);
			// Check button input
			CheckButtonInput ();
			// Update the layer text
			UpdateLayerText ();
			// Get the mouse position before click
			Vector3 mousePos = Input.mousePosition;
			// Set the position in the z axis to the opposite of the camera's so that the position is on the world
			//  so ScreenToWorldPoint will give us valid values.
			mousePos.z = Camera.main.transform.position.z * -1;
			Vector3 pos = Camera.main.ScreenToWorldPoint (mousePos);
			// Deal with the mouse being not exactly on a block
			int posX = Mathf.FloorToInt (pos.x + .5f);
			int posY = Mathf.FloorToInt (pos.y + .5f);
			// Handle input only when a valid position is clicked
			if (ValidPosition (posX, posY, selectedLayer)) {
				HandleInput (posX, posY);
			}
		}
	}

	// Update the width of the prefabParent object, the height will be scaled automatically
	private void UpdatePrefabParentWidth(){
		prefabParent.GetComponent<RectTransform> ().sizeDelta = new Vector2 (PrefabsContainerWidth, 100f);
		selectedTile.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (-PrefabsContainerWidth, 0);
	}

	// Update the size of the prefab tile objects, the images will be square to keep the aspect ratio original
	private void UpdatePrefabButtonsSize(){
		prefabParent.GetComponent<GridLayoutGroup> ().cellSize = new Vector2 (buttonSize, buttonSize);
	}

	// Update the size of the selected tile game object, the images will be scaled to half that
	private void UpdateSelectedTileSize(){
		selectedTile.GetComponent<RectTransform> ().sizeDelta = new Vector2 (selectedTileSize, selectedTileSize);
		selectedTileImage.GetComponent<RectTransform> ().sizeDelta = new Vector2 (selectedTileSize/2, selectedTileSize/2);
	}

	// Check for mouse button clicks and handle accordingly
	private void HandleInput(int posX, int posY){
		// Left click - Create object (check hotControl and not over UI)
		if (Input.GetMouseButton (0) && GUIUtility.hotControl == 0 && !EventSystem.current.IsPointerOverGameObject()) {
			// Only allow additions if the selectedTile is not EMPTY (cannot add/fill nothing)
			if (selectedTileIndex != EMPTY) {
				// If fill mode, fill, else click position (pencil mode)
				if (fillMode) {
					Fill (posX, posY, true);
				} else {
					ClickedPosition (posX, posY);
				}
			}
		}
		// Right clicking - Delete object (check hotControl and not over UI)
		if (Input.GetMouseButton (1) && GUIUtility.hotControl == 0 && !EventSystem.current.IsPointerOverGameObject()) {
			// If we hit something (!= EMPTY), we want to destroy the object and update the gameObject array and level array
			if (level [posX, posY, selectedLayer] != EMPTY) {
				DestroyImmediate (gameObjects [posX, posY, selectedLayer].gameObject);
				level [posX, posY, selectedLayer] = EMPTY;
			} 
			// If we hit nothing and previewTile is null, remove it
			else if (previewTile != null) {
				DestroyImmediate (previewTile.gameObject);
				// Set selected tile and image to EMPTY
				SetSelectedTile (EMPTY);
			}
		}
	}

	// If fill mode is enabled, update cursor (only show fill cursor on grid)
	private void UpdateFillModeCursor(Vector3 worldMousePosition){
		if (fillMode){
			// If valid position, set cursor to bucket
			if (ValidPosition ((int)worldMousePosition.x, (int)worldMousePosition.y, 0)) {
				Cursor.SetCursor (fillCursor, new Vector2 (30, 25), CursorMode.Auto);
			}
			// Else use default cursor
			else {
				Cursor.SetCursor (null, Vector2.zero , CursorMode.Auto);
			}
		} 
	}

	// Update previewTile position
	private void UpdatePreviewTilePosition(Vector3 worldMousePosition){
		if (previewTile != null) {
			if (ValidPosition ((int)worldMousePosition.x, (int)worldMousePosition.y, 0)) {
				previewTile.position = new Vector3 (Mathf.RoundToInt (worldMousePosition.x), Mathf.RoundToInt (worldMousePosition.y), -1);
			}
		}
	}

	// Check for any button presses (undo/redo, zooming and fill/pencil mode)
	private void CheckButtonInput(){
		// If Z is pressed, undo action
		if (Input.GetKeyDown (KeyCode.Z)) {
			Undo ();
		}

		// If Y is pressed, redo action
		if(Input.GetKeyDown(KeyCode.Y)){
			Redo ();
		}

		// If Equals is pressed, zoom in
		if(Input.GetKeyDown(KeyCode.Equals)){
			ZoomIn ();
		}
		// if Minus is pressed, zoom in
		if(Input.GetKeyDown(KeyCode.Minus)){
			ZoomOut ();
		}
		// If 0 is pressed, reset zoom
		if(Input.GetKeyDown(KeyCode.Alpha0)){
			ZoomDefault ();
		}
		// If F is pressed, toggle FillMode;
		if(Input.GetKeyDown(KeyCode.F)){
			ToggleFillMode ();
		}
	}

	// Method that updates the LayerText
	private void UpdateLayerText()
	{
		layerText.text = "" + (selectedLayer + 1);
	}

	// Method that toggles the grid
	private void ToggleGrid(bool enabled)
	{
		GridOverlay.instance.enabled = enabled;
		// Update UI 
		if (enabled) {
			gridClosedEyeImage.SetActive (true);
			gridEyeImage.SetActive (false);
			gridEyeToggleComponent.targetGraphic = gridClosedEyeImage.GetComponent<Image> ();
		} else {
			gridEyeImage.SetActive (true);
			gridClosedEyeImage.SetActive (false);
			gridEyeToggleComponent.targetGraphic = gridEyeImage.GetComponent<Image> ();
		}
	}

	// Method that increments the selected layer
	private void LayerUp()
	{
		selectedLayer = Mathf.Min (selectedLayer + 1, LAYERS - 1);
		UpdateLayerVisibility ();
	}

	// Method that decrements the selected layer
	private void LayerDown()
	{
		selectedLayer = Mathf.Max (selectedLayer - 1, 0);
		UpdateLayerVisibility ();
	}

	// Method that handles the UI toggle to only show the current layer
	private void ToggleOnlyShowCurrentLayer(bool onlyShow){
		onlyShowCurrentLayer = onlyShow;
		// Update UI
		if (onlyShowCurrentLayer) {
			layerEyeImage.SetActive (true);
			layerClosedEyeImage.SetActive (false);
			onlyShowCurrentLayerToggleComponent.targetGraphic = layerEyeImage.GetComponent<Graphic> ();
		} else {
			layerClosedEyeImage.SetActive (true);
			layerEyeImage.SetActive (false);
			onlyShowCurrentLayerToggleComponent.targetGraphic = layerClosedEyeImage.GetComponent<Graphic> ();
		}
		// Update layer visibility
		UpdateLayerVisibility ();
	}

	// Method that updates which layers should be shown
	private void UpdateLayerVisibility(){
		if (onlyShowCurrentLayer) {
			OnlyShowCurrentLayer ();
		} else {
			ShowAllLayers ();
		}
	}

	// Method that enables/disables all layerParents
	private void ToggleLayerParents(bool show){
		foreach (GameObject layerParent in layerParents.Values) {
			layerParent.SetActive (show);
		}
	}

	// Method that enables all layers
	private void ShowAllLayers(){
		ToggleLayerParents (true);
	}

	// Method that disables all layers except the current one
	private void OnlyShowCurrentLayer(){
		ToggleLayerParents (false);
		GetLayerParent (selectedLayer).SetActive (true);
	}

	// Method to determine whether a layer is empty
	private bool EmptyLayer(int layer)
	{
		bool result = true;
		for (int x = 0; x < WIDTH; x++) {
			for (int y = 0; y < HEIGHT; y++) {
				if (level [x, y, layer] != EMPTY) {
					result = false;
				}
			}
		}
		return result;
	}

	// Method that returns the parent GameObject for a layer
	private GameObject GetLayerParent(int layer)
	{
		if (!layerParents.ContainsKey (layer)) {
			GameObject layerParent = new GameObject ("Layer " + layer);
			layerParent.transform.parent = tileLevelParent.transform;
			layerParents.Add (layer, layerParent);
		}
		return layerParents [layer];
	}

	// Returns whether the script is enabled (e.g. whether input is registered) 
	public bool GetScriptEnabled(){
		return scriptEnabled;
	}

	// Close the level editor panel, test level mode
	public void CloseLevelEditorPanel ()
	{
		scriptEnabled = false;
		levelEditorPanel.SetActive (false);
		openButton.SetActive(true);
	}

	// Open the level editor panel, level editor mode
	public void OpenLevelEditorPanel()
	{
		levelEditorPanel.SetActive (true);
		openButton.SetActive(false);
		scriptEnabled = true;
	}

	// Enables/disables the level editor, (script, overlay and panel)
	public void ToggleLevelEditor(bool enabled){
		scriptEnabled = enabled;
		GridOverlay.instance.enabled = enabled;
		levelEditorPanel.SetActive (enabled);
	}

	// Open a file browser to save and load files
	public void OpenFileBrowser(FileBrowserMode fileBrowserMode){
		preFileBrowserState = scriptEnabled;
		// Disable the LevelEditor while the fileBrowser is open
		ToggleLevelEditor (false);
		// Create the file browser and name it
		GameObject fileBrowserObject = Instantiate (fileBrowserPrefab, this.transform);
		fileBrowserObject.name = "FileBrowser";
		// Set the mode to save or load
		FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser> ();
		if (fileBrowserMode == FileBrowserMode.Save) {
			fileBrowserScript.SaveFilePanel (this, "SaveLevelUsingPath", "Level", fileExtension);
		} else {
			fileBrowserScript.OpenFilePanel (this, "LoadLevelUsingPath", fileExtension);
		}
	}

	// Save the level to a variable and file using FileBrowser and SaveLevelUsingPath
	private void SaveLevel()
	{
		List<string> newLevel = new List<string> ();
		// Loop through the layers
		for (int layer = 0; layer < LAYERS; layer++) {
			// If the layer is not empty, add it and add \t at the end"
			if (!EmptyLayer (layer)) {
				// Loop through the rows and add \n at the end"
				for (int y = 0; y < HEIGHT; y++) {
					string newRow = "";
					for (int x = 0; x < WIDTH; x++) {
						newRow += +level [x, y, layer] + ",";
					}
					if (y != 0) {
						newRow += "\n";
					}
					newLevel.Add (newRow);
				}
				newLevel.Add ("\t" + layer);
			}
		}

		// Reverse the rows to make the final version rightside up
		newLevel.Reverse ();
		string levelComplete = "";
		foreach (string level in newLevel) {
			levelComplete += level;
		}
		// Temporarily save the level to save it using SaveLevelUsingPath
		levelToSave = levelComplete;
		// Open file browser to get the path and file name
		OpenFileBrowser (FileBrowserMode.Save);
	}

	// Save to a file using a path
	public void SaveLevelUsingPath (string path)
	{
		// Enable the LevelEditor when the fileBrowser is done
		ToggleLevelEditor (preFileBrowserState);
		if (path.Length != 0) {
			// Save the level to file
			BinaryFormatter bFormatter = new BinaryFormatter ();
			FileStream file = File.Create (path);
			bFormatter.Serialize (file, levelToSave);
			file.Close ();
			// Reset the temporary variable
			levelToSave = null;
		} else {
			Debug.Log ("Invalid path given");
		}
	}

	// Method that resets the GameObjects and layers
	private void ResetTransformsAndLayers()
	{
		// Destroy everything inside our currently level that's created dynamically
		foreach (Transform child in tileLevelParent.transform) {
			Destroy (child.gameObject);
		}
		layerParents = new Dictionary<int, GameObject> ();
	}

	// Method that resets the level and GameObject before a load
	private void ResetBeforeLoad()
	{
		// Destroy everything inside our currently level that's created dynamically
		foreach (Transform child in tileLevelParent.transform) {
			Destroy (child.gameObject);
		}
		level = CreateEmptyLevel ();
		layerParents = new Dictionary<int, GameObject> ();
		// Reset undo and redo stacks
		undoStack = new FiniteStack<int[,,]> ();
		redoStack = new FiniteStack<int[,,]> ();
	}

	// Load the level from a file using FileBrowser and LoadLevelUsingPath
	private void LoadLevel()
	{
		// Open file browser to get the path and file name
		OpenFileBrowser (FileBrowserMode.Load);
	}

	// Load from a file using a path
	public void LoadLevelUsingPath(string path)
	{
		// Enable the LevelEditor when the fileBrowser is done
		ToggleLevelEditor (preFileBrowserState);
		if (path.Length != 0) {
			BinaryFormatter bFormatter = new BinaryFormatter ();
			// Reset the level
			ResetBeforeLoad ();
			FileStream file = File.OpenRead (path);
			// Convert the file from a byte array into a string
			string levelData = bFormatter.Deserialize (file) as string;
			// We're done working with the file so we can close it
			file.Close ();
			LoadLevelFromStringLayers (levelData);
		} else {
			Debug.Log ("Invalid path given");
		}
	}

	// Method that loads the layers
	private void LoadLevelFromStringLayers(string content)
	{
		// Split our level on layers by the new tabs (\t)
		List <string> layers = new List <string> (content.Split ('\t'));
		int layerCounter = 0;
		foreach (string layer in layers) {
			if (layer.Trim () != "") {
				LoadLevelFromString (int.Parse (layer [0].ToString ()), layer.Substring (1));
				layerCounter++;
			}
		}
	}

	// Method that loads one layer
	private void LoadLevelFromString(int layer, string content)
	{
		// Split our layer on rows by the new lines (\n)
		List <string> lines = new List <string> (content.Split ('\n'));
		// Place each block in order in the correct x and y position
		for (int i = 0; i < lines.Count; i++) {
			string[] blockIDs = lines [i].Split (',');
			for (int j = 0; j < blockIDs.Length - 1; j++) {
				CreateBlock (int.Parse (blockIDs [j]), j, lines.Count - i - 1, layer);
			}
		}
		// Update to only show the correct layer(s)
		UpdateLayerVisibility ();
	}
}
