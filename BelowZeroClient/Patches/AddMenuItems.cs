using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
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
                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionSuccess/Text").GetComponent<TextMeshProUGUI>().text = "Server found !";

                        GameObject.Destroy(multiplayerMenu.transform.Find("SubscriptionInProgress/Text").GetComponent<TranslationLiveUpdate>());
                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionInProgress/Text").GetComponent<TextMeshProUGUI>().text = "Searching server...";

                        GameObject.Destroy(multiplayerMenu.transform.Find("SubscriptionError/Text").GetComponent<TranslationLiveUpdate>());
                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionError/Text").GetComponent<TextMeshProUGUI>().text = "Server not found";

                        GameObject playerNameBox = GameObject.Instantiate(multiplayerMenu.FindChild("InputField"), multiplayerMenu.transform);
                        playerNameBox.name = "PlayerNameInput";
                        playerNameBox.transform.localPosition = new Vector3(-202.7f, -85.0f, 0.0f);
                        playerNameBox.GetComponent<TMP_InputField>().text = "";

                        TextMeshProUGUI placeholderTest = GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/PlayerNameInput/Placeholder").GetComponent<TextMeshProUGUI>();
                        placeholderTest.text = "Enter a player name...";

                        ErrorMessage.AddMessage("Got this far...");

                        Image backGroundImage = multiplayerMenu.transform.GetComponent<Image>();
                        FileLog.Log($"Background Image");
                        FileLog.Log($"SIZE: {backGroundImage.rectTransform.sizeDelta}");
                        backGroundImage.rectTransform.sizeDelta = new Vector2(401.3f, 111.0f);

                        multiplayerMenu.FindChild("Subscribe").GetComponent<Button>().onClick.AddListener(() =>
                        {
                            GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionInProgress").SetActive(true);
                            try
                            {
                                // Save the socket info for next session
                                string enteredSocket = multiplayerMenu.FindChild("InputField").GetComponent<TMP_InputField>().text;
                                ApplicationSettings.SaveSocket(enteredSocket);
                                NetworkClient.Instance.AttemptServerConnection(enteredSocket);

                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionInProgress").SetActive(false);

                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionSuccess").SetActive(true);
                            }
                            catch (SocketException e)
                            {
                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionInProgress").SetActive(false);

                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionError").SetActive(true);
                            }
                            catch (FormatException e)
                            {
                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionInProgress").SetActive(false);

                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionError/Text").GetComponent<TextMeshProUGUI>().text = "Invalid address ip";
                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionError").SetActive(true);
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionInProgress").SetActive(false);

                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionError/Text").GetComponent<TextMeshProUGUI>().text = "No port specified or wrong ip address format";
                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionError").SetActive(true);
                            }
                            catch (Exception e)
                            {
                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionInProgress").SetActive(false);

                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionError/Text").GetComponent<TextMeshProUGUI>().text = "Uknown error occured.";
                                GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionError").SetActive(true);
                            }
                        });

                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/HeaderText").GetComponent<TextMeshProUGUI>().text = "Join a server";
                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/InputField/Placeholder").GetComponent<TextMeshProUGUI>().text = "Enter the ip adress of the server...";

                        string socket = ApplicationSettings.LoadSocket();

                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/InputField").GetComponent<TMP_InputField>().text = socket;

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
