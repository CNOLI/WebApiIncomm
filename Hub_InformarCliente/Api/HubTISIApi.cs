using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Hub_InformarCliente.Hub_Token;

namespace Hub_InformarCliente
{
    public class HubTISIApi
    {
        private HttpClient api = new HttpClient();
        public HubTISIApi(string ApiURL, string UserName, string Password)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            api = new HttpClient(clientHandler);
            api.Timeout = TimeSpan.FromMinutes(5);

            api.BaseAddress = new Uri(ApiURL);

            api.DefaultRequestHeaders.Accept.Clear();

            var token = GetTokenAsync(UserName, Password).Result;
            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);
        }
        public async Task<AccessToken> GetTokenAsync(string UserName, string Password)
        {
            AccessToken accessToken = new AccessToken();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                UserToken usrToken = new UserToken();
                usrToken.UserName = UserName;
                usrToken.Password = Password;

                api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpContent = new StringContent(JsonConvert.SerializeObject(usrToken), Encoding.UTF8, "application/json");

                response = await api.PostAsync("Auth/token", httpContent).ConfigureAwait(false);

                accessToken = JsonConvert.DeserializeObject<AccessToken>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return accessToken;

        }

        public async Task<Hub_Result> InformarTransaccion(Hub_Informar modelo)
        {
            Hub_Result result = new Hub_Result();

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");

                response = await api.PostAsync("Transaccion/informar", httpContent).ConfigureAwait(false);

                result = JsonConvert.DeserializeObject<Hub_Result>(await response.Content.ReadAsStringAsync());
                if (result == null)
                {
                    result = new Hub_Result();
                    result.codigo = "99";
                    result.mensaje = "El servicio no se encuentra disponible.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return result;
        }
    }
}
