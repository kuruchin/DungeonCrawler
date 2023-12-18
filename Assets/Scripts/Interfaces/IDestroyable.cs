using UnityEngine;
using System;

public interface IDestroyable
{
    Action<OnDestroyArgs> onDestroy { get; set; }
}

public class OnDestroyArgs : EventArgs
{
    public GameObject gameObject;
    public AmmoType ammoType;
}