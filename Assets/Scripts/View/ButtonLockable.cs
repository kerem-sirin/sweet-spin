using UnityEngine;
using UnityEngine.UI;

namespace SweetSpin
{
    public class ButtonLockable : UILockableComponent
    {
        private Button button;

        protected override void Start()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"ButtonLockable: No Button component found on {gameObject.name}");
            }
            base.Start();
        }

        protected override void SetLocked(bool locked)
        {
            if (button != null)
            {
                button.interactable = !locked;
            }
        }
    }
}