using UnityEngine;
using UnityEngine.EventSystems;

public class UiAudioFeedback : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField] private CustomAudioClip hoverSound;
    [SerializeField] private CustomAudioClip clickSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioPlayer.Instance.PlaySoundEffect(hoverSound);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AudioPlayer.Instance.PlaySoundEffect(clickSound);
    }
}
