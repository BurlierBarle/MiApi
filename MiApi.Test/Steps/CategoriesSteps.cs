using FluentAssertions;
using MiApi.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;
using TechTalk.SpecFlow;

namespace MiApi.Test.Steps
{
    [Binding]
    public class CategoriesSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly HttpClient _client;
        private HttpResponseMessage _response;
        private CategoryDto? _categoryDto;

        public CategoriesSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            var appFactory = new WebApplicationFactory<Program>();
            _client = appFactory.CreateClient();
        }

        [Given(@"I have data to create a category with name ""(.*)""")]
        public void GivenIHaveDataToCreateACategoryWithName(string name)
        {
            _categoryDto = new CategoryDto
            {
                Name = name,
                Description = "Sample description"
            };
        }

        [When(@"I send a POST request to ""(.*)"" with the category data")]
        public async Task WhenISendAPOSTRequestToWithTheCategoryData(string url)
        {
            _response = await _client.PostAsJsonAsync(url, _categoryDto);
        }

        [Then(@"the response status should be (.*)")]
        public void ThenTheResponseStatusShouldBe(int statusCode)
        {
            ((int)_response.StatusCode).Should().Be(statusCode);
        }
    }
}
