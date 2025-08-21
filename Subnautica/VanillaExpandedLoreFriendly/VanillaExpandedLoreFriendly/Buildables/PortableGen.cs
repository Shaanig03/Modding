using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace VanillaExpandedLoreFriendly.Buildables
{
    public class PortableGen : MonoBehaviour
    {
        private StorageContainer _storageContainer;
        private Transform _storageRoot;
        private PowerSource _powerSource;

        private GameObject[] _display_powerCellModels = new GameObject[2];
        public Battery[] powerCells = new Battery[2];

        private TextMeshProUGUI txt;

        void Start()
        {
            _storageContainer = GetComponent<StorageContainer>();
            _powerSource = GetComponent<PowerSource>();
            _storageRoot = _storageContainer.storageRoot.transform;

            _storageContainer.container.onAddItem += Container_onAddItem;
            _storageContainer.container.onRemoveItem += Container_onRemoveItem;

            UpdatePowerCells();

            txt = transform.Find("Canvas/txt").GetComponent<TextMeshProUGUI>();

            StartCoroutine(IPowerStation());

            
        }

        void UpdateDisplay()
        {
            txt.text = $"{Mathf.Round((_powerSource.power / _powerSource.maxPower) * 100)}%";
        }
        System.Collections.IEnumerator IPowerStation()
        {

            WaitForSeconds delay = new WaitForSeconds(Vars.config.portablePowerStation_powerGenDelay);

            while(this != null)
            {
                UpdateDisplay();

                float power = _powerSource.power;
                float maxPower = _powerSource.maxPower;

                // if power hasn't reached max power
                if (power < maxPower)
                {
                    // get space left 
                    float spaceLeft = maxPower - power;
                    
                    // generating power units
                    float generatingPowerUnits = Vars.config.portablePowerStation_powerGenUnits;

                    // loop through each power cell
                    foreach (Battery _battery in powerCells)
                    {
                       

                        // if power cell is found
                        if(_battery != null)
                        {
                            float charge = _battery.charge;

                            // & has any charge left
                            if (charge >= 0)
                            {
                                float genValue = 0;

                                // if charge >= to generating power units (from config)
                                if (charge >= generatingPowerUnits)
                                {
                                    genValue = generatingPowerUnits;
                                }
                                else
                                {
                                    // else set gen value to remaining charge
                                    genValue = charge;
                                }

                                // if generating value is greater than left space
                                if (genValue > spaceLeft)
                                {
                                    // limit to space left
                                    genValue = spaceLeft;
                                }

                                //_powerSource.power += genValue;
                                float amountStored;
                                _powerSource.AddEnergy(genValue, out amountStored);

                                //_powerSource.AddEnergy(genValue, out amountStored);
                                _battery.charge -= genValue;
                                if (genValue > 0) { break; }
                            }

                        }
                        
                    }
                }
                yield return delay;
            }
        }

        private void Container_onAddItem(InventoryItem item)
        {
            UpdatePowerCells();
        }
        private void Container_onRemoveItem(InventoryItem item)
        {
            UpdatePowerCells(item, true);
        }

        private void UpdatePowerCells(InventoryItem removeadd_item = null, bool removing = false)
        {
            // destroy existing power cell models   
            for (int i = 0; i < _display_powerCellModels.Length; i++)
            {
                GameObject model = _display_powerCellModels[i];
                if (model != null) { UnityEngine.Object.Destroy(model); _display_powerCellModels[i] = null; } 
                  
            }
            // set power cells to null
            for (int i = 0; i < powerCells.Length; i++)
            {
                powerCells[i] = null;
            }

            // exit if there aren't any items
            if (_storageRoot.childCount == 0) { return; }

            //pc 1 pos: 0 0.472 0.13
            //pc 2 pos: 0 0.28 0.13
            //pc scale: 0.5 0.5 0.5

            int i2 = 0;
            // loop through each item 
            foreach (Transform _item in _storageRoot.transform)
            {
                // if item is found && and if its a power cell
                if (_item != null && _item.name.ToLower().Contains("powercell"))
                {
                    Battery battery = _item.GetComponent<Battery>();
                    Pickupable pickupable_item = _item.GetComponent<Pickupable>();

                    // if this is the item being removed then ignore this item
                    bool pass = true;
                    if(removing && removeadd_item.item == pickupable_item) { pass = false; }

                    // if battery component is found
                    if (battery != null && pass)
                    {
                        // store battery
                        powerCells[i2] = battery;

                        // create dummy power cell
                        GameObject displayDummy = GameObject.Instantiate(_item.gameObject);

                        // set pickupable to false
                        Pickupable pickupable = displayDummy.GetComponent<Pickupable>();
                        pickupable.isPickupable = false;

                        //GameObject.Destroy();
                        GameObject.Destroy(displayDummy.GetComponent<PrefabIdentifier>());


                        // set parent, pos & rot & scale
                        displayDummy.transform.SetParent(transform);
                        Vector3 localPos = Vector3.zero;
                        if (i2 == 0)
                        {
                            localPos = new Vector3(0, 0.472f, 0.13f);
                        }else if(i2 == 1){
                            localPos = new Vector3(0, 0.28f, 0.13f);
                        }
                        displayDummy.transform.localPosition = localPos;
                        displayDummy.transform.localRotation = Quaternion.identity;
                        displayDummy.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                        // activate dummy object
                        displayDummy.gameObject.SetActive(true);
                        

                        // store display dummy
                        _display_powerCellModels[i2] = displayDummy;

                    }
                }
                i2++;
            }

        }
    }
}
