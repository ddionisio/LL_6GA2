using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuildItemInfo {
    public GridEntityData data;    
    public Bounds bounds;
    public int[,] heightGrid; //[row, col] = height (0 if empty)
    public GridEntityEditController[] entityEdits;
}

public class GridBuildController : MonoBehaviour {
    [Header("Display")]
    public Transform root;

    [Header("Build Animation")]
    public float buildRiseDelay = 1f;
    public DG.Tweening.Ease buildRiseEase = DG.Tweening.Ease.InOutSine;
    public ParticleSystem buildRiseFX;

    [Header("Animation")]
    public M8.Animator.Animate fallAnimator;
    [M8.Animator.TakeSelector(animatorField = "fallAnimator")]
    public string fallTakePlay;

    [Header("Signal Invoke")]
    public M8.SignalVector3 signalInvokeCameraPanTo;
    public M8.Signal signalInvokeBuildComplete;
    

    private int[,] mHeightMap;

    void OnDestroy() {
        if(GridEditController.isInstantiated)
            GridEditController.instance.editChangedCallback -= OnEditModeChanged;
    }

    void Awake() {
        var editCtrl = GridEditController.instance;
        var gridCtrl = editCtrl.entityContainer.controller;

        editCtrl.editChangedCallback += OnEditModeChanged;

        mHeightMap = new int[gridCtrl.cellSize.row, gridCtrl.cellSize.col];

        if(fallAnimator)
            fallAnimator.gameObject.SetActive(false);
    }

    void OnEditModeChanged() {
        if(GridEditController.instance.editMode == GridEditController.EditMode.Build) {
            StartCoroutine(DoBuild());
        }
    }

