using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image _healthBar;

    [SerializeField]
    private float _speed = 2f;
    private float _target = 1f;

    private void Start()
    {
        _healthBar.fillAmount = 1f;
    }

    public void UpdateHealth(float maxHealth, float currentHealth)
    {
        _target = currentHealth / maxHealth;
    }

    private void SetHealthBar(float healthNormalized)
    {
        _healthBar.fillAmount = healthNormalized;
    }

    public bool IsAtTarget()
    {
        return Mathf.Approximately(_healthBar.fillAmount, _target);
    }

    private void Update()
    {
        if (_healthBar.fillAmount != _target)
        {
            float fillAmount = Mathf.MoveTowards(
                _healthBar.fillAmount,
                _target,
                _speed * Time.deltaTime
            );
            SetHealthBar(fillAmount);
        }
    }
}
