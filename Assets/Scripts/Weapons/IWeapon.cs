using Unity.VisualScripting;
using UnityEngine;

interface IWeapon
{
    public int Damage { get; set; }
    public void Attack();
    public void Equip();
    public void Unequip();
    void Initialize(Camera cam);

}
