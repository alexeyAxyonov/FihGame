using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : Entity, IDataPersistence
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame

    //public TextMeshProUGUI playerHealthUI;
    //public GameObject gameOverUI;

    public GameObject bloodyScreen; //aint workin'
    //public GameObject fadeOutScreen;

    private int deathCount;
    // BUG FIX: Entity.Awake is now virtual and initializes currentHealth. A plain
    // "private void Awake()" here HIDES it (CS0114) and skips that init, so this
    // must override and chain to base.Awake().
    protected override void Awake()
    {
        base.Awake();
        deathCount = 0;
    }
    private void Start()
    {
        Health = maxHealth;
        UIManager.Instance.playerHealthUI.text = $"HP: {Health}/{maxHealth}";
        Debug.Log($"Health: {Health}, maxHealth: {maxHealth}, UI text: {UIManager.Instance.playerHealthUI.text}");
    }
    public void LoadData(GameData data)
    {
        deathCount = data.deathCount;
    }
    public void SaveData(ref GameData data)
    {
        data.deathCount = deathCount;
    }
    public override void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitNormal, float distance)
    {
        base.TakeDamage(amount, hitPoint, hitNormal, distance);
        UIManager.Instance.playerHealthUI.text = $"HP: {Health}/{maxHealth}";
        //StartCoroutine(BloodyScreenEffect()); bloody screen is buggy due to Unity shenanigans
    }

    public override void Heal(int amount)
    {
        base.Heal(amount);
        UIManager.Instance.playerHealthUI.text = $"HP: {Health}/{maxHealth}";
    }

    private IEnumerator BloodyScreenEffect()
    {
        if (bloodyScreen.activeInHierarchy == false)
        {
            bloodyScreen.SetActive(true);
        }

        var image = bloodyScreen.GetComponentInChildren<Image>();

        // Set the initial alpha value to 1 (fully visible).
        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;

        float duration = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the new alpha value using Lerp.
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // Update the color with the new alpha value.
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;

            // Increment the elapsed time.
            elapsedTime += Time.deltaTime;

            yield return null; ; // Wait for the next frame.
        }


        if (bloodyScreen.activeInHierarchy == true)
        {
            bloodyScreen.SetActive(false);
        }
    }

    protected override void Die()
    {
        base.Die();
        //GetComponent<MouseMovement>().enabled = false;
        GetComponent<PlayerController>().enabled = false;
        UIManager.Instance.playerHealthUI.gameObject.SetActive(false);
        UIManager.Instance.fadeOutScreen.SetActive(true);
        
        
        StartCoroutine(ShowGameOverUI());
    }
    private IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSeconds(1f);
        UIManager.Instance.gameOverUI.SetActive(true);
    }
}
