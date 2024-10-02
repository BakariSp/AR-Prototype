using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARMultipleImageTrackingManager : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager _arTrackedImageManager;
    [SerializeField] private List<GameObject> prefabsToSpawn;
    
    private Dictionary<string, GameObject> _arObjects;

    private void Awake()
    {
        // Ensure the ARTrackedImageManager is assigned
        _arTrackedImageManager = GetComponent<ARTrackedImageManager>();
        _arObjects = new Dictionary<string, GameObject>();
    }

    private void Start()
    {
        // Subscribe to the trackedImagesChanged event
        _arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        // Spawn prefabs for all images in the scene
        foreach (GameObject prefab in prefabsToSpawn)
        {
            GameObject spawnedPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            spawnedPrefab.name = prefab.name; // Ensure the name matches the reference image
            spawnedPrefab.SetActive(false);
            _arObjects.Add(spawnedPrefab.name, spawnedPrefab);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the trackedImagesChanged event
        _arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Handle added images
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);
        }

        // Handle updated images
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
        }

        // Handle removed images
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            _arObjects[trackedImage.referenceImage.name].SetActive(false);
        }
    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        // Check if trackedImage or its referenceImage is null
        if (trackedImage == null)
        {
            Debug.LogError("TrackedImage is null.");
            return;
        }

        if (trackedImage.referenceImage == null)
        {
            Debug.LogError("ReferenceImage is null for trackedImage.");
            return;
        }

        string referenceImageName = trackedImage.referenceImage.name;

        // Check if referenceImageName is null or empty
        if (string.IsNullOrEmpty(referenceImageName))
        {
            Debug.LogError("ReferenceImage name is null or empty.");
            return;
        }

        // Check tracking status of the tracked image
        if (trackedImage.trackingState is TrackingState.Limited or TrackingState.None)
        {
            if (_arObjects.ContainsKey(referenceImageName))
            {
                _arObjects[referenceImageName].SetActive(false);
            }
            else
            {
                Debug.LogWarning($"No AR object found for reference image: '{referenceImageName}'.");
            }
            return;
        }

        // Show, hide, or update the position of the spawned prefab
        if (prefabsToSpawn != null)
        {
            if (_arObjects.TryGetValue(referenceImageName, out GameObject arObject))
            {
                arObject.SetActive(true);
                arObject.transform.position = trackedImage.transform.position;
                arObject.transform.rotation = trackedImage.transform.rotation;
            }
            else
            {
                Debug.LogWarning($"No AR object found for reference image: '{referenceImageName}'.");
            }
        }
    }
}