using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine;


public class PlayerCombat : MonoBehaviour
{
	/// Simplified structure:
	// - WeaponController currentWeapon
	// - List<WeaponData> inventory
	// - int currentWeaponIndex

	// // Methods:
	// public void PrimaryAttack() => currentWeapon?.PrimaryAttack()
	// public void SecondaryAttack() => currentWeapon?.SecondaryAttack()
	// public void SwitchWeapon(int direction)
	// public void EquipWeapon(WeaponData data)




	[SerializeField] private WeaponController m_currentWeapon;
	[SerializeField] private Transform m_weaponHolderPosition;
	[SerializeField] private List<WeaponData> m_weaponInventory;
	[SerializeField] private int currentWeaponIndex = 0;



	void Start()
	{
		if (m_currentWeapon == null)
		{
			currentWeaponIndex = 0;
			m_currentWeapon = m_weaponInventory[currentWeaponIndex].weaponPrefab;

		}
	}

	public void AddWeaponToInventory(WeaponData newWeapon)
	{
		m_weaponInventory.Add(newWeapon);
		SwitchWeapon(m_weaponInventory.Count - 1);

	}

	public void RemoveWeaponFromInventory(WeaponData newWeapon)
	{
		m_weaponInventory.Remove(newWeapon);
	}

	public void SwitchWeapon(int weaponIndex)
	{
		if (weaponIndex < 0 || weaponIndex > m_weaponInventory.Count) return;
		currentWeaponIndex = weaponIndex;
		// set animator to first animation state
		m_currentWeapon = m_weaponInventory[currentWeaponIndex].weaponPrefab;
	}







}