using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
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

        public UniTask<TResponse> GetAsync<TResponse>(string path) 
            => SendAsync<object, TResponse>("GET", path, null);

        public UniTask<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body)
            => SendAsync<TRequest, TResponse>("POST", path, body);

        public UniTask<TResponse> PutAsync<TRequest, TResponse>(string path, TRequest body) 
            => SendAsync<TRequest, TResponse>("PUT", path, body);

        public UniTask<TResponse> PatchAsync<TRequest, TResponse>(string path, TRequest body)
            => SendAsync<TRequest, TResponse>("PATCH", path, body);

        public async UniTask DeleteAsync(string path) => await SendRawAsync("DELETE", path, null);

        private async UniTask<TResponse> SendAsync<TRequest, TResponse>(string method, string path, TRequest body) {
            string jsonBody = body == null ? null : JsonConvert.SerializeObject(body);
            string responseJson = await SendRawAsync(method, path, jsonBody);

            if (string.IsNullOrWhiteSpace(responseJson)) return default;
            return JsonConvert.DeserializeObject<TResponse>(responseJson);
        }

        private async UniTask<string> SendRawAsync(string method, string path, string jsonBody) {
            // path에 '/'가 중복으로 들어가는 것을 방지
            string url = $"{baseUrl}/{path.TrimStart('/')}";

            using var request = new UnityWebRequest(url, method);

            // 서버 응답 body를 메모리에 저장하기 위함
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // 응답을 json으로 줄 것을 요청
            request.SetRequestHeader("Accept", "application/json");

            if (!string.IsNullOrEmpty(jwtAccessToken)) {
                request.SetRequestHeader("Authorization", "Bearer " + jwtAccessToken);
            }

            if (!string.IsNullOrEmpty(jsonBody)) {
                // HTTP 요청 body는 문자열이 아니라 바이트로 전송하기 때문
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                
                // 요청이 json이라는 것을 알려줌
                request.SetRequestHeader("Content-Type", "application/json");
            }

            await request.SendWebRequest();

            string responseBody = request.downloadHandler?.text;
            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"RestApiClient.SendRawAsync::{request.responseCode}/{request.error}/{responseBody}");
            }
            return responseBody;
        }
    }
}
