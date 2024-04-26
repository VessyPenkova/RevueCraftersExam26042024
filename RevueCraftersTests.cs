using RestSharp;
using RestSharp.Authenticators;
using RevueCrafters.Models;
using System.Net;
using System.Text.Json;

namespace RevueCrafters
{
    public class TestsRevueCraftersTests
    {
        private RestClient _client;
        private RestRequest? _request;
        private RestResponse? _response;
        private ApiResponseDTO? _responseData;
        private RevueDTO? _body;

        private const string BASE_URL = "https://d2925tksfvgq8c.cloudfront.net/";
        private const string USERNAME = "penkov4itu";
        private const string PASSWORD = "penkov4ituNaPopravka";
        private const string EMAIL    = "penko4itu@mail.com";

        private static readonly string endPointForCreate      = "api/Revue/Create";
        private static readonly string endPointForEdit        = "api/Revue/Edit?revueId=";
        private static readonly string endPointForDelete      = "api/Revue/Delete?revueId=";
        private static readonly string endPointAuthentication = "api/User/Authentication";
        private static readonly string endPointToGetRevueId   = "api/Revue/All";
   
        private string? revueTitle;
        private string? revueId;

        [OneTimeSetUp]
        public void Setup()
        {
            var jwtToken = GetJwtToken(EMAIL, PASSWORD);

            var options = new RestClientOptions(BASE_URL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this._client = new RestClient(options);

            Random random = new();
            int randomNumber = random.Next(0, 999);
            revueTitle = "Revue Name: " + randomNumber;
        }

        //Positive
        [Test, Order(1)]
        public void CreateNewRevue_WithValidRequiredFields_ShouldReturn_Successfully_Created_Status_Code_200()
        {
            //Arrange
            this._body = new RevueDTO
            {
                Title = revueTitle,
                Description = $"This is a description for the revue with the {revueTitle}",
                Url = "",
            };
            this._request = new RestRequest(endPointForCreate, Method.Post);
            _request.AddJsonBody(_body);


            //Act
            this._response = this._client.Execute(_request);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(_response.Content);
            responseData.StatusCode = _response.StatusCode;
            string expectedMessage = "Successfully created!";
         

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(responseData.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(responseData.Message, Is.EqualTo(expectedMessage));
            });
        }
        // Negative
        [Test, Order(2)]
        public void TryToCreateNewRevue_WithoutRequiredFields_ShouldReturn_Bad_Request_Status_Code_400()
        {
            //Arrange
            this._body = new RevueDTO
            {
                Title = "",
                Description = "",
                Url = "",
            };
            this._request = new RestRequest(endPointForCreate, Method.Post);
            _request.AddJsonBody(_body);


            //Act
            this._response = this._client.Execute(_request);
            _responseData = JsonSerializer.Deserialize<ApiResponseDTO>(_response.Content);
            _responseData.StatusCode = _response.StatusCode;


            //Assert
            Assert.That(_responseData.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test, Order(3)]
        public void GetAll_Revues_ShouldReturnNotEmptyArray_Status_Code_200()
        {
            _request = new RestRequest(endPointToGetRevueId, Method.Get);
            _response = this._client.Execute(_request);
          
            var responseData = JsonSerializer.Deserialize<List<ApiResponseDTO>>(_response.Content);
            Assert.IsNotNull(responseData);
            Assert.IsNotEmpty(responseData);
            Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            revueId = responseData.LastOrDefault()?.RevueId;

        }

        //Positive      
        [Test, Order(4)]
        public void EditRevue_ShouldReturn_Successfully_Edited_Status_Code_200()
        {
            //Arrange
            revueId = GetRevueId();
            this._body = new RevueDTO
            {
                Title = revueTitle + " was edited at " + DateTime.Now.Date,
                Description = $"Description of {revueTitle}",
                Url = "",
            };
            this._request = new RestRequest($"{endPointForEdit}{revueId}", Method.Put);
            _request.AddQueryParameter("revueId", revueId);
            _request.AddBody(this._body);


            //Act
            _response = this._client.Execute(_request);
            _responseData = JsonSerializer.Deserialize<ApiResponseDTO>(_response.Content);
            _responseData.StatusCode = _response.StatusCode;
            string expectedMessage = "Successfully edited";


            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(_responseData.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(_responseData.Message, Is.EqualTo(expectedMessage));
            });
        }
        //Negative
        [Test, Order(5)]
        public void EditNotExistingRevue_ShouldReturn_There_Is_No_Such_Revue_Status_Code_400()
        {
            //Arrange
            revueId = "notExcitingRevueId";

            _body = new RevueDTO
            {
                Title = revueTitle,
                Description = $"This is the edited description of {_body.Title}",
                Url = string.Empty,
            };

            this._request = new RestRequest($"{endPointForEdit}{revueId}", Method.Put);
            _request.AddQueryParameter("revueId", revueId);
            _request.AddBody(this._body);
         

            //Act
            _response = this._client.Execute(_request);
            _responseData = JsonSerializer.Deserialize<ApiResponseDTO>(_response.Content);
            _responseData.StatusCode = _response.StatusCode;
            string expectedMessage = "There is no such revue!";


            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(_responseData.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(_responseData.Message, Is.EqualTo(expectedMessage));
            });
        }
        //Negative
        [Test, Order(6)]
        public void TryToDeleteNonExistingRevue_ShouldReturnUnable_To_Delete_This_Revue_Status_Code_400()
        {

            //Arrange
            revueId = "notExcitingRevue";
            this._request = new RestRequest($"{ endPointForDelete }{  revueId}", Method.Delete);
            _request.AddQueryParameter("revueId", revueId);


            //Act
            _response = this._client.Execute(_request);
            _responseData = JsonSerializer.Deserialize<ApiResponseDTO>(_response.Content);
            _responseData.StatusCode = _response.StatusCode;
            string expectedResponse = "There is no such revue!";


            Assert.Multiple(() =>
            {
                Assert.That(_responseData.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(_responseData.Message, Is.EqualTo(expectedResponse));
            });

        }
        //Positive
        [Test, Order(7)]
        public void DeleteLastRevue_ShouldReturn_Deleted_SuccessfullyStatus_Code_200()
        {

            //Arrange
            revueId = GetRevueId();
            this._request = new RestRequest($"{endPointForDelete}{revueId}", Method.Delete);
            _request.AddQueryParameter("revueId", revueId);


            //Act
            _response = this._client.Execute(_request);
            _responseData = JsonSerializer.Deserialize<ApiResponseDTO>(_response.Content);
            _responseData.StatusCode = _response.StatusCode;
            string expectedResponse = "The revue is deleted!";


            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(_responseData.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(_responseData.Message, Is.EqualTo(expectedResponse));
            });

        }


        [OneTimeTearDown]
        public void ShutDown()
        {
            _client.Dispose();
        }

        private string GetJwtToken(string email, string password)
        {
            this._client = new RestClient(BASE_URL);
            this._request = new RestRequest(endPointAuthentication, Method.Post);

            this._body = new RevueDTO
            {
                Email = email,
                Password = password
            };
            this._request.AddJsonBody(this._body);

            this._response = _client.Execute(_request);

            if (_response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(_response.Content);

                var accessToken = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new InvalidOperationException("The JWT token is null or empty.");
                }
                return accessToken;

            }
            else
            {
                throw new InvalidOperationException($"Authentication failed: {_response.StatusCode}, {_response.Content}");
            }

        }

        private string GetRevueId()
        {
            this._request = new RestRequest(endPointToGetRevueId, Method.Get);
            this._response = _client.Execute(_request);

            var content = JsonSerializer.Deserialize<List<ApiResponseDTO>>(_response.Content);
            return content.LastOrDefault()?.Id;
        }
    }
}