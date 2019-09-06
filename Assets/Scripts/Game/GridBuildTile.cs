using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Grid Build Tile Data")]
public class GridBuildTileData : ScriptableObject {
    [System.Flags]
    public enum Flags {
        None = 0x0,
        Up = 0x1,
        Down = 0x2,
        Left = 0x4,
        Right = 0x8,

        //diagonals
        UpperLeft = 0x10,
        UpperRight = 0x20,
        LowerLeft = 0x40,
        LowerRight = 0x80
    }

    [Header("Corners")]
    public GameObject upperLeft;
    public GameObject upperRight;
    public GameObject lowerLeft;
    public GameObject lowerRight;

    [Header("Corners Invert")]
    public GameObject upperLeftInvert;
    public GameObject upperRightInvert;
    public GameObject lowerLeftInvert;
    public GameObject lowerRightInvert;

    [Header("Horizontals")]
    public GameObject horizontalFront;
    public GameObject horizontalBack;

    [Header("Verticals")]
    public GameObject verticalLeft;
    public GameObject verticalRight;

    [Header("Fill")]
    public GameObject fill;

    /// <summary>
    /// Returns which sides and corners are filled relative to given row/col
    /// </summary>
    public static Flags GetFilledEdgeFlags(int[,] heightMap, int heightMapRowCount, int heightmapColCount, int row, int col, int height) {
        Flags retFlags = Flags.None;

        //check sides
        if(row > 0 && heightMap[row - 1, col] >= height)
            retFlags |= Flags.Down;
        if(row < heightMapRowCount - 1 && heightMap[row + 1, col] >= height)
            retFlags |= Flags.Up;

        if(col > 0 && heightMap[row, col - 1] >= height)
            retFlags |= Flags.Left;
        if(col < heightmapColCount - 1 && heightMap[row, col + 1] >= height)
            retFlags |= Flags.Right;

        //check diagonals
        if(row > 0 && col > 0 && heightMap[row - 1, col - 1] >= height)
            retFlags |= Flags.LowerLeft;

        if(row > 0 && col < heightmapColCount - 1 && heightMap[row - 1, col + 1] >= height)
            retFlags |= Flags.LowerRight;

        if(row < heightMapRowCount - 1 && col > 0 && heightMap[row + 1, col - 1] >= height)
            retFlags |= Flags.UpperLeft;

        if(row < heightMapRowCount - 1 && col < heightmapColCount - 1 && heightMap[row + 1, col + 1] >= height)
            retFlags |= Flags.UpperRight;

        return retFlags;
    }

    public GameObject GetVisibleGO(Flags corner, Flags filledEdgeFlags, bool isTop) {
        switch(corner) {
            case Flags.UpperLeft:
                //check if up/left is empty
                if((filledEdgeFlags & (Flags.Up | Flags.Left)) == Flags.None)
                    return upperLeft;
                //check if up/left is filled, and upperleft is empty
                if(CheckFlags(filledEdgeFlags, Flags.Up | Flags.Left) && (filledEdgeFlags & Flags.UpperLeft) == Flags.None)
                    return upperLeftInvert;
                //check if left is filled, and up is empty
                if(CheckFlags(filledEdgeFlags, Flags.Left) && (filledEdgeFlags & Flags.Up) == Flags.None)
                    return horizontalFront;
                //check if up is filled, and left is empty
                if(CheckFlags(filledEdgeFlags, Flags.Up) && (filledEdgeFlags & Flags.Left) == Flags.None)
                    return verticalLeft;
                //check if up/left/upperleft is filled
                if(CheckFlags(filledEdgeFlags, Flags.Up | Flags.Left | Flags.UpperLeft))
                    return isTop ? fill : null;
                break;

            case Flags.UpperRight:
                //check if up/right is empty
                if((filledEdgeFlags & (Flags.Up | Flags.Right)) == Flags.None)
                    return upperRight;
                //check if up/right is filled, and upperright is empty
                if(CheckFlags(filledEdgeFlags, Flags.Up | Flags.Right) && (filledEdgeFlags & Flags.UpperRight) == Flags.None)
                    return upperRightInvert;
                //check if right is filled, and up is empty
                if(CheckFlags(filledEdgeFlags, Flags.Right) && (filledEdgeFlags & Flags.Up) == Flags.None)
                    return horizontalFront;
                //check if up is filled, and right is empty
                if(CheckFlags(filledEdgeFlags, Flags.Up) && (filledEdgeFlags & Flags.Right) == Flags.None)
                    return verticalRight;
                //check if up/right/upperright is filled
                if(CheckFlags(filledEdgeFlags, Flags.Up | Flags.Right | Flags.UpperRight))
                    return isTop ? fill : null;
                break;

            case Flags.LowerLeft:
                //check if down/left is empty
                if((filledEdgeFlags & (Flags.Down | Flags.Left)) == Flags.None)
                    return lowerLeft;
                //check if down/left is filled, and lowerleft is empty
                if(CheckFlags(filledEdgeFlags, Flags.Down | Flags.Left) && (filledEdgeFlags & Flags.LowerLeft) == Flags.None)
                    return lowerLeftInvert;
                //check if left is filled, and down is empty
                if(CheckFlags(filledEdgeFlags, Flags.Left) && (filledEdgeFlags & Flags.Down) == Flags.None)
                    return horizontalBack;
                //check if down is filled, and left is empty
                if(CheckFlags(filledEdgeFlags, Flags.Down) && (filledEdgeFlags & Flags.Left) == Flags.None)
                    return verticalLeft;
                //check if down/left/lowerleft is filled
                if(CheckFlags(filledEdgeFlags, Flags.Down | Flags.Left | Flags.LowerLeft))
                    return isTop ? fill : null;
                break;

            case Flags.LowerRight:
                //check if down/right is empty
                if((filledEdgeFlags & (Flags.Down | Flags.Right)) == Flags.None)
                    return lowerRight;
                //check if down/right is filled, and lowerright is empty
                if(CheckFlags(filledEdgeFlags, Flags.Down | Flags.Right) && (filledEdgeFlags & Flags.LowerRight) == Flags.None)
                    return lowerRightInvert;
                //check if right is filled, and down is empty
                if(CheckFlags(filledEdgeFlags, Flags.Right) && (filledEdgeFlags & Flags.Down) == Flags.None)
                    return horizontalBack;
                //check if down is filled, and right is empty
                if(CheckFlags(filledEdgeFlags, Flags.Down) && (filledEdgeFlags & Flags.Right) == Flags.None)
                    return verticalRight;
                //check if down/right/lowerright is filled
                if(CheckFlags(filledEdgeFlags, Flags.Down | Flags.Right | Flags.LowerRight))
                    return isTop ? fill : null;
                break;
        }

        return null;
    }

    private bool CheckFlags(Flags filledEdgeFlags, Flags checkFlags) {
        return (filledEdgeFlags & checkFlags) == checkFlags;
    }

    
}
