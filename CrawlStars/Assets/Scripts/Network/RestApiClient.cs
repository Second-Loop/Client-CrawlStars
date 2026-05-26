using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Network {
    public sealed class RestApiClient {
        private readonly string baseUrl;
        private string jwtAccessToken;

        public RestApiClient(string baseUrl) {
            this.baseUrl = baseUrl.TrimEnd('/');
        }

        public void SetJwtToken(string accessToken) {
            jwtAccessToken = accessToken;
        }

        public void ClearJwtToken() {
            jwtAccessToken = null;
        }

        public UniTask<TResponse> GetAsync<TResponse>(string path) {
            return SendAsync<object, TResponse>("GET", path, null);
        }

        public UniTask<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body) {
            return SendAsync<TRequest, TResponse>("POST", path, body);
        }

        public UniTask<TResponse> PutAsync<TRequest, TResponse>(string path, TRequest body) {
            return SendAsync<TRequest, TResponse>("PUT", path, body);
        }

        public UniTask<TResponse> PatchAsync<TRequest, TResponse>(string path, TRequest body) {
            return SendAsync<TRequest, TResponse>("PATCH", path, body);
        }

        public async UniTask DeleteAsync(string path) {
            await SendRawAsync("DELETE", path, null);
        }

        private async UniTask<TResponse> SendAsync<TRequest, TResponse>(string method, string path, TRequest body) {
            string jsonBody = body == null
                ? null
                : JsonConvert.SerializeObject(body);

            string responseJson = await SendRawAsync(method, path, jsonBody);

            if (string.IsNullOrWhiteSpace(responseJson)) {
                return default;
            }

            return JsonConvert.DeserializeObject<TResponse>(responseJson);
        }

        private async UniTask<string> SendRawAsync(string method, string path, string jsonBody) {
            string url = $"{baseUrl}/{path.TrimStart('/')}";

            using UnityWebRequest request = new UnityWebRequest(url, method);

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");

            if (!string.IsNullOrEmpty(jwtAccessToken)) {
                request.SetRequestHeader("Authorization", "Bearer " + jwtAccessToken);
            }

            if (!string.IsNullOrEmpty(jsonBody)) {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            await request.SendWebRequest();

            string responseBody = request.downloadHandler?.text ?? string.Empty;

            if (request.result == UnityWebRequest.Result.Success) {
                return responseBody;
            }

            throw new RestApiException(
                request.responseCode,
                request.error,
                responseBody
            );
        }
    }
}

public sealed class RestApiException : Exception {
    public long StatusCode { get; }
    public string ResponseBody { get; }

    public RestApiException(long statusCode, string message, string responseBody)
        : base(message) {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}