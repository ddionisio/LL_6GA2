using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuildTile : MonoBehaviour {
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
    public GameObject frontHorizontal;
    public GameObject backHorizontal;

    [Header("Verticals")]
    public GameObject leftVertical;
    public GameObject rightVertical;

    [Header("Fill")]
    public GameObject fill;

    /// <summary>
    /// Returns which sides and corners are filled relative to given row/col
    /// </summary>
    public Flags GetFilledEdgeFlags(int[,] heightMap, int heightMapRowCount, int heightmapColCount, int row, int col, int height) {
        Flags retFlags = Flags.None;

        //check sides
        if(row > 0 && heightMap[row - 1, col] >= height)
            retFlags |= Flags.Up;
        if(row < heightMapRowCount && heightMap[row + 1, col] >= height)
            retFlags |= Flags.Down;

        if(col > 0 && heightMap[row, col - 1] >= height)
            retFlags |= Flags.Left;
        if(col < heightmapColCount && heightMap[row, col + 1] >= height)
            retFlags |= Flags.Right;

        //check diagonals
        if(row > 0 && col > 0 && heightMap[row - 1, col - 1] >= height)
            retFlags |= Flags.UpperLeft;

        if(row > 0 && col < heightmapColCount && heightMap[row - 1, col + 1] >= height)
            retFlags |= Flags.UpperRight;

        if(row < heightMapRowCount && col > 0 && heightMap[row + 1, col - 1] >= height)
            retFlags |= Flags.LowerLeft;

        if(row < heightMapRowCount && col < heightmapColCount && heightMap[row + 1, col + 1] >= height)
            retFlags |= Flags.LowerRight;

        return retFlags;
    }

    public bool IsVisible(Flags corner, Flags filledEdgeFlags, bool isTop) {
        var visibleGO = GetVisibleGO(corner, filledEdgeFlags, isTop);
        return visibleGO != null;
    }

    public void ApplyVisible(Flags corner, Flags filledEdgeFlags, bool isTop) {
        var visibleGO = GetVisibleGO(corner, filledEdgeFlags, isTop);

        if(upperLeft) upperLeft.SetActive(upperLeft == visibleGO);
        if(upperRight) upperRight.SetActive(upperRight == visibleGO);
        if(lowerLeft) lowerLeft.SetActive(lowerLeft == visibleGO);
        if(lowerRight) lowerRight.SetActive(lowerRight == visibleGO);

        if(upperLeftInvert) upperLeftInvert.SetActive(upperLeftInvert == visibleGO);
        if(upperRightInvert) upperRightInvert.SetActive(upperRightInvert == visibleGO);
        if(lowerLeftInvert) lowerLeftInvert.SetActive(lowerLeftInvert == visibleGO);
        if(lowerRightInvert) lowerRightInvert.SetActive(lowerRightInvert == visibleGO);

        if(frontHorizontal) frontHorizontal.SetActive(frontHorizontal == visibleGO);
        if(backHorizontal) backHorizontal.SetActive(backHorizontal == visibleGO);

        if(leftVertical) leftVertical.SetActive(leftVertical == visibleGO);
        if(rightVertical) rightVertical.SetActive(rightVertical == visibleGO);

        if(fill) fill.SetActive(fill == visibleGO);
    }

    private GameObject GetVisibleGO(Flags corner, Flags filledEdgeFlags, bool isTop) {
        switch(corner) {
            case Flags.UpperLeft:
                //check if up/left is empty
                if((filledEdgeFlags & (Flags.Up | Flags.Left)) == Flags.None)
                    return upperLeft;
                //check if up/left is filled, and upperleft is empty
                if((filledEdgeFlags & (Flags.Up | Flags.Left)) != Flags.None && (filledEdgeFlags & Flags.UpperLeft) == Flags.None)
                    return upperLeftInvert;
                //check if left is filled, and up is empty
                if((filledEdgeFlags & Flags.Left) != Flags.None && (filledEdgeFlags & Flags.Up) == Flags.None)
                    return frontHorizontal;
                //check if up is filled, and left is empty
                if((filledEdgeFlags & Flags.Up) != Flags.None && (filledEdgeFlags & Flags.Left) == Flags.None)
                    return leftVertical;
                //check if up/left/upperleft is filled
                if((filledEdgeFlags & (Flags.Up | Flags.Left | Flags.UpperLeft)) != Flags.None)
                    return isTop ? fill : null;
                break;

            case Flags.UpperRight:
                //check if up/right is empty
                if((filledEdgeFlags & (Flags.Up | Flags.Right)) == Flags.None)
                    return upperRight;
                //check if up/right is filled, and upperright is empty
                if((filledEdgeFlags & (Flags.Up | Flags.Right)) != Flags.None && (filledEdgeFlags & Flags.UpperRight) == Flags.None)
                    return upperRightInvert;
                //check if right is filled, and up is empty
                if((filledEdgeFlags & Flags.Right) != Flags.None && (filledEdgeFlags & Flags.Up) == Flags.None)
                    return frontHorizontal;
                //check if up is filled, and right is empty
                if((filledEdgeFlags & Flags.Up) != Flags.None && (filledEdgeFlags & Flags.Right) == Flags.None)
                    return rightVertical;
                //check if up/right/upperright is filled
                if((filledEdgeFlags & (Flags.Up | Flags.Right | Flags.UpperRight)) != Flags.None)
                    return isTop ? fill : null;
                break;

            case Flags.LowerLeft:
                //check if down/left is empty
                if((filledEdgeFlags & (Flags.Down | Flags.Left)) == Flags.None)
                    return lowerLeft;
                //check if down/left is filled, and lowerleft is empty
                if((filledEdgeFlags & (Flags.Down | Flags.Left)) != Flags.None && (filledEdgeFlags & Flags.LowerLeft) == Flags.None)
                    return lowerLeftInvert;
                //check if left is filled, and down is empty
                if((filledEdgeFlags & Flags.Left) != Flags.None && (filledEdgeFlags & Flags.Down) == Flags.None)
                    return backHorizontal;
                //check if down is filled, and left is empty
                if((filledEdgeFlags & Flags.Down) != Flags.None && (filledEdgeFlags & Flags.Left) == Flags.None)
                    return leftVertical;
                //check if down/left/lowerleft is filled
                if((filledEdgeFlags & (Flags.Down | Flags.Left | Flags.LowerLeft)) != Flags.None)
                    return isTop ? fill : null;
                break;

            case Flags.LowerRight:
                //check if down/right is empty
                if((filledEdgeFlags & (Flags.Down | Flags.Right)) == Flags.None)
                    return lowerRight;
                //check if down/right is filled, and lowerright is empty
                if((filledEdgeFlags & (Flags.Down | Flags.Right)) != Flags.None && (filledEdgeFlags & Flags.LowerRight) == Flags.None)
                    return lowerRightInvert;
                //check if right is filled, and down is empty
                if((filledEdgeFlags & Flags.Right) != Flags.None && (filledEdgeFlags & Flags.Down) == Flags.None)
                    return backHorizontal;
                //check if down is filled, and right is empty
                if((filledEdgeFlags & Flags.Down) != Flags.None && (filledEdgeFlags & Flags.Right) == Flags.None)
                    return rightVertical;
                //check if down/right/lowerright is filled
                if((filledEdgeFlags & (Flags.Down | Flags.Right | Flags.LowerRight)) != Flags.None)
                    return isTop ? fill : null;
                break;
        }

        return null;
    }
}