    IEnumerator DoBuild() {
        yield return null;

        var buildRiseEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(buildRiseEase);

        var editCtrl = GridEditController.instance;
        var gridCtrl = editCtrl.entityContainer.controller;
        var evals = editCtrl.goalEvaluations;

        var unitSize = gridCtrl.unitSize;
        var unitSizeHalf = unitSize * 0.5f;
        var unitSizeQuart = unitSizeHalf * 0.5f;

        Bounds bounds = new Bounds();
        Vector3 minBound, maxBound;
        int minRow, minCol, maxRow, maxCol;
        int rowCount, colCount;
        int maxHeight;

        for(int i = 0; i < evals.Length; i++) {
            var eval = evals[i];

            //initiate info
            minBound = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxBound = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            minRow = minCol = int.MaxValue; maxRow = maxCol = int.MinValue;
            maxHeight = 0;

            for(int j = 0; j < eval.entityEdits.Length; j++) {
                var ent = eval.entityEdits[j].entity;

                var entTrans = ent.transform;
                var entBound = ent.bounds;
                var entCellInd = ent.cellIndex;
                var entCellIndEnd = ent.cellEnd;

                var entBoundMin = entTrans.localPosition + entBound.min;
                var entBoundMax = entTrans.localPosition + entBound.max;

                if(entBoundMin.x < minBound.x)
                    minBound.x = entBoundMin.x;
                if(entBoundMin.y < minBound.y)
                    minBound.y = entBoundMin.y;
                if(entBoundMin.z < minBound.z)
                    minBound.z = entBoundMin.z;

                if(entBoundMax.x > maxBound.x)
                    maxBound.x = entBoundMax.x;
                if(entBoundMax.y > maxBound.y)
                    maxBound.y = entBoundMax.y;
                if(entBoundMax.z > maxBound.z)
                    maxBound.z = entBoundMax.z;

                if(entCellInd.row < minRow)
                    minRow = entCellInd.row;
                if(entCellIndEnd.row > maxRow)
                    maxRow = entCellIndEnd.row;

                if(entCellInd.col < minCol)
                    minCol = entCellInd.col;
                if(entCellIndEnd.col > maxCol)
                    maxCol = entCellIndEnd.col;

                if(ent.cellSize.b > maxHeight)
                    maxHeight = ent.cellSize.b;
            }

            bounds.min = minBound;
            bounds.max = maxBound;

            var startY = bounds.min.y - (bounds.size.y + unitSize);
            var endY = bounds.min.y;

            rowCount = maxRow - minRow + 1;
            colCount = maxCol - minCol + 1;

            ResetHeightMap(rowCount, colCount);

            //populate height map
            for(int j = 0; j < eval.entityEdits.Length; j++) {
                var ent = eval.entityEdits[j].entity;

                var entCellInd = ent.cellIndex;
                var entCellSize = ent.cellSize;
                
                for(int r = 0; r < entCellSize.row; r++) {
                    for(int c = 0; c < entCellSize.col; c++)
                        mHeightMap[(entCellInd.row - minRow) + r, (entCellInd.col - minCol) + c] = entCellSize.b;
                }
            }

            //generate container
            var containerGO = new GameObject("build" + i);

            var container = containerGO.transform;
            container.SetParent(root, false);
            container.localPosition = new Vector3(bounds.center.x, startY, bounds.center.z); //start fully submerged

            //construct tiles
            var dat = eval.data;

            for(int r = 0; r < rowCount; r++) {
                var pos = new Vector3(-bounds.extents.x + unitSizeHalf, 0f, -bounds.extents.z + r * unitSize + unitSizeHalf);

                for(int c = 0; c < colCount; c++) {
                    var height = mHeightMap[r, c];

                    if(height == 1) {
                        GridBuildTileData tile;

                        //determine which template to grab, priority: top, base, bottom
                        if(dat.buildTileTop)
                            tile = dat.buildTileTop;
                        else if(dat.buildTileBase)
                            tile = dat.buildTileBase;
                        else
                            tile = dat.buildTileBottom;

                        if(tile)
                            GenerateTiles(tile, container, rowCount, colCount, r, c, height, true, pos, unitSizeQuart);
                    }
                    else if(height >= 2) {
                        //generate bottom
                        GridBuildTileData bottomTile;
                        if(dat.buildTileBottom)
                            bottomTile = dat.buildTileBottom;
                        else if(dat.buildTileBase)
                            bottomTile = dat.buildTileBase;
                        else
                            bottomTile = dat.buildTileTop;

                        if(bottomTile)
                            GenerateTiles(bottomTile, container, rowCount, colCount, r, c, 1, false, pos, unitSizeQuart);

                        //generate base
                        GridBuildTileData baseTile;
                        if(dat.buildTileBase)
                            baseTile = dat.buildTileBase;
                        else if(dat.buildTileBottom)
                            baseTile = dat.buildTileBottom;
                        else
                            baseTile = dat.buildTileTop;

                        if(baseTile) {
                            for(int h = 2; h < height; h++) {
                                GenerateTiles(baseTile, container, rowCount, colCount, r, c, height, true, new Vector3(pos.x, pos.y + (h - 1) * unitSize, pos.z), unitSizeQuart);
                            }
                        }

                        //generate top
                        GridBuildTileData topTile;
                        if(dat.buildTileTop)
                            topTile = dat.buildTileTop;
                        else if(dat.buildTileBase)
                            topTile = dat.buildTileBase;
                        else
                            topTile = dat.buildTileBottom;

                        if(topTile)
                            GenerateTiles(topTile, container, rowCount, colCount, r, c, height, true, new Vector3(pos.x, pos.y + (height - 1) * unitSize, pos.z), unitSizeQuart);
                    }

                    pos.x += unitSize;
                }
            }

            yield return null;

            var centerBasePos = new Vector3(bounds.center.x, endY, bounds.center.z);

            if(signalInvokeCameraPanTo)
                signalInvokeCameraPanTo.Invoke(centerBasePos);

            //animate crash into
            if(fallAnimator && !string.IsNullOrEmpty(fallTakePlay)) {
                fallAnimator.transform.position = root.TransformPoint(centerBasePos);

                fallAnimator.gameObject.SetActive(true);

                yield return fallAnimator.PlayWait(fallTakePlay);

                fallAnimator.gameObject.SetActive(false);
            }

            //build rise
            buildRiseFX.transform.localPosition = centerBasePos;

            //apply shape to fx
            var buildRiseFXShape = buildRiseFX.shape;
            var buildRiseFXShapeScale = buildRiseFXShape.scale;
            buildRiseFXShapeScale.x = bounds.size.x;
            buildRiseFXShapeScale.z = bounds.size.z;
            buildRiseFXShape.scale = buildRiseFXShapeScale;

            buildRiseFX.Play();

            var curTime = 0f;
            while(curTime < buildRiseDelay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = buildRiseEaseFunc(curTime, buildRiseDelay, 0f, 0f);

                float y = Mathf.Lerp(startY, endY, t);

                container.localPosition = new Vector3(bounds.center.x, y, bounds.center.z);
            }

            buildRiseFX.Stop();
        }


        if(signalInvokeBuildComplete)
            signalInvokeBuildComplete.Invoke();

        editCtrl.editMode = GridEditController.EditMode.BuildComplete;
    }

