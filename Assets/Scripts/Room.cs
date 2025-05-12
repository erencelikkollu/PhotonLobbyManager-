using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class Room : MonoBehaviour
{
   public TMP_Text Name;
   public TMP_Text RoomUsername;
   public TMP_Text PlayerCount;
   

   public void joinRoom()
   {
     GameObject.Find("ServerManager").GetComponent<ServerManager>().joinRoomInList(Name.text);
   }

}
