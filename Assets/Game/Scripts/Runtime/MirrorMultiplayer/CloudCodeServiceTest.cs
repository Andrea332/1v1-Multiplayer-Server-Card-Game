using System;
using UnityEngine;

namespace Game
{
    public class CloudCodeServiceTest : MonoBehaviour
    {
        private static string serviceAccountKeyId = "e75a23d0-fa1a-4cea-a1b1-28578ffa5403";
        private static string serviceAccountSecretKey = "MkgIeyE5z_aFCUiksHkY5bYtc4G2fAVs";
        private static string projectId = "c1003709-927a-4012-bf44-37f4d8736696";
        private static string moduleName = "Authentication_Module";
        private static string functionName = "HelloWorld";
        private static string cloudFunctionUrl = $"https://cloud-code.services.api.unity.com/v1/projects/{projectId}/modules/{moduleName}/{functionName}";

        /*void Start()
    {
        StartCoroutine(InvokeCloudFunction("nomeFunzione"));
    }

    IEnumerator RetriveStatelessToken(string serviceAccountKeyId, string serviceAccountSecretKey)
    {

        UnityWebRequest request = UnityWebRequest.Post("https://services.api.unity.com/auth/v1/token-exchange","{\"params\": { }}","application/json" );
        request.SetRequestHeader("Authorization", "Basic");
    }
    IEnumerator InvokeCloudFunction(string functionName)
    {
        string url = $"{cloudFunctionUrl}{functionName}";
        UnityWebRequest request = UnityWebRequest.Post(url);
        request.SetRequestHeader("Authorization", $"Bearer {serviceAccountToken}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Risposta: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError("Errore nella chiamata a Cloud Code");
        }
    }*/
        [ContextMenu("CreateServiceAccountCredentials")]
        public string CreateBase64ServiceAccountCredentials()
        {
            return EncodeToBase64(serviceAccountKeyId + ":" + serviceAccountSecretKey);
        }

        public string EncodeToBase64(string input)
        {
            byte[] bytesToEncode = System.Text.Encoding.UTF8.GetBytes(input);
            string encodedText = Convert.ToBase64String(bytesToEncode);
            return encodedText;
        }
    }
}