    private void GenerateTiles(GridBuildTileData tile, Transform container, int rowCount, int colCount, int r, int c, int height, bool isTop, Vector3 center, float cornerOfs) {
        var filledFlags = GridBuildTileData.GetFilledEdgeFlags(mHeightMap, rowCount, colCount, r, c, height);

        //upper left
        tile.InstantiateMesh(container, center, cornerOfs, GridBuildTileData.Flags.UpperLeft, filledFlags, isTop);

        //upper right
        tile.InstantiateMesh(container, center, cornerOfs, GridBuildTileData.Flags.UpperRight, filledFlags, isTop);

        //lower left
        tile.InstantiateMesh(container, center, cornerOfs, GridBuildTileData.Flags.LowerLeft, filledFlags, isTop);

        //lower right
        tile.InstantiateMesh(container, center, cornerOfs, GridBuildTileData.Flags.LowerRight, filledFlags, isTop);

        /*GameObject tileTemplate;

        //upper left
        tileTemplate = tile.GetVisibleGO(GridBuildTileData.Flags.UpperLeft, filledFlags, isTop);
        if(tileTemplate) {
            var tilePos = new Vector3(center.x - cornerOfs, 0f, center.z + cornerOfs);
            var tileInst = Instantiate(tileTemplate, container);
            tileInst.transform.localPosition = tilePos;
        }

        //upper right
        tileTemplate = tile.GetVisibleGO(GridBuildTileData.Flags.UpperRight, filledFlags, isTop);
        if(tileTemplate) {
            var tilePos = new Vector3(center.x + cornerOfs, 0f, center.z + cornerOfs);
            var tileInst = Instantiate(tileTemplate, container);
            tileInst.transform.localPosition = tilePos;
        }

        //lower left
        tileTemplate = tile.GetVisibleGO(GridBuildTileData.Flags.LowerLeft, filledFlags, isTop);
        if(tileTemplate) {
            var tilePos = new Vector3(center.x - cornerOfs, 0f, center.z - cornerOfs);
            var tileInst = Instantiate(tileTemplate, container);
            tileInst.transform.localPosition = tilePos;
        }

        //lower right
        tileTemplate = tile.GetVisibleGO(GridBuildTileData.Flags.LowerRight, filledFlags, isTop);
        if(tileTemplate) {
            var tilePos = new Vector3(center.x + cornerOfs, 0f, center.z - cornerOfs);
            var tileInst = Instantiate(tileTemplate, container);
            tileInst.transform.localPosition = tilePos;
        }*/
    }

    private void ResetHeightMap(int rowSize, int colSize) {
        for(int r = 0; r < rowSize; r++) {
            for(int c = 0; c < colSize; c++)
                mHeightMap[r, c] = 0;
        }
    }
}
