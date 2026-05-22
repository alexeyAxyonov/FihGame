using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : Entity
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame

    public GameObject bloodyScreen;
    private void Start()
    {
        Health = maxHealth;
    }

    public override void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitNormal, float distance)
    {
        base.TakeDamage(amount, hitPoint, hitNormal, distance);
        StartCoroutine(BloodyScreenEffect());
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
        Debug.Log("player dead!");
    }
}
