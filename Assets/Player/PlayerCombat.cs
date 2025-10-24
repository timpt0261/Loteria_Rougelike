using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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


	[SerializeField] private PlayerController m_playerController;

	[SerializeField] private WeaponController m_currentWeapon;
	[SerializeField] private Transform m_weaponHolderPosition;
	[SerializeField] private List<WeaponData> m_weaponInventory;
	[SerializeField] private int currentWeaponIndex = 0;



	void Start()
	{
		m_playerController = GetComponent<PlayerController>();
		currentWeaponIndex = 0;
		SetWeapon(currentWeaponIndex);
	}

	public void AddWeaponToInventory(WeaponData newWeapon)
	{
		m_weaponInventory.Add(newWeapon);
		SetWeapon(m_weaponInventory.Count - 1);

	}

	public void RemoveWeaponFromInventory(WeaponData newWeapon)
	{
		m_weaponInventory.Remove(newWeapon);
	}



	public void SetWeapon(int weaponIndex)
	{
		if (weaponIndex < 0 || weaponIndex > m_weaponInventory.Count) return;
		if (m_currentWeapon != null)
		{
			m_currentWeapon.OnUnequip();
			Destroy(m_currentWeapon.gameObject);
		}


		currentWeaponIndex = weaponIndex;

		m_currentWeapon = Instantiate(m_weaponInventory[currentWeaponIndex].weaponPrefab, m_weaponHolderPosition);
		m_currentWeapon.Initialize(m_playerController);
	}







}