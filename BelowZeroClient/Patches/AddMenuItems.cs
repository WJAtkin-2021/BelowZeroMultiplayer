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
                        GameObject gameObject = GameObject.Instantiate(GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox"));
                        gameObject.name = "MultiplayerMenu";
                        gameObject.transform.parent = GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/").transform;

                        gameObject.transform.position = new Vector3(GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox").transform.position.x, 0.25f, (float)(GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox").transform.position.z - 0.01));
                        gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                        gameObject.transform.transform.rotation = (GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox").transform.rotation);

                        GameObject.Destroy(gameObject.FindChild("HeaderText").GetComponent<TranslationLiveUpdate>());

                        GameObject.Destroy(gameObject.transform.Find("SubscriptionSuccess/Text").GetComponent<TranslationLiveUpdate>());
                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionSuccess/Text").GetComponent<TextMeshProUGUI>().text = "Server found !";

                        GameObject.Destroy(gameObject.transform.Find("SubscriptionInProgress/Text").GetComponent<TranslationLiveUpdate>());
                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionInProgress/Text").GetComponent<TextMeshProUGUI>().text = "Searching server...";

                        GameObject.Destroy(gameObject.transform.Find("SubscriptionError/Text").GetComponent<TranslationLiveUpdate>());
                        GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionError/Text").GetComponent<TextMeshProUGUI>().text = "Server not found";

                        gameObject.FindChild("Subscribe").GetComponent<Button>().onClick.AddListener(() =>
                        {
                            GameObject.Find("Menu canvas/Panel/MainMenu/RightSide/MultiplayerMenu/SubscriptionInProgress").SetActive(true);
                            try
                            {
                                // Save the socket info for next session
                                string enteredSocket = gameObject.FindChild("InputField").GetComponent<TMP_InputField>().text;
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

                        uGUI_InputField playerNameInputField = gameObject.GetComponent<uGUI_InputField>();

                        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("XMenu"));

                        gameObject.SetActive(true);
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
