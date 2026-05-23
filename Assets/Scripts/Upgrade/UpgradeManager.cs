using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private GameManager gameManager;
    private GameUI gameUI;
    private int pendingUpgradeCount;
    private bool isShowingUpgrade;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    public void ShowUpgradeChoices()
    {
        QueueUpgradeChoices(1);
    }

    public void QueueUpgradeChoices(int count)
    {
        pendingUpgradeCount += Mathf.Max(0, count);
        ShowNextUpgradeIfNeeded();
    }

    private void ShowNextUpgradeIfNeeded()
    {
        if (gameUI == null)
        {
            gameUI = GetComponent<GameUI>();
        }

        if (isShowingUpgrade || pendingUpgradeCount <= 0)
        {
            return;
        }

        isShowingUpgrade = true;
        pendingUpgradeCount--;
        Time.timeScale = 0f;
        gameUI.ShowUpgradePanel(GetRandomOptions());
    }

    public void ApplyUpgrade(UpgradeOption option)
    {
        CharacterStats stats = gameManager.PlayerStats;

        if (option.Type == UpgradeType.AttackDamage)
        {
            stats.AttackDamage += 10;
        }
        else if (option.Type == UpgradeType.SkillDamage)
        {
            stats.SkillDamage += 5;
        }
        else if (option.Type == UpgradeType.MaxHealth)
        {
            gameManager.PlayerHealth.IncreaseMaxHealth(20);
            stats.MaxHealth = gameManager.PlayerHealth.MaxHealth;
        }
        else if (option.Type == UpgradeType.MoveSpeed)
        {
            stats.MoveSpeed *= 1.1f;
        }
        else if (option.Type == UpgradeType.AttackCooldown)
        {
            stats.AttackCooldown = Mathf.Max(0.08f, stats.AttackCooldown * 0.9f);
        }
        else if (option.Type == UpgradeType.Heal)
        {
            gameManager.PlayerHealth.Heal(40);
        }

        if (gameUI == null)
        {
            gameUI = GetComponent<GameUI>();
        }

        gameUI.HideUpgradePanel();
        isShowingUpgrade = false;
        gameManager.RefreshUI();
        if (pendingUpgradeCount > 0)
        {
            ShowNextUpgradeIfNeeded();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private List<UpgradeOption> GetRandomOptions()
    {
        List<UpgradeOption> pool = GameData.CreateUpgradeOptions();
        List<UpgradeOption> result = new List<UpgradeOption>();

        while (result.Count < 3 && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}
