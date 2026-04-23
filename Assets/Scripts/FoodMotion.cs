using UnityEngine;

public class FoodMotion : MonoBehaviour
{
    public Transform assignedPlate;
    public string foodID;

    [Header("Exact position on plate")]
    public Vector3 localOffset = new Vector3(0f, 0.35f, 0f);

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void SetFoodID(string id)
    {
        foodID = NormalizeName(id);
    }

    public void AssignPlate(Transform plate)
    {
        assignedPlate = plate;
        SnapToAssignedPlate();
    }

    public void SnapToAssignedPlate()
    {
        if (assignedPlate == null) return;

        transform.SetParent(assignedPlate, false);
        transform.localPosition = localOffset;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;
    }

    public void ClearPlate()
    {
        assignedPlate = null;
        transform.SetParent(null, true);
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
}