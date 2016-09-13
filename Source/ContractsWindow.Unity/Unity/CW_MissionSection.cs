﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionSection : MonoBehaviour
	{
		[SerializeField]
		private GameObject ContractSectionPrefab = null;
		[SerializeField]
		private Transform ContractSectionTransform = null;

		private string missionTitle;
		private IMissionSection missionInterface;
		private List<Guid> activeContracts = new List<Guid>();
		private List<Guid> hiddenContracts = new List<Guid>();
		private Dictionary<Guid, CW_ContractSection> masterList = new Dictionary<Guid, CW_ContractSection>();

		public bool MasterMission
		{
			get
			{
				if (missionInterface == null)
					return false;

				return missionInterface.MasterMission;
			}
		}

		public IMissionSection MissionInterface
		{
			get { return missionInterface; }
		}

		public void setMission(IMissionSection section)
		{
			if (section == null)
				return;

			missionInterface = section;

			missionTitle = missionInterface.MissionTitle;

			CreateContractSections(section.GetContracts);
		}

		private void CreateContractSections(IList<IContractSection> contracts)
		{
			if (contracts == null)
				return;

			if (ContractSectionPrefab == null || ContractSectionTransform == null)
				return;

			if (CW_Window.Window == null)
				return;

			if (missionInterface == null)
				return;

			for (int i = contracts.Count - 1; i >= 0; i--)
			{
				IContractSection contract = contracts[i];

				if (contract == null)
					continue;

				if (masterList.ContainsKey(contract.ID))
					continue;

				CreateContractSection(contract);
			}
		}

		private void CreateContractSection(IContractSection contract)
		{
			GameObject obj = Instantiate(ContractSectionPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(ContractSectionTransform, false);

			CW_ContractSection contractObject = obj.GetComponent<CW_ContractSection>();

			if (contractObject == null)
				return;

			contractObject.setContract(contract, CW_Window.Window, this);

			if (contract.IsHidden)
				hiddenContracts.Add(contract.ID);
			else
				activeContracts.Add(contract.ID);

			masterList.Add(contract.ID, contractObject);

			contractObject.gameObject.SetActive(contract.IsHidden && missionInterface.ShowHidden || !contract.IsHidden && !missionInterface.ShowHidden);
		}

		public void AddContract(IContractSection contract)
		{
			if (contract == null)
				return;

			if (missionInterface == null)
				return;

			if (masterList.ContainsKey(contract.ID))
				return;

			CreateContractSection(contract);

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			CW_ContractSection section = GetContract(contract.ID);

			if (section == null)
				return;

			var texts = section.GetComponentsInChildren<Text>();

			for (int i = texts.Length - 1; i >= 0; i--)
			{
				Text t = texts[i];

				if (t == null)
					continue;

				t.fontSize += CW_Window.Window.Interface.LargeFont ? 1 : 0;
			}

		}

		private CW_ContractSection GetContract(Guid id)
		{
			if (masterList.ContainsKey(id))
				return masterList[id];

			return null;
		}

		public void SwitchContract(Guid id, bool hidden)
		{
			if (id == null)
				return;

			if (!masterList.ContainsKey(id))
				return;

			if (hidden)
			{
				ListRemove(activeContracts, id);
				hiddenContracts.Add(id);
			}
			else
			{
				ListRemove(hiddenContracts, id);
				activeContracts.Add(id);
			}
		}

		public void SortChildren(List<Guid> sortedList)
		{
			if (missionInterface == null)
				return;

			if (ContractSectionTransform == null)
				return;

			int l = sortedList.Count;

			for (int i = l - 1; i >= 0; i--)
			{
				Guid id = sortedList[i];

				CW_ContractSection c = GetContract(id);

				if (c == null)
					continue;

				c.transform.SetParent(null);
			}			

			for (int i = 0; i < l; i++)
			{
				Guid id = sortedList[i];

				CW_ContractSection c = GetContract(id);

				if (c == null)
					continue;

				c.transform.SetParent(ContractSectionTransform);
			}
		}

		public void UpdateChildren()
		{
			foreach (CW_ContractSection contract in masterList.Values)
			{
				if (contract == null)
					continue;

				contract.UpdateContract();
			}
		}

		public void RefreshContract(IContractSection contract)
		{
			if (contract == null)
				return;

			CW_ContractSection c = GetContract(contract.ID);

			if (c == null)
				return;

			c.RefreshParameters();
		}

		public void RemoveContract(Guid id)
		{
			if (id == null)
				return;

			if (ListRemove(activeContracts, id) || ListRemove(hiddenContracts, id))
				RemoveFromMasterList(id);
			
			if (CW_Window.Window != null)
				CW_Window.Window.UpdateTooltips();
		}

		private void RemoveFromMasterList(Guid id)
		{
			if (masterList.ContainsKey(id))
			{
				masterList[id].gameObject.SetActive(false);
				Destroy(masterList[id]);
				masterList.Remove(id);
			}
		}

		private bool ListRemove(List<Guid> list, Guid id)
		{
			if (list.Contains(id))
			{
				list.Remove(id);
				return true;
			}

			return false;
		}

		public void SetMissionVisible(bool isOn)
		{
			if (missionInterface == null)
				return;

			missionInterface.IsVisible = isOn;
			
			gameObject.SetActive(isOn);
		}

		public void ToggleContracts(bool showHidden)
		{
			for (int i = activeContracts.Count - 1; i >= 0; i--)
			{
				Guid id = activeContracts[i];

				if (id == null)
					continue; ;

				if (!masterList.ContainsKey(id))
					continue;

				masterList[id].gameObject.SetActive(!showHidden);
			}

			for (int i = hiddenContracts.Count - 1; i >= 0; i--)
			{
				Guid id = hiddenContracts[i];

				if (id == null)
					continue; ;

				if (!masterList.ContainsKey(id))
					continue;

				masterList[id].gameObject.SetActive(showHidden);
			}
		}

		public string MissionTitle
		{
			get { return missionTitle; }
		}

		public void DestroyChild(GameObject obj)
		{
			if (obj == null)
				return;

			Destroy(obj);
		}
	}
}
