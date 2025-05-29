using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatBar : MonoBehaviour
{
	private Character currentCharacter;
    public Image healthImage;
    public Image healthDelayImage;
    public Image powerImage;
	private bool isRecovering;

	private void Update()
	{
		if(healthDelayImage.fillAmount > healthImage.fillAmount)
		{
			healthDelayImage.fillAmount -= Time.deltaTime;
		}

		if(isRecovering)
		{
			float persentage = currentCharacter.currentPower / currentCharacter.maxPower;
			powerImage.fillAmount =persentage;
			if(persentage >= 1)
			{
				isRecovering = false;
				return;
			}
		}
	}
	//调整血量
	public void OnHealthChange(float persentage)
    {
        healthImage.fillAmount = persentage;
    }

	//调整能量条
	public void OnPowerChange(Character character)
	{
		isRecovering = true;
		currentCharacter = character;
	}
}
