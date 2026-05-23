using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private List<GameObject> weaponPrefabs;
    [SerializeField] private Camera playerCamera;

    private List<IWeapon> weapons = new();
    private int currentIndex = 0;
    private IWeapon currentWeapon;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        foreach (var prefab in weaponPrefabs)
        {
            GameObject instance = Instantiate(prefab, transform);
            IWeapon weapon = instance.GetComponent<IWeapon>();
            weapon.Initialize(playerCamera);
            weapons.Add(weapon);
            instance.SetActive(false);
        }

        if (weapons.Count > 0)
            EquipWeapon(0);
    }

    public void Attack()
    {
        currentWeapon?.Attack();
    }
    public void TryParry()
    {
        currentWeapon?.Parry();
    }
    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;

        currentWeapon?.Unequip();

        currentIndex = index;
        currentWeapon = weapons[index];
        currentWeapon.Equip();
    }

    public void SwitchToNext()
    {
        int next = (currentIndex + 1) % weapons.Count;
        EquipWeapon(next);
    }

    public void SwitchToPrevious()
    {
        int prev = (currentIndex - 1 + weapons.Count) % weapons.Count;
        EquipWeapon(prev);
    }
}