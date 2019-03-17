using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManageScene : MonoBehaviour {

    public static UIManageScene instance;
    public GameObject Panel;
    public Sprite imageWin;
    public Sprite imageFail;

    private void Start()
    {
        instance = this;
        Panel.SetActive(false);
    }

    public void OnPanelCloseBtnClick()
    {
        SceneManager.LoadScene("Start");
    }

    public void OpenPanel(bool isWin)
    {
        Battle.instance.ClearBattle();
        Transform Image = Panel.transform.Find("Image");
        Image bgImage = Image.GetComponent<Image>();
        if (isWin)
        {
            bgImage.sprite = imageWin;
        }
        else
        {
            bgImage.sprite = imageFail;
        }
        Panel.SetActive(true);
    }
}
