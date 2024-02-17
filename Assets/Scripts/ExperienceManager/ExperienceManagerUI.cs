using TMPro;
using UnityEngine;

public class ExperienceManagerUI : MonoBehaviour
{
    [SerializeField] ExperienceManager _experienceManager;
    [SerializeField] TextMeshProUGUI _levelIndicatorTmp, _experienceIndicatorTmp;
    void Start()
    {
        _experienceManager.OnExperienceChange += ExperienceManager_OnExperienceChange;
    }
    public void ExperienceManager_OnExperienceChange(object sender, ExperienceManager.ExperienceArgs experienceArgs) {
        _levelIndicatorTmp.text = experienceArgs.level.ToString();
        _experienceIndicatorTmp.text = experienceArgs.experience.ToString();
    }
}
