using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using Photon.Pun;

public class TabMenu : MonoBehaviour
{
    public static TabMenu Instance;
    [SerializeField] private GameObject tabMenuPanel;
    [SerializeField] private PhotonView pv;
    [SerializeField] private TabMenuItem tabMenuItem;
    public Transform tabList;
    
    public void Awake()
    {
        Instance = this;
        pv = GetComponent<PhotonView>();
    }

    public void Add(Role player)
    {
        Instantiate(tabMenuItem, tabList).GetComponent<TabMenuItem>().SetUp(player);
        UpdateTabItem();
    }
    
    private void UpdateTabItem()
    {
        
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowTab();    
        }
        else
        {
            HideTab();
        }
    }

    private void ShowTab()
    {
        
    }

    private void HideTab()
    {
        
    }
}
