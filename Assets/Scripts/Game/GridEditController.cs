using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles edit state
/// </summary>
public class GridEditController : MonoBehaviour {
    public enum Mode {
        None,
        Placement,
        Commit
    }
}
