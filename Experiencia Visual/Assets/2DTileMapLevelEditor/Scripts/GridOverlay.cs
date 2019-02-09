using UnityEngine;
using System.Collections;

public class GridOverlay : MonoBehaviour {

	public static GridOverlay instance = null;

	// Sizes of the grid
	private int gridSizeX = 10;
	private int gridSizeY = 10;
	private int gridSizeZ = 0;

	// Steps taken when moving the grid
	public float smallStep = 0.5f;
	// Steps to define the dimensions of each square
	public float largeStep = 1;

	// Starting position
	public float startX = 0;
	public float startY = 0;
	public float startZ = 0;

	// Offsets
	private float offsetX = -0.5f;
	private float offsetY = -0.5f;

	// Material of the grid
	public Material lineMaterial;

	// Color of the grid
	public Color mainColor = new Color(1f,1f,1f,1f);

	// Method to Instantiate the GridOverlay instance and keep it from destroying
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

	// Changes the width size of the grid
	public void SetGridSizeX(int x){
		gridSizeX = x;
	}

	// Changes the height size of the grid
	public void SetGridSizeY(int y){
		gridSizeY = y;
	}

	// Updates the dimensions per square with 0.5
	public void GridSizeUp(){
		largeStep += 0.5f;
	}

	// Update the dimensions per square with -0.5
	public void GridSizeDown(){
		largeStep -= 0.5f;
	}

	// Move the grid up by smallStep amount
	public void GridUp(){
		offsetY += smallStep;
	}

	// Move the grid down by smallStep amount
	public void GridDown(){
		offsetY -= smallStep;
	}

	// Move the grid left by smallStep amount
	public void GridLeft(){
		offsetX -= smallStep;
	}

	// Move the grid right by smallStep amount
	public void GridRight(){
		offsetX += smallStep;
	}

	// Draws the grid
	void OnPostRender()
	{  
		// Make sure largeStep never <= 0, since then the program crashes
		largeStep = Mathf.Max (largeStep, 0.5f);
		// set the current material
		lineMaterial.SetPass (0);

		GL.Begin (GL.LINES);

		GL.Color (mainColor);

		//Layers
		for (float j = 0; j <= gridSizeY; j += largeStep) {
			//X axis lines
			for (float i = 0; i <= gridSizeZ; i += largeStep) {
				GL.Vertex3 (startX + offsetX, j + offsetY, startZ + i);
				GL.Vertex3 (gridSizeX + offsetX, j + offsetY, startZ + i);
			}

			//Z axis lines
			for (float i = 0; i <= gridSizeX; i += largeStep) {
				GL.Vertex3 (startX + i + offsetX, j + offsetY, startZ);
				GL.Vertex3 (startX + i + offsetX, j + offsetY, gridSizeZ);
			}
		}

		//Y axis lines
		for (float i = 0; i <= gridSizeZ; i += largeStep) {
			for (float k = 0; k <= gridSizeX; k += largeStep) {
				GL.Vertex3 (startX + k + offsetX, startY + offsetY, startZ + i);
				GL.Vertex3 (startX + k + offsetX, gridSizeY + offsetY, startZ + i);
			}
		}

		GL.End ();
	}
}
