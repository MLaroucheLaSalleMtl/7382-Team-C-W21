using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class SelectCharacter : MonoBehaviour
{
    [SerializeField] private MMONetworkManager manager;
    [SerializeField] private GameObject selectCharacterMenu;
    [SerializeField] private CreateCharacter createCharacterMenu;
    [SerializeField] private Button playButton;
    [SerializeField] private Button deleteButton;
    
    // Update is called once per frame
    void Update()
    {
        // show while in lobby and while not creating a character
        if (manager.state == NetworkState.Lobby && !createCharacterMenu.IsVisible())
        {
            selectCharacterMenu.SetActive(true);

            // characters available message received already?
            if (manager.availableCharactersMsg != null)
            {
                AvailableCharactersMsg.CharacterPreview[] characters = manager.availableCharactersMsg.characters;

                // start button: calls AddPLayer which calls OnServerAddPlayer
                // -> button sends a request to the server
                // -> if we press button again while request hasn't finished
                //    then we will get the error:
                //    'ClientScene::AddPlayer: playerControllerId of 0 already in use.'
                //    which will happen sometimes at low-fps or high-latency
                // -> internally ClientScene.AddPlayer adds to localPlayers
                //    immediately, so let's check that first
                playButton.interactable = (manager.selectedCharacter != -1);
                playButton.onClick.SetListener(() => {
                    // Set client "ready"
                    ClientScene.Ready(NetworkClient.connection);

                    // Send SelectCharacter message (need to be ready first!)
                    NetworkClient.connection.Send(new SelectCharacterMsg{ index = manager.selectedCharacter });

                    // Clear character selection previews
                    manager.ClearPreviews();

                    // make sure we can't select twice and call AddPlayer twice
                    selectCharacterMenu.SetActive(false);
                });

                // Delete button
                deleteButton.interactable = (manager.selectedCharacter != -1);
                
                // Check if we're not already trying to delete a character
                // so we don't end up setting up a different listener if raycast
                // hits the character behind the Confirm button
                if (!manager.popupWindow.IsVisible())
                {
                    int selectedCharacterCache = manager.selectedCharacter;
                    deleteButton.onClick.SetListener(() =>
                    {
                        manager.popupWindow.Show(
                            "Do you really want to delete <b>" + characters[selectedCharacterCache].name + "</b>?",
                            "Confirm",
                            () => { NetworkClient.Send(new DeleteCharacterMsg {index = selectedCharacterCache}); }
                        );
                    });
                }
            }
        }
    }
    
    public void Show()
    {
        selectCharacterMenu.SetActive(true);
    }
    
    public void Hide()
    {
        selectCharacterMenu.SetActive(false);
    }

    public bool IsVisible()
    {
        return selectCharacterMenu.activeSelf;
    }
}
