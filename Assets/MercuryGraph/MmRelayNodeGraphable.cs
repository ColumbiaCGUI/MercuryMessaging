using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MercuryMessaging;
using NewGraph;
using UnityEngine;
using Drawing;

public class MmRelayNodeGraphable : MmRelayNode
{
    // The node view that represents this graphable in the graph view.
    public NodeController nodeController;
    // The position of the graph node in the graph view.
    public Vector2 nodePosition = new Vector2(0, 0);

    public bool testToggle = false;

    // private bool signalingOn = false;

    // the private variable for the path on/off
    public GameManager gameManager;

    // private MmMessage signalMessage;

    public bool signalingOn = false;

    private float displayPeriod = 2.0f;

    private float time = 0.0f;

    private List<MmMessage> messageBuffer = new List<MmMessage>();

    public override void Start()
    {
        base.Start();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameObject.AddComponent<Outline>();
        gameObject.GetComponent<Outline>().enabled = false;
    }

    public void LateUpdate()
    {
        // get messaging items
        List<MmRoutingTableItem> itemsToGo = new List<MmRoutingTableItem>();

        // get the routing table child nodes
        List<MmRoutingTableItem> childItems = RoutingTable.GetMmRoutingTableItems(MmRoutingTable.ListFilter.All,MmLevelFilter.Child);

        if(signalingOn == false)
        {
            foreach (MmRoutingTableItem item in childItems)
            {
                Vector3 targetPosition = item.Responder.transform.position;
                Vector3 currentPosition = transform.position;
                if(gameManager.pathOn && item.Responder.gameObject.activeSelf)
                {
                    Draw3DArrow(currentPosition, targetPosition, Color.white);
                }
            }
        }
        // draw the path for 5 seconds
        else if( time <displayPeriod && signalingOn == true)
        {
            // increment the time
            time += Time.deltaTime;

            foreach (MmMessage message in messageBuffer)
            {
                List<MmRoutingTableItem> items = new List<MmRoutingTableItem>();
                if(message.MetadataBlock.LevelFilter == MmLevelFilter.Parent)
                {
                    items = RoutingTable.GetMmRoutingTableItems(MmRoutingTable.ListFilter.All,MmLevelFilter.Parent);
                }
                else if(message.MetadataBlock.LevelFilter == MmLevelFilter.Child)
                {
                    items = RoutingTable.GetMmRoutingTableItems(MmRoutingTable.ListFilter.All,MmLevelFilter.Child);
                }

                foreach (MmRoutingTableItem item in items)
                {
                    if(item.Tags == message.MetadataBlock.Tag || message.MetadataBlock.Tag == (MmTag)(-1))
                    {
                        itemsToGo.Add(item);
                    }
                }
            }
            // no messaging items
            List<MmRoutingTableItem> itemsRest = childItems.Except(itemsToGo).ToList();

            // draw messaging items
            foreach (MmRoutingTableItem item in itemsToGo)
            {
                Vector3 targetPosition = item.Responder.transform.position;
                Vector3 currentPosition = transform.position;
                
                if(gameManager.pathOn && item.Responder.gameObject.activeSelf)
                {
                    SignalVisualizer(currentPosition, targetPosition, time);
                    gameObject.GetComponent<Outline>().OutlineColor = Color.green;
                    gameObject.GetComponent<Outline>().OutlineWidth = 4f;
                    gameObject.GetComponent<Outline>().enabled = true;

                    item.Responder.gameObject.GetComponent<Outline>().OutlineColor = Color.green;
                    item.Responder.gameObject.GetComponent<Outline>().OutlineWidth = 4f;
                    item.Responder.gameObject.GetComponent<Outline>().enabled = true;

                }
            }

            // // draw the rest of the items
            // foreach (MmRoutingTableItem item in itemsRest)
            // {
            //     Vector3 targetPosition = item.Responder.transform.position;
            //     Vector3 currentPosition = transform.position;
            //     if(gameManager.pathOn && item.Responder.gameObject.activeSelf)
            //     {
            //         Draw3DArrow(currentPosition, targetPosition, Color.white);
            //     }
            // }

        }
        else if(time >= displayPeriod && signalingOn == true)
        {
            signalingOn = false;
            time = 0.0f;

            foreach (MmRoutingTableItem item in itemsToGo)
            {
                item.Responder.gameObject.GetComponent<Outline>().enabled = false;
            }
            gameObject.GetComponent<Outline>().enabled = false;
            
            messageBuffer.Clear();
        }

    }

    public override void MmInvoke(MmMessage message)
    {
        signalingOn = true;
        messageBuffer.Add(message);
        base.MmInvoke(message);
    }

    // draw a 3d arrow between two points
    public void Draw3DArrow(Vector3 from, Vector3 to, Color color)
    {
        var draw = Draw.ingame;
        Vector3 distance = to - from;

        int numArrows = (int)(distance.magnitude / 0.35f);

        for (int i = 1; i < numArrows-1; i++)
        {
            Vector3 pointA = Vector3.Lerp(from, to, i / (float)numArrows);
            
            draw.Arrowhead(pointA, distance.normalized, 0.08f, color);
        }
    }

    // draw a 3d arrow between invoked nodes
    public void SignalVisualizer(Vector3 from, Vector3 to,float time)
    {
        float ratio = time / displayPeriod;
        
        var draw = Draw.ingame;
        Vector3 distance = to - from;

        int numArrows = (int)(distance.magnitude / 0.35f);

        int greenElement = (int)( ratio * (float)numArrows);

        for (int i = 1; i < numArrows-1; i++)
        {
            Vector3 pointA = Vector3.Lerp(from, to, i / (float)numArrows);
            // Debug.Log("Green Element: " + greenElement);
            
            if(i == greenElement)
            {
                draw.Arrowhead(pointA, distance.normalized, 0.12f, Color.green);
            }
            else
            {
                draw.Arrowhead(pointA, distance.normalized, 0.08f, Color.cyan);
            }
        }
    }

}
