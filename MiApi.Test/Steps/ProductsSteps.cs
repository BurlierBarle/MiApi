using FluentAssertions;
using MiApi.Context;
using MiApi.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;
using TechTalk.SpecFlow;
using MiApi.Models;
using Newtonsoft.Json;

namespace MiApi.Test.Steps
{
    [Binding]
    public class ProductsSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly HttpClient _client;
        private HttpResponseMessage _response;
        private ProductDto? _productDto;
        public ProductsSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            var appFactory = new WebApplicationFactory<Program>();
            _client = appFactory.CreateClient();
        }

        [Given(@"the database has products")]
        public async Task GivenTheDatabaseHasProducts()
        {
            // Seed the database with products if necessary
        }

        [Given(@"I have data to view a product with name ""(.*)""")]
        public void GivenIHaveDataToViewAProductWithName(string name)
        {
            _productDto = new ProductDto
            {
                Name = name
            };
        }

        [Given(@"I have data to create a product with name ""(.*)"" in the category id ""(.*)""")]
        public void GivenIHaveDataToCreateAProductWithNameInTheCategoryId(string productName, string categoryId)
        {
            _productDto = new ProductDto
            {
                Name = productName,
                Description = "Sample description",
                CategoryIds = [6]
            };
        }


        [When(@"I send a GET request to ""(.*)"" with the products data")]
        public async Task WhenISendAGETRequestToWithTheProductsData(string url)
        {
            _response = await _client.GetAsync(url);
        }

        [When(@"I send a GET request products to ""(.*)""")]
        public async Task WhenISendAGETRequestProductsTo(string url)
        {
            _response = await _client.GetAsync(url);
        }

        [When(@"I send a POST request to ""(.*)"" with the product data")]
        public async Task WhenISendAPostRequestToWithTheProductData(string url)
        {
            _response = await _client.PostAsJsonAsync(url, _productDto);
        }

        [Then(@"The response status should be (.*)")]
        public void ThenTheResponseStatusShouldBe(int statusCode)
        {
            ((int)_response.StatusCode).Should().Be(statusCode);
        }

        [Then(@"the response should contain a list of products with the categories")]
        public async Task ThenTheResponseShouldContainAListOfProductsWithTheCategories()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Then(@"the product should be created in the database")]
        public void ThenTheProductShouldBeCreatedInTheDatabase()
        {
            // Verify that the category does not exist in the database
        }

        [Then(@"The response should get an error message that the name is already in use")]
        public async Task ThentTheResponseShouldGetAnErrorMessageThatTheNameIsAlreadyInUse()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().Contain("The name is already in use.");
        }
    }
}
