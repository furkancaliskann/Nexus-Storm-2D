using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreatorBuilding : MonoBehaviour
{
    private CreatorCameraMovement creatorCameraMovement;

    private Dictionary<Vector2, GameObject> usedPositions = new Dictionary<Vector2, GameObject>();
    private Dictionary<Vector2, GameObject> redSpawnPoints = new Dictionary<Vector2, GameObject>();
    private Dictionary<Vector2, GameObject> blueSpawnPoints = new Dictionary<Vector2, GameObject>();

    private List<GameObject> prefabs = new List<GameObject>();
    private List<GameObject> mapTextures = new List<GameObject>();

    [SerializeField] private GameObject mapTexturePrefab;
    private string selectedPrefabName = "";
    [SerializeField] private Transform mapTexturesPanelParent;

    [SerializeField] private GameObject mapParent;
    [SerializeField] private InputField saveLoadNameInputfield;
    

    void Awake()
    {
        creatorCameraMovement = GetComponent<CreatorCameraMovement>();
    }
    void Start()
    {
        GetTextures();
    }
    void Update()
    {
        CheckInputs();
    }
    private void GetTextures()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("Map Creator Prefabs");
        prefabs.AddRange(loadedPrefabs);

        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject mapTexture = Instantiate(mapTexturePrefab, mapTexturesPanelParent);
            mapTexture.GetComponentsInChildren<Image>()[2].sprite = prefabs[i].GetComponent<SpriteRenderer>().sprite;

            string referanceName = prefabs[i].name;
            mapTexture.GetComponent<Button>().onClick.AddListener(() => SelectTextures(referanceName));
            mapTextures.Add(mapTexture);
        }
    }
    private void SelectTextures(string textureName)
    {
        for (int i = 0; i < mapTextures.Count; i++)
        {
            Image selectedImage = mapTextures[i].GetComponentsInChildren<Image>()[1];
            if (selectedImage.enabled) selectedImage.enabled = false;
        }
        mapTextures[prefabs.FindIndex(x => x.name == textureName)].GetComponentsInChildren<Image>()[1].enabled = true;

        selectedPrefabName = textureName;
    }
    private void CheckInputs()
    {
        if (IsInputfieldFocused()) return;

        if (Input.GetMouseButton(0)) Place();
        if (Input.GetMouseButton(1)) Remove();
    }
    private void Place()
    {
        if (EventSystem.current.IsPointerOverGameObject() || selectedPrefabName == "") return;

        Vector2 mousePosition = FindMousePosition();
        if (selectedPrefabName != "red_spawn_point" && selectedPrefabName != "blue_spawn_point" && usedPositions.ContainsKey(mousePosition)) return;
        if (selectedPrefabName == "red_spawn_point" && redSpawnPoints.ContainsKey(mousePosition)) return;
        if (selectedPrefabName == "blue_spawn_point" && blueSpawnPoints.ContainsKey(mousePosition)) return;

        GameObject prefab = Instantiate(prefabs.Find(x => x.name == selectedPrefabName), mousePosition, Quaternion.identity, mapParent.transform);
        switch(selectedPrefabName)
        {
            case "red_spawn_point": redSpawnPoints.Add(mousePosition, prefab); break;
            case "blue_spawn_point": blueSpawnPoints.Add(mousePosition, prefab); break;
            default: usedPositions.Add(mousePosition, prefab); break;
        }
            
        prefab.name = selectedPrefabName;
    }
    private void PlaceWith(Vector2 pos, string prefabName)
    {
        GameObject prefab = Instantiate(prefabs.Find(x => x.name == prefabName), pos, Quaternion.identity, mapParent.transform);
        switch (prefabName)
        {
            case "red_spawn_point": redSpawnPoints.Add(pos, prefab); break;
            case "blue_spawn_point": blueSpawnPoints.Add(pos, prefab); break;
            default: usedPositions.Add(pos, prefab); break;
        }
        prefab.name = prefabName;
    }
    private void Remove()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 mousePosition = FindMousePosition();

        if (redSpawnPoints.ContainsKey(mousePosition))
        {
            Destroy(redSpawnPoints[mousePosition]);
            redSpawnPoints.Remove(mousePosition);
        }
        if (blueSpawnPoints.ContainsKey(mousePosition))
        {
            Destroy(blueSpawnPoints[mousePosition]);
            blueSpawnPoints.Remove(mousePosition);
        }
        if (usedPositions.ContainsKey(mousePosition))
        {
            Destroy(usedPositions[mousePosition]);
            usedPositions.Remove(mousePosition);
        }
    }
    private Vector2 FindMousePosition()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.x = Mathf.RoundToInt(mousePosition.x);
        mousePosition.y = Mathf.RoundToInt(mousePosition.y);

        return mousePosition;
    }
    private void ResetMapVariables()
    {
        usedPositions.Clear();
        redSpawnPoints.Clear();
        blueSpawnPoints.Clear();
        selectedPrefabName = string.Empty;
        saveLoadNameInputfield.text = string.Empty;
        saveLoadNameInputfield.DeactivateInputField();

        for (int i = 0; i < mapParent.transform.childCount; i++)
        {
            Destroy(mapParent.transform.GetChild(i).gameObject);
        }
        creatorCameraMovement.ResetCameraSettings();
    }
    public bool IsInputfieldFocused()
    {
        return saveLoadNameInputfield.isFocused;
    }
    public void Save()
    {
#if UNITY_EDITOR
        if (saveLoadNameInputfield.text == "")
        {
            Debug.Log("Enter the map name!");
            return;
        }

        string prefabPath = "Assets/Resources/Maps/" + saveLoadNameInputfield.text + ".prefab";
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(mapParent, prefabPath);

        saveLoadNameInputfield.text = string.Empty;
        saveLoadNameInputfield.DeactivateInputField();

        Debug.Log("Prefab created!");
#endif
    }
    public void Load()
    {
        if (saveLoadNameInputfield.text == "")
        {
            Debug.Log("Enter the map name!");
            return;
        }

        GameObject loadedMap = Resources.Load<GameObject>("Maps/" + saveLoadNameInputfield.text);
        if (loadedMap == null)
        {
            Debug.Log("Map not found!");
            return;
        }

        ResetMapVariables();

        GameObject spawnedMap = Instantiate(loadedMap);

        for (int i = 0; i < spawnedMap.transform.childCount; i++)
        {
            Transform child = spawnedMap.transform.GetChild(i);
            Vector2 pos = child.transform.position;
            pos.x = Mathf.RoundToInt(pos.x);
            pos.y = Mathf.RoundToInt(pos.y);
            PlaceWith(pos, child.name);
        }

        Destroy(spawnedMap);
    }  
}