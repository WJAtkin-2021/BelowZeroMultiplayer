using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BelowZeroClient
{
    class AddMenuItems
    {
        [HarmonyPatch(typeof(MainMenuRightSide), "OpenGroup")]
        public class Patches
        {
            [HarmonyPostfix]
            static void PostFix(string target, MainMenuRightSide __instance)
            {
                if (GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu") == null)
                {
                    // Create the network client
                    GameObject networkClientGO = new GameObject("NetworkClient");
                    NetworkClient networkClient = networkClientGO.AddComponent<NetworkClient>();
                    ThreadManager threadManager = networkClientGO.AddComponent<ThreadManager>();

                    if (target == "SavedGames")
                    {
                        // Setup the socket entry
                        GameObject multiplayerMenu = GameObject.Instantiate(GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox"));
                        multiplayerMenu.name = "MultiplayerMenu";
                        multiplayerMenu.transform.parent = GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/").transform;
                        multiplayerMenu.transform.position = new Vector3(GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox").transform.position.x, 0.285f, (float)(GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox").transform.position.z - 0.01));
                        multiplayerMenu.transform.localScale = new Vector3(1f, 1f, 1f);
                        multiplayerMenu.transform.transform.rotation = (GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox").transform.rotation);

                        GameObject.Destroy(multiplayerMenu.FindChild("HeaderText").GetComponent<TranslationLiveUpdate>());
                        GameObject.Destroy(multiplayerMenu.transform.Find("SubscriptionSuccess/Text").GetComponent<TranslationLiveUpdate>());
                        GameObject.Destroy(multiplayerMenu.transform.Find("SubscriptionInProgress/Text").GetComponent<TranslationLiveUpdate>());
                        GameObject.Destroy(multiplayerMenu.transform.Find("SubscriptionError/Text").GetComponent<TranslationLiveUpdate>());

                        GameObject playerNameBox = GameObject.Instantiate(multiplayerMenu.FindChild("InputField"), multiplayerMenu.transform);
                        playerNameBox.name = "PlayerNameInput";
                        playerNameBox.transform.localPosition = new Vector3(-202.7f, -85.0f, 0.0f);

                        TextMeshProUGUI placeholderTest = GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/PlayerNameInput/Placeholder").GetComponent<TextMeshProUGUI>();
                        placeholderTest.text = "Enter a player name...";

                        Image backGroundImage = multiplayerMenu.transform.GetComponent<Image>();
                        backGroundImage.rectTransform.sizeDelta = new Vector2(401.3f, 111.0f);

                        NetworkClient.m_instance.OnFailedToConnect += (() => {
                            multiplayerMenu.FindChild("Subscribe").GetComponent<Button>().interactable = true;
                        });

                        NetworkClient.m_instance.OnConnectionRefused += (() => {
                            multiplayerMenu.FindChild("Subscribe").GetComponent<Button>().interactable = true;
                        });

                        multiplayerMenu.FindChild("Subscribe").GetComponent<Button>().onClick.AddListener(() =>
                        {
                            try
                            {
                                // Save the socket info for next session
                                string enteredSocket = multiplayerMenu.FindChild("InputField").GetComponent<TMP_InputField>().text;
                                string enteredPlayerName = playerNameBox.GetComponent<TMP_InputField>().text;
                                ApplicationSettings.SaveSocket(enteredSocket);
                                ApplicationSettings.SavePlayerName(enteredPlayerName);
                                multiplayerMenu.FindChild("Subscribe").GetComponent<Button>().interactable = false;
                                NetworkClient.m_instance.AttemptServerConnection(enteredSocket, enteredPlayerName);
                            }
                            catch { }
                        });

                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/HeaderText").GetComponent<TextMeshProUGUI>().text = "Join a server";
                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/InputField/Placeholder").GetComponent<TextMeshProUGUI>().text = "Enter the ip adress of the server...";

                        string socket = ApplicationSettings.LoadSocket();
                        string name = ApplicationSettings.LoadPlayerName();

                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/InputField").GetComponent<TMP_InputField>().text = socket;
                        playerNameBox.GetComponent<TMP_InputField>().text = name;

                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/ViewPastUpdates/").SetActive(false);

                        uGUI_InputField playerNameInputField = multiplayerMenu.GetComponent<uGUI_InputField>();

                        SceneManager.MoveGameObjectToScene(multiplayerMenu, SceneManager.GetSceneByName("XMenu"));

                        multiplayerMenu.SetActive(true);
                    }
                }
                else
                {
                    GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu").SetActive(target == "SavedGames");
                }
            }
        }
    }
}
