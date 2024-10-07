using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Services.Apis;
using Unity.Services.Apis.Economy;
using Unity.Services.Apis.PlayerAuthentication;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utilities
{
    public class UGSUtilities : EditorWindow
    {
        public VisualTreeAsset m_InspectorXML;

        private const string UgsServiceAccountPath = "Assets/Utilities/UGSUtilities/Editor/SO/UGSServiceAccount.asset";
        private const string UgsProjectIdPath = "Assets/Utilities/UGSUtilities/Editor/SO/UGSProjectId.asset";
        private const string UgsEnvironmentIdPath = "Assets/Utilities/UGSUtilities/Editor/SO/UGSEnvironmentId.asset";
        private const string UgsScopesPath = "Assets/Utilities/UGSUtilities/Editor/SO/UGSScopes.asset";
        private static string ServiceAccountKeyId 
        {
            get
            {
                UGSServiceAccount ugsServiceAccount = AssetDatabase.LoadAssetAtPath<UGSServiceAccount>(UgsServiceAccountPath);
                if(ugsServiceAccount == null) Debug.LogError($"UGSServiceAccount asset not found at: {UgsServiceAccountPath}");
                return ugsServiceAccount.serviceAccountKeyId;
            }
        }
        private static string ServiceAccountSecretKey
        {
            get
            {
                UGSServiceAccount ugsServiceAccount = AssetDatabase.LoadAssetAtPath<UGSServiceAccount>(UgsServiceAccountPath);
                if(ugsServiceAccount == null) Debug.LogError($"UGSServiceAccount asset not found at: {UgsServiceAccountPath}");
                return ugsServiceAccount.serviceAccountSecretKey;
            }
        }
        private static string ProjectId 
        {
            get
            {
                UGSProjectId ugsProjectId = AssetDatabase.LoadAssetAtPath<UGSProjectId>(UgsProjectIdPath);
                if(ugsProjectId == null) Debug.LogError($"UGSProjectId asset not found at: {UgsProjectIdPath}");
                return ugsProjectId.projectId;
            }
        }
        private static string EnvironmentId 
        {
            get
            {
                UGSEnvironmentId ugsEnvironmentId = AssetDatabase.LoadAssetAtPath<UGSEnvironmentId>(UgsEnvironmentIdPath);
                if(ugsEnvironmentId == null) Debug.LogError($"UGSEnvironmentId asset not found at: {UgsEnvironmentIdPath}");
                return ugsEnvironmentId.environmentId;
            }
        }
        private static List<string> Scopes
        {
            get
            {
                UGSScopes ugsScopes = AssetDatabase.LoadAssetAtPath<UGSScopes>(UgsScopesPath);
                if(ugsScopes == null) Debug.LogError($"UGSScopes asset not found at: {UgsScopesPath}");
                return ugsScopes.scopes;
            }
        }
        
        private TextField playerIdTextField;
        private TextField itemIdTextField;
        
        private TextField playerUsernameTextField;
        private TextField playerPasswordTextField;
        
        [MenuItem("Services/UGS Utilities")]
        public static void OpenWindow()
        {
            UGSUtilities wnd = GetWindow<UGSUtilities>();
            wnd.titleContent = new GUIContent("UGS Utilities");
            wnd.minSize = new Vector2(350, 400);
        }
        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            
            root.Add(m_InspectorXML.Instantiate());
            
            #region SIGN_IN
            
            var signInButton = root.Q<Button>("SignIn");
            signInButton.RegisterCallback<MouseUpEvent>(OnSignIn);
            
            #endregion
            
            #region ADD_ITEM_TO_PLAYER

            var addItemButton = root.Q<Button>("AddItem");
            addItemButton.RegisterCallback<MouseUpEvent>(OnAddItemToPlayerInventory);
            playerIdTextField = root.Q<TextField>("PlayerID");
            itemIdTextField = root.Q<TextField>("ItemID");

            #endregion

            #region CREATE_NEW_PLAYER_ACCOUNT_WITH_USERNAME_AND_PASSWORD

            var addPlayerAccountButton = root.Q<Button>("AddPlayerAccount");
            addPlayerAccountButton.RegisterCallback<MouseUpEvent>(OnCreatePlayerAccountWithUsernameAndPassword);
            playerUsernameTextField = root.Q<TextField>("PlayerUsername");
            playerPasswordTextField = root.Q<TextField>("PlayerPassword");
            
            #endregion
        }
        
        private void OnSignIn(MouseUpEvent evt)
        {
            SignInWithServiceAccount().Forget();
        }
        private async UniTask<ITrustedClient> SignInWithServiceAccount()
        {
            var iTrustedClient = ApiService.CreateTrustedClient();
            
            iTrustedClient.SetServiceAccount(ServiceAccountKeyId, ServiceAccountSecretKey);
            
            var exchangeResponse = await iTrustedClient.SignInWithServiceAccount(ProjectId, EnvironmentId, Scopes);

            if (exchangeResponse.IsSuccessful)
            {
                Debug.Log("Sign in success " + iTrustedClient.AccessToken);
                return iTrustedClient;
            }
            
            Debug.LogError("Sign in error: " + exchangeResponse.ErrorText);
            return null;
        }
        
        private void OnAddItemToPlayerInventory(MouseUpEvent evt)
        {
            AddItemToPlayerInventory(playerIdTextField.value, itemIdTextField.value);
        }
        private async void AddItemToPlayerInventory(string playerId, string itemId)
        {
            ITrustedClient iTrustedClient = await SignInWithServiceAccount();
            
            AddInventoryRequest addInventoryRequest = new AddInventoryRequest(itemId);
            
            //await EconomyService.Instance.Configuration.SyncConfigurationAsync();
            
            var inventoryResponse = await iTrustedClient.EconomyInventory.AddInventoryItem(ProjectId, playerId, addInventoryRequest);

            if (inventoryResponse.IsSuccessful)
            {
                Debug.Log("Add Inventory Item success");
            }
            else
            {
                Debug.LogError("Add Inventory Item error: " + inventoryResponse.ErrorText);
            }
        }
        
        private void OnCreatePlayerAccountWithUsernameAndPassword(MouseUpEvent evt)
        {
            CreatePlayerAccountWithUsernameAndPassword(playerUsernameTextField.value, playerPasswordTextField.value);
        }
        
        private async void CreatePlayerAccountWithUsernameAndPassword(string playerUsername, string playerPassword)
        {
            var iGameClient = ApiService.CreateGameClient();
            
            UsernamePasswordRequest usernamePasswordRequest = new UsernamePasswordRequest(playerUsername, playerPassword);

            var authenticationResponse = await iGameClient.SignUpWithUsernamePassword(ProjectId, usernamePasswordRequest);

            if (authenticationResponse.IsSuccessful)
            {
                Debug.Log("Account sign up success");
            }
            else
            {
                Debug.LogError("Account sign up error: " + authenticationResponse.ErrorText);
            }
        }
    }
}


