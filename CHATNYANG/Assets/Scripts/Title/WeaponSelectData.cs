using UnityEngine;

public class WeaponSelectData : MonoBehaviour
{
    public static WeaponSelectData Instance { get; private set; }

    public GameObject SelectedWeaponPrefab { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 넘어도 유지
    }

    public void SetWeapon(GameObject prefab)
    {
        SelectedWeaponPrefab = prefab;
    }
}