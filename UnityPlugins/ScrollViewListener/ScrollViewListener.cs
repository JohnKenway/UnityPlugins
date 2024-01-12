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
    //��������
    public enum MoveDirection {
        None = 0,
        Left,
        Right,
    }
    public float SingleItemWidth;//��������ҳ�Ŀ��
    public RectTransform content;//��ǰScrollView��Content
    public float DragMinValue = 5f;//�϶��������������С����קֵ�����ڴ�ֵ�Ͳ�����ק����ִ�з�ҳ�¼�
    private MoveDirection direction = MoveDirection.None;
    private int CurIndex = 0;//��ǰҳ��
    private int MaxIndex = 3;//���ҳ��
    public bool canMove = true;//�Ƿ����ƶ�
    private Vector3 originalPos;
    private float maxDeltaX = 0f;//ȡ�����϶������е����ֵ
    public Action<int> OnPageChange;//�����ṩҳ���޸ĵĻص�

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
    /// ������һҳ
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
    /// ���õ�ǰ�����б��ҳ�������ֵ
    /// </summary>
    /// <param name="max"></param>
    public void SetMaxIndex(int max) {
        MaxIndex = max - 1;//����±�ֵΪҳ����1
    }

    /// <summary>
    /// ���õ�ǰҳ
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
            //������һҳ��Ŀ�ĵ� Ȼ���ƶ�
            if (canMove) {
                MoveToNext(getNextIndex());
            }
        } else {
            ResetPosition();
        }
        maxDeltaX = 0f;
    }
}