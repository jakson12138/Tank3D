using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManage : MonoBehaviour {

    public GameObject panelTitle;
    public GameObject panelInfo;
    public GameObject panelStart;
    public Dropdown dropdown1;
    public Dropdown dropdown2;

    public static int n1;
    public static int n2;

    private void Start()
    {
        panelTitle.SetActive(true);
        panelInfo.SetActive(false);
        panelStart.SetActive(false);

        //Transform dropTrans1 = panelStart.transform.Find("Dropdown1");
        //dropdown1 = dropTrans1.GetComponent<Dropdown>();
        //Transform dropTrans2 = panelStart.transform.Find("Dropdown2");
        //dropdown2 = dropTrans1.GetComponent<Dropdown>();

        n1 = dropdown1.value + 1;
        n2 = dropdown2.value + 1;
    }

    public void OnPanelTitleStartBtnClick()
    {
        panelStart.SetActive(true);
    }

    public void OnPanelTitleInfoBtnClick()
    {
        //panelTitle.SetActive(false);
        panelInfo.SetActive(true); 
    }

    public void OnPanelInfoCloseBtnClick()
    {
        //panelTitle.SetActive(true);
        panelInfo.SetActive(false);
    }

    public void OnPanelStartStartBtnClick()
    {
        SceneManager.LoadScene("scene");
        //Battle.instance.StartTwoCampBattle(n1, n2);
    }

    public void OnPanelStartCloseBtnClick()
    {
        panelStart.SetActive(false);
    }

    public void OnPanelStartValueChange1()
    {
        n1 = dropdown1.value + 1;
    }

    public void OnPanelStartValueChange2()
    {
        n2 = dropdown2.value + 1;
    }
}
