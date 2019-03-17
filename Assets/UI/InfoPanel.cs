using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : PanelBase {

    private Button closeBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "InfoPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        closeBtn = skinTrans.Find("CloseBtn").GetComponent<Button>();

        closeBtn.onClick.AddListener(OnCloseClick);
    }

    public void OnCloseClick()
    {
        Close();
    }

}
