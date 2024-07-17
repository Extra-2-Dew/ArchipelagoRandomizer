using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArchipelagoRandomizer
{
    public class Tabbable : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private Selectable me;
        private bool selected = false;

        private void Start()
        {
            me = GetComponent<Selectable>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
        }

        public void OnDeselect(BaseEventData eventData) 
        { 
            selected = false; 
        }

        private void Update()
        {
            if (selected)
            {
                if (!Input.GetKey(KeyCode.LeftAlt) || !Input.GetKey(KeyCode.RightAlt))
                {
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        Selectable target;
                        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                        {
                            target = me.FindSelectableOnDown();
                        }
                        else
                        {
                            target = me.FindSelectableOnUp();
                        }
                        if (target != null)
                        {
                            StartCoroutine(WaitToSelect(target));
                        }
                    }
                }
            }
        }

        // Believe it or not, this wait ISN'T to avoid a crash
        // Just need to make sure it's not the same frame you pressed Tab
        private IEnumerator WaitToSelect(Selectable target)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            target.Select();
            target.GetComponent<InputField>()?.ActivateInputField();
        }
    }
}
