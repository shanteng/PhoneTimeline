using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class TestLoad : MonoBehaviour
{
    public GameObject TestUI;
    public Button _btnLoad;
    public Transform _Canvas;
    public PlayableDirector _testTimeLine;
    private PhoneTimeLineUi _view;

    private void Awake()
    {
        this._btnLoad.onClick.AddListener(LoadShowPanel);
    }

    private void LoadShowPanel()
    {
        _testTimeLine.Play();
        if (this._view == null)
        {
            GameObject obj = Resources.Load<GameObject>("PhoneTimeLineUi");
            GameObject ui = GameObject.Instantiate(obj, _Canvas, false);
            var rectForm = ui.GetComponent<RectTransform>();
            var offmini = rectForm.offsetMin;
            var offmax = rectForm.offsetMax;
            rectForm.offsetMax = Vector2.zero;
            rectForm.offsetMin = Vector2.zero;
            rectForm.localScale = Vector3.one;
            rectForm.localPosition = Vector3.zero;
            this._view = ui.GetComponent<PhoneTimeLineUi>();
        }
        this._view.transform.SetAsLastSibling();
        this._view.gameObject.SetActive(true);
        this.TestUI.SetActive(false);
        this._view.Init();
    }

}//end class