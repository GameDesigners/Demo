using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MesageTag
{
    System,
}

/// <summary>
/// 游戏消息处理系统
/// </summary>
public class GameMessageProc
{
    /// <summary>
    /// 消息对象
    /// </summary>
    private struct MSGObject
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="handler">处理的事件</param>
        public MSGObject(object sender,EventHandler<EventArgs> handler)
        {
            this.m_objSend = sender;
            this.m_handler = handler;
        }

        public object m_objSend;
        public EventHandler<EventArgs> m_handler;
    }


    private Dictionary<int, List<MSGObject>> m_msgList;
    private GameMessageProc m_instance;
    public GameMessageProc Instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new GameMessageProc();
            return m_instance;
        }

        private set { m_instance = value; }
    }



    private GameMessageProc()
    {
        m_msgList = new Dictionary<int, List<MSGObject>>();
    }

    

    /// <summary>
    /// 向消息系统添加消息
    /// </summary>
    /// <param name="msgType">消息类型</param>
    /// <param name="sender">消息发送者</param>
    /// <param name="handler">事件</param>
    /// <returns></returns>
    public bool AddMessage(int msgType,object sender,EventHandler<EventArgs> handler)
    {
        List<MSGObject> msgObjects;
        if(m_msgList.TryGetValue(msgType,out msgObjects))
        {
            MSGObject msgObj = new MSGObject(sender, handler);
            if (msgObjects.Contains(msgObj))
            {
                Debug.LogWarning("[Warning]已经存在此消息对象...");
                return false;
            }

            m_msgList[msgType].Add(msgObj);
        }
        else
        {
            msgObjects = new List<MSGObject>();
            msgObjects.Add(new MSGObject(sender, handler));
            m_msgList.Add(msgType, msgObjects);
        }

        return true;
    }

    /// <summary>
    /// 删除某一个消息
    /// </summary>
    /// <param name="msgType">消息类型</param>
    /// <param name="sender">消息发送者</param>
    /// <param name="handler">消息事件</param>
    /// <returns></returns>
    public bool RemoveMssage(int msgType,object sender, EventHandler<EventArgs> handler=null)
    {

        bool isDeleted = false;
        List<MSGObject> msgObjects;

        if(m_msgList.TryGetValue(msgType, out msgObjects))
        {
            MSGObject msgObj = new MSGObject(sender, handler);
            if(handler!=null)
            {
                if (msgObjects.Contains(msgObj))
                {
                    msgObjects.Remove(msgObj);
                    isDeleted = true;
                }
            }
            else
            {
                /*为了删除干净，注意这里使用了倒序遍历*/
                for(int index=msgObjects.Count-1;index>=0;index--)
                {
                    if (msgObjects[index].m_objSend == sender)
                    {
                        isDeleted = true;
                        msgObjects.Remove(msgObjects[index]);
                    }
                }
                return true;
            }
        }

        if (!isDeleted)
            Debug.LogWarning("[Warning]不存在消息对象...");

        return isDeleted;
    }


    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="msgType">消息类型</param>
    /// <param name="eventArgs">消息参数</param>
    public void SendMessage(int msgType,EventArgs eventArgs)
    {
        List<MSGObject> msgObjects;
        if(m_msgList.TryGetValue(msgType,out msgObjects))
        {
            for(int index=0;index<msgObjects.Count;index++)
            {
                msgObjects[index].m_handler(msgObjects[index].m_objSend, eventArgs);
            }
        }
    }

}
