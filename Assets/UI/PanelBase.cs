using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour {

    public string skinPath;
    public GameObject skin;
    public PanelLayer layer;
    public object[] args;

    #region
    public virtual void Init(params object[] args)
    {
        this.args = args;
    }

    public virtual void OnShowing()
    {

    }

    public virtual void OnShowed()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void OnClosing()
    {

    }

    public virtual void OnClosed()
    {

    }
    #endregion

    #region
    protected virtual void Close()
    {
        string name = this.GetType().ToString();
        PanelMgr.instance.ClosePanel(name);
    }
    #endregion
    // Use this for initialization
    void Start () {
		
	}
}
