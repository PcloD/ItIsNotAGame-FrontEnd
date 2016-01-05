﻿using UnityEngine;
using System.Collections;

using Regulus.Project.ItIsNotAGame1.Data;

public class ItemDescription : MonoBehaviour {

    Client _Client;

    public UnityEngine.UI.Text Name;
    public UnityEngine.UI.Text Effect;
    private IInventoryNotifier _InventoryNotifier;

    private System.Guid _Id ;

    void OnDestroy()
    {
        if (_Client != null)
        {
            _Client.User.InventoryNotifierProvider.Supply -= InventoryNotifierProvider_Supply;
        }
    }
    void Start () {
        _Client = Client.Instance;
        if (_Client != null)
        {
            _Client.User.InventoryNotifierProvider.Supply += InventoryNotifierProvider_Supply;
        }
    }

    private void InventoryNotifierProvider_Supply(IInventoryNotifier obj)
    {
        _InventoryNotifier = obj;
    }

    void Update ()
    {
	        
	}

    public void Set(Item item)
    {
        _Id = item.Id;
        Name.text = item.Name;
        string effectText = "";
        foreach(var effect in item.Effects)
        {
            effectText += effect.Type.ToString() + ":" + effect.Value;
        }
        Effect.text = effectText;
    }

    public void Discard()
    {
        _InventoryNotifier.Discard(_Id);
    }
}
