using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColorBasedOnTeam : MonoBehaviour
{
    [SerializeField] private int teamInt = 0;
    private enum matchingColorSetting { not, player, UI }
    [SerializeField] private matchingColorSetting autoColor = matchingColorSetting.not;
    public bool turnOffIfNotMatching = false;
    private GameMaster gm = null;
    private PlayerControls p = null;
    private UIWorker ui = null;

    private Material material = null;
    private Image image = null;
    private Light ourLight = null;


    private void GetGMAndPlayer()
    {
        if(gm == null)
            gm = Qk.GM;
        if(p == null)
            p = Qk.Player;

        if (ui == null && autoColor == matchingColorSetting.UI)
            ui = Qk.UIWorker;
    }

    private void GetMaterial()
    {
        GetGMAndPlayer();
        material = GetComponent<Renderer>().material;
    }

    private void GetImage()
    {
        GetGMAndPlayer();
        image = GetComponent<Image>();
    }

    private void GetLight()
    {
        GetGMAndPlayer();
        ourLight = GetComponent<Light>();
    }

    private void UpdateTeam()
    {
        if (autoColor == matchingColorSetting.player && p.CheckTeam() != null)
            teamInt = PlayerControls.playingInt;
        else if (autoColor == matchingColorSetting.UI)
            teamInt = ui.teamInt;
    }

    public void ChangeColorMaterial()
    {
        if (material == null)
            GetMaterial();

        UpdateTeam();

        material.color = gm.teamColors[teamInt % gm.teamColors.Count];
    }

    public void ChangeColorLight()
    {
        if (ourLight == null)
            GetLight();

        UpdateTeam();

        if (turnOffIfNotMatching)
        {
            switch(autoColor)
            {
                case (matchingColorSetting.not):
                    break;
                case matchingColorSetting.player:
                    if (teamInt != Qk.Player.teamInt)
                        ourLight.gameObject.SetActive(false);
                    else
                        ourLight.gameObject.SetActive(true);
                    break;
                case matchingColorSetting.UI:
                    if(teamInt != Qk.UIWorker.teamInt)
                        ourLight.gameObject.SetActive(false);
                    else
                        ourLight.gameObject.SetActive(true);
                    break;
            }
        }

        ourLight.color = gm.teamColors[teamInt % gm.teamColors.Count];
    }

    public void ChangeColorImage()
    {
        if(image == null)
            GetImage();

        UpdateTeam();

        if(image != null)
            image.color = gm.teamColors[teamInt % gm.teamColors.Count];
    }
}
