using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlateMotionController : MonoBehaviour
{
    [Header("Plate References")]
    public Transform plate1;
    public Transform plate2;
    public Transform plate3;

    [Header("Food Prefabs / Templates")]
    public List<FoodMotion> foodPrefabs = new List<FoodMotion>();

    private Dictionary<string, FoodMotion> foodPrefabMap = new Dictionary<string, FoodMotion>();
    private List<FoodMotion> spawnedFoods = new List<FoodMotion>();

    private Coroutine plateAnimRoutine;

    private void Awake()
    {
        BuildFoodPrefabMap();
    }

    private void BuildFoodPrefabMap()
    {
        foodPrefabMap.Clear();

        foreach (var food in foodPrefabs)
        {
            if (food == null) continue;

            string rawID = !string.IsNullOrEmpty(food.foodID) ? food.foodID : food.gameObject.name;
            string key = NormalizeName(rawID);

            if (!foodPrefabMap.ContainsKey(key))
            {
                foodPrefabMap.Add(key, food);
                Debug.Log($"Registered food prefab: '{food.gameObject.name}' => key '{key}'");
            }
            else
            {
                Debug.LogWarning($"Duplicate food prefab key detected: '{key}' from '{food.gameObject.name}'");
            }
        }
    }

    public FoodMotion SpawnFoodForPlate(string foodID, Transform plate)
    {
        if (plate == null)
        {
            Debug.LogWarning("SpawnFoodForPlate failed: plate is null.");
            return null;
        }

        string key = NormalizeName(foodID);

        // 🔁 Alias mapping (fix mismatches here)
        key = ResolveAlias(key);

        if (!foodPrefabMap.ContainsKey(key))
        {
            Debug.LogWarning(
                $"No food prefab found for key: {key}. Available keys: {string.Join(", ", foodPrefabMap.Keys)}"
            );
            return null;
        }

        FoodMotion prefab = foodPrefabMap[key];
        FoodMotion clone = Instantiate(prefab, plate.position, Quaternion.identity);

        clone.gameObject.name = prefab.gameObject.name + "_Clone";
        clone.SetFoodID(key);
        clone.AssignPlate(plate);

        spawnedFoods.Add(clone);
        return clone;
    }
    private string ResolveAlias(string key)
    {
        switch (key)
        {
            case "beefsteakgarlic":
                return "steakgarlic";

            // Add more aliases here if needed
            // case "vegroll":
            //     return "sushiroll";

            default:
                return key;
        }
    }
    public void StartPlateAnimation(MonoBehaviour caller, List<FoodMotion> foodsToAnimate)
    {
        if (plateAnimRoutine != null)
            StopCoroutine(plateAnimRoutine);

        plateAnimRoutine = StartCoroutine(AnimatePlatesWithFoods(foodsToAnimate));
    }

    private IEnumerator AnimatePlatesWithFoods(List<FoodMotion> foodsToAnimate)
    {
        if (foodsToAnimate != null)
        {
            foreach (FoodMotion food in foodsToAnimate)
            {
                if (food == null) continue;
                food.SnapToAssignedPlate();
            }
        }

        Vector3 p1Start = plate1.position;
        Vector3 p2Start = plate2.position;
        Vector3 p3Start = plate3.position;

        Vector3 p1Target = new Vector3(-7f, p1Start.y, p1Start.z);
        Vector3 p2Target = new Vector3(0f, p2Start.y, p2Start.z);
        Vector3 p3Target = new Vector3(7f, p3Start.y, p3Start.z);

        Vector3 p1DropTarget = new Vector3(p1Target.x, -0.5f, p1Target.z);
        Vector3 p2DropTarget = new Vector3(p2Target.x, -0.25f, p2Target.z);
        Vector3 p3DropTarget = new Vector3(p3Target.x, -0.1f, p3Target.z);

        float t = 0f;
        float durationX = 4f;

        while (t < durationX)
        {
            t += Time.deltaTime;
            float smooth = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / durationX));

            plate1.position = Vector3.Lerp(p1Start, p1Target, smooth);
            plate2.position = Vector3.Lerp(p2Start, p2Target, smooth);
            plate3.position = Vector3.Lerp(p3Start, p3Target, smooth);

            yield return null;
        }

        plate1.position = p1Target;
        plate2.position = p2Target;
        plate3.position = p3Target;

        t = 0f;
        float durationY = 1f;

        Vector3 p1YStart = plate1.position;
        Vector3 p2YStart = plate2.position;
        Vector3 p3YStart = plate3.position;

        while (t < durationY)
        {
            t += Time.deltaTime;
            float smooth = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / durationY));

            plate1.position = Vector3.Lerp(p1YStart, p1DropTarget, smooth);
            plate2.position = Vector3.Lerp(p2YStart, p2DropTarget, smooth);
            plate3.position = Vector3.Lerp(p3YStart, p3DropTarget, smooth);

            yield return null;
        }

        plate1.position = p1DropTarget;
        plate2.position = p2DropTarget;
        plate3.position = p3DropTarget;

        plateAnimRoutine = null;
    }

    public void ResetPlates(bool toDefault = true)
    {
        if (!toDefault) return;

        plate1.position = new Vector3(-34f, 2.05f, 0f);
        plate2.position = new Vector3(-27f, 2.2f, 0f);
        plate3.position = new Vector3(-20f, 2.35f, 0f);

        for (int i = 0; i < spawnedFoods.Count; i++)
        {
            if (spawnedFoods[i] != null)
                Destroy(spawnedFoods[i].gameObject);
        }

        spawnedFoods.Clear();
    }

    private string NormalizeName(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";

        value = value.ToLower();
        value = value.Replace(" ", "");
        value = value.Replace("_", "");
        value = value.Replace("-", "");
        value = value.Replace("(clone)", "");
        value = value.Replace("(", "");
        value = value.Replace(")", "");

        return value;
    }

    public void ResetFoodsOnly()
    {
        for (int i = 0; i < spawnedFoods.Count; i++)
        {
            if (spawnedFoods[i] != null)
                Destroy(spawnedFoods[i].gameObject);
        }

        spawnedFoods.Clear();
    }

    public IEnumerator ResetFoodsOnlyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetFoodsOnly();
    }
}