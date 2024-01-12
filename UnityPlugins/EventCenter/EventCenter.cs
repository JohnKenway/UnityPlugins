using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum eEventType { 
    changeLanguageText,
    showCharacterInfo,
    offScrollViewRay,
    openScrollViewRay
}

public class EventCenter : MonoBehaviour {

    public static Dictionary<eEventType, Delegate> m_EventTable = new Dictionary<eEventType, Delegate>();

    public static void OnListenerAdding(eEventType eventType, Delegate callBack) {
        if (!m_EventTable.ContainsKey(eventType)) { 
            m_EventTable.Add(eventType, callBack);
        }

        Delegate d = m_EventTable[eventType];

        if (d != null && d.GetType() != callBack.GetType()) {
            throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托，当前事件对应类型的委托为{1}，要添加的委托为{2}", eventType, d.GetType(), callBack.GetType()));
        }
    }

    public static void OnListenerRemoving(eEventType eventType, Delegate callBack) {
        if (m_EventTable.ContainsKey(eventType)) {
            Delegate d = m_EventTable[eventType];
            if (d == null) {
                throw new Exception(string.Format("移除事件错误：事件{0}没有对应的委托", eventType));
            } else if (d.GetType() != callBack.GetType()) {
                throw new Exception(string.Format("移除事件错误：尝试为事件{0}移除不同类型的委托，当前事件委托为{1}，要移除的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
            }
        } else {
            throw new Exception(string.Format("移除事件错误：没有事件码{0}", eventType));
        }
    }

    public static void OnListenerRemoved(eEventType eventType) {
        if (m_EventTable[eventType] == null) {
            m_EventTable.Remove(eventType);
        }
    }

    public static void AddListener(eEventType eventType, CallBack callBack) {
        OnListenerAdding(eventType, callBack);
        m_EventTable[eventType] = (CallBack)m_EventTable[eventType] + callBack;
    }

    public static void AddListener<T>(eEventType eventType, CallBack<T> callBack) {
        OnListenerAdding(eventType, callBack);
        m_EventTable[eventType] = (CallBack<T>)m_EventTable[eventType] + callBack;
    }


    public static void RemoveListener(eEventType eventType, CallBack callBack) {
        OnListenerRemoving(eventType, callBack);
        m_EventTable[eventType] = (CallBack)m_EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);
    }

    public static void RemoveListener<T>(eEventType eventType, CallBack<T> callBack) {
        OnListenerRemoving(eventType, callBack);
        m_EventTable[eventType] = (CallBack<T>)m_EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);
    }

    public static void BroadCast(eEventType eventType) {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d)) {
            CallBack callBack = d as CallBack;
            if (callBack != null) {
                callBack();
            } else {
                throw new Exception(string.Format("广播事件错误：事件{0}的委托具有不同类型", eventType));
            }
        }
    }

    public static void BroadCast<T>(eEventType eventType, T arg) {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d)) {
            CallBack<T> callBack = d as CallBack<T>;
            if (callBack != null) {
                callBack(arg);
            } else {
                throw new Exception(string.Format("广播事件错误：事件{0}的委托具有不同类型", eventType));
            }
        }
    }



}
