using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static EasyLayoutNS.EasyLayoutFlexSettings;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class ScrollViewListener : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    //滑动方向
    public enum MoveDirection {
        None = 0,
        Left,
        Right,
    }
    public float SingleItemWidth;//单个滑动页的宽度
    public RectTransform content;//当前ScrollView的Content
    public float DragMinValue = 5f;//拖动过程中允许的最小的拖拽值，低于此值就不算拖拽，不执行翻页事件
    private MoveDirection direction = MoveDirection.None;
    private int CurIndex = 0;//当前页码
    private int MaxIndex = 3;//最大页码
    public bool canMove = true;//是否能移动
    private Vector3 originalPos;
    private float maxDeltaX = 0f;//取整个拖动过程中的最大值
    public Action<int> OnPageChange;//对外提供页码修改的回调

    private int getNextIndex() {
        if (direction == MoveDirection.Left) {
            if (CurIndex < MaxIndex) {
                return CurIndex++;
            }
        }
        if (direction == MoveDirection.Right) {
            if (CurIndex > 0) {
                return CurIndex--;
            }
        }
            return -1;
    }

    /// <summary>
    /// 滑到下一页
    /// </summary>
    public void MoveToNext(int targrtIndex) {
        Debug.Log("originalPos.x - SingleItemWidth * CurIndex = " + (originalPos.x - SingleItemWidth * CurIndex));
        if (targrtIndex < 0) {
            return;
        }
        canMove = false;
        content.DOLocalMoveX(originalPos.x - SingleItemWidth * CurIndex, 1f).OnComplete(() => {
            if (null != OnPageChange) {
                OnPageChange(CurIndex);
            }
            canMove = true;
        });
        /*
        var curPos = new Vector3(originalPos.x - SingleItemWidth * CurIndex, originalPos.y, originalPos.z);
        if (direction == MoveDirection.Left) {
            if (CurIndex < MaxIndex) {
                CurIndex++;
                canMove = false;
                content.DOLocalMoveX(curPos.x - SingleItemWidth, 1f).OnComplete(() => {
                    if (null != OnPageChange) {
                        OnPageChange(CurIndex);
                    }
                    canMove = true;
                });
            }
        } else if (direction == MoveDirection.Right) {
            if (CurIndex > 0) {
                CurIndex--;
                canMove = false;
                content.DOLocalMoveX(curPos.x + SingleItemWidth, 1f).OnComplete(() => {
                    if (null != OnPageChange) {
                        OnPageChange(CurIndex);
                    }
                    canMove = true;
                });
            }
        }
        */
    }

    /// <summary>
    /// 设置当前滑动列表的页数的最大值
    /// </summary>
    /// <param name="max"></param>
    public void SetMaxIndex(int max) {
        MaxIndex = max - 1;//最大下标值为页数减1
    }

    /// <summary>
    /// 设置当前页
    /// </summary>
    /// <param name="index"></param>
    public void SetCurIndex(int index) {
        CurIndex = index;
        float x = originalPos.x - SingleItemWidth * CurIndex;
        content.localPosition = new Vector3(x, content.localPosition.y, content.localPosition.z);
    }
    public void ResetPosition() {
        float x = originalPos.x - SingleItemWidth * CurIndex;
        canMove = false;
        content.DOLocalMoveX(x, 0.2f).OnComplete(() => {
            
            canMove = true;
        });
        //content.localPosition = new Vector3(x, originalPos.y, originalPos.z);
    }
    private void Awake() {
        CurIndex = 0;
        originalPos = new Vector3(0, 0, 0);
        Debug.Log(originalPos.x + " " + originalPos.y);
        EventCenter.AddListener(eEventType.openScrollViewRay, openScrollViewRay);
        EventCenter.AddListener(eEventType.offScrollViewRay, offScrollViewRay);

    }

    private void OnDestroy() {
        EventCenter.RemoveListener(eEventType.openScrollViewRay, openScrollViewRay);
        EventCenter.RemoveListener(eEventType.offScrollViewRay, offScrollViewRay);
    }

    public void openScrollViewRay() {
        gameObject.transform.GetComponent<Image>().raycastTarget = true;
    }

    public void offScrollViewRay() {
        gameObject.transform.GetComponent<Image>().raycastTarget = false;
    }
    public void OnDrag(PointerEventData eventData) {
        //if (Mathf.Abs(eventData.delta.x) > maxDeltaX) {
        //    maxDeltaX = Mathf.Abs(eventData.delta.x);
        //}
    }
    public void OnBeginDrag(PointerEventData eventData) {
        if (eventData.delta.x > 0) {
            direction = MoveDirection.Right;
        } else if (eventData.delta.x < 0) {
            direction = MoveDirection.Left;
        } else {
            direction = MoveDirection.None;
        }
        //if (Mathf.Abs(eventData.delta.x) > maxDeltaX) {
        //    maxDeltaX = Mathf.Abs(eventData.delta.x);
        //}
    }
    public void OnEndDrag(PointerEventData eventData) {
        /*
        if (Mathf.Abs(eventData.delta.x) > maxDeltaX) {
            maxDeltaX = Mathf.Abs(eventData.delta.x);
        }
        */
        float x = originalPos.x - SingleItemWidth * CurIndex;
        maxDeltaX = Mathf.Abs(x - content.localPosition.x);
        if (maxDeltaX > DragMinValue) {
            //计算下一页的目的点 然后移动
            if (canMove) {
                MoveToNext(getNextIndex());
            }
        } else {
            ResetPosition();
        }
        maxDeltaX = 0f;
    }
}