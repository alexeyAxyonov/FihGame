using UnityEngine;
using UnityEngine.UI;

public class DumbScript : MonoBehaviour
{
    [SerializeField] private Image myImage;
    [SerializeField] private Sprite mySprite;
    void Start()
    {
        myImage.sprite = mySprite;
    }
}
