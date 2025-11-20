using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
	[SerializeField] private PlayerController m_playerController;
	[SerializeField] private WeaponController m_currentWeapon;
	[SerializeField] private Transform m_weaponHolderPosition;
	[SerializeField] private List<WeaponData> m_weaponInventory;
	[SerializeField] private int currentWeaponIndex = 0;

	public WeaponController CurrentWeapon => m_currentWeapon;

	void Start()
	{
		m_playerController = GetComponent<PlayerController>();
		currentWeaponIndex = 0;
		SetWeapon(currentWeaponIndex);
	}

	public void PrimaryAttack() => m_currentWeapon?.PrimaryAttack();

	public void SecondaryAttack() => m_currentWeapon?.SecondaryAttack();

	public void Reload() => m_currentWeapon?.Reload();

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