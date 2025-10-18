using System.Collections.Generic;
using UnityEngine;
using Meta.XR.BuildingBlocks.AIBlocks;

public class HighlightObject : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private List<string> objectsToHighlight = new List<string> { "banana" };
    [SerializeField] private Color highlightColor = Color.green;
    [SerializeField] private float sphereRadius = 0.1f; // Radius in world units
    [SerializeField] private Material highlightMaterial;
    
    [Header("Camera Reference")]
    [SerializeField] private Camera passthroughCamera;
    
    private List<GameObject> activeHighlights = new List<GameObject>();
    
    void Start()
    {
        Debug.Log("HighlightObject Start() called");
        
        // Find the passthrough camera if not assigned
        if (passthroughCamera == null)
        {
            passthroughCamera = FindPassthroughCamera();
        }
        
        // Create default highlight material if not assigned
        if (highlightMaterial == null)
        {
            CreateDefaultMaterial();
        }
        
        Debug.Log($"Using passthrough camera: {passthroughCamera?.name}");
        
        // Test: Create a simple sphere at origin to test visibility
        StartCoroutine(CreateTestSphere());
    }
    
    private System.Collections.IEnumerator CreateTestSphere()
    {
        yield return new UnityEngine.WaitForSeconds(2f); // Wait 2 seconds
        
        Debug.Log("Creating test sphere at origin...");
        
        // Create a simple test sphere at origin
        Vector3 testPos = Vector3.zero;
        GameObject testSphere = CreateSphereHighlight(testPos);
        if (testSphere != null)
        {
            activeHighlights.Add(testSphere);
            Debug.Log($"Test sphere created successfully at {testPos}");
        }
        else
        {
            Debug.LogError("Failed to create test sphere!");
        }
    }
    
    private Camera FindPassthroughCamera()
    {
        // Try to find the passthrough camera by name
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            if (cam.name.ToLower().Contains("passthrough") || 
                cam.name.ToLower().Contains("passthru") ||
                cam.name.ToLower().Contains("ovr") ||
                cam.name.ToLower().Contains("xr"))
            {
                Debug.Log($"Found potential passthrough camera: {cam.name}");
                return cam;
            }
        }
        
        // If no specific passthrough camera found, return the first camera
        if (cameras.Length > 0)
        {
            Debug.Log($"Using first available camera: {cameras[0].name}");
            return cameras[0];
        }
        
        return null;
    }
    
    private void CreateDefaultMaterial()
    {
        // Try to find a suitable shader for transparency
        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        }
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }
        
        if (shader != null)
        {
            highlightMaterial = new Material(shader);
            highlightMaterial.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.8f);
            Debug.Log($"Created material with shader: {shader.name}, color: {highlightMaterial.color}");
        }
        else
        {
            Debug.LogError("Could not find suitable shader for transparent material!");
            // Fallback to opaque material
            highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = highlightColor;
        }
    }
    
    public void OnDetectionResponseReceived(List<BoxData> detectedObjects)
    {        
        Debug.Log($"OnDetectionResponseReceived called with {detectedObjects.Count} detected objects");
        
        // Clear existing highlights
        ClearHighlights();
        
        // Process each detected object
        foreach (BoxData boxData in detectedObjects)
        {            
            Debug.Log($"Processing object: {boxData.label} at position {boxData.position}");
            
            // Check if this object should be highlighted
            if (ShouldHighlightObject(boxData.label))
            {
                Debug.Log($"Highlighting object: {boxData.label}");
                DrawHighlightSphere(boxData);
            }
            else
            {
                Debug.Log($"Object {boxData.label} not in highlight list");
            }
        }
    }
    
    private bool ShouldHighlightObject(string label)
    {
        // Extract the base label (remove confidence score)
        string baseLabel = label.Split(' ')[0].ToLower();
        
        // Check if this object is in our highlight list
        foreach (string targetObject in objectsToHighlight)
        {
            if (baseLabel.Contains(targetObject.ToLower()))
            {
                return true;
            }
        }
        return false;
    }
    
    private void DrawHighlightSphere(BoxData boxData)
    {
        if (passthroughCamera == null)
        {
            Debug.LogError("Passthrough camera is null! Cannot create sphere.");
            return;
        }
        
        // Convert screen coordinates to world position
        Vector3 screenPos = new Vector3(boxData.position.x, boxData.position.y, 1f);
        Vector3 worldPos = passthroughCamera.ScreenToWorldPoint(screenPos);
        
        Debug.Log($"Screen position: {boxData.position}, World position: {worldPos}");
        
        // Create sphere highlight
        GameObject sphere = CreateSphereHighlight(worldPos);
        if (sphere != null)
        {
            activeHighlights.Add(sphere);
            Debug.Log($"Sphere created successfully. Active highlights count: {activeHighlights.Count}");
        }
        else
        {
            Debug.LogError("Failed to create sphere highlight!");
        }
    }
    
    private GameObject CreateSphereHighlight(Vector3 worldPosition)
    {
        Debug.Log($"Creating sphere highlight at world position: {worldPosition}");
        
        // Create sphere GameObject
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "HighlightSphere";
        
        // Position the sphere
        sphere.transform.position = worldPosition;
        sphere.transform.localScale = Vector3.one * sphereRadius * 2; // Diameter = radius * 2
        
        Debug.Log($"Sphere created: {sphere.name}, position: {sphere.transform.position}, scale: {sphere.transform.localScale}");
        
        // Apply material
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            Debug.Log($"Renderer found: {renderer.name}");
            if (highlightMaterial != null)
            {
                Debug.Log($"Applying material: {highlightMaterial.name}, color: {highlightMaterial.color}");
                renderer.material = highlightMaterial;
            }
            else
            {
                Debug.LogError("Highlight material is null!");
            }
        }
        else
        {
            Debug.LogError("No renderer found on sphere!");
        }
        
        // Make it semi-transparent and always face camera
        sphere.AddComponent<Billboard>();
        
        Debug.Log($"Sphere setup complete. Active in hierarchy: {sphere.activeInHierarchy}");
        
        return sphere;
    }
    
    private void ClearHighlights()
    {
        foreach (GameObject highlight in activeHighlights)
        {
            if (highlight != null)
            {
                Destroy(highlight);
            }
        }
        activeHighlights.Clear();
    }
    
    // Public method to add objects to highlight list at runtime
    public void AddObjectToHighlight(string objectName)
    {
        if (!objectsToHighlight.Contains(objectName.ToLower()))
        {
            objectsToHighlight.Add(objectName.ToLower());
        }
    }
    
    // Public method to remove objects from highlight list
    public void RemoveObjectFromHighlight(string objectName)
    {
        objectsToHighlight.Remove(objectName.ToLower());
    }
    
    // Public method to set highlight color
    public void SetHighlightColor(Color color)
    {
        highlightColor = color;
        if (highlightMaterial != null)
        {
            highlightMaterial.color = color;
        }
    }
    
    // Public method to set the passthrough camera manually
    public void SetPassthroughCamera(Camera camera)
    {
        passthroughCamera = camera;
        Debug.Log($"Set passthrough camera to: {camera?.name}");
    }
}

// Simple Billboard component to make spheres always face the camera
public class Billboard : MonoBehaviour
{
    private Camera cameraToLookAt;
    
    void Start()
    {
        cameraToLookAt = Camera.main;
        if (cameraToLookAt == null)
        {
            cameraToLookAt = FindObjectOfType<Camera>();
        }
    }
    
    void Update()
    {
        if (cameraToLookAt != null)
        {
            transform.LookAt(cameraToLookAt.transform);
            transform.Rotate(0, 180, 0); // Flip to face camera properly
        }
    }
}