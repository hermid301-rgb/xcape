using UnityEngine;
using UnityEngine.UI;

namespace XCAPE.UI
{
    // Muestra/oculta un banner en el Main Menu si el proyecto no está vinculado a UGS
    public class UGSVerificationBanner : MonoBehaviour
    {
        [SerializeField] private Text messageText;
        [SerializeField] private string linkedOkMessage = ""; // vacío para ocultar
        [SerializeField] private string notLinkedMessage = "Proyecto no vinculado a UGS. Abre Tools > XCAPE > Setup Wizard y usa 'Link to UGS' o Project Settings > Services.";
        [SerializeField] private bool autoHideWhenLinked = true;
        [SerializeField] private float recheckSeconds = 2f;

        private void OnEnable()
        {
            UpdateBanner();
            CancelInvoke(nameof(UpdateBanner));
            InvokeRepeating(nameof(UpdateBanner), recheckSeconds, recheckSeconds);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(UpdateBanner));
        }

        private void UpdateBanner()
        {
            bool linked = !string.IsNullOrEmpty(Application.cloudProjectId);
            if (linked)
            {
                if (messageText) messageText.text = linkedOkMessage;
                if (autoHideWhenLinked) gameObject.SetActive(false);
            }
            else
            {
                if (messageText) messageText.text = notLinkedMessage;
                if (!gameObject.activeSelf) gameObject.SetActive(true);
            }
        }
    }
}
