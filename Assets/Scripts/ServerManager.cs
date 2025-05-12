using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class ServerManager : MonoBehaviourPunCallbacks
{
    
     public GameObject roomPrefab;
     public GameObject PlayerPrefab;
     public GameObject content;
     public GameObject contentPlayer;
    public GameObject ServerPage;
    public GameObject MainPage;
    public GameObject UsernamePage;
    public GameObject RoomPage;
    public GameObject RoomInPage;
    public TMP_InputField Username;
    public GameObject[] AllRooms;

    private bool isMasterClient = false;
    public GameObject StartGameButton;
    
    public void onClickJoinServerInMainPage()
    {
         UsernamePage.SetActive(true);
         MainPage.SetActive(false);
    }
    public void onClickJoinRoomInServerPage()
    {
        ServerPage.SetActive(false);
        RoomPage.SetActive(true);
    }

    public void onClickJoinServerInUsernamePage()
    {
        if(Username.text == "")
        {   
            Debug.Log("Username boş olamaz.");
        }
        else
        {
            Debug.Log(Username.text+" : Lobiye giriş yaptınız");
            connectServer();
            UsernamePage.SetActive(false);
            ServerPage.SetActive(true);
        } 
    }
   
    public override void OnJoinedLobby()
    {
        Debug.Log("Denendi.");
    }
    
    
   
  
    
   public override void OnRoomListUpdate(List<RoomInfo> roomList)
{
    foreach (RoomInfo room in roomList)
    {
        
        GameObject roomObject = FindRoomObject(room.Name);
        if (roomObject != null)
        {
            if (!room.IsOpen || !room.IsVisible || room.PlayerCount == 0)
            {
               
                Destroy(roomObject);
            }
            else
            {
               
                string playerCount = room.PlayerCount + "/" + room.MaxPlayers;
                roomObject.GetComponentInChildren<Room>().PlayerCount.text = playerCount;
            }
        }
        else
        {
            if (room.IsOpen && room.IsVisible && room.PlayerCount > 0)
            {
                
                string playerCount = room.PlayerCount + "/" + room.MaxPlayers;
                GameObject newRoomObject = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, content.transform);
                newRoomObject.GetComponentInChildren<Room>().Name.text = room.Name;
                newRoomObject.GetComponentInChildren<Room>().RoomUsername.text = room.CustomProperties["RoomCreator"].ToString();
                newRoomObject.GetComponentInChildren<Room>().PlayerCount.text = playerCount;
            }
        }
    }
}


// Verilen oda ismine göre listedeki oda oyun nesnesini bulur
private GameObject FindRoomObject(string roomName)
{
    foreach (Transform child in content.transform)
    {
        Room roomComponent = child.GetComponentInChildren<Room>();
        if (roomComponent != null && roomComponent.Name.text == roomName)
        {
            return child.gameObject;
        }
    }
    return null;
}





    
    public void onClickBackInUsernamePage()
    {
        UsernamePage.SetActive(false);
        MainPage.SetActive(true);
    }
    public void onClickBackInServerPage()
    {
        UsernamePage.SetActive(true);
        ServerPage.SetActive(false);
        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("Odadan Serv");
        }
        disconnectServer();
    }
     public void onClickBackInRoomsPage()
    {
        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("Odadan çıkış yapıldı");
        }
        RoomPage.SetActive(false);
        ServerPage.SetActive(true);
        
    }
    public void onClickCreateRoom()
{
    int roomID = Random.Range(999,999999);
    RoomOptions options = new RoomOptions();
    options.MaxPlayers = 8;
    ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
    customProperties.Add("RoomCreator", PhotonNetwork.NickName);
    options.CustomRoomProperties = customProperties;
    options.CustomRoomPropertiesForLobby = new string[] { "RoomCreator" }; 
    PhotonNetwork.CreateRoom(roomID.ToString(), options, TypedLobby.Default);
    RoomInPage.SetActive(true);
    RoomPage.SetActive(false);
    ServerPage.SetActive(false);
}
    public void connectServer()
    {
        PhotonNetwork.ConnectUsingSettings();  
    }
    public void disconnectServer()
    {
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Oda oluşturuldu.");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Oda oluşturulamadı.");
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.NickName = Username.text;
        PhotonNetwork.JoinLobby();
        Debug.Log("Sunucuya giriş yapıldı.");
    }
   

    public void joinRoomInList(string RoomName)
    {
        PhotonNetwork.JoinRoom(RoomName);
    }
    public void StartGame()
{
    // Sadece odanın sahibi (master client) oyuncu bu butona basabilir
    if (PhotonNetwork.IsMasterClient)
    {
        // Tüm oyunculara oyun sahnesini yükleme komutu gönder
        photonView.RPC("LoadGameScene", RpcTarget.All);
    }
}

[PunRPC]
private void LoadGameScene()
{
    // Tüm oyuncuları game.scene sahnesine aktar
    PhotonNetwork.LoadLevel("LoadGameScene");
}
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        RoomInPage.SetActive(true);
        RoomPage.SetActive(false);
        Debug.Log("Odaya giriş yaptınız");

        // Odadaki tüm oyuncuları listele
        foreach (var player in PhotonNetwork.PlayerList)
        {
            InstantiateUserListItem(player.NickName);
        }

          // Odanın sahibi ise Start Game butonunu aktif et
        if (PhotonNetwork.IsMasterClient)
        {
            isMasterClient = true;
            StartGameButton.SetActive(true);
        }
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        // Oyuncu odadan çıktığında contentPlayer içindeki tüm objeleri temizle
        foreach (Transform child in contentPlayer.transform)
        {
            Destroy(child.gameObject);
        }
         isMasterClient = false;
            StartGameButton.SetActive(false);
    }
    public void LeaveRoomButton()
    {
        PhotonNetwork.LeaveRoom();
        RoomInPage.SetActive(false);
        Debug.Log("Odadan çıktınız!");
        RoomPage.SetActive(true);
    }

  public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        InstantiateUserListItem(newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
         // Eğer odadan ayrılan oyuncu odanın sahibi ise Start Game butonunu kapat
        if (otherPlayer.IsMasterClient)
        {
            isMasterClient = false;
            StartGameButton.SetActive(false);
        }
        DestroyUserListItem(otherPlayer.NickName);
    }
     public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        // Yeni odanın sahibi ise Start Game butonunu aktif et
        if (newMasterClient == PhotonNetwork.LocalPlayer)
        {
            isMasterClient = true;
            StartGameButton.SetActive(true);
        }
        else
        {
            isMasterClient = false;
            StartGameButton.SetActive(false);
        }
    }
    private void InstantiateUserListItem(string nickname)
    {
        GameObject newUserItem = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, contentPlayer.transform);
        newUserItem.GetComponentInChildren<RoomIn>().Name.text = nickname;
        
        
    }
    private void DestroyUserListItem(string nickname)
{
    foreach (Transform child in contentPlayer.transform)
    {
        if(child.GetComponent<RoomIn>().Name.text.Equals(nickname))
        {
            Destroy(child.gameObject);
            break; // Found and destroyed, exit the loop
        }
    }
}
    
}